using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ImageOrganizer.BL.Operations.Helper;
using ImageOrganizer.BL.Provider;
using ImageOrganizer.BL.Provider.Impl;
using ImageOrganizer.Data;
using ImageOrganizer.Data.Container;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using ImageOrganizer.Resources;
using Tauron;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.FileImporter)]
    public class FileImporterRule : IOBusinessRuleBase<ImporterInput, Exception>
    {
        [Inject]
        private Lazy<ProviderManager> _providerManager;

        public override Exception ActionImpl(ImporterInput input)
        {
            try
            {
                BackUpDatabase.MakeBackup();

                ManualResetEvent pause = new ManualResetEvent(true);
                int isSet = 1;
                CancellationTokenSource source = new CancellationTokenSource();

                try
                {
                    input.OnPostMessage(UIResources.FileImporterRule_Starting, 0, 0, true);

                    // ReSharper disable AccessToDisposedClosure
                    input.Pause += () =>
                    {

                        if (isSet == 1)
                        {
                            Interlocked.Decrement(ref isSet);
                            pause?.Reset();
                        }
                        else
                        {
                            Interlocked.Increment(ref isSet);
                            pause?.Set();
                        }
                    };
                    input.Stop += () => source?.Cancel();
                    // ReSharper restore AccessToDisposedClosure

                    var provider = _providerManager.Value.Get(input.Provider);

                    using (var db = RepositoryFactory.Enter())
                    {
                        var images = RepositoryFactory.GetRepository<IImageRepository>();
                        var downloads = RepositoryFactory.GetRepository<IDownloadRepository>();

                        var context = db.GetContext<DatabaseImpl>();
                        List<ImageEntity> toSort = images.Query().ToList();

                        using (var fileTransaction = FileContainerManager.GetContainerTransaction())
                        {
                            Controller controller = new Controller(fileTransaction);
                            controller.PostMessage += input.OnPostMessage;
                            input.Pause += controller.OnPause;

                            source.Token.Register(controller.OnStop);

                            string[] files = input.FileLocation.GetFiles();
                            int amount = 0;
                            List<string> filesToCopy = new List<string>();

                            foreach (var file in files)
                            {
                                input.OnPostMessage(string.Format(UIResources.FileImporterRule_ImportFiles, amount, files.Length), amount, files.Length, false);

                                try
                                {
                                    pause.WaitOne(source.Token);
                                }
                                catch (OperationCanceledException)
                                {
                                }

                                if (source.IsCancellationRequested)
                                {
                                    return null;
                                }

                                string fileName = file.GetFileName();
                                string providerId = NonProvider.ProviderNon;

                                if (!provider.IsValid(fileName))
                                    providerId = provider.Id;

                                if (!FileContainerManager.CanAdd(file, Path.GetFileName))
                                    continue;

                                filesToCopy.Add(file);

                                var ent = new ImageEntity
                                {
                                    Added = DateTime.Now,
                                    Name = fileName,
                                    ProviderName = providerId
                                };

                                images.Add(ent);
                                toSort.Add(ent);
                                downloads.Add(fileName, DownloadType.DownloadTags, DateTime.Now, providerId, false);

                                amount++;
                            }

                            using (var dbTrabsaction = context.Database.BeginTransaction())
                            {
                                input.OnPostMessage(UIResources.FileImporterRule_SortingEntrys, 0, 0, true);
                                pause.WaitOne(source.Token);
                                toSort.SetOrder();

                                input.OnPostMessage(UIResources.FileImporterRule_SaveToDatabase, 0, 1, true);
                                pause.WaitOne(source.Token);                               
                                // ReSharper disable once MethodSupportsCancellation
                                db.SaveChangesAsync(source.Token).Wait();
                                
                                try
                                {
                                    if (FileContainerManager.Save(filesToCopy.ToArray(), Path.GetFileName, controller))
                                    {
                                        dbTrabsaction.Commit();
                                        fileTransaction.Commit();
                                    }
                                    else
                                    {
                                        dbTrabsaction.Rollback();
                                        fileTransaction.Rollback();
                                    }
                                }
                                catch (Exception)
                                {
                                    BackUpDatabase.Revert();
                                    throw;
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    pause.Dispose();
                    source.Dispose();

                    pause = null;
                    source = null;
                }

                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}
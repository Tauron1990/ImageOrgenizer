using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.BL.Operations.Helper;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrganizer.BL.Resources;
using Tauron.Application.ImageOrganizer.Container;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.FileImporter)]
    public class FileImporterRule : IOBusinessRuleBase<ImporterInput, Exception>
    {
        [Inject]
        private Lazy<IProviderManager> _providerManager;

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
                    input.OnPostMessage(BuissinesLayerResources.FileImporterRule_Starting, 0, 0, true);

                    // ReSharper disable AccessToDisposedClosure
                    input.Pause += () =>
                    {

                        if (isSet == 1)
                        {
                            Interlocked.Decrement(ref isSet);
                            // ReSharper disable once AccessToModifiedClosure
                            pause?.Reset();
                        }
                        else
                        {
                            Interlocked.Increment(ref isSet);
                            // ReSharper disable once AccessToModifiedClosure
                            pause?.Set();
                        }
                    };
                    // ReSharper disable once AccessToModifiedClosure
                    input.Stop += () => source?.Cancel();
                    // ReSharper restore AccessToDisposedClosure

                    var provider = _providerManager.Value.Get(input.Provider);
                    bool revertBackup = false;

                    using (var db = RepositoryFactory.Enter())
                    {
                        var images = RepositoryFactory.GetRepository<IImageRepository>();
                        var downloads = RepositoryFactory.GetRepository<IDownloadRepository>();
                        int downloadsCount = 0;

                        List<ImageEntity> toSort = images.Query(false).ToList();

                        using (var fileTransaction = FileContainerManager.GetContainerTransaction())
                        {
                            Controller controller = new Controller(fileTransaction);
                            controller.PostMessage += input.OnPostMessage;
                            input.Pause += controller.OnPause;

                            source.Token.Register(controller.OnStop);

                            string[] files = input.FileLocation.GetFiles();
                            int amount = 0;
                            List<string> filesToCopy = new List<string>();
                            List<ImageEntity> newImages = new List<ImageEntity>();

                            foreach (var file in files)
                            {
                                input.OnPostMessage(string.Format(BuissinesLayerResources.FileImporterRule_ImportFiles, amount, files.Length), amount, files.Length, false);

                                try
                                {
                                    Wait(pause, source.Token);
                                }
                                catch (OperationCanceledException) { }

                                if (source.IsCancellationRequested)
                                {
                                    return null;
                                }

                                string fileName = file.GetFileName();
                                string providerId = AppConststands.ProviderNon;

                                if (provider.IsValid(fileName))
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

                                newImages.Add(ent);
                                toSort.Add(ent);

                                DateTime downloadTime = DateTime.Now;
                                if (downloadsCount > 300)
                                    downloadTime = downloadTime + TimeSpan.FromDays(downloadsCount / 300d);

                                downloads.Add(fileName, DownloadType.DownloadTags, downloadTime, providerId, false, false, null);
                                downloadsCount++;

                                amount++;
                            }

                            images.AddRange(newImages);

                            input.OnPostMessage(BuissinesLayerResources.FileImporterRule_SortingEntrys, 0, 0, true);
                            Wait(pause, source.Token);
                            toSort.SetOrder();

                            input.OnPostMessage(BuissinesLayerResources.FileImporterRule_SaveToDatabase, 0, 1, true);
                            Wait(pause, source.Token);
                            // ReSharper disable once MethodSupportsCancellation

                            try
                            {
                                if (FileContainerManager.Save(filesToCopy.ToArray(), Path.GetFileName, controller))
                                {
                                    db.SaveChangesAsync(source.Token).Wait();
                                    fileTransaction.Commit();
                                }
                                else
                                {
                                    fileTransaction.Rollback();
                                    revertBackup = true;
                                }
                            }
                            catch (Exception)
                            {
                                db.Dispose();
                                BackUpDatabase.Revert();
                                throw;
                            }
                        }
                    }

                    if(revertBackup)
                        BackUpDatabase.Revert();
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

        private void Wait(WaitHandle handle, CancellationToken token)
        {
            while (!handle.WaitOne(1000))
                if (token.IsCancellationRequested)
                    break;
        }
    }
}
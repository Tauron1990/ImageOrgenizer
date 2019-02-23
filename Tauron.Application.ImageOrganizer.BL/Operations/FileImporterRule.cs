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
        private int _stepMax;
        private int _stepCount;

        [Inject]
        private Lazy<IProviderManager> _providerManager;

        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        [InjectRepo]
        public IDownloadRepository DownloadRepository { get; set; }

        private (int current, int max) GetStepCount(int min, int max)
        {
            if (min == 0 && max == 0) return (0, 0);

            int stepPlace = _stepCount * _stepMax;

            return (stepPlace + min, max);
        }

        public override Exception ActionImpl(ImporterInput input)
        {
            try
            {
                _stepMax = 5000;
                _stepCount = 0;

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

                    using (var db = Enter())
                    {
                        int downloadsCount = 0;

                        string[] filesBase = input.FileLocation.GetFiles();

                        foreach (var files in filesBase.Split(_stepMax).Select(r => r.ToArray()))
                        {
                            using (var fileTransaction = FileContainerManager.GetContainerTransaction())
                            {
                                Controller controller = new Controller(fileTransaction);
                                controller.PostMessage += (message, minimum, maximum, intermidiate) =>
                                {
                                    var real = GetStepCount(minimum, filesBase.Length);

                                    input.OnPostMessage(message, real.current, real.max, intermidiate);
                                };
                                input.Pause += controller.OnPause;

                                source.Token.Register(controller.OnStop);

                                
                                int amount = 0;
                                List<string> filesToCopy = new List<string>();
                                //List<ImageEntity> newImages = new List<ImageEntity>();

                                foreach (var file in files)
                                {
                                    var realstep = GetStepCount(amount, filesBase.Length);
                                    input.OnPostMessage(string.Format(BuissinesLayerResources.FileImporterRule_ImportFiles, realstep.current, realstep.max), realstep.current, realstep.max, false);

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

                                    ImageRepository.Add(ent);

                                    DateTime downloadTime = DateTime.Now;
                                    if (downloadsCount > 300)
                                        downloadTime = downloadTime + TimeSpan.FromHours((downloadsCount / 300d) * 2);

                                    DownloadRepository.Add(fileName, DownloadType.DownloadTags, downloadTime, providerId, false, false, null);
                                    downloadsCount++;

                                    amount++;
                                }
                                
                                Wait(pause, source.Token);

                                try
                                {
                                    if (FileContainerManager.Save(filesToCopy.ToArray(), Path.GetFileName, controller))
                                    {
                                        input.OnPostMessage(BuissinesLayerResources.FileImporterRule_SaveToDatabase, 0, 1, true);
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

                            _stepCount++;
                        }


                        input.OnPostMessage(BuissinesLayerResources.FileImporterRule_SortingEntrys, 0, 0, true);
                        Wait(pause, source.Token);
                        ImageRepository.SetOrder(ImageNaturalStringComparer.Comparer);

                        db.SaveChanges();
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
                var ex = e.Unwrap();

                return ex.InnerException ?? ex;
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
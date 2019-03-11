using System;
using System.IO;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.BL.Resources;
using Tauron.Application.ImageOrganizer.Container;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.Core.IO;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.SwitchContainer)]
    public class SwitchContainerRule : IOBusinessRuleBase<SwitchContainerInput, SwitchContainerOutput>
    {
        [Inject]
        public IDBSettings Settings { get; set; }

        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        private string CurrentDatabase => SettingsManager.Settings?.CurrentDatabase;
        private ContainerType CurrentContainerType => FileContainerManager.CurrentContainerType;
        private IIOInterface _io = IOInterfaceProvider.IOInterface;

        public override SwitchContainerOutput ActionImpl(SwitchContainerInput input)
        {
            using (FileContainerManager.EnterLock())
            {
                var old = FileContainerManager.ContainerFile;
                if (old == null) return BuildOutput();
                bool error = false;

                IContainerTransaction trans = null;
                try
                {
                    if (input.ContainerType == CurrentContainerType)
                    {
                        switch (input.ContainerType)
                        {
                            case ContainerType.Compose when Settings.CustomMultiPath == input.CustomPath:
                                return BuildOutput();
                            case ContainerType.Compose when Settings.CustomMultiPath != input.CustomPath:
                                if (!CheckSize(input.CustomPath))
                                    return BuildOutput(BuissinesLayerResources.SwitchContainnerRule_LowSpace);
                                if (IsSame(input))
                                    input.ClearCustom();
                                trans = Switch(input);
                                //FileContainerManager.Import(old, trans, input.OnMessage);
                                _io.Delete(!string.IsNullOrWhiteSpace(Settings.CustomMultiPath) ? Settings.CustomMultiPath 
                                    : SettingsManager.Settings?.CurrentDatabase.Replace('.', '-'), true, true);
                                trans.Commit();
                                return BuildOutput(true);
                            default:
                                return BuildOutput();
                        }
                    }
                    else switch (input.ContainerType)
                    {
                        case ContainerType.Compose when CurrentContainerType != ContainerType.Compose:
                        {
                            if (CurrentContainerType == ContainerType.Single || input.CustomPath == string.Empty || IsSame(input))
                            {
                                input.ClearCustom();
                                Switch(input, false);
                                return BuildOutput(true);
                            }

                            trans = FileContainerManager.GetContainerTransaction();
                            string tempLoc = CurrentDatabase + "tmp";
                            if (!CheckSize(tempLoc))
                                return BuildOutput(BuissinesLayerResources.SwitchContainnerRule_LowSpace);
                            if (!CurrentDatabase.ExisDirectory())
                                return BuildOutput(BuissinesLayerResources.SwitchContainnerRule_DicNotExis);

                            var result = _io.MoveTransacted(trans.TryCast<IKernelTransaction>(), CurrentDatabase, tempLoc);
                            if (result.ErrorCode != 0)
                                return BuildOutput(result.ErrorMessage);

                            Switch(input, false);
                            old = ContainerFactory.Begin().UseMulti().Initialize(tempLoc);
                            FileContainerManager.Import(old, trans, input.OnMessage);

                            _io.DeleteTransacted(trans.TryCast<IKernelTransaction>(), tempLoc, true, true);
                            trans.Commit();
                            return BuildOutput();
                        }
                        case ContainerType.Single when CurrentContainerType == ContainerType.Multi:
                        case ContainerType.Multi when CurrentContainerType == ContainerType.Single:
                        {
                            trans = Switch(input);
                            FileContainerManager.Import(old, trans, input.OnMessage);

                            foreach (var containerName in old.GetContainerNames())
                            {
                                if (containerName.HasExtension())
                                {
                                    if (File.Exists(containerName))
                                        _io.DeleteTransacted(trans.TryCast<IKernelTransaction>(), containerName, true);
                                }
                                else
                                {
                                    if(Directory.Exists(containerName))
                                        _io.DeleteTransacted(trans.TryCast<IKernelTransaction>(), containerName, true, true);
                                }
                            }

                            trans.Commit();
                            return BuildOutput();
                        }
                        default:
                            return BuildOutput(BuissinesLayerResources.SwitchContainnerRule_UnknowenConfig);
                    }
                }
                catch(Exception e)
                {
                    error = true;
                    trans?.Rollback();
                    return BuildOutput($"{e.GetType()} -- {e.Message}");
                }
                finally
                {
                    trans?.Dispose();

                    if (!error)
                    {
                        Settings.CustomMultiPath = input.CustomPath;
                        Settings.ContainerType = input.ContainerType;
                    }
                }
            }
        }

        private bool CheckSize(string input)
        {
            DriveInfo rootNew = new DriveInfo(Argument.CheckResult(Path.GetPathRoot(string.IsNullOrEmpty(input)
                ? CurrentDatabase
                : input), "Not RootPath Was given"));
            long original = FileContainerManager.ComputeSize();

            return rootNew.AvailableFreeSpace > original;
        }

        private bool IsSame(SwitchContainerInput input) => input.CustomPath == CurrentDatabase;

        private IContainerTransaction Switch(SwitchContainerInput input, bool transact = true)
        {
            FileContainerManager.Switch(CurrentDatabase, input.ContainerType, input.CustomPath);
            return transact ? FileContainerManager.GetContainerTransaction() : null;
        }

        private SwitchContainerOutput BuildOutput(bool sync = false) => new SwitchContainerOutput(sync, true, string.Empty);
        private SwitchContainerOutput BuildOutput(string error) => new SwitchContainerOutput(false, false, error);
    }
}
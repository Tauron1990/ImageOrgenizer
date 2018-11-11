using System;
using Alphaleonis.Win32.Filesystem;
using ImageOrganizer.Data.Container;
using ImageOrganizer.Resources;
using Tauron;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.SwitchContainer)]
    public class SwitchContainerRule : IOBusinessRuleBase<SwitchContainerInput, SwitchContainerOutput>
    {
        [Inject]
        public ISettings Settings { get; set; }

        private string CurrentDatabase => Properties.Settings.Default.CurrentDatabase;
        private ContainerType CurrentContainerType => FileContainerManager.CurrentContainerType;

        public override SwitchContainerOutput ActionImpl(SwitchContainerInput input)
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
                                return BuildOutput(UIResources.SwitchContainnerRule_LowSpace);
                            if (IsSame(input))
                                input.ClearCustom();
                            trans = Switch(input);
                            //FileContainerManager.Import(old, trans, input.OnMessage);
                            Directory.Delete(!string.IsNullOrWhiteSpace(Settings.CustomMultiPath) ? Settings.CustomMultiPath 
                                : Properties.Settings.Default.CurrentDatabase.Replace('.', '-'), true, true);
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
                            return BuildOutput(UIResources.SwitchContainnerRule_LowSpace);
                        if (!CurrentDatabase.ExisDirectory())
                            return BuildOutput(UIResources.SwitchContainnerRule_DicNotExis);

                        var result = Directory.MoveTransacted(trans.TryCast<KernelTransaction>(), CurrentDatabase, tempLoc);
                        if (result.ErrorCode != 0)
                            return BuildOutput(result.ErrorMessage);

                        Switch(input, false);
                        old = Factory.Begin().UseMulti().Initialize(tempLoc);
                        FileContainerManager.Import(old, trans, input.OnMessage);

                        Directory.DeleteTransacted(trans.TryCast<KernelTransaction>(), tempLoc, true, true);
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
                                    File.DeleteTransacted(trans.TryCast<KernelTransaction>(), containerName, true);
                            }
                            else
                            {
                                if(Directory.Exists(containerName))
                                    Directory.DeleteTransacted(trans.TryCast<KernelTransaction>(), containerName, true, true);
                            }
                        }

                        trans.Commit();
                        return BuildOutput();
                    }
                    default:
                        return BuildOutput(UIResources.SwitchContainnerRule_UnknowenConfig);
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

        private bool CheckSize(string input)
        {
            DriveInfo rootNew = new DriveInfo(Path.GetPathRoot(string.IsNullOrEmpty(input)
                ? CurrentDatabase
                : input));
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
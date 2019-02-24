using System.Collections.Generic;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.ContainerManager;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;
using CT = Tauron.Application.ImageOrganizer.BL.ContainerType;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.ContainerManager)]
    public class ContainerManagerViewModel : MainViewControllerBase
    {
        private string _customMultiPath;
        private ContainerTypeUi _containerType;

        public override string ProgrammTitle { get; } = UIResources.ContainerManager_ProgrammTitle;
        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }
        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;
        public override void OnClick() => MainWindowViewModel.ShowImagesAction();

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManager { get; set; }

        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        [Inject]
        public IContainerService Operator { get; set; }

        [Inject]
        public IDownloadService DownloadService { get; set; }

        [Inject]
        public IDBSettings Settings { get; set; }

        public ContainerTypeUi ContainerType
        {
            get => _containerType;
            set => SetProperty(ref _containerType, value);
        }

        public IEnumerable<ContainerTypeUi> ContainerTypeUis { get; private set; }

        public string CustomMultiPath
        {
            get => _customMultiPath;
            set => SetProperty(ref _customMultiPath, value);
        }

        public UIObservableCollection<UiBase> OperationResults { get; } = new UIObservableCollection<UiBase>();

        public override void EnterView()
        {
            List<ContainerTypeUi> uis = new List<ContainerTypeUi>
            {
                new ContainerTypeUi(CT.Single, UIResources.ContainerManager_ContainerType_Single),
                new ContainerTypeUi(CT.Multi, UIResources.ContainerManager_ContainerType_Multi),
                new ContainerTypeUi(CT.Compose, UIResources.ContainerManager_ContainerType_Compose)
            };
            ContainerTypeUis = uis;

            ContainerType = uis.Find(ct => ct.Type == Settings.ContainerType);
            CustomMultiPath = Settings.CustomMultiPath;
            base.EnterView();
        }

        public override void ExitView()
        {
            OperationResults.Clear();
            base.ExitView();
        }

        [CommandTarget]
        public void SearchPath()
        {
            string path = Dialogs.ShowOpenFolderDialog(MainWindow, UIResources.ContainerManager_CustomPathSearch_Description, SettingsManager.Settings?.CurrentDatabase, true, false,
                out var result);

            if (result == true)
                CustomMultiPath = path;
        }

        [CommandTarget]
        public void Sync()
        {
            var op = OperationManager.EnterOperation(true);
            OperationResults.Clear();

            Task.Run(() => Operator.Defrag(new DefragInput(e => OperationResults.Add(new SyncError(e.Name, e.Error, DownloadService)), 
                    s => OperationManager.PostMessage(s, 0, 0))))
                .ContinueWith(t => op.Dispose());
        }

        [CommandTarget]
        public void Recuvery()
        {
            using (OperationManager.EnterOperation(true))
            {
                OperationManager.PostMessage(UIResources.ContianerManager_RecuveryProgress_Start, 0, 0);

                string path = Dialogs.ShowOpenFolderDialog(MainWindow, UIResources.ContainerManager_OpenFolder_Recuvery, SettingsManager.Settings?.CurrentDatabase, true, false,
                    out var isOk);

                if(isOk == null || isOk == false) return;

                Operator.Recuvery(new RecuveryInput(path, message =>
                {
                    OperationManager.PostMessage(UIResources.ContainerManager_RecuveryProgress_Run.SFormat(message.Current), 0, 0);
                    OperationResults.Add(new RecuveryResult(message));
                }));
            }
        }

        [CommandTarget]
        public void Apply()
        {
            var op = OperationManager.EnterOperation(true, true);
            OperationResults.Clear();
            Task.Run(() => Operator.SwitchContainer(new SwitchContainerInput(CustomMultiPath, ContainerType.Type))).ContinueWith(t =>
            {
                try
                {
                    var result = t.Result;
                    if(result.Sucssed && !result.NeedSync) return; 
                    if(result.Sucssed && result.NeedSync)
                        OperationResults.Add(new ApplyError(UIResources.ContainerManager_NeedSync_Message));
                    else
                        OperationResults.Add(new ApplyError(result.Error));
                }
                finally
                {
                    op.Dispose();
                }
            });
        }
    }
}
using System.Collections.Generic;
using ImageOrganizer.BL;
using ImageOrganizer.BL.Operations;
using ImageOrganizer.Resources;
using ImageOrganizer.Views.ContainerManager;
using ImageOrganizer.Views.Models;
using Tauron;
using Tauron.Application;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace ImageOrganizer.Views
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
        public Operator Operator { get; set; }

        [Inject]
        public ISettings Settings { get; set; }

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
                new ContainerTypeUi(BL.ContainerType.Single, UIResources.ContainerManager_ContainerType_Single),
                new ContainerTypeUi(BL.ContainerType.Multi, UIResources.ContainerManager_ContainerType_Multi),
                new ContainerTypeUi(BL.ContainerType.Compose, UIResources.ContainerManager_ContainerType_Compose)
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
            string path = Dialogs.ShowOpenFolderDialog(MainWindow, UIResources.ContainerManager_CustomPathSearch_Description, Properties.Settings.Default.CurrentDatabase, true, false,
                out var result);

            if (result == true)
                CustomMultiPath = path;
        }

        [CommandTarget]
        public void Sync()
        {
            var op = OperationManager.EnterOperation(true);
            OperationResults.Clear();

            Operator.Defrag(new DefragInput(e => OperationResults.Add(new SyncError(e.Name, e.Error, Operator)), s => OperationManager.PostMessage(s, 0, 0)))
                .ContinueWith(t => op.Dispose());
        }

        [CommandTarget]
        public void Recuvery()
        {
            using (OperationManager.EnterOperation(true))
            {
                OperationManager.PostMessage(UIResources.ContianerManager_RecuveryProgress_Start, 0, 0);

                string path = Dialogs.ShowOpenFolderDialog(MainWindow, UIResources.ContainerManager_OpenFolder_Recuvery, Properties.Settings.Default.CurrentDatabase, true, false,
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
            Operator.SwitchContainer(new SwitchContainerInput(CustomMultiPath, ContainerType.Type)).ContinueWith(t =>
            {
                var result = t.Result;
                if(result.Sucssed && !result.NeedSync) return; 
                if(result.Sucssed && result.NeedSync)
                    OperationResults.Add(new ApplyError(UIResources.ContainerManager_NeedSync_Message));
                else
                    OperationResults.Add(new ApplyError(result.Error));

                op.Dispose();
            });
        }
    }
}
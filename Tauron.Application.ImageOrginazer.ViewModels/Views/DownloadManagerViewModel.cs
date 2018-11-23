using System;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.DownloadManager)]
    public class DownloadManagerViewModel : MainViewControllerBase
    {
        private string _pauseLabel;

        public override string ProgrammTitle { get; } = UIResources.DonwloadManager_ProgrammTitle;

        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }

        public event Func<string> GetPersistentStade;
        public string PersistentStade => Settings.DownloadManagerGridStade;

        [Inject]
        public IOperator Operator { get; set; }

        [Inject]
        public IDBSettings Settings { get; set; }

        [InjectModel(AppConststands.DownloadManagerModel)]
        public DownloadManagerModel ManagerModel { get; set; }
        
        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;

        public DownloadManagerViewModel() => PauseLabel = UIResources.DownloadManager_ButtonLabel_Pause;

        public override void OnClick() => MainWindowViewModel.ShowImagesAction();

        public override void EnterView()
        {
            ManagerModel.Attach();
            ManagerModel.FetchDataAsync();

            PauseLabel = ManagerModel.DownloadManager.IsPaused ? UIResources.DownloadManager_ButtonLabel_Resume : UIResources.DownloadManager_ButtonLabel_Pause;
        }

        public override void ExitView()
        {
            ManagerModel.DeAttach();
            Settings.DownloadManagerGridStade = GetPersistentStade?.Invoke();
        }

        public string PauseLabel
        {
            get => _pauseLabel;
            set => SetProperty(ref _pauseLabel, value);
        }

        [CommandTarget]
        public void SwitchPause()
        {
            var man = ManagerModel.DownloadManager;
            if (man.IsPaused)
            {
                PauseLabel = UIResources.DownloadManager_ButtonLabel_Pause;
                man.Start();
            }
            else
            {
                PauseLabel = UIResources.DownloadManager_ButtonLabel_Resume;
                man.Pause();
            }
        }

        [CommandTarget]
        public void StartImageDownload() => Operator.StartDownloads();
    }
}
﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;
using Tauron.Application.ImageOrganizer.Data.Entities;
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
        public IDownloadService Operator { get; set; }

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

            PauseLabel = ManagerModel.DownloadManager.IsPaused ? UIResources.DownloadManager_ButtonLabel_Resume : UIResources.DownloadManager_ButtonLabel_Pause;
        }

        public override void ExitView()
        {
            Task.Run(() =>
            {
                ManagerModel.DeAttach();
                QueueWorkitem(() => Settings.DownloadManagerGridStade = GetPersistentStade?.Invoke());
            });
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
        public void StartImageDownload()
        {
            Log.Info("Start Download Images");

            Task.Run(() => Operator.StartDownloads()).ContinueWith(t =>
            {
                foreach (var downloadItem in ManagerModel.DownloadItems.Where(di => di.DownloadStade == DownloadStade.Paused))
                    downloadItem.DownloadStade = DownloadStade.Queued;
            });
        }
    }
}
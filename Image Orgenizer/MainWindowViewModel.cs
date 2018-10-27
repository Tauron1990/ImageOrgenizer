using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using ImageOrganizer.BL;
using ImageOrganizer.BL.Operations;
using ImageOrganizer.Resources;
using ImageOrganizer.Views;
using ImageOrganizer.Views.Models;
using Tauron.Application;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace ImageOrganizer
{
    [ExportViewModel(AppConststands.MainWindowName), Shared]
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _editMode;

        private IMainViewController _mainView; //= new PlaceHolder();
        private string _currentDatabase;

        public MainWindowViewModel()
        {
            ShowImagesAction = ShowImages;
            ShowDownloadManagerAction = ShowDownloadManager;
            ShowImagesRefreshAction = () => RefreshAll();
        }

        public static Action ShowImagesAction { get; private set; }
        public static Action ShowImagesRefreshAction { get; private set; }
        public static Action ShowDownloadManagerAction { get; private set; }

        public IMainViewController MainView
        {
            get => _mainView;
            set
            {
                _mainView = value;
                OnPropertyChanged();
            }
        }

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManagerModel { get; set; }

        [InjectModel(AppConststands.DownloadManagerModel)]
        public DownloadManagerModel ManagerModel { get; set; }

        [InjectModel(AppConststands.ImageManagerModel)]
        public ImageViewerModel ViewerModel { get; set; }

        [Inject]
        public Operator Operator { get; set; }

        [Inject]
        public ISettings Settings { get; set; }

        public List<PossiblePager> Pagers { get; set; }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public string CurrentDatabase
        {
            get => _currentDatabase;
            set => SetProperty(ref _currentDatabase, value);
        }

        [CommandTarget]
        public void ControlClick() => MainView?.OnClick();
  
        [CommandTarget]
        public void Next() => MainView?.Next();

        [CommandTarget]
        public void Back() => MainView?.Back();

        [CommandTarget]
        public void FullScreen() => ViewManager.CreateWindow(AppConststands.ImageFullScreen).ShowDialogAsync(MainWindow);

        [CommandTarget]
        public void Open()
        {
            var path = Dialogs.ShowOpenFileDialog(MainWindow, false, "*.imgdb", true, "Database|*.imgdb", false, UIResources.Dialog_Title_Open, true, true, out var dialogOk)
                .FirstOrDefault();

            if (!dialogOk.HasValue || !dialogOk.Value || string.IsNullOrWhiteSpace(path)) return;

            RefreshAll(path);
        }

        [CommandTarget]
        public void ShowImages()
        {
            if(CanShowImages())
                SwitchView(AppConststands.ImageViewer);
        }

        [CommandTarget]
        public bool CanShowImages() => !(MainView is ImageViewerViewModel);

        [CommandTarget]
        public void ShowFileImport() => SwitchView(AppConststands.FileImporter);

        [CommandTarget]
        public bool CanShowFileImport() => !(MainView is FileImporterViewModel);

        [CommandTarget]
        public void ShowDownloadManager()
        {
            if(CanShowDownloadManager())
                SwitchView(AppConststands.DownloadManager);
        }

        [CommandTarget]
        public bool CanShowDownloadManager() => !(MainView is DownloadManagerViewModel);

        [CommandTarget]
        public void ShowImageEditor() => SwitchView(AppConststands.ImageEditorName);

        [CommandTarget]
        public bool CanShowImageEditor() => !(MainView is ImageEditorViewModel);

        [CommandTarget]
        public void ShowContainerManager() => SwitchView(AppConststands.ContainerManager);

        [CommandTarget]
        public bool CanShowContainerManager() => !(MainView is ContainerManagerViewModel);

        [CommandTarget]
        public void PauseOp() => OperationManagerModel.Pause();

        [CommandTarget]
        public void StopOp() => OperationManagerModel.Stop();

        [CommandTarget]
        public void Redownload()
        {
            string name = MainView.GetCurrentImageName();
            if(name.IsNullOrEmpty()) return;

            Operator.ScheduleRedownload(name);
        }

        [CommandTarget]
        public void DeleteImage() => Operator.DeleteImage(MainView.GetCurrentImageName());

        [EventTarget]
        public void EditModeStart()
        {
            MainView?.RefreshNavigatorText();
            EditMode = true;
        }

        public void EditModeStop()
        {
            using (OperationManagerModel.EnterOperation())
            {
                MainView?.RefreshNavigatorItems();
                EditMode = false;
            }
        }

        [EventTarget]
        public void RefreshLoaded()
        {
            using (OperationManagerModel.EnterOperation())
            {
                ManagerModel.StartClipBoardListening();
                RefreshAll();
            }
        }

        [EventTarget]
        public void Closing()
        {
            MainView.Closing();
            QueueWorkitem(CommonApplication.Current.Shutdown, false);
        }

        [CommandTarget]
        public void FavoriteClick()
        {
            ViewerModel.CurrentImage.Favorite = !ViewerModel.CurrentImage.Favorite;
            Operator.MarkFavorite(ViewerModel.CurrentImage);
        }

        [CommandTarget]
        public bool CanFavoriteClick() => ViewerModel.CurrentImage != null;

        [CommandTarget]
        public void ShowProfileManager() => SwitchView(AppConststands.ProfileManagerName);

        [CommandTarget]
        public bool CanShowProfileManager() => !(MainView is ProfileManagerViewModel);

        private void RefreshAll(string db = null)
        {
            using (OperationManagerModel.OperationRunning ? new OperationManagerModel.NullDispose() : OperationManagerModel.EnterOperation())
            {
                if (CanShowImages())
                    SwitchView(AppConststands.ImageViewer);

                CurrentDatabase = db ?? Properties.Settings.Default.CurrentDatabase;
                Operator.UpdateDatabase(CurrentDatabase);

                var lastProfile = Settings.LastProfile ?? string.Empty;
                MainView.RefreshAll(Settings.ProfileDatas.TryGetValue(lastProfile, out var data) ? data : null, lastProfile);
            }

        }

        private void SwitchView(string name)
        {
            var controller = MainView;
            MainView = null;
            controller?.ExitView();

            controller = (IMainViewController) ResolveViewModel(name);
            controller.EnterView();
            MainView = controller;
        }

        public override void BuildCompled() => Pagers = ViewerModel.ImagePagers.ToList();
    }
}
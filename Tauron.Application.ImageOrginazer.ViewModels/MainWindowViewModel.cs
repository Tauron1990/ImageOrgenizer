using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels
{
    [ExportViewModel(AppConststands.MainWindowName), Shared]
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _editMode;

        private IMainViewController _mainView; //= new PlaceHolder();
        private string _currentDatabase;
        private string _searchText;
        private bool _isDatabaseValid;

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

        [InjectModel(AppConststands.LockScreenModel)]
        public LockScreenManagerModel LockScreenManagerModel { get; set; }

        [InjectModel(AppConststands.ProfileManagerModelName)]
        public ProfileManager ProfileManager { get; set; }

        [Inject]
        public IOperator Operator { get; set; }

        [Inject]
        public IDBSettings Settings { get; set; }

        [Inject]
        public Lazy<IShutdownWindowShower> ShutdownWindow { get; set; }

        [Inject]
        public ISettingsManager SettingsManager { get; set; }

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

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value, InvalidateRequerySuggested);
        }

        public bool IsDatabaseValid
        {
            get => _isDatabaseValid;
            set => SetProperty(ref _isDatabaseValid, value);
        }

        [CommandTarget]
        public void ControlClick() => MainView?.OnClick();

        [CommandTarget]
        public bool CanControlClick() => IsDatabaseValid;

        [CommandTarget]
        public void Next() => MainView?.Next();

        [CommandTarget]
        public void Back() => MainView?.Back();

        [CommandTarget]
        public bool CanNext() => MainView?.CanNext() ?? false;

        [CommandTarget]
        public bool CanBack() => MainView?.CanBack() ?? false;

        [CommandTarget]
        public void FullScreen()
        {
            LockScreenManagerModel.Stop();
            ViewManager.CreateWindow(AppConststands.ImageFullScreen)
                .ShowDialogAsync(MainWindow)
                .ContinueWith(t => LockScreenManagerModel.OnLockscreenReset());
        }

        [CommandTarget]
        public void Open()
        {
            var path = Dialogs.ShowOpenFileDialog(MainWindow, false, "*.imgdb", true, "Database|*.imgdb", false, UIResources.Dialog_Title_Open, true, true, out var dialogOk)
                .FirstOrDefault();

            if (!dialogOk.HasValue || !dialogOk.Value || string.IsNullOrWhiteSpace(path)) return;

            RefreshAll(path);
        }

        [CommandTarget]
        public void Close()
        {
            if (SettingsManager.Settings == null) return;

            SettingsManager.Settings.CurrentDatabase = string.Empty;
            SettingsManager.Save();

            RefreshAll();
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
            ViewerModel.OpenUrl();

            //string name = MainView.GetCurrentImageName();
            //if (string.IsNullOrEmpty(name)) return;

            //Operator.ScheduleRedownload(name);
        }

        [CommandTarget]
        public void ReplaceImage()
        {
            try
            {
                var path = Dialogs.ShowOpenFileDialog(MainWindow, true, "", true, "*.*", false, "",
                    true, true, out var ok).Single();
                if(ok != true) return;

                if(Operator.ReplaceImage(new ReplaceImageInput(File.ReadAllBytes(path), ViewerModel.CurrentImage.Name))) return;
                Dialogs.ShowMessageBox(MainWindow, UIResources.MainWWindowViewModel_Replace_Failed, "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
            }
            catch
            {
                Dialogs.ShowMessageBox(MainWindow, UIResources.MainWWindowViewModel_Replace_Failed, "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
            }
        }

        [CommandTarget]
        public void DeleteImage()
        {
            MainView.PrepareDeleteImage();
            Operator.DeleteImage(MainView.GetCurrentImageName());
        }

        [CommandTarget]
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
            //using (OperationManagerModel.EnterOperation())
            //{
            //    ManagerModel.StartClipBoardListening();
            //    RefreshAll();
            //}
        }

        [EventTarget]
        public void Closing()
        {
            SystemDispatcher.Invoke(() => ShutdownWindow.Value.Show());
            ManagerModel.Shutdown();
            MainView.Closing();
            QueueWorkitem(CommonApplication.Current.Shutdown);
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
        public void ShowProfileManager() => SwitchView(AppConststands.ProfileManagerViewModelName);

        [CommandTarget]
        public bool CanShowProfileManager() => !(MainView is ProfileManagerViewModel);

        [CommandTarget]
        public void CreateProfile()
        {
            string name = Dialogs.GetText(MainWindow, UIResources.ProfileManager_CreateLable_Instruction, null, UIResources.ProfileManager_CreateLabel_Caption, true, null);
            if (string.IsNullOrWhiteSpace(name))
            {
                TrySetError(UIResources.MainWindow_ProfileCreationError_InvalidName);
                return;
            }

            if (Settings.ProfileDatas.ContainsKey(name))
            {
                TrySetError(UIResources.ProfileManager_Create_Duplicate);
                return;
            }

            var data = ViewerModel.CreateProfileData();
            Settings.ProfileDatas.Add(name, data);
            TrySetError(null);
        }

        [CommandTarget]
        public bool CanCreateProfile() => MainView?.CanCreateProfile() ?? false;

        [CommandTarget]
        public void Search()
        {
            using (OperationManagerModel.EnterOperation())
            {
                if (CanShowImages())
                    SwitchView(AppConststands.ImageViewer);

                var result = Operator.SearchLocation(SearchText);
                if (result == null)
                {
                    TrySetError(UIResources.ImageViewer_Error_NoData);
                    return;
                }

                var lastProfile = Settings.LastProfile ?? string.Empty;
                MainView.RefreshAll(result, lastProfile, IsDatabaseValid);
            }
        }

        [CommandTarget]
        public bool CanSearch() => !string.IsNullOrWhiteSpace(SearchText);

        private void TrySetError(string error) => (MainView as ImageViewerViewModel)?.SetError(error);

        public void RefreshAll(string db = null)
        {
            using (OperationManagerModel.OperationRunning ? new OperationManagerModel.NullDispose() : OperationManagerModel.EnterOperation())
            {
                if (CanShowImages())
                    SwitchView(AppConststands.ImageViewer);

                if (db == null)
                    db = SettingsManager.Settings?.CurrentDatabase;

                if (CurrentDatabase != db)
                {
                    CurrentDatabase = db;
                    IsDatabaseValid = Operator.UpdateDatabase(CurrentDatabase);
                }

                var lastProfile = Settings.LastProfile ?? string.Empty;
                MainView.RefreshAll(Settings.ProfileDatas.TryGetValue(lastProfile, out var data) ? data : null, lastProfile, IsDatabaseValid);
            }
        }

        private void SwitchView(string name)
        {
            using (OperationManagerModel.EnterOperation())
            {
                var controller = MainView;
                MainView = null;
                controller?.ExitView();

                controller = (IMainViewController) ResolveViewModel(name);
                controller.EnterView();
                MainView = controller;
            }
        }

        public override void BuildCompled() 
            => Pagers = ViewerModel.ImagePagers.ToList();
    }
}
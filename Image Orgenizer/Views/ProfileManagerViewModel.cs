using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageOrganizer.BL;
using ImageOrganizer.BL.Operations;
using ImageOrganizer.Resources;
using ImageOrganizer.Views.Controls;
using ImageOrganizer.Views.Models;
using Tauron.Application;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace ImageOrganizer.Views
{
    [ExportViewModel(AppConststands.ProfileManagerName)]
    public class ProfileManagerViewModel : MainViewControllerBase
    {
        private ProfileDataUi _selectedProfile;
        private string _sqlText;

        [InjectModel(AppConststands.ImageManagerModel)]
        public ImageViewerModel ViewerModel { get; set; }

        [Inject]
        public OperationManagerModel OperationManager { get; set; }

        [Inject]
        public ISettings Settings { get; set; }

        [Inject]
        public Operator Operator { get; set; }
        
        public CustomQueryViewModel QueryViewModel { get; set; }

        public override string ProgrammTitle { get; } = UIResources.ProfileManager_ProgrammTitle;
        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }

        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;

        public UIObservableCollection<ProfileDataUi> Profiles { get; } = new UIObservableCollection<ProfileDataUi>();

        public IEnumerable<PossiblePager> Pagers { get; private set; }

        public ProfileDataUi SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value, () => QueryViewModel.SelectedProfile = value);
        }

        public override void OnClick() => MainWindowViewModel.ShowImagesAction();

        public override void EnterView()
        {
            Pagers = ViewerModel.ImagePagers;
            QueryViewModel.SqlText = UIResources.ProfileManager_DefaultQuery;
            string currentProfile = Settings.LastProfile;
            Profiles.AddRange(Settings.ProfileDatas.Select(d => new ProfileDataUi(d.Value, d.Key, Pagers)));
            SelectedProfile = Profiles.FirstOrDefault(pd => pd.Name == currentProfile);
        }

        public override void ExitView()
        {
            Pagers = null;

            foreach (var dataUi in Profiles.Where(p => p.IsEdited))
                Settings.ProfileDatas[dataUi.Name] = dataUi.CreateNew();
            Profiles.Clear();
        }

        [CommandTarget]
        public void CreateProfile() => CreateProfile(ViewerModel.CreateProfileData);

        [CommandTarget]
        public void SwitchProfile()
        {
            Settings.LastProfile = SelectedProfile.Name;
            MainWindowViewModel.ShowImagesRefreshAction();
        }

        [CommandTarget]
        public bool CanSwitchProfile() => SelectedProfile != null;

        private void ShowPreview(ProfileData data)
        {
            using (OperationManager.EnterOperation())
                ShowPreview(ViewerModel.GetImageData(data));
        }
        private Task ShowPreview(ImageData data) => ViewManager.CreateWindow(AppConststands.PreviewWindowName, data).ShowDialogAsync(MainWindow);

        private bool CreateProfile(Func<ProfileData> dataGetter)
        {
            string name = Dialogs.GetText(MainWindow, UIResources.ProfileManager_CreateLable_Instruction, null, UIResources.ProfileManager_CreateLabel_Caption, true, null);
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (Settings.ProfileDatas.ContainsKey(name))
            {
                Dialogs.ShowMessageBox(MainWindow, UIResources.ProfileManager_Create_Duplicate, ProgrammTitle, MsgBoxButton.Ok, MsgBoxImage.Warning, null);
                return false;
            }

            var data = dataGetter();
            var uiProfile = new ProfileDataUi(data, name, Pagers) { IsEdited = true };

            Profiles.Add(uiProfile);
            SelectedProfile = uiProfile;

            return true;
        }

        public override void BuildCompled()
        {
            QueryViewModel = (CustomQueryViewModel) ResolveViewModel(AppConststands.CustomQueryControl);
            QueryViewModel.ProfileDataCreated += p => CreateProfile(() => p);
        }
    }
}
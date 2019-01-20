using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Controls;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.ProfileManagerViewModelName)]
    public class ProfileManagerViewModel : MainViewControllerBase
    {
        private ProfileDataUi _selectedProfile;
        private string _sqlText;

        [InjectModel(AppConststands.ImageManagerModel)]
        public ImageViewerModel ViewerModel { get; set; }

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManager { get; set; }

        [Inject]
        public IDBSettings Settings { get; set; }

        [Inject]
        public IOperator Operator { get; set; }

        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        [InjectModel(AppConststands.ProfileManagerModelName)]
        public ProfileManager ProfileManager { get; set; }

        public CustomQueryViewModel QueryViewModel { get; set; }

        public override string ProgrammTitle { get; } = UIResources.ProfileManager_ProgrammTitle;
        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }

        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;

        public UIObservableCollection<ProfileDataUi> Profiles { get; } = new UIObservableCollection<ProfileDataUi>();

        public IEnumerable<PossiblePager> Pagers { get; private set; }

        private ImageData _imageData;
        public ProfileDataUi SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value, () => _imageData = null);
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
        public void CreateProfile() => CreateProfile(() => ViewerModel.CreateProfileData());

        [CommandTarget]
        public void SwitchProfile() => ProfileManager.SwitchProfile(SelectedProfile.Name);

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
                Dialogs.ShowMessageBox(MainWindow, UIResources.ProfileManager_Create_Duplicate, ProgrammTitle, MsgBoxButton.Ok, MsgBoxImage.Warning);
                return false;
            }

            var data = dataGetter();
            var uiProfile = new ProfileDataUi(data, name, Pagers) { IsEdited = true };

            Profiles.Add(uiProfile);
            SelectedProfile = uiProfile;

            return true;
        }

        [EventTarget(Converter = typeof(ControlConverter))]
        private void RecordDeleted(IDataGrid sender, IRecordDeletedArgs e)
        {
            List<string> toDelete = new List<string>();

            foreach (var eItem in e.Items)
            {
                if(eItem is ProfileDataUi data)
                    toDelete.Add(data.Name);
            }

            Task.Run(() =>
            {
                foreach (var del in toDelete)
                    Settings.ProfileDatas.Remove(del);
            });
        }

        public override void BuildCompled()
        {
            QueryViewModel = (CustomQueryViewModel) ResolveViewModel(AppConststands.CustomQueryControl);
            QueryViewModel.ValidateResult += p => CreateProfile(() => CreateData(p));
            QueryViewModel.GetImageData = () =>
            {
                if (SelectedProfile == null) return null;

                using (OperationManager.EnterOperation())
                    return ViewerModel.GetImageData(SelectedProfile.ProfileData);
            };
            QueryViewModel.Update = data => SelectedProfile?.Update(CreateData(data), ViewerModel.ImagePagers);
            QueryViewModel.CanGeneratePreviewFunc = () => SelectedProfile != null;
        }

        private ProfileData CreateData(RawSqlResult result) =>
            new ProfileData(result.Position + SettingsManager.Settings?.PageCount ?? 20, 0, string.Empty, result.Position, ImageViewerModel.OrderedPager, false);
    }
}
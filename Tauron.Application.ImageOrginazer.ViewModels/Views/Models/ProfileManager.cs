using System.Linq;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [ExportModel(AppConststands.ProfileManagerModelName)]
    public class ProfileManager : ModelBase
    {
        private bool _supress;
        private ProfileManagerElement _activeProfile;

        [Inject]
        public IDBSettings DbSettings { get; set; }

        public UIObservableCollection<ProfileManagerElement> Profiles { get; } = new UIObservableCollection<ProfileManagerElement>();

        public ProfileManagerElement ActiveProfile
        {
            get => _activeProfile;
            set => SetProperty(ref _activeProfile, value);
        }

        public void SwitchProfile(string name)
        {
            if(DbSettings.LastProfile == name) return;

            var ele = Profiles.FirstOrDefault(e => e.Name == name);

            if (ele != null)
                ele.Active = true;
        }

        public override void BuildCompled()
        {
            DbSettings.ProfileChanged += DbSettingsOnProfileChanged;
            DbSettings.Initilized += DbSettingsOnInitilized;
            DbSettingsOnInitilized();
        }

        private void DbSettingsOnInitilized()
        {
            Profiles.Clear();
            Profiles.AddRange(DbSettings.ProfileDatas.Select(sel =>
            {
                var ne = new ProfileManagerElement {Name = sel.Key, Active = sel.Key == DbSettings.LastProfile, Data = sel.Value};
                ne.Toggle += ElementToggle;
                return ne;
            }));
            ActiveProfile = Profiles.SingleOrDefault(e => e.Active);
        }

        private void DbSettingsOnProfileChanged(string name, ProfileData data, bool remove)
        {
            if (remove)
            {
                Profiles.Remove(Profiles.FirstOrDefault(p => p.Name == name));
                if (ActiveProfile.Name != name) return;

                var f = Profiles.FirstOrDefault();
                if (f != null)
                    f.Active = true;

                return;
            }

            var ele = Profiles.FirstOrDefault(e => e.Name == name);
            if (ele == null)
            {
                ele = new ProfileManagerElement { Name = name };
                ele.Toggle += ElementToggle;
                Profiles.Add(ele);
            }

            ele.Data = data;
        }

        private void ElementToggle(ProfileManagerElement ele)
        {
            Log.Info($"Activation Profile: {ele.Name}");

            if(_supress) return;
            _supress = true;

            try
            {
                if (Profiles.All(e => !e.Active))
                {
                    ele.Active = true;
                    return;
                }

                ActiveProfile.Active = false;
                ActiveProfile = ele;

                Task.Run(() =>
                {
                    DbSettings.LastProfile = ele.Name;
                    MainWindowViewModel.ShowImagesRefreshAction();
                });
            }
            finally
            {
                _supress = false;
            }
        }
    }
}
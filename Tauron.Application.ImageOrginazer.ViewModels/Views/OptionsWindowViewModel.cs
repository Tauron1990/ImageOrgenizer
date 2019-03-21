using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    public class OptionsFetcherUI
    {
        private readonly string _id;
        private readonly IDBSettings _dbSettings;

        public string DisplayName { get; }

        public string Value
        {
            get => _dbSettings.FetcherData[_id];
            set => _dbSettings.FetcherData[_id] = value;
        }

        public OptionsFetcherUI(string displayName, string id, IDBSettings dbSettings)
        {
            DisplayName = displayName;
            _id = id;
            _dbSettings = dbSettings;
        }
    }

    [ExportViewModel(AppConststands.OptionsWindowName)]
    public class OptionsWindowViewModel : ViewModelBase
    {
        private bool _needRewfresh;

        [Inject]
        public IDBSettings DBSettings { get; set; }

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        public string ContainerTypeDisplay { get; set; }

        public List<string> Profiles { get; } = new List<string>();

        public List<OptionsFetcherUI> FetcherUis { get; } = new List<OptionsFetcherUI>();

        public List<ProfileDataUi> ProfileDatas { get; } = new List<ProfileDataUi>();

        public string SelectProfile
        {
            get => GetProperty<string>();
            set => SetProperty(value, () =>
            {
                DBSettings.LastProfile = value;
                _needRewfresh = true;
            });
        }

        public int MaxPageCount
        {
            get => GetProperty<int>();
            set => SetProperty(value, () =>
            {
                DBSettings.MaxOnlineViewerPage = value;
            });
        }

        public override void BuildCompled()
        {
            switch (DBSettings.ContainerType)
            {
                case ContainerType.Compose:
                    ContainerTypeDisplay = UIResources.ContainerManager_ContainerType_Compose;
                    break;
                case ContainerType.Single:
                    ContainerTypeDisplay = UIResources.ContainerManager_ContainerType_Single;
                    break;
                case ContainerType.Multi:
                    ContainerTypeDisplay = UIResources.ContainerManager_ContainerType_Compose;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Profiles.AddRange(DBSettings.ProfileDatas.Keys);
            SelectProfile = DBSettings.LastProfile;
            MaxPageCount = DBSettings.MaxOnlineViewerPage;

            FetcherUis.AddRange(ProviderManager.GetFetchers().Select(f => new OptionsFetcherUI(f.Name, f.Id, DBSettings)));
            ProfileDatas.AddRange(DBSettings.ProfileDatas.Select(p => new ProfileDataUi(p.Value, p.Key, null)));

            base.BuildCompled();
        }

        public override void AfterShow(IWindow window) 
            => window.Closed += (sender, args) =>
            {
                if(_needRewfresh)
                    Task.Run(MainWindowViewModel.ShowImagesRefreshAction);
            };
    }
}
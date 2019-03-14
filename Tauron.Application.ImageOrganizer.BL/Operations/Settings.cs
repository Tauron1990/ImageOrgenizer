using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;
using Tauron.Application.ImageOrganizer.BL.Core;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [Export(typeof(IDBSettings))]
    public class IdbSettings : ObservableObject, IDBSettings, INotifyBuildCompled
    {
        private const string FetcherPrfix = "Fetcher_";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private DatabaseDictionary<string, string> _options;
        public static Action InitAction { get; private set; }

        [Inject]
        public RepositoryFactory RepositoryFactory { get; set; }

        //[Inject]
        //public Lazy<IOperator> Operator { get; set; }

        public void BuildCompled()
        {
            InitAction = Init;
            Init();
        }

        public event Action<string, ProfileData, bool> ProfileChanged;
        public event Action Initilized;

        public IDictionary<string, ProfileData> ProfileDatas { get; private set; } = new Dictionary<string, ProfileData>();

        public IDictionary<string, string> FetcherData { get; private set; } = new Dictionary<string, string>();

        public string LastProfile
        {
            get => GetValue(nameof(LastProfile), string.Empty);
            set => SetValue(nameof(LastProfile), value);
        }

        public string DownloadManagerGridStade
        {
            get => GetValue(nameof(DownloadManagerGridStade), string.Empty);
            set => SetValue(nameof(DownloadManagerGridStade), value); }

        public ContainerType ContainerType
        {
            get => (ContainerType) GetValue(nameof(ContainerType), (int)ContainerType.Single);
            set => SetValue(nameof(ContainerType), (int) value);
        }

        public string CustomMultiPath
        {
            get => GetValue(nameof(CustomMultiPath), string.Empty);
            set => SetValue(nameof(CustomMultiPath), value);
        }

        public void Initialize() => InitAction();

        private void Init()
        {
            try
            {
                using (RepositoryFactory.Enter())
                {
                    var profileRepository = RepositoryFactory.GetRepository<IProfileRepository>();
                    var optionsRepository = RepositoryFactory.GetRepository<IOptionRepository>();

                    ProfileDatas = new DatabaseDictionary<string, ProfileData>(ProfileChangeAction,
                        profileRepository.GetProfileData()
                            .ToDictionary(ek => ek.Name, entity => new ProfileData(entity)));

                    _options = new DatabaseDictionary<string, string>(OptionsChanged, optionsRepository.GetAllValues().ToDictionary(ek => ek.Name, ev => ev.Value));
                    FetcherData = new DatabaseDictionary<string, string>(
                        FetcherChanged, 
                        _options
                            .Where(p => p.Key.StartsWith(FetcherPrfix))
                            .Select(p => (p.Key.Replace(FetcherPrfix, string.Empty), p.Value))
                            .ToDictionary(ek => ek.Item1, ev => ev.Item2));
                }
            }
            catch (Exception e)
            {
                if (e.IsCriticalApplicationException())
                    throw;

                Logger.Warn(e, "Ignored Exception -- Settings.Init");
            }

            try
            {
                Initilized?.Invoke();
            }
            catch (Exception e)
            {
                if (e.IsCriticalApplicationException())
                    throw;

                Logger.Warn(e, "Ignored Exception -- Settings.Init -- Initilized");
            }
        }

        private void FetcherChanged(string key, string value, DatabaseAction databaseAction)
        {
            switch (databaseAction)
            {
                case DatabaseAction.Add:
                case DatabaseAction.Update:
                    _options[FetcherPrfix + key] = value;
                    break;
                case DatabaseAction.Remove:
                    _options.Remove(key);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseAction), databaseAction, null);
            }
        }

        private void OptionsChanged(string name, string value, DatabaseAction databaseAction)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IOptionRepository>();

                switch (databaseAction)
                {
                    case DatabaseAction.Update:
                    case DatabaseAction.Add:
                        repo.SetValue(name, value);
                        break;
                    case DatabaseAction.Remove:
                        repo.Remove(name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(databaseAction), databaseAction, null);
                }

                db.SaveChanges();
            }
        }

        private void ProfileChangeAction(string name, ProfileData profileData, DatabaseAction databaseAction)
        {
            try
            {
                ProfileChanged?.Invoke(name, profileData, databaseAction == DatabaseAction.Remove);
            }
            catch(Exception e)
            {
                Logger.Warn(e, "Ignored Exception -- ProfileChanged.InvokeFast");
            }


            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IProfileRepository>();

                switch (databaseAction)
                {
                    case DatabaseAction.Update:
                        var uEnt = repo.GetEntity(name);
                        uEnt.CurrentPosition = profileData.CurrentPosition;
                        uEnt.FilterString = profileData.FilterString;
                        uEnt.NextImage = profileData.NextImages;
                        uEnt.CurrentImages = profileData.CurrentImages;
                        uEnt.PageType = profileData.PageType;
                        break;
                    case DatabaseAction.Add:
                        repo.Save(new ProfileEntity
                        {
                            Name =  name,
                            CurrentPosition = profileData.CurrentPosition,
                            FilterString = profileData.FilterString,
                            NextImage = profileData.NextImages,
                            CurrentImages = profileData.CurrentImages,
                            PageType = profileData.PageType,                           
                        });
                        break;
                    case DatabaseAction.Remove:
                        repo.Remove(name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(databaseAction), databaseAction, null);
                }

                db.SaveChanges();
            }
        }

        private T GetValue<T>(string name, T defaultValue)
        {
            if (_options == null) return defaultValue;
            if (!_options.TryGetValue(name, out var sValue)) return defaultValue;

            return (T) Convert.ChangeType(sValue, typeof(T));
        }

        private void SetValue<T>(string name, T value)
        {
            if (_options == null)
                return;
            _options[name] = value.ToString();
        }
    }
}
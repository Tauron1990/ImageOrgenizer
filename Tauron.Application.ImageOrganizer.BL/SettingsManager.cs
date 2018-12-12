using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL
{
    [Export(typeof(ISettingsManager))]
    public class SettingsManager : ISettingsManager
    {
        private AppSettings _appSettings;

        [Inject]
        public ITauronEnviroment TauronEnviroment { get; set; }

        public ISettings Settings { get; private set; }

        public void Load(string name)
        {
            _appSettings = new AppSettings(AppConststands.ApplicationName, TauronEnviroment.DefaultProfilePath);
            _appSettings.Load(name);
            Settings = _appSettings;
        }

        public void Save() => _appSettings.Save();
    }
}
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateDatabase)]
    public class UpdateDatabaseRule : IBusinessRuleBase<string>
    {
        [Inject]
        public IDBSettings InternalSettings { get; set; }

        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        [Inject]
        public IDatabaseSchema DatabaseSchema { get; set; }

        private ISettings Settings => SettingsManager.Settings;

        public override void ActionImpl(string path)
        {
            if(string.IsNullOrWhiteSpace(path) || Settings == null) return;

            if (Settings.CurrentDatabase != path)
            {
                Settings.CurrentDatabase = path;
                SettingsManager.Save();
            }
            
            DatabaseSchema.Update(path);
            InternalSettings.Initialize();
            FileContainerManager.Switch(path, InternalSettings.ContainerType, InternalSettings.CustomMultiPath);
        }
    }
}
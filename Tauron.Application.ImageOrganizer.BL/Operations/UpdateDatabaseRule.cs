using System;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateDatabase)]
    public class UpdateDatabaseRule : IOBusinessRuleBase<string, bool>
    {
        [Inject]
        public IDBSettings InternalSettings { get; set; }

        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        [Inject]
        public IDatabaseSchema DatabaseSchema { get; set; }

        private ISettings Settings => SettingsManager.Settings;

        public override bool ActionImpl(string path)
        {
            if(string.IsNullOrWhiteSpace(path) || Settings == null) return false;
            if (!Uri.TryCreate(path, UriKind.Absolute, out var tempUri) || !tempUri.IsFile) return false;

            if (Settings.CurrentDatabase != path)
            {
                Settings.CurrentDatabase = path;
                SettingsManager.Save();
            }
            
            DatabaseSchema.Update(path);
            InternalSettings.Initialize();
            FileContainerManager.Switch(path, InternalSettings.ContainerType, InternalSettings.CustomMultiPath);

            return true;
        }
    }
}
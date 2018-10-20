using ImageOrganizer.Data;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateDatabase)]
    public class UpdateDatabaseRule : IBusinessRuleBase<string>
    {
        [Inject]
        public ISettings InternalSettings { get; set; }

        public override void ActionImpl(string path)
        {
            if(string.IsNullOrWhiteSpace(path)) return;

            if (Properties.Settings.Default.CurrentDatabase != path)
            {
                Properties.Settings.Default.CurrentDatabase = path;
                Properties.Settings.Default.Save();
            }
            
            DatabaseImpl.UpdateSchema(path);
            Settings.InitAction();
            FileContainerManager.Switch(path, InternalSettings);
        }
    }
}
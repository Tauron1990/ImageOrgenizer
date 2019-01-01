using System.IO;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{ 
    [ExportRule(RuleNames.GetDownloadCount)]
    public class GetDownloadCountRule : OBusinessRuleBase<int>
    {
        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        public override int ActionImpl()
        {
            if (!File.Exists(SettingsManager.Settings?.CurrentDatabase)) return 0;

            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();
                
                return repo.Get(false).Count(di => di.DownloadStade == DownloadStade.Queued);
            }
        }
    }
}
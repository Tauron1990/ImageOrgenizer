using System;
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
    [ExportRule(RuleNames.GetDownloadItems)]
    public class GetDownloadItems : IOBusinessRuleBase<bool ,DownloadItem[]>
    {
        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        public override DownloadItem[] ActionImpl(bool fetchAll)
        {
            if(!File.Exists(SettingsManager.Settings?.CurrentDatabase))
                return new DownloadItem[0];

            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                if (fetchAll)
                    return repo.Get(false).Where(de => de.DownloadStade != DownloadStade.Compled).Select(e => new DownloadItem(e)).ToArray();

                var time = DateTime.Now;
                
                var temp = repo.Get(false)
                    .OrderBy(e => e.Schedule)
                    .Where(de => de.DownloadStade == DownloadStade.Queued)
                    .Where(de => de.Schedule < time)
                    .Take(10)
                    .Select(e => new DownloadItem(e))
                    .ToArray();
                
                return temp;
            }
        }
    }
}
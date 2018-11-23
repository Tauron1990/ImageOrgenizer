using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.StartDownloads)]
    public class StartDownloadsRule : BusinessRuleBase
    {
        public override void ActionImpl()
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                foreach (var entity in repo.Get(true).Where(de => de.DownloadStade == DownloadStade.Paused))
                    entity.DownloadStade = DownloadStade.Queued;

                db.SaveChanges();
            }
        }
    }
}
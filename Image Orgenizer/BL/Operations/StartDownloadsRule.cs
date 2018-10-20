using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
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
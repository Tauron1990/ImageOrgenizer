using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.DownloadCompled)]
    public class DownloadCompledRule : IBusinessRuleBase<DownloadItem>
    {
        public override void ActionImpl(DownloadItem input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                var entity = repo.Get(true).FirstOrDefault(e => e.Id == input.Id);
                if(entity == null) return;

                entity.DownloadStade = DownloadStade.Compled;

                db.SaveChanges();
            }
        }
    }
}
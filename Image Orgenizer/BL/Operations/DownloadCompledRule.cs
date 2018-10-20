using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
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
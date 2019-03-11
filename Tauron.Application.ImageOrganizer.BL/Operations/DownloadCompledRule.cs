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
        [InjectRepo]
        public IDownloadRepository DownloadRepository { get; set; }

        public override void ActionImpl(DownloadItem input)
        {
            using (var db = Enter())
            {
                var entity = DownloadRepository.Get(true).FirstOrDefault(e => e.Id == input.Id);
                if(entity == null) return;

                entity.DownloadStade = DownloadStade.Compled;

                db.SaveChanges();
            }
        }
    }
}
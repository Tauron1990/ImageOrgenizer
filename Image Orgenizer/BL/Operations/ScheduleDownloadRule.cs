using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ScheduleDonwnload)]
    public class ScheduleDownloadRule : IBusinessRuleBase<DownloadItem>
    {
        public override void ActionImpl(DownloadItem input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                repo.Add(input.Image, input.DownloadType, input.Schedule, input.Provider, input.AvoidDouble);

                db.SaveChanges();
            }
        }
    }
}
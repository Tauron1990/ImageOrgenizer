using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ScheduleDonwnload)]
    public class ScheduleDownloadRule : IBusinessRuleBase<DownloadItem[]>
    {
        public override void ActionImpl(DownloadItem[] inputs)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                foreach (var input in inputs)
                {
                    if(input.AvoidDouble && repo.Contains(input.Image)) continue;

                    repo.Add(input.Image, input.DownloadType, input.Schedule, input.Provider, input.AvoidDouble, input.RemoveImageOnFail);
                }

                db.SaveChanges();
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ScheduleDonwnload)]
    public class ScheduleDownloadRule : IOBusinessRuleBase<DownloadItem[], DownloadItem[]>
    {
        public override DownloadItem[] ActionImpl(DownloadItem[] inputs)
        {
            List<DownloadEntity> items = new List<DownloadEntity>();

            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();
                var imgRepo = RepositoryFactory.GetRepository<IImageRepository>();

                foreach (var input in inputs)
                {
                    if (input.DownloadType == DownloadType.DownloadImage || input.DownloadType == DownloadType.ReDownload)
                    {
                        AppConststands.NotImplemented();
                        continue;
                    }

                    if(input.AvoidDouble)
                    {
                        if(repo.Contains(input.Image) || input.DownloadType == DownloadType.DownloadImage && imgRepo.Containes(input.Image))
                            continue;
                    }

                    items.Add(repo.Add(input.Image, input.DownloadType, input.Schedule, input.Provider, input.AvoidDouble, input.RemoveImageOnFail, input.Metadata));
                }

                db.SaveChanges();
            }

            return items.Select(de => new DownloadItem(de)).ToArray();
        }
    }
}
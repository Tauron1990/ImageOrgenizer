using System;
using System.Collections.Generic;
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
    public class GetDownloadItems : IOBusinessRuleBase<GetDownloadItemInput ,DownloadItem[]>
    {
        [Inject]
        public ISettingsManager SettingsManager { get; set; }

        public override DownloadItem[] ActionImpl(GetDownloadItemInput input)
        {
            if (!File.Exists(SettingsManager.Settings?.CurrentDatabase))
                return new DownloadItem[0];
            var fetchAll = input.FetchAll;

            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();
                
                var time = DateTime.Now;
                IQueryable<DownloadEntity> temp = repo.Get(false)
                    .OrderBy(e => e.Schedule);
                temp = fetchAll ? temp.Where(de => de.DownloadStade != DownloadStade.Compled) : temp.Where(de => de.DownloadStade == DownloadStade.Queued);

                if (!fetchAll)
                {
                    temp = temp.Where(de => de.Schedule < time);
                    foreach (var delay in input.Delays) temp = temp.Where(de => de.Provider != delay);
                }

                var group = temp.GroupBy(de => de.DownloadType);

                var filter = new IEnumerable<DownloadEntity>[7];

                foreach (var grouping in group)
                {
                    switch (grouping.Key)
                    {
                        case DownloadType.UpdateTags:
                            filter[4] = grouping;
                            break;
                        case DownloadType.DownloadTags:
                            filter[3] = grouping;
                            break;
                        case DownloadType.DownloadImage:
                            var prio = new List<DownloadEntity>();
                            var normal = new List<DownloadEntity>();
                            foreach (var downloadEntity in grouping)
                            {
                                if (downloadEntity.DownloadStade == DownloadStade.Paused)
                                    prio.Add(downloadEntity);
                                else
                                    normal.Add(downloadEntity);
                            }

                            filter[0] = prio;
                            filter[1] = normal;
                            break;
                        case DownloadType.ReDownload:
                            filter[2] = grouping;
                            break;
                        case DownloadType.UpdateColor:
                            filter[5] = grouping;
                            break;
                        case DownloadType.UpdateDescription:
                            filter[6] = grouping;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                var downloads = filter.SelectMany(g => g ?? Enumerable.Empty<DownloadEntity>()).Select(e => new DownloadItem(e));

                if (fetchAll)
                {
                    return downloads
                        .Take(1000)
                        .ToArray();
                }

                return downloads.Take(10).ToArray();
            }
        }
    }
}
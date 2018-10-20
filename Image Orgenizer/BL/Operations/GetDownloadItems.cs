using System;
using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetDownloadItems)]
    public class GetDownloadItems : IOBusinessRuleBase<bool ,DownloadItem[]>
    {
        public override DownloadItem[] ActionImpl(bool fetchAll)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                if (fetchAll)
                    return repo.Get(false).Where(de => de.DownloadStade != DownloadStade.Compled).Select(e => new DownloadItem(e)).ToArray();

                var time = DateTime.Now;

                return repo.Get(false)
                    .Where(de => de.DownloadStade == DownloadStade.Queued)
                    .Where(de => de.Schedule < time)
                    .OrderBy(e => e.Schedule)
                    .Take(10)
                    .Select(e => new DownloadItem(e))
                    .ToArray();
            }
        }
    }
}
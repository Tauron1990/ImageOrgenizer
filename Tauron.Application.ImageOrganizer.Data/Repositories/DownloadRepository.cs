using System;
using System.Linq;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public class DownloadRepository : Repository<DownloadEntity, int>, IDownloadRepository
    {
        public DownloadEntity Add(string name, DownloadType downloadType, DateTime schedule, string provider, bool avoidDouble, bool removeImageOnFail, string metadata)
        {
            if (avoidDouble && QueryAsNoTracking().Any(de => de.Image == name))
                return null;

            var ent = new DownloadEntity
            {
                Image = name,
                DownloadType = downloadType,
                Schedule = schedule,
                DownloadStade = DownloadStade.Queued,
                Provider = provider,
                RemoveImageOnFail = removeImageOnFail,
                Metadata = metadata
            };

            Add(ent);
            return ent;
        }

        public IQueryable<DownloadEntity> Get(bool tracking) => tracking ? Query() : QueryAsNoTracking();
        public bool Contains(string url) => QueryAsNoTracking().Any(de => de.Image == url);

        public DownloadRepository(IDatabase database) : base(database)
        {
            
        }
    }
}
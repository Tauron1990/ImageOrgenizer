using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public class DownloadRepository : Repository<DownloadEntity, int>, IDownloadRepository
    {
        public DownloadEntity Add(string name, DownloadType downloadType, DateTime schedule, string provider, bool avoidDouble, bool removeImageOnFail, string metadata, DownloadStade downloadStade)
        {
            var ent = new DownloadEntity
            {
                Image = name,
                DownloadType = downloadType,
                Schedule = schedule,
                DownloadStade = downloadStade,
                Provider = provider,
                RemoveImageOnFail = removeImageOnFail,
                Metadata = metadata
            };

            Add(ent);
            return ent;
        }

        public IQueryable<DownloadEntity> Get(bool tracking) => tracking ? Query() : QueryAsNoTracking();

        public bool Contains(string image, string meta, DownloadType type)
        {
            if (string.IsNullOrWhiteSpace(meta))
                meta = null;

            var items = QueryAsNoTracking().Where(di => di.Image == image).ToArray();
            return items.Any(i => i.DownloadType == type && i.Metadata == meta);
        }

        public bool Contains(string url) => QueryAsNoTracking().Any(de => de.Image == url);

        public DownloadRepository(IDatabase database) : base(database)
        {
            
        }
    }
}
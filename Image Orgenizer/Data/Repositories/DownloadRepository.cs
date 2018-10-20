using System;
using System.Linq;
using ImageOrganizer.Data.Entities;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Repositories
{
    public class DownloadRepository : Repository<DownloadEntity, int>, IDownloadRepository
    {
        public void Add(string name, DownloadType downloadType, DateTime schedule, string provider, bool avoidDouble)
        {
            if (avoidDouble && QueryAsNoTracking().Any(de => de.Image == name))
            {
               
            }

            Add(new DownloadEntity
            {
                Image = name,
                DownloadType = downloadType,
                Schedule = schedule,
                DownloadStade = DownloadStade.Queued,
                Provider = provider
            });
        }

        public IQueryable<DownloadEntity> Get(bool tracking) => tracking ? Query() : QueryAsNoTracking();

        public DownloadRepository(IDatabase database) : base(database)
        {
            
        }
    }
}
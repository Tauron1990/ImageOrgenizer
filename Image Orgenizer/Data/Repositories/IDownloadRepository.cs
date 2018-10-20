using System;
using System.Linq;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.Data.Repositories
{
    public interface IDownloadRepository
    {
        void Add(string name, DownloadType downloadType, DateTime schedule, string provider, bool avoidDouble);

        IQueryable<DownloadEntity> Get(bool tracking);

        void Remove(DownloadEntity downloadEntity);
    }
}
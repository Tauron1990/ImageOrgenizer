using System;
using System.Linq;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.Data.Repositories
{
    public interface IDownloadRepository
    {
        DownloadEntity Add(string name, DownloadType downloadType, DateTime schedule, string provider, bool avoidDouble, bool removeImageOnFail);

        IQueryable<DownloadEntity> Get(bool tracking);

        bool Contains(string url);
    }
}
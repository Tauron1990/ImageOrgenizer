using System;
using System.Linq;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public interface IDownloadRepository
    {
        DownloadEntity Add(string name, DownloadType downloadType, DateTime schedule, string provider, bool avoidDouble, bool removeImageOnFail, string metadata);

        IQueryable<DownloadEntity> Get(bool tracking);

        bool Contains(string image, string meta, DownloadType type);
    }
}
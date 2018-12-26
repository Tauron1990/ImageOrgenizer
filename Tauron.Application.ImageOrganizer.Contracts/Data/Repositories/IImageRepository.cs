using System.Collections.Generic;
using System.Linq;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public interface IImageRepository
    {
        IQueryable<ImageEntity> Query(bool include);
        IQueryable<ImageEntity> QueryAsNoTracking(bool include);
        IQueryable<ImageEntity> QueryFromSql(string sql, bool include);
        void Remove(ImageEntity entity);
        void Add(ImageEntity entity);
        void SetOrder(IComparer<ImageEntity> sorter);
        void AddRange(IEnumerable<ImageEntity> newImages);
        bool Containes(string inputImage);
    }
}
using System.Linq;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.Data.Repositories
{
    public interface IImageRepository
    {
        IQueryable<ImageEntity> Query();
        IQueryable<ImageEntity> QueryAsNoTracking();
        ImageEntity Find(int key);
        void Update(ImageEntity entity);
        void Remove(ImageEntity entity);
        void Add(ImageEntity entity);
    }
}
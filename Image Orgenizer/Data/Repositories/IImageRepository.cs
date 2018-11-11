using System.Collections.Generic;
using System.Linq;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.Data.Repositories
{
    public interface IImageRepository
    {
        IQueryable<ImageEntity> Query();
        IQueryable<ImageEntity> QueryAsNoTracking();
        void Remove(ImageEntity entity);
        void Add(ImageEntity entity);
        void AddRange(IEnumerable<ImageEntity> newImages);
        bool Containes(string inputImage);
    }
}
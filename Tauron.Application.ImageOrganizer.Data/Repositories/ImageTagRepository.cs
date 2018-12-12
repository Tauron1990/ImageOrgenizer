using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public class ImageTagRepository : IImageTagRepository
    {
        private readonly DbContext _database;

        public ImageTagRepository(IDatabase database) => _database = (DbContext)database.Context;

        public void Add(ImageTag tag) => _database.Add(tag);

        public void Remove(ImageTag tag) => _database.Remove(tag);
    }
}
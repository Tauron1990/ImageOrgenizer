using System.Collections.Generic;
using ImageOrganizer.Data.Entities;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Repositories
{
    public sealed class ImageRepository : Repository<ImageEntity, int>, IImageRepository
    {
        public ImageRepository(IDatabase database) : base(database)
        {
        }

    }
}
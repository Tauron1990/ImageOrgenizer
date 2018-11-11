using System.Linq;
using ImageOrganizer.Data.Entities;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Repositories
{
    public sealed class ImageRepository : Repository<ImageEntity, int>, IImageRepository
    {
        public ImageRepository(IDatabase database) : base(database)
        {
        }

        public bool Containes(string inputImage) => QueryAsNoTracking().Any(ie => ie.Name.StartsWith(inputImage));
    }
}
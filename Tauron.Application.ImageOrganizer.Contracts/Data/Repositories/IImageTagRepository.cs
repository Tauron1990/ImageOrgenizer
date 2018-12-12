using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public interface IImageTagRepository
    {
        void Add(ImageTag tag);
        void Remove(ImageTag tag);
    }
}
using System.Linq;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public interface ITagTypeRepository
    {
        TagTypeEntity Get(string name, bool tracking);
        IQueryable<TagTypeEntity> QueryAll();
        void Add(TagTypeEntity entity);
        void Remove(TagTypeEntity entity);
        bool Contains(string name);
    }
}
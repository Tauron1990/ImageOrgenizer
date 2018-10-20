using System.Linq;
using ImageOrganizer.BL;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.Data.Repositories
{
    public interface ITagTypeRepository
    {
        TagTypeEntity Get(string name, bool tracking);
        IQueryable<TagTypeEntity> QueryAll();
        void Add(TagTypeEntity entity);
        void Remove(TagTypeEntity entity);
    }
}
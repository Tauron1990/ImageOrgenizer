using System.Linq;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.Data.Repositories
{
    public interface ITagRepository
    {
        TagEntity GetName(string name, bool tracking);
        IQueryable<TagEntity> QueryAll();
        void Add(TagEntity tag);
        void Remove(TagEntity tag);
    }
}
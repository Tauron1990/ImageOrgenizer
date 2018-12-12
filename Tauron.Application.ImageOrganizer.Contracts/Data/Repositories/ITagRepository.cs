using System.Linq;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public interface ITagRepository
    {
        TagEntity GetName(string name, bool tracking);
        IQueryable<TagEntity> QueryAll();
        void Add(TagEntity tag);
        void Remove(TagEntity tag);
        bool Contains(string name);
    }
}
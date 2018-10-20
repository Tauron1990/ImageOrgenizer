using System.Linq;
using ImageOrganizer.Data.Entities;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Repositories
{
    public sealed class TagTypeRepository : Repository<TagTypeEntity, string>, ITagTypeRepository
    {
        public TagTypeRepository(IDatabase database) : base(database)
        {
        }

        public TagTypeEntity Get(string name, bool tracking)
        {
            var query = tracking ? Query() : QueryAsNoTracking();

            return query.FirstOrDefault(e => e.Id == name);
        }

        public IQueryable<TagTypeEntity> QueryAll() => QueryAsNoTracking();
    }
}
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public sealed class TagRepository : Repository<TagEntity, int>, ITagRepository
    {
        private Dictionary<string, TagEntity> _tagEntities;

        public TagRepository(IDatabase database) : base(database) => _tagEntities = new Dictionary<string, TagEntity>();

        public TagEntity GetName(string name, bool tracking)
        {
            if (!tracking && _tagEntities.TryGetValue(name, out var ctag))
                return ctag;

            var query = tracking ? Query() : QueryAsNoTracking();

            var ent = query.Include(e => e.Type).FirstOrDefault(e => e.Name == name);
            if(!tracking && ent != null)
                _tagEntities[name] = ent;

            return ent;
        }

        public IQueryable<TagEntity> QueryAll() => QueryAsNoTracking().Include(e => e.Type);
        public bool Contains(string name) => QueryAsNoTracking().Any(td => td.Name == name);
    }
}
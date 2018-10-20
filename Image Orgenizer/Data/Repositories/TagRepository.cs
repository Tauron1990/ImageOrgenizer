using System.Collections.Generic;
using System.Linq;
using ImageOrganizer.BL;
using ImageOrganizer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Repositories
{
    public sealed class TagRepository : Repository<TagEntity, string>, ITagRepository
    {
        private Dictionary<string, TagEntity> _tagEntities;

        public TagRepository(IDatabase database) : base(database) => _tagEntities = new Dictionary<string, TagEntity>();

        public TagEntity GetName(string name, bool tracking)
        {
            if (!tracking && _tagEntities.TryGetValue(name, out var ctag))
                return ctag;

            var query = tracking ? Query() : QueryAsNoTracking();

            var ent = query.Include(e => e.Type).First(e => e.Id == name);
            if(!tracking)
                _tagEntities[name] = ent;

            return ent;
        }

        public IQueryable<TagEntity> QueryAll() => QueryAsNoTracking().Include(e => e.Type);
    }
}
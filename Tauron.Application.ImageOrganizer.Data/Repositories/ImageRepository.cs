using System.Linq;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public sealed class ImageRepository : Repository<ImageEntity, int>, IImageRepository
    {
        public ImageRepository(IDatabase database) : base(database) { }

        public IQueryable<ImageEntity> Query(bool include)
        {
            var query = Query();
            if (include)
            {
                query.Include(e => e.Tags)
                    .ThenInclude(e => e.TagEntity)
                    .ThenInclude(e => e.Type);
            }

            return query;
        }

        public IQueryable<ImageEntity> QueryAsNoTracking(bool include)
        {
            var query = QueryAsNoTracking();
            if (include)
            {
                query.Include(e => e.Tags)
                    .ThenInclude(e => e.TagEntity)
                    .ThenInclude(e => e.Type);
            }

            return query;
        }

        public IQueryable<ImageEntity> QueryFromSql(string sql, bool include)
        {
            var query = Query().FromSql(new RawSqlString(sql));
            if (include)
            {
                query.Include(e => e.Tags)
                    .ThenInclude(e => e.TagEntity)
                    .ThenInclude(e => e.Type);
            }

            return query;
        }

        public bool Containes(string inputImage) => QueryAsNoTracking().Any(ie => ie.Name.StartsWith(inputImage));
    }
}
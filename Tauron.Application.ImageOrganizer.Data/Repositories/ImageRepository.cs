using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                query = query.Include(e => e.Tags)
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
                query = query.Include(e => e.Tags)
                    .ThenInclude(e => e.TagEntity)
                    .ThenInclude(e => e.Type);
            }
            
            return query;
        }

        public IQueryable<ImageEntity> QueryFromSql(string sql, bool include)
        {
            var query = Query();
            if (include)
            {
                query = query.Include(e => e.Tags)
                    .ThenInclude(e => e.TagEntity)
                    .ThenInclude(e => e.Type);
            }

            return query.FromSql(new RawSqlString(sql));
        }

        public void SetOrder(IComparer<ImageEntity> sorter)
        {
            var db = GetContext<DatabaseImpl>();

            StringBuilder commandBuilder = new StringBuilder();
            List<string> ids = new List<string>();

            var temp = db.Images.ToList();
            temp.Sort(sorter);

            foreach (var suElements in temp.Split(7000).Select(sel => sel.ToArray()))
            {
                ids.Clear();
                commandBuilder.Clear();

                commandBuilder//.AppendLine("BEGIN TRANSACTION;")
                    .AppendLine("Update Images")
                    .AppendLine("SET SortOrder = Case Id");

                for (var index = 0; index < suElements.Length; index++)
                {
                    var entity = suElements[index];
                    ids.Add(entity.Id.ToString());
                    commandBuilder.AppendLine($"    WHEN {entity.Id} THEN '{index}'");
                }

                commandBuilder.AppendLine("END").Append("WHERE Id IN(");

                commandBuilder.Append(string.Join(", ", ids));

                commandBuilder.AppendLine(")");

                //commandBuilder.AppendLine("COMMIT;");

                using (var trans = db.Database.BeginTransaction())
                {
                    var command = commandBuilder.ToString();
                    db.Database.ExecuteSqlCommand(new RawSqlString(command));
                    trans.Commit();
                }
            }
        }

        public bool Containes(string inputImage)
        {
            string pattern = "%" + inputImage + "%";

            if (QueryAsNoTracking().Any(ie => EF.Functions.Like(ie.Name, pattern))) return true;
            return false;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public sealed class OptionRepository : Repository<OptionEntity, string>, IOptionRepository
    {
        public OptionRepository(IDatabase database) : base(database) { }

        public IEnumerable<(string Name, string Value)> GetAllValues()
        {
            foreach (var optionEntity in QueryAsNoTracking())
                yield return (optionEntity.Id, optionEntity.Value);
        }

        public void Remove(string name) => Remove(Query().First(entity => entity.Id == name));

        public void SetValue(string name, string value)
        {
            var ent = Query().FirstOrDefault(e => e.Id == name);
            if (ent == null)
            {
                Add(new OptionEntity {Id = name, Value = value});
            }
            else
            {
                ent.Value = value;
                Update(ent);
            }
        }

        public string GetValue(string name) => Query().FirstOrDefault(e => e.Id == name)?.Value;
    }
}
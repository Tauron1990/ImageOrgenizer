using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateTag)]
    public class UpdateTagRule : IOBusinessRuleBase<TagData, TagData>
    {
        public override TagData ActionImpl(TagData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var tagRepository = RepositoryFactory.GetRepository<ITagRepository>();
                var tagTypeRepository = RepositoryFactory.GetRepository<ITagTypeRepository>();

                var tag = tagRepository.GetName(input.Name, true);
                if (tag == null)
                {
                    var ent = input.ToEntity();
                    if (ent.Type != null)
                        ent.Type = Update(ent.Type, tagTypeRepository);

                    tagRepository.Add(ent);
                    tag = ent;
                }
                else
                {
                    tag.Description = input.Description;
                    tag.Type = Update(tag.Type, tagTypeRepository);
                }

                db.SaveChanges();
                return new TagData(tag);
            }
        }

        private TagTypeEntity Update(TagTypeEntity entity, ITagTypeRepository repo)
        {
            var type = repo.Get(entity.Id, true);
            if (type == null)
            {
                repo.Add(entity);
                return entity;
            }

            type.Color = entity.Color;
            return type;
        }
    }
}
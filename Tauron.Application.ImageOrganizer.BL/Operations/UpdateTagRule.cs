using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateTag)]
    public class UpdateTagRule : IOBusinessRuleBase<UpdateTagInput, TagData>
    {
        public override TagData ActionImpl(UpdateTagInput updateTag)
        {
            var input = updateTag.TagData;

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
                    if(!updateTag.IgnoreTagType && input.Type.Color != tag.Type.Color)
                       tag.Type = Update(input.Type.ToEntity(), tagTypeRepository);
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
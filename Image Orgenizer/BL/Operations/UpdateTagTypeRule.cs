using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateTagType)]
    public class UpdateTagTypeRule : IOBusinessRuleBase<TagTypeData, TagTypeData>
    {
        public override TagTypeData ActionImpl(TagTypeData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<ITagTypeRepository>();

                var tt = repo.Get(input.Name, true);

                if (tt == null)
                {
                    tt = input.ToEntity();
                    repo.Add(tt);
                }
                else
                    tt.Color = input.Color;

                db.SaveChanges();
                return new TagTypeData(tt);
            }
        }
    }
}
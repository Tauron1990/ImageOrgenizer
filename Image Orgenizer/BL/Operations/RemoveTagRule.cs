using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RemoveTag)]
    public class RemoveTagRule : IOBusinessRuleBase<TagData, bool>
    {
        public override bool ActionImpl(TagData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<ITagRepository>();

                var tag = repo.GetName(input.Name, true);
                if (tag == null) return false;

                repo.Remove(tag);
                db.SaveChanges();

                return true;
            }
        }
    }
}
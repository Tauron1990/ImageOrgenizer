using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
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
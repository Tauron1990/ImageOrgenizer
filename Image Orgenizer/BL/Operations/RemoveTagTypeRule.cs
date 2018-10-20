using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RemoveTagType)]
    public class RemoveTagTypeRule : IOBusinessRuleBase<TagTypeData, bool>
    {
        public override bool ActionImpl(TagTypeData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<ITagTypeRepository>();

                var temp = repo.Get(input.Name, true);
                if (temp == null) return false;
                repo.Remove(temp);

                db.SaveChanges();
                return true;
            }
        }
    }
}
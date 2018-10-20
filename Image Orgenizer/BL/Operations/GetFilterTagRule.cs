using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetFilterTag)]
    public class GetFilterTagRule : IOBusinessRuleBase<string, TagFilterElement>
    {
        public override TagFilterElement ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<ITagRepository>();

                var tag = repo.GetName(input, false);
                if (tag == null)
                    return null;

                return new TagFilterElement(new TagElement(tag));
            }
        }
    }
}
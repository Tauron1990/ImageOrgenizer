using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetFilterTag)]
    public class GetFilterTagRule : IOBusinessRuleBase<string, TagElement>
    {
        public override TagElement ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<ITagRepository>();

                var tag = repo.GetName(input, false);
                return tag == null ? null : new TagElement(tag);
            }
        }
    }
}
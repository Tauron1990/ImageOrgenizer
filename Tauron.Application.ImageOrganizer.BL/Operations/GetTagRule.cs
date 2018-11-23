using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetTag)]
    public class GetTagRule : IOBusinessRuleBase<string, TagData>
    {
        public override TagData ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<ITagRepository>();

                var temp = repo.GetName(input, false);
                return temp == null ? null : new TagData(temp);
            }
        }
    }
}
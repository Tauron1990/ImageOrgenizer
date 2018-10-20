using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
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
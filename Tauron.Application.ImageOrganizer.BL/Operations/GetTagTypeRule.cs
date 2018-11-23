using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetTagType)]
    public class GetTagTypeRule : IOBusinessRuleBase<string, TagTypeData>
    {
        public override TagTypeData ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<ITagTypeRepository>();

                var temp = repo.Get(input, false);
                return temp == null ? null : new TagTypeData(temp);
            }
        }
    }
}
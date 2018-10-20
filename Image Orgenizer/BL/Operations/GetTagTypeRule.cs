using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
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
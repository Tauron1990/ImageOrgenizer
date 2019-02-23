using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetTagType)]
    public class GetTagTypeRule : IOBusinessRuleBase<string, TagTypeData>
    {
        [InjectRepo]
        public ITagTypeRepository TagTypeRepository { get; set; }

        public override TagTypeData ActionImpl(string input)
        {
            using (Enter())
            {
                var temp = TagTypeRepository.Get(input, false);
                return temp == null ? null : new TagTypeData(temp);
            }
        }
    }
}
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetTag)]
    public class GetTagRule : IOBusinessRuleBase<string, TagData>
    {
        [InjectRepo]
        public ITagRepository TagRepository { get; set; }

        public override TagData ActionImpl(string input)
        {
            using (Enter())
            {
                var temp = TagRepository.GetName(input, false);
                return temp == null ? null : new TagData(temp);
            }
        }
    }
}
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetFilterTag)]
    public class GetFilterTagRule : IOBusinessRuleBase<string, TagElement>
    {
        [InjectRepo]
        public ITagRepository TagRepository { get; set; }

        public override TagElement ActionImpl(string input)
        {
            using (Enter())
            {
                var tag = TagRepository.GetName(input, false);
                return tag == null ? null : new TagElement(tag);
            }
        }
    }
}
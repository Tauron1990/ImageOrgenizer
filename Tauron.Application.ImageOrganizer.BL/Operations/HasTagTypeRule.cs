using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.HasTagType)]
    public class HasTagTypeRule : IOBusinessRuleBase<string, bool>
    {
        [InjectRepo]
        public ITagTypeRepository TagTypeRepository { get; set; }

        public override bool ActionImpl(string input)
        {
            using (Enter())
                return TagTypeRepository.Contains(input);
        }
    }
}
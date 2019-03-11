using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.HasTag)]
    public class HasTagRule : IOBusinessRuleBase<string, bool>
    {
        [InjectRepo]
        public ITagRepository TagRepository { get; set; }

        public override bool ActionImpl(string input)
        {
            using (Enter())
                return TagRepository.Contains(input);
        }
    }
}
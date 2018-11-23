using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.Recuvery)]
    public class RecuveryRule : IBusinessRuleBase<RecuveryInput>
    {
        public override void ActionImpl(RecuveryInput input)
        {
            FileContainerManager.Recuvery(input.ExportPath, input.Report);
        }
    }
}
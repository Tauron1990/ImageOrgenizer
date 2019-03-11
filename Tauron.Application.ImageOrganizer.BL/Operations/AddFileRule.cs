using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.AddFile)]
    public class AddFileRule : IOBusinessRuleBase<AddFileInput, bool>
    {
        public override bool ActionImpl(AddFileInput input) 
            => FileContainerManager.CanAdd(input.Name, s => s) && FileContainerManager.AddFile(input.Bytes, input.Name);
    }
}
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ReplaceImage)]
    public sealed class ReplaceImageRule : IOBusinessRuleBase<ReplaceImageInput, bool>
    {
        public override bool ActionImpl(ReplaceImageInput input)
        {
            using (var trans = FileContainerManager.GetContainerTransaction())
            {
                if (FileContainerManager.Contains(input.Name))
                    FileContainerManager.Remove(input.Name, trans);
                FileContainerManager.AddFile(input.Data, input.Name, trans);

                trans.Commit();
            }

            return true;
        }
    }
}
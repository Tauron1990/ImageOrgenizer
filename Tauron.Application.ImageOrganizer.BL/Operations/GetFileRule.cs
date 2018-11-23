using System.IO;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetFile)]
    public class GetFileRule : IOBusinessRuleBase<string, Stream>
    {
        public override Stream ActionImpl(string input) => FileContainerManager.GetFile(input);
    }
}
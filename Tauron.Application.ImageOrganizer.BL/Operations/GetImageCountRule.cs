using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetImageCount)]
    public class GetImageCountRule : OBusinessRuleBase<int>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        public override int ActionImpl()
        {
            using (Enter())
                return ImageRepository.QueryAsNoTracking(false).Count();
        }
    }
}
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetImageCount)]
    public class GetImageCountRule : OBusinessRuleBase<int>
    {
        public override int ActionImpl()
        {
            using (RepositoryFactory.Enter())
                return RepositoryFactory.GetRepository<IImageRepository>().QueryAsNoTracking(false).Count();
        }
    }
}
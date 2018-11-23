using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetImageData)]
    public class GetImageDataRule : IOBusinessRuleBase<string, ImageData>
    {
        public override ImageData ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();

                var result = repo.QueryAsNoTracking(true).FirstOrDefault(e => e.Name == input);
                return result == null ? null : new ImageData(result);
            }
        }
    }
}
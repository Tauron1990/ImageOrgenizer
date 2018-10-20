using System.Linq;
using ImageOrganizer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetImageData)]
    public class GetImageDataRule : IOBusinessRuleBase<string, ImageData>
    {
        public override ImageData ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();

                var result = repo.QueryAsNoTracking().Include(e => e.ImageTags).ThenInclude(it => it.TagEntity).FirstOrDefault(e => e.Name == input);
                return result == null ? null : new ImageData(result);
            }
        }
    }
}
using System.Linq;
using NLog;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.BL.Operations.Helper;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.GetImageData)]
    public class GetImageDataRule : IOBusinessRuleBase<string, ImageData>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        public override ImageData ActionImpl(string input)
        {
            using (Enter())
            {
                Logger.Trace($"Try Get Image: {input}");
                // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
                var result = ImageRepository.QueryAsNoTracking(true).Where(e => e.Name == input).ToArray().FirstOrDefault();
                Logger.Trace(result == null ? $"Image not found: {input}" : $"Found: {input}");
                return result == null ? null : new ImageData(result, NaturalStringComparer.Comparer);
            }
        }
    }
}
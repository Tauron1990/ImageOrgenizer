using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.IncreaseViewCount)]
    public sealed class IncreaseViewCountRule : IBusinessRuleBase<IncreaseViewCountInput>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        public override void ActionImpl(IncreaseViewCountInput input)
        {
            using (var db = Enter())
            {
                var imageEntity = ImageRepository.Query(false).First(ie => ie.Name == input.Name);

                if (input.IsRandom)
                    imageEntity.RandomCount++;
                imageEntity.ViewCount++;

                db.SaveChanges();
            }
        }
    }
}
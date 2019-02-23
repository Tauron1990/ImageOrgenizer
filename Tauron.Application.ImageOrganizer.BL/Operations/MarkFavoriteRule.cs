using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.MarkFavorite)]
    public class MarkFavoriteRule : IBusinessRuleBase<ImageData>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        public override void ActionImpl(ImageData input)
        {
            using (var db = Enter())
            {
                var entity = ImageRepository.Query(false).FirstOrDefault(e => e.Id == input.Id);
                if(entity == null) return;

                entity.Favorite = !entity.Favorite;

                db.SaveChanges();
            }
        }
    }
}
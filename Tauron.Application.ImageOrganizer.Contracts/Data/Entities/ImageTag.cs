using JetBrains.Annotations;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class ImageTag
    {
        public int ImageEntityId { get; set; }
        public ImageEntity ImageEntity { get; set; }

        public int TagEntityId { get; set; }
        public TagEntity TagEntity { get; set; }

        public ImageTag SetTag(TagEntity tag)
        {
            TagEntity = tag;
            return this;
        }
    }
}
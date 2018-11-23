using JetBrains.Annotations;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class ImageTag
    {
        public int Id { get; set; }

        public int ImageEntityId { get; set; }
        public ImageEntity ImageEntity { get; set; }

        public string TagEntityId { get; set; }
        public TagEntity TagEntity { get; set; }
    }
}
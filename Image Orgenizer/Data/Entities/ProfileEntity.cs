using ImageOrganizer.BL;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Entities
{
    public class ProfileEntity : GenericBaseEntity<int>
    {
        public string Name { get; set; }

        public int CurrentPosition { get; set; }

        public int NextImage { get; set; }

        public string FilterString { get; set; }

        public int CurrentImages { get; set; }

        public string PageType { get; set; }

        public bool Favorite { get; set; }

        public static ProfileEntity FromData(ProfileData data)
        {
            return new ProfileEntity
            {
                CurrentPosition = data.CurrentPosition,
                CurrentImages = data.CurrentImages,
                FilterString = data.FilterString,
                Name = data.PageType,
                NextImage = data.NextImages
            };
        }
    }
}
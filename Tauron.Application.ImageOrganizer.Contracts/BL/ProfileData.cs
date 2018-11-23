using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class ProfileData
    {
        public ProfileData(int nextImages, int currentPosition, string filterString, int currentImages, string pageType, bool favorite)
        {
            NextImages = nextImages;
            CurrentPosition = currentPosition;
            FilterString = filterString;
            CurrentImages = currentImages;
            PageType = pageType;
            Favorite = favorite;
        }

        public ProfileData(ProfileEntity entity)
            : this(entity.NextImage, entity.CurrentPosition, entity.FilterString, entity.CurrentImages, entity.PageType, entity.Favorite)
        {
        }

        public int NextImages { get; }

        public int CurrentImages { get; }

        public int CurrentPosition { get; }

        public string FilterString { get; }

        public string PageType { get; }

        public bool Favorite { get; }
    }
}
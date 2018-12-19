using Tauron.Application.Common.BaseLayer.Data;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    public class ProfileEntity : GenericBaseEntity<int>
    {
        private string _name;
        private int _currentPosition;
        private int _nextImage;
        private string _filterString;
        private int _currentImages;
        private string _pageType;
        private bool _favorite;

        public string Name
        {
            get => _name;
            set => SetWithNotify(ref _name, value);
        }

        public int CurrentPosition
        {
            get => _currentPosition;
            set => SetWithNotify(ref _currentPosition, value);
        }

        public int NextImage
        {
            get => _nextImage;
            set => SetWithNotify(ref _nextImage, value);
        }

        public string FilterString
        {
            get => _filterString;
            set => SetWithNotify(ref _filterString, value);
        }

        public int CurrentImages
        {
            get => _currentImages;
            set => SetWithNotify(ref _currentImages, value);
        }

        public string PageType
        {
            get => _pageType;
            set => SetWithNotify(ref _pageType, value);
        }

        public bool Favorite
        {
            get => _favorite;
            set => SetWithNotify(ref _favorite, value);
        }
        
    }
}
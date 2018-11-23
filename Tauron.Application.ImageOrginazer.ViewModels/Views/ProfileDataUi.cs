using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ImageOrganizer;
using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
    public class ProfileDataUi : ObservableObject
    {
        private int _nextImages;
        private int _currentImages;
        private int _currentPosition;
        private string _filterString;
        private PossiblePager _pageType;
        private bool _favorite;

        [Bindable(false)]
        public ProfileData ProfileData { get; private set; }
        
        public string Name { get; }

        public int NextImages
        {
            get => _nextImages;
            set => SetProperty(ref _nextImages, value, () => IsEdited = true);
        }

        public int CurrentImages
        {
            get => _currentImages;
            set => SetProperty(ref _currentImages, value, () => IsEdited = true);
        }

        public int CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value, () => IsEdited = true);
        }

        public string FilterString
        {
            get => _filterString;
            set => SetProperty(ref _filterString, value, () => IsEdited = true);
        }

        public PossiblePager PageType
        {
            get => _pageType;
            set => SetProperty(ref _pageType, value, () => IsEdited = true);
        }

        public bool Favorite
        {
            get => _favorite;
            set => SetProperty(ref _favorite, value, () => IsEdited = true);
        }

        public bool IsEdited { get; set; }

        public ProfileData CreateNew() => new ProfileData(NextImages, CurrentPosition, FilterString, CurrentImages, PageType?.Name, Favorite);

        public ProfileDataUi(ProfileData profileData, string name, IEnumerable<PossiblePager> pagers)
        {
            Update(profileData, pagers);

            Name = name;
        }

        public void Update(ProfileData profileData, IEnumerable<PossiblePager> pagers)
        {
            ProfileData = profileData;

            NextImages = profileData.NextImages;
            CurrentImages = profileData.CurrentImages;
            CurrentPosition = profileData.CurrentPosition;
            FilterString = profileData.FilterString;
            var possiblePagers = pagers as PossiblePager[] ?? pagers.ToArray();
            PageType = possiblePagers.FirstOrDefault(p => p.Name == profileData.PageType) ?? possiblePagers.FirstOrDefault(p => p.Name == ImageViewerModel.OrderedPager);

            Favorite = profileData.Favorite;
        }
    }
}
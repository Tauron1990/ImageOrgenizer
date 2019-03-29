using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.Core;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models.Helper
{
    public sealed class PagingHelper
    {
        private readonly ISettingsManager _settingsManager;
        private static readonly Logger Logger = LogManager.GetLogger(nameof(PagingHelper));

        public int CurrentIndex { get; private set; }
        private int _relativeIndex;
        private int _allImages;

        private Task<PagerOutput> _next;
        private Task<PagerOutput> _current;
        private Task<PagerOutput> _prevorius;

        private IList<ImageData> _currentImages;
        public IImagePager ImagePager { get; private set; }

        private int PageCount => _settingsManager.Settings?.PageCount ?? 20;

        public PagingHelper(ISettingsManager settingsManager) => _settingsManager = settingsManager;

        public void Initialize([CanBeNull] ProfileData data, IImagePager pager, int? allImages)
        {
            Logger.Info("Initialize Paging Helper");

            ImagePager = Argument.NotNull(pager, nameof(pager));

            if (allImages != null)
                _allImages = allImages.Value;
            if(data == null) return;

            CurrentIndex = data.CurrentImages;
            _relativeIndex = 0;//data.CurrentPosition;

            var (current, previous, next) = ImagePager.Initialize(data);

            _next = next;
            _current = current;
            _prevorius = previous;

            _currentImages = _current.Result.ImageData;
        }

        public ProfileData CreateProfileData(string filter, bool favorite) 
            => new ProfileData(CurrentIndex + PageCount, 0, filter, CurrentIndex, ImagePager.Name, favorite);

        public ImageData GetNext(bool favorite)
        {
            CurrentIndex++;
            _relativeIndex++;
            return GetCurrent(favorite);
        }

        public ImageData GetPrevorius(bool favorite)
        {
            CurrentIndex--;
            _relativeIndex--;
            return GetCurrent(favorite);
        }

        public ImageData GetCurrent(bool favorite)
        {
            if (_relativeIndex == -1)
            {
                _current = _prevorius;
                if (CurrentIndex < 0)
                    CurrentIndex = 0;
                var pageValue = _current.Result.Start - PageCount;
                if (CurrentIndex == 0)
                    pageValue = 0;

                _prevorius = ImagePager.GetPage(pageValue, favorite);

                _relativeIndex = _current.Result.ImageData.Count - 1;
                _currentImages = _current.Result.ImageData;
            }
            else if(_relativeIndex == _currentImages.Count)
            {
                if (_current.Result.SetNull || CurrentIndex >= _allImages)
                    CurrentIndex = 0;

                _current = _next;
                
                _next = ImagePager.GetPage(_current.Result.SetNull ? 0 : _current.Result.Start + PageCount, favorite);

                _relativeIndex = 0;
                _currentImages = _current.Result.ImageData;
            }

            if (_currentImages.Count == 0) return null;
            return _currentImages[_relativeIndex];    
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageOrganizer.BL;
using ImageOrganizer.Resources;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace ImageOrganizer.Views.Models
{
    [ExportModel(AppConststands.ImageManagerModel)]
    public class ImageViewerModel : ModelBase
    {
        private class OrderedPagerImpl : IImagePager
        {
            private readonly int _pageCount = Properties.Settings.Default.PageCount;
            private Func<IEnumerable<string>> _filterFunc;

            public PagerOutput GetCurrent(ProfileData data) => Operator.GetNextImages(new PagerInput(data.CurrentImages, _pageCount, data.Favorite, GetTagFilter())).Result;

            public Task<PagerOutput> GetPage(PageType type, int next, bool favorite)
            {
                switch (type)
                {
                    case PageType.Next:
                        return Operator.GetNextImages(new PagerInput(next, _pageCount, favorite, GetTagFilter()));
                    case PageType.Proverius:
                        return Operator.GetNextImages(new PagerInput(next, _pageCount, favorite, GetTagFilter()));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            public (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile)
            {
                var currentPage = Operator.GetNextImages(new PagerInput(profile.CurrentImages, _pageCount, profile.Favorite, GetTagFilter()));
                var previousPage = Operator.GetNextImages(new PagerInput(profile.CurrentImages - 1, _pageCount, profile.Favorite, GetTagFilter()));
                var nextPage = Operator.GetNextImages(new PagerInput(currentPage.Result.Next, _pageCount, profile.Favorite, GetTagFilter()));

                return (currentPage, previousPage, nextPage);
            }

            private IEnumerable<string> GetTagFilter() => _filterFunc == null ? Enumerable.Empty<string>() : _filterFunc();

            public void SetFilter(Func<IEnumerable<string>> filter) => _filterFunc = filter;

            public void IncreaseViewCount(ImageData data) => Operator.IncreaseViewCount(new IncreaseViewCountInput(data.Name, false));

            public Operator Operator { get; set; }
        }

        private class RandomPagerImpl : IImagePager
        {
            private readonly int _pageCount = Properties.Settings.Default.PageCount;
            private Func<IEnumerable<string>> _filterFunc;

            public (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile)
            {
                var currentPage = Operator.GetRandomImages(new PagerInput(profile.CurrentImages, _pageCount, profile.Favorite, GetTagFilter()));
                var previousPage = Operator.GetRandomImages(new PagerInput(profile.CurrentImages, _pageCount, profile.Favorite, GetTagFilter()));
                var nextPage = Operator.GetRandomImages(new PagerInput(currentPage.Result.Next, _pageCount, profile.Favorite, GetTagFilter()));

                return (currentPage, previousPage, nextPage);
            }

            public PagerOutput GetCurrent(ProfileData data) => throw new NotSupportedException();

            public Task<PagerOutput> GetPage(PageType type, int next, bool favorite) => Operator.GetRandomImages(new PagerInput(next, _pageCount, favorite, GetTagFilter()));

            private IEnumerable<string> GetTagFilter()
            {
                if (_filterFunc == null) return Enumerable.Empty<string>();

                return _filterFunc();
            }

            public void SetFilter(Func<IEnumerable<string>> filter) => _filterFunc = filter;

            public void IncreaseViewCount(ImageData data) => Operator.IncreaseViewCount(new IncreaseViewCountInput(data.Name, true));

            public Operator Operator { get; set; }
        }

        public const string OrderedPager = "ImageViewerModel_PagerName_Ordered";
        private const string RandomPager = "ImageViewerModel_PagerName_Random";

        private Dictionary<string, Lazy<IImagePager>> _imagePagers = new Dictionary<string, Lazy<IImagePager>>
        {
            {OrderedPager, new Lazy<IImagePager>(() => new OrderedPagerImpl())},
            {RandomPager, new Lazy<IImagePager>(() => new RandomPagerImpl())}
        };
        private IImagePager _imagePager;
        private Func<string> _navigatorTextFunc;


        //private int _currentImage;
        private int _currentImagePosition;

        private Task<PagerOutput> _currentPage;
              
        //private int _nextImage;
        private Task<PagerOutput> _nextPage;
        private Task<PagerOutput> _previousPage;
        private bool _favorite;
        private ImageData _currentImageData;
        private string _currentPager;

        [Inject]
        public Operator Operator { get; set; }

        private PossiblePager[] _possiblePagers;
        public IEnumerable<PossiblePager> ImagePagers => _possiblePagers ?? (_possiblePagers =
                                                             _imagePagers.Keys.Select(pagersKey => new PossiblePager(pagersKey, UIResources.ResourceManager.GetString(pagersKey))).ToArray());

        public string CurrentPager
        {
            get => _currentPager;
            set => SetPager(value);
        }

        public ImageData CurrentImage
        {
            get => _currentImageData;
            private set => SetProperty(ref _currentImageData, value);
        }

        public bool Favorite
        {
            get => _favorite;
            set => SetProperty(ref _favorite, value);
        }

        public void SetPager(string name)
        {
            if (name == null || !_imagePagers.ContainsKey(name))
                name = OrderedPager;

            if(CurrentPager == name) return;

            _imagePager = _imagePagers[name].Value;
            _currentPager = name;
            _imagePager.Operator = Operator;

            OnPropertyChanged(nameof(CurrentPager));
        }

        public void Initialize(ProfileData data, Func<string> navigatorTextFunc)
        {
            _navigatorTextFunc = navigatorTextFunc;

            SetPager(data.PageType);
            if(_imagePager == null)
                SetPager(OrderedPager);

            _currentImagePosition = data.CurrentPosition;
            //_currentImage = data.CurrentImages;
            //_nextImage = data.NextImages;

            var pages = _imagePager.Initialize(data);

            _currentPage = pages.Current;
            _nextPage = pages.Next;
            _previousPage = pages.Previous;

            CurrentImage = GetCurrentImage(() => NextAction(GetNext));
        }

        public void SetFilter(Func<IEnumerable<string>> filter) => _imagePager?.SetFilter(filter);

        public ImageData Next()
        {
            _currentImagePosition++;
            CurrentImage = GetCurrentImage(() => NextAction(GetNext));
            return CurrentImage;
        }

        public ImageData Previous()
        {
            _currentImagePosition--;
            CurrentImage = GetCurrentImage(() => NextAction(GetPrevious));
            return CurrentImage;
        }

        public ProfileData CreateProfileData() => new ProfileData(_currentPage.Result.Next, _currentImagePosition, _navigatorTextFunc(), _currentPage.Result.Start, CurrentPager, Favorite);

        public ImageData GetImageData(ProfileData data)
        {
            var pager = _imagePagers[OrderedPager].Value;
            var imageData = pager.GetCurrent(data).ImageData;
            var ele = imageData.ElementAtOrDefault(data.CurrentPosition);
            return ele ?? imageData.FirstOrDefault();
        }

        public void Shutdowm() => Task.WaitAll(_currentPage, _nextPage, _previousPage);

        public void IncreaseViewCount() => _imagePager.IncreaseViewCount(CurrentImage);

        private Task<PagerOutput> GetNext()
        {
            _currentImagePosition = 0;
            var temp = _nextPage;
            _nextPage = _imagePager.GetPage(PageType.Next, temp.Result.Next, Favorite);
            //_nextPage.ContinueWith(po => _nextImage = po.Result.Next);

            return temp;
        }

        private Task<PagerOutput> GetPrevious()
        {
            _currentImagePosition = _previousPage.Result.ImageData.Count - 1;
            var temp = _previousPage;
            _previousPage = _imagePager.GetPage(PageType.Proverius, temp.Result.Start - 1, Favorite);

            return temp;
        }

        private void NextAction(Func<Task<PagerOutput>> nextPager)
        {
            var nextTask = nextPager();
            //var next = nextTask.Result;

            //_currentImage = next.Next;
            _currentPage = nextTask;
        }

        private ImageData GetCurrentImage(Action nextAction)
        {
            var result = _currentPage.Result;
            if (result.ImageData.Count == 0) return null;

            if (_currentImagePosition >= result.ImageData.Count || _currentImagePosition < 0)
                nextAction();

            return _currentPage.Result.ImageData[_currentImagePosition];
        }
    }
}
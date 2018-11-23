using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageOrganizer;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [ExportModel(AppConststands.ImageManagerModel)]
    public sealed class ImageViewerModel : ModelBase
    {
        private abstract class ImagePagerBase : IImagePager
        {
            private readonly ISettingsManager _settingsManager = CommonApplication.Current.Container.Resolve<ISettingsManager>();

            protected int PageCount => _settingsManager.Settings?.PageCount ?? 20;

            public abstract (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile);
            public abstract PagerOutput GetCurrent(ProfileData data);
            public abstract Task<PagerOutput> GetPage(PageType type, int next, bool favorite);
            public abstract void SetFilter(Func<IEnumerable<string>> filter);
            public abstract void IncreaseViewCount(ImageData data);
            public abstract IOperator Operator { get; set; }
        }

        private class OrderedPagerImpl : ImagePagerBase
        {
            private Func<IEnumerable<string>> _filterFunc;

            public override PagerOutput GetCurrent(ProfileData data) => Operator.GetNextImages(new PagerInput(data.CurrentImages, PageCount, data.Favorite, GetTagFilter())).Result;

            public override Task<PagerOutput> GetPage(PageType type, int next, bool favorite)
            {
                switch (type)
                {
                    case PageType.Next:
                        return Operator.GetNextImages(new PagerInput(next, PageCount, favorite, GetTagFilter()));
                    case PageType.Proverius:
                        return Operator.GetNextImages(new PagerInput(next, PageCount, favorite, GetTagFilter()));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            public override (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile)
            {
                var currentPage = Operator.GetNextImages(new PagerInput(profile.CurrentImages, PageCount, profile.Favorite, GetTagFilter()));
                var previousPage = Operator.GetNextImages(new PagerInput(profile.CurrentImages - 1, PageCount, profile.Favorite, GetTagFilter()));
                var nextPage = Operator.GetNextImages(new PagerInput(currentPage.Result.Next, PageCount, profile.Favorite, GetTagFilter()));

                return (currentPage, previousPage, nextPage);
            }

            private IEnumerable<string> GetTagFilter() => _filterFunc == null ? Enumerable.Empty<string>() : _filterFunc();

            public override void SetFilter(Func<IEnumerable<string>> filter) => _filterFunc = filter;

            public override void IncreaseViewCount(ImageData data) => Operator.IncreaseViewCount(new IncreaseViewCountInput(data.Name, false));

            public override IOperator Operator { get; set; }
        }

        private class RandomPagerImpl : ImagePagerBase
        {
            private Func<IEnumerable<string>> _filterFunc;

            public override (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile)
            {
                var currentPage = Operator.GetRandomImages(new PagerInput(profile.CurrentImages, PageCount, profile.Favorite, GetTagFilter()));
                var previousPage = Operator.GetRandomImages(new PagerInput(profile.CurrentImages, PageCount, profile.Favorite, GetTagFilter()));
                var nextPage = Operator.GetRandomImages(new PagerInput(currentPage.Result.Next, PageCount, profile.Favorite, GetTagFilter()));

                return (currentPage, previousPage, nextPage);
            }

            public override PagerOutput GetCurrent(ProfileData data) => throw new NotSupportedException();

            public override Task<PagerOutput> GetPage(PageType type, int next, bool favorite) => Operator.GetRandomImages(new PagerInput(next, PageCount, favorite, GetTagFilter()));

            private IEnumerable<string> GetTagFilter() => _filterFunc == null ? Enumerable.Empty<string>() : _filterFunc();

            public override void SetFilter(Func<IEnumerable<string>> filter) => _filterFunc = filter;

            public override void IncreaseViewCount(ImageData data) => Operator.IncreaseViewCount(new IncreaseViewCountInput(data.Name, true));

            public override IOperator Operator { get; set; }
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
        public IOperator Operator { get; set; }

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        private PossiblePager[] _possiblePagers;
        private Func<IEnumerable<string>> _filter;

        public IEnumerable<PossiblePager> ImagePagers => _possiblePagers ?? (_possiblePagers =
                                                             _imagePagers.Keys.Select(pagersKey => new PossiblePager(pagersKey, UIResources.ResourceManager.GetString(pagersKey))).ToArray());

        public event EventHandler<EventArgs> ResetEvent;

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
            set => SetProperty(ref _favorite, value, OnResetEvent);
        }

        public void SetPager(string name, bool supressOnReset = false)
        {
            if (name == null || !_imagePagers.ContainsKey(name))
                name = OrderedPager;

            if(CurrentPager == name) return;

            _imagePager = _imagePagers[name].Value;
            _currentPager = name;
            _imagePager.Operator = Operator;
            _imagePager.SetFilter(_filter);

            OnPropertyChanged(nameof(CurrentPager));
            if(!supressOnReset)
                OnResetEvent();
        }

        public void Initialize(ProfileData data, Func<string> navigatorTextFunc)
        {
            _navigatorTextFunc = navigatorTextFunc;


            SetPager(data.PageType, true);
            if(_imagePager == null)
                SetPager(OrderedPager, true);

            _currentImagePosition = data.CurrentPosition;
            //_currentImage = data.CurrentImages;
            //_nextImage = data.NextImages;

            var pages = _imagePager.Initialize(data);

            _currentPage = pages.Current;
            _nextPage = pages.Next;
            _previousPage = pages.Previous;

            CurrentImage = GetCurrentImage(() => NextAction(GetNext));
        }

        public void SetFilter(Func<IEnumerable<string>> filter)
        {
            _filter = filter;
            _imagePager?.SetFilter(filter);
        }

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

        public ProfileData CreateProfileData(bool pageZero = false) => 
            pageZero
            ? new ProfileData(1, 0, _navigatorTextFunc(), 0, CurrentPager, Favorite)
            : new ProfileData(_currentPage.Result.Next, _currentImagePosition, _navigatorTextFunc(), _currentPage.Result.Start, CurrentPager, Favorite);

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

        private void OnResetEvent() => ResetEvent?.Invoke(this, EventArgs.Empty);

        public void OpenUrl() => ProviderManager.Get(CurrentImage.ProviderName).ShowUrl(CurrentImage.Name);
    }
}
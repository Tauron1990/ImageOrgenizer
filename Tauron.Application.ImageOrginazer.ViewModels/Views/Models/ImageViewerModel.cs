using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrganizer.BL.Services;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models.Helper;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [ExportModel(AppConststands.ImageManagerModel)]
    public sealed class ImageViewerModel : ModelBase
    {
        private static readonly ISettingsManager SettingsManager = CommonApplication.Current.Container.Resolve<ISettingsManager>();

        private static int PageCount => SettingsManager.Settings?.PageCount ?? 20;

        private static readonly Logger PagerLogger = GlobalLogConststands.PagerLogger;

        private abstract class ImagePagerBase : IImagePager
        {
            public abstract string Name { get; }
            public abstract (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile);
            public abstract PagerOutput GetCurrent(ProfileData data);
            public abstract Task<PagerOutput> GetPage(int next, bool favorite);
            public abstract void SetFilter(Func<IEnumerable<string>> filter);
            public abstract void IncreaseViewCount(ImageData data);
            public abstract IImageService Operator { get; set; }
        }

        private class OrderedPagerImpl : ImagePagerBase
        {
            private Func<IEnumerable<string>> _filterFunc;

            public override PagerOutput GetCurrent(ProfileData data) => Operator.GetNextImages(new PagerInput(data.CurrentImages, PageCount, data.Favorite, GetTagFilter()));

            public override Task<PagerOutput> GetPage(int next, bool favorite)
            {
                PagerLogger.Trace($"Next Page: {next} -- Favorite: {favorite}");

                return Task.Run( () => Operator.GetNextImages(new PagerInput(next, PageCount, favorite, GetTagFilter())));
            }

            public override string Name { get; } = OrderedPager;

            public override (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile)
            {
                var currentPage = Task.Run(() => Operator.GetNextImages(new PagerInput(profile.CurrentImages, PageCount, profile.Favorite, GetTagFilter())));
                var previousPage = Task.Run(() => Operator.GetNextImages(new PagerInput(currentPage.Result.Start - PageCount, PageCount, profile.Favorite, GetTagFilter())));
                var nextPage = Task.Run(() => Operator.GetNextImages(new PagerInput(currentPage.Result.Start + PageCount, PageCount, profile.Favorite, GetTagFilter())));

                return (currentPage, previousPage, nextPage);
            }

            private IEnumerable<string> GetTagFilter() => _filterFunc == null ? Enumerable.Empty<string>() : _filterFunc();

            public override void SetFilter(Func<IEnumerable<string>> filter) => _filterFunc = filter;

            public override void IncreaseViewCount(ImageData data) => Operator.IncreaseViewCount(new IncreaseViewCountInput(data.Name, false));

            public override IImageService Operator { get; set; }
        }

        private class RandomPagerImpl : ImagePagerBase
        {
            private Func<IEnumerable<string>> _filterFunc;

            public override string Name { get; } = RandomPager;

            public override (Task<PagerOutput> Current, Task<PagerOutput> Previous, Task<PagerOutput> Next) Initialize(ProfileData profile)
            {
                var currentPage = Task.Run(() => Operator.GetRandomImages(new PagerInput(profile.CurrentImages, PageCount, profile.Favorite, GetTagFilter())));
                var previousPage = Task.Run(() => Operator.GetRandomImages(new PagerInput(profile.CurrentImages, PageCount, profile.Favorite, GetTagFilter())));
                var nextPage = Task.Run(() => Operator.GetRandomImages(new PagerInput(currentPage.Result.Start, PageCount, profile.Favorite, GetTagFilter())));

                return (currentPage, previousPage, nextPage);
            }

            public override PagerOutput GetCurrent(ProfileData data) => throw new NotSupportedException();

            public override Task<PagerOutput> GetPage(int next, bool favorite) => Task.Run(() => Operator.GetRandomImages(new PagerInput(next, PageCount, favorite, GetTagFilter())));

            private IEnumerable<string> GetTagFilter() => _filterFunc == null ? Enumerable.Empty<string>() : _filterFunc();

            public override void SetFilter(Func<IEnumerable<string>> filter) => _filterFunc = filter;

            public override void IncreaseViewCount(ImageData data) => Operator.IncreaseViewCount(new IncreaseViewCountInput(data.Name, true));

            public override IImageService Operator { get; set; }
        }

        public const string OrderedPager = "ImageViewerModel_PagerName_Ordered";
        public const string RandomPager = "ImageViewerModel_PagerName_Random";

        private Dictionary<string, Lazy<IImagePager>> _imagePagers = new Dictionary<string, Lazy<IImagePager>>
        {
            {OrderedPager, new Lazy<IImagePager>(() => new OrderedPagerImpl())},
            {RandomPager, new Lazy<IImagePager>(() => new RandomPagerImpl())}
        };

        private Func<string> _navigatorTextFunc;

        private PagingHelper _pagingHelper;

        private bool _favorite;
        private ImageData _currentImageData;
        private string _currentPager;

        [Inject]
        public IImageService Operator { get; set; }

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        private PossiblePager[] _possiblePagers;
        private Func<IEnumerable<string>> _filter;
        private int _page;
        private int _index;

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

        public int Page
        {
            get => _page;
            set => SetProperty(ref _page, value);
        }

        public int Index
        {
            get => _index;
            set => SetProperty(ref  _index, value);
        }

        private IImagePager GetPager(string name)
        {
            if (name == null || !_imagePagers.ContainsKey(name))
                name = OrderedPager;

            var op = _imagePagers[name].Value;
            op.Operator = Operator;
            op.SetFilter(_filter);

            return op;
        }

        private bool _supressOnReset;

        public void SetPager(string name)
        {
            if (CurrentPager == name) return;

            var imagePager = GetPager(name);
            _currentPager = name;
            
            _pagingHelper.Initialize(null, imagePager, null);

            OnPropertyChanged(nameof(CurrentPager));
            if(!_supressOnReset)
                OnResetEvent();
        }

        public void Initialize(ProfileData data, Func<string> navigatorTextFunc)
        {
            try
            {
                _supressOnReset = true;
                _navigatorTextFunc = navigatorTextFunc;

                Favorite = data.Favorite;
                SetPager(data.PageType);
                _pagingHelper.Initialize(data, GetPager(data.PageType), Operator.GetImageCount());

                CurrentImage = _pagingHelper.GetCurrent(Favorite);
                SetPage();
            }
            finally
            {
                _supressOnReset = false;
            }
        }

        public void SetFilter(Func<IEnumerable<string>> filter)
        {
            _filter = filter;
            _pagingHelper?.ImagePager?.SetFilter(filter);
        }

        private void SetPage()
        {
            Index = _pagingHelper.CurrentIndex + 1;
            Page = Index / PageCount + 1;
        }

        public ImageData Next()
        {
            PagerLogger.Trace("Start: Next");
            
            CurrentImage = _pagingHelper.GetNext(Favorite);
            SetPage();

            PagerLogger.Trace("End: Next");

            return CurrentImage;
        }

        public ImageData Previous()
        {
            PagerLogger.Trace("Start: Previous");
            
            CurrentImage = _pagingHelper.GetPrevorius(Favorite);
            SetPage();

            PagerLogger.Trace("End: Previous");

            return CurrentImage;
        }

        public ProfileData CreateProfileData(bool pageZero = false) => 
            pageZero
            ? new ProfileData(20, 0, _navigatorTextFunc(), 0, CurrentPager, Favorite)
            : _pagingHelper.CreateProfileData(_navigatorTextFunc(), Favorite);

        public ImageData GetImageData(ProfileData data) => _imagePagers[OrderedPager].Value.GetPage(data.CurrentImages, data.Favorite).Result.ImageData.FirstOrDefault();


        public void IncreaseViewCount() => _pagingHelper.ImagePager.IncreaseViewCount(CurrentImage);
        
        private void OnResetEvent() => ResetEvent?.Invoke(this, EventArgs.Empty);

        public void OpenUrl() => ProviderManager.Get(CurrentImage.ProviderName).ShowUrl(CurrentImage.Name);

        public override void BuildCompled() => _pagingHelper = new PagingHelper(SettingsManager);
    }
}
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.OnlineViewer)]
    public sealed class OnlineViewerViewModel : MainViewControllerBase
    {
        public class FetcherType
        {
            public string DisplayName { get; }

            public IViewFetcher ViewFetcher { get; }

            public FetcherType(string displayName, IViewFetcher viewFetcher)
            {
                DisplayName = displayName;
                ViewFetcher = viewFetcher;
            }
        }

        private bool _canNext;

        private bool _canBack;

        public UIObservableCollection<PageEntrie> Entries { get; } = new UIObservableCollection<PageEntrie>();

        public UIObservableCollection<FetcherType> FetcherTypes { get; } = new UIObservableCollection<FetcherType>();

        public FetcherType SelectFetcherType
        {
            get => GetProperty<FetcherType>();
            set => SetProperty(value);
        }

        public int CurrentPage
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public BorderHelper BorderBrushHelper { get; private set; }

        [Inject]
        public IClipboardManager ClipboardManager { get; set; }

        [Inject]
        public IDownloadManager DownloadManager { get; set; }

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        [Inject]
        public IDBSettings DbSettings { get; set; }

        [InjectModel(AppConststands.OnlineViewerModel)]
        public OnlineViewerModel FetcherModel { get; set; }

        public override string ProgrammTitle { get; } = UIResources.OnlineViewerView_Title;

        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }

        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;

        public override void OnClick() => MainWindowViewModel.ShowImagesAction();

        public override void EnterView()
        {
            Entries.AddRange(ExtractEntries(FetcherModel.GetActual()));

            FetcherModel.PageingStade += OnFetcherModelOnPageingStade;
            OnFetcherModelOnPageingStade(FetcherModel.PagingStade);

            FetcherModel.FetchingCompled += FetcherModelOnFetchingCompled;
            FetcherModel.PageEntrieList.CollectionChanged += PageEntrieListOnCollectionChanged;

            FetcherTypes.AddRange(ProviderManager.GetFetchers().Select(vF => new FetcherType(vF.Name, vF)));
            SelectFetcherType = FetcherTypes.FirstOrDefault(f => f.ViewFetcher == FetcherModel.ViewFetcher) ?? FetcherTypes.First();

            DownloadManager.Pause();
            BorderBrushHelper.Reset();
            base.EnterView();
        }

        private void OnFetcherModelOnPageingStade((bool CanBack, bool CanNext) stade)
        {
            var (canBack, canNext) = stade;

            _canBack = canBack;
            _canNext = canNext;

            InvalidateRequerySuggested();
        }

        private void PageEntrieListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(Entries.Count == 0)
                Entries.AddRange(ExtractEntries(FetcherModel.GetActual()));
        }

        private void FetcherModelOnFetchingCompled() 
            => InvalidateRequerySuggested();

        public override void ExitView()
        {
            FetcherModel.FetchingCompled -= FetcherModelOnFetchingCompled;
            FetcherModel.PageEntrieList.CollectionChanged -= PageEntrieListOnCollectionChanged;

            if (FetcherModel.IsNotFetching)
                DownloadManager.Start();
            BorderBrushHelper.Clear();

            if (!_canNext && FetcherModel.IsNotFetching && FetcherModel.Collector != null)
            {
                Log.Info("Online View Fetching Compled");
                FetcherModel.Collector.FetchCompled();
                FetcherModel.Clear();
            }

            base.ExitView();
        }

        [CommandTarget]
        public void NextPage()
        {
            Entries.Clear();
            Entries.AddRange(ExtractEntries(FetcherModel.GetNext()));
        }

        [CommandTarget]
        public bool CanNextPage()
            => _canNext;

        [CommandTarget]
        public void PrevoriusPage()
        {
            Entries.Clear();
            Entries.AddRange(ExtractEntries(FetcherModel.GetPrevorius()));
        }

        [CommandTarget]
        public bool CanPrevoriusPage()
            => _canBack;

        [CommandTarget]
        public void StartFetch() => FetcherModel.StartFetching(SelectFetcherType.ViewFetcher);

        [CommandTarget]
        public void StopFetch()
        {
            FetcherModel.StopFetching();
            Entries.Clear();
        }

        private IEnumerable<PageEntrie> ExtractEntries((IEnumerable<PageEntrie> Images, int Page) entrieTuple)
        {
            var (images, page) = entrieTuple;

            CurrentPage = page;
            return images;
        }

        public override void BuildCompled()
        {
            BorderBrushHelper = new BorderHelper(DbSettings);
            base.BuildCompled();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Tauron.Application.Commands;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.OnlineViewer)]
    public sealed class OnlineViewerViewModel : MainViewControllerBase, IDisposable
    {
        private sealed class PagingHelper
        {
            private const int PageCount = 20;

            public event Action<(bool CanBack, bool CanNext)> PageingStade;

            private readonly ObservableCollection<PageEntrie> _entries;

            private int _currentPosition;

            public PagingHelper(ObservableCollection<PageEntrie> entries)
            {
                _entries = entries;
                _entries.CollectionChanged += EntriesOnCollectionChanged;
            }

            private void EntriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
                => OnPageingStade((_currentPosition > 0, _currentPosition < _entries.Count));

            public IEnumerable<PageEntrie> GetNext()
            {
                var temp = _entries.Skip(_currentPosition).Take(PageCount);
                _currentPosition += PageCount;

                OnPageingStade((true, _currentPosition < _entries.Count));

                return temp;
            }

            public IEnumerable<PageEntrie> GetPrevorius()
            {
                _currentPosition -= PageCount;

                var temp = _entries.Skip(_currentPosition).Take(PageCount);

                OnPageingStade((_currentPosition > 0, true));

                return temp;
            }

            public void Reset()
            {
                _currentPosition = 0;
                OnPageingStade((false, _entries.Count > 0));

            }

            private void OnPageingStade((bool CanBack, bool CanNext) obj) => PageingStade?.Invoke(obj);
        }

        public sealed class PageEntrieList : ObservableCollection<PageEntrie>
        {
            private bool _blocked;

            public IDisposable Block()
            {
                _blocked = true;
                return new ActionDispose(() =>
                {
                    _blocked = false;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            }

            public void AddRange(IEnumerable<PageEntrie> entries)
            {
                foreach (var pageEntry in entries) Add(pageEntry);
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if(_blocked) return;
                base.OnCollectionChanged(e);
            }

            protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if(_blocked) return;
                base.OnPropertyChanged(e);
            }
        }

        public class PageEntrie
        {
            private readonly IClipboardManager _manager;

            public byte[] Source { get; }

            public string Link { get; }

            public ICommand Click { get; }

            public string Info { get; }

            public PageEntrie(byte[] source, string link, IClipboardManager manager, string info)
            {
                _manager = manager;
                Info = info;
                Source = source;
                Link = link;
                Click = new SimpleCommand(OnClick);
            }

            private void OnClick(object obj) => _manager.SetValue(Link);
        }

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

        private PageEntrieList _pageEntries = new PageEntrieList();

        private PagingHelper _pagingHelper;

        private bool _canNext;

        private bool _canBack;

        private ManualResetEvent _stopWatch;

        private int _stop;

        public UIObservableCollection<PageEntrie> Entries { get; } = new UIObservableCollection<PageEntrie>();

        public UIObservableCollection<FetcherType> FetcherTypes { get; } = new UIObservableCollection<FetcherType>();

        public FetcherType SelectFetcherType
        {
            get => GetProperty<FetcherType>();
            set => SetProperty(value);
        }

        public string ActualError
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string Delayed
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Inject]
        public IClipboardManager ClipboardManager { get; set; }

        [Inject]
        public IDownloadManager DownloadManager { get; set; }

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        [Inject]
        public IDBSettings DbSettings { get; set; }

        public OnlineViewerViewModel()
        {
            _pagingHelper = new PagingHelper(_pageEntries);
            _pagingHelper.PageingStade += stade =>
            {
                _canBack = stade.CanBack;
                _canNext = stade.CanNext;

                InvalidateRequerySuggested();
            };
        }

        public override string ProgrammTitle { get; } = UIResources.OnlineViewerView_Title;

        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }

        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;

        public override void OnClick() => MainWindowViewModel.ShowImagesAction();

        public bool IsFetching
        {
            get => GetProperty<bool>();
            set => SetProperty(value, () => IsFetching = !value);
        }

        public bool IsNotFetching
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }
        
        public override void EnterView()
        {
            FetcherTypes.Clear();
            var old = SelectFetcherType;
            FetcherTypes.AddRange(ProviderManager.GetFetchers().Select(vF => new FetcherType(vF.Name, vF)));
            SelectFetcherType = FetcherTypes.FirstOrDefault(f => f.DisplayName == old?.DisplayName) ?? FetcherTypes.First();
            
            DownloadManager.Pause();
            base.EnterView();
        }

        public override void ExitView()
        {
            FetcherTypes.Clear();
            SelectFetcherType = null;
            ActualError = null;
            Delayed = null;

            StopFetchingInternal();
            IsFetching = false;
            _pageEntries.Clear();
            Entries.Clear();
            _pagingHelper.Reset();

            DownloadManager.Start();
            base.ExitView();
        }

        [CommandTarget]
        public void NextPage()
        {
            Entries.Clear();
            Entries.AddRange(_pagingHelper.GetNext());
        }

        [CommandTarget]
        public bool CanNextPage() 
            => _canNext && IsFetching;

        [CommandTarget]
        public void PrevoriusPage()
        {
            Entries.Clear();
            Entries.AddRange(_pagingHelper.GetPrevorius());
        }

        [CommandTarget]
        public bool CanPrevoriusPage()
            => _canBack && IsFetching;

        [CommandTarget]
        public void StartFetch() => StartFetchingInternal();

        [CommandTarget]
        public void StopFetch()
        {
            StopFetchingInternal();
            _pageEntries.Clear();
            Entries.Clear();
            _pagingHelper.Reset();
        }

        private void StopFetchingInternal()
        {
            if(IsNotFetching) return;

            _stopWatch.Reset();
            Interlocked.Exchange(ref _stop, 1);
            _stopWatch.WaitOne();
        }

        private void StartFetchingInternal()
        {
            if(IsFetching) return;

            Interlocked.Exchange(ref _stop, 0);

            _pageEntries.Clear();
            Entries.Clear();

            IsFetching = true;
            InvalidateRequerySuggested();
            Task.Run(Fetcher);
        }

        private void Fetcher()
        {
            try
            {
                var fetcher = SelectFetcherType.ViewFetcher;
                var value = DbSettings.FetcherData.TryGetOrDefault(fetcher.Id);

                if(!fetcher.IsValidLastValue(ref value))
                {
                    if(_stop == 1) return;
                    var text = Dialogs.GetText(MainWindow, UIResources.OnlineViewView_Label_LastSelect_Instraction,
                        null, UIResources.OnlineViewView_Label_LastSelect_Caption, true, null);

                    if(string.IsNullOrEmpty(text)) return;
                }

                FetcherResult fetcherResult = null;
                if (_stop == 1) return;

                do
                {
                    bool first = fetcherResult == null;
                    fetcherResult = fetcher.GetNext(fetcherResult?.Next, value);

                    if(_stop == 1) return;
                    if (!fetcherResult.Sucseeded)
                    {
                        var localResult = fetcherResult;
                        if (first)
                            fetcherResult = null;

                        ActualError = localResult.Error;
                        if (localResult.Delay)
                        {
                            DateTime target = DateTime.Now.AddMinutes(5);
                            Delayed = string.Format(UIResources.OnlineViewerView_Delayed_Template,
                                target.ToString("T", CultureInfo.CurrentUICulture));

                            if (_stop == 1) return;

                            do
                            {
                                Thread.Sleep(1000);
                                if (_stop == 1) return;
                                
                            } while (target > DateTime.Now);

                            Delayed = null;
                        }
                    }
                    else
                    {
                        ActualError = null;

                        if(first)
                            DbSettings.FetcherData[fetcher.Id] = fetcherResult.Last;

                        using (_pageEntries.Block())
                            _pageEntries.AddRange(fetcherResult.Images.Select(fi => new PageEntrie(fi.Image, fi.Link, ClipboardManager, fi.Info)));

                        if(first)
                            NextPage();
                    }


                } while (fetcherResult == null || !fetcherResult.LastArrived);
            }
            finally
            {
                _stopWatch.Set();
                IsFetching = false;
                InvalidateRequerySuggested();
            }
        }

        public void Dispose() 
            => _stopWatch.Dispose();
    }
}

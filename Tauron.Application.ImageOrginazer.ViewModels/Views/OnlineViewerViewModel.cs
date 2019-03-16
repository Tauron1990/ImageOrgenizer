using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
            public const int PageCount = 20;

            public event Action<(bool CanBack, bool CanNext)> PageingStade;

            private readonly ObservableCollection<PageEntrie> _entries;

            private int _currentPosition;

            public PagingHelper(ObservableCollection<PageEntrie> entries)
            {
                _entries = entries;
                _entries.CollectionChanged += EntriesOnCollectionChanged;
            }

            private void EntriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
                => OnPageingStade((_currentPosition - PageCount > 0, _currentPosition < _entries.Count));

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

                OnPageingStade((_currentPosition - PageCount > 0, true));

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

        public sealed class BorderHelper : ObservableObject
        {
            private readonly IDBSettings _settings;
            private static char[] _sepreator = {','};

            private string _blacklist;
            private string _whiteList;

            public BorderHelper(IDBSettings settings) => _settings = settings;

            public void Reset()
            {
                Blacklist = _settings.BlacklistTags;
                WhiteList = _settings.WhitelistTags;
            }

            public void Clear()
            {
                _blacklist = null;
                _whiteList = null;
                BlackTags.Clear();
                WhiteTags.Clear();
            }

            public string Blacklist
            {
                get => _blacklist;
                set => SetProperty(ref _blacklist, value, BlacklistChanged);
            }

            private void BlacklistChanged()
            {
                _settings.BlacklistTags = _blacklist;
                var tags = _blacklist.Split(_sepreator, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                BlackTags.Clear();
                BlackTags.AddRange(tags);
            }

            public string WhiteList
            {
                get => _whiteList;
                set => SetProperty(ref _whiteList, value, WhiteListChanged);
            }

            private void WhiteListChanged()
            {
                _settings.WhitelistTags = _whiteList;
                var tags = _whiteList.Split(_sepreator, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                WhiteTags.Clear();
                WhiteTags.AddRange(tags);
            }

            private List<string> WhiteTags { get; } = new List<string>();

            private List<string> BlackTags { get; } = new List<string>();

            public string GetBorderColor(string info)
            {
                bool white = WhiteTags.All(info.Contains);

                bool black = BlackTags.Any(info.Contains);

                if (black) return "Red";
                return white ? "Green" : "Black";
            }
        }

        public sealed class PageEntrie
        {
            private readonly IClipboardManager _manager;
            private readonly BorderHelper _helper;

            public byte[] Source { get; }

            public string Link { get; }

            public ICommand Click { get; }

            public ICommand OpenClick { get; }

            public string Info { get; }

            public string BorderBrush => _helper.GetBorderColor(Info);

            public PageEntrie(byte[] source, string link, IClipboardManager manager, string info, BorderHelper helper)
            {
                _manager = manager;
                _helper = helper;
                Info = info;
                Source = source;
                Link = link;
                Click = new SimpleCommand(OnClick);
                OpenClick = new SimpleCommand(OnOpenClick);
            }

            private void OnOpenClick(object obj)
            {
                try
                {
                    var ps = new ProcessStartInfo(Link)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };

                    Process.Start(ps)?.Dispose();
                }
                catch (Win32Exception)
                {
                    Process.Start("IExplore.exe", Link)?.Dispose();
                }
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

        private readonly ManualResetEvent _stopWatch = new ManualResetEvent(true);

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

        public BorderHelper BorderBrushHelper { get; private set; }

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
            set => SetProperty(value, () => IsNotFetching = !value);
        }

        public bool IsNotFetching
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }
        
        public override void EnterView()
        {
            IsFetching = false;

            FetcherTypes.Clear();
            var old = SelectFetcherType;
            FetcherTypes.AddRange(ProviderManager.GetFetchers().Select(vF => new FetcherType(vF.Name, vF)));
            SelectFetcherType = FetcherTypes.FirstOrDefault(f => f.DisplayName == old?.DisplayName) ?? FetcherTypes.First();
            
            DownloadManager.Pause();
            BorderBrushHelper.Reset();
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
            BorderBrushHelper.Clear();
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
            IsFetching = true;

            Interlocked.Exchange(ref _stop, 0);

            _pageEntries.Clear();
            Entries.Clear();
            InvalidateRequerySuggested();
            Task.Run(Fetcher);
        }

        private void Fetcher()
        {
            try
            {
                LinkedList<PageEntrie> localEntries = new LinkedList<PageEntrie>();
                var fetcher = SelectFetcherType.ViewFetcher;
                var value = DbSettings.FetcherData.TryGetOrDefault(fetcher.Id);

                if(!fetcher.IsValidLastValue(ref value))
                {
                    if(_stop == 1) return;
                    var text = Dialogs.GetText(MainWindow, UIResources.OnlineViewView_Label_LastSelect_Instraction,
                        null, UIResources.OnlineViewView_Label_LastSelect_Caption, true, null).Trim();

                    if(!fetcher.IsValidLastValue(ref text)) return;

                    value = text;
                }

                FetcherResult fetcherResult = null;
                if (_stop == 1) return;
                int page = 0;

                do
                {
                    page++;
                    ActualError = UIResources.OnlineViewerView_Fetcher_Page + page;
                    bool first = fetcherResult == null;
                    fetcherResult = fetcher.GetNext(fetcherResult?.Next, value);

                    if(_stop == 1) return;
                    if (!fetcherResult.Sucseeded)
                    {
                        var localResult = fetcherResult;
                        if (first)
                            fetcherResult = null;

                        ActualError = localResult.Error;
                        if (!localResult.Delay) continue;

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
                    else
                    {
                        ActualError = null;
                        if (_stop == 1) return;

                        if (first)
                            DbSettings.FetcherData[fetcher.Id] = fetcherResult.Last;

                        if (_stop == 1) return;

                        foreach (var pageEntry in fetcherResult.Images.Select(fi => new PageEntrie(fi.Image, fi.Link, ClipboardManager, fi.Info, BorderBrushHelper)))
                            localEntries.AddFirst(pageEntry);
                        
                        if (localEntries.Count >= PagingHelper.PageCount || fetcherResult.LastArrived)
                        {
                            using (_pageEntries.Block())
                            {
                                for (int i = 0; i < PagingHelper.PageCount; i++)
                                {
                                    if (_stop == 1) return;

                                    if (localEntries.Count == 0) break;
                                    var node = localEntries.Last;
                                    localEntries.RemoveLast();
                                    _pageEntries.Add(node.Value);
                                }
                            }
                        }

                        if (_stop == 1) return;
                        if (first)
                            NextPage();
                    }


                } while (fetcherResult == null || !fetcherResult.LastArrived);
            }
            finally
            {
                bool stopped = _stop == 1;

                ActualError = stopped
                    ? UIResources.OnlineViewerView_Fetcher_Stopped
                    : UIResources.OnlineViewerView_Fetcher_Finished;

                _stopWatch.Set();
                IsFetching = false;
                InvalidateRequerySuggested();
            }
        }

        public void Dispose() 
            => _stopWatch.Dispose();

        public override void BuildCompled()
        {
            BorderBrushHelper = new BorderHelper(DbSettings);
            base.BuildCompled();
        }
    }
}

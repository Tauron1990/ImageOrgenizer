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
using NLog;
using Tauron.Application.Commands;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    public sealed class BorderHelper : ObservableObject
    {
        private readonly IDBSettings _settings;
        private static char[] _sepreator = { ',' };

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
            bool white = WhiteTags.Any(info.Contains);

            bool black = BlackTags.Any(info.Contains);

            if (black) return "Red";
            return white ? "Green" : "Black";
        }
    }

    public sealed class PageEntrie
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
                try
                {
                    Process.Start("IExplore.exe", Link)?.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private void OnClick(object obj)
        {
            while (true)
            {
                int errorCount = 0;
                try
                {
                    _manager.SetValue(Link);
                    break;
                }
                catch (Exception e)
                {
                    if (errorCount == 10)
                    {
                        Logger.Error(e);
                        break;
                    }
                    // ReSharper disable once RedundantAssignment
                    errorCount++;
                }
            }
        }
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
            if (_blocked) return;
            base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_blocked) return;
            base.OnPropertyChanged(e);
        }
    }

    [ExportModel(AppConststands.OnlineViewerModel)]
    public class OnlineViewerModel : ModelBase, IDisposable
    {
        public const int PageCount = 20;

        private sealed class PagingHelper
        {
            public event Action<(bool CanBack, bool CanNext)> PageingStade;

            private readonly ObservableCollection<PageEntrie> _entries;

            private int _currentPosition;

            public PagingHelper(ObservableCollection<PageEntrie> entries)
            {
                _entries = entries;
                _entries.CollectionChanged += EntriesOnCollectionChanged;
            }

            private void EntriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                => OnPageingStade(GetStade());

            public (IEnumerable<PageEntrie> Images, int Page) GetActual() => (_entries.Skip(_currentPosition * PageCount).Take(PageCount), _currentPosition);

            public void SetNext()
            {
                if(_entries.Count == 0) return;

                _currentPosition += 1;
                OnPageingStade(GetStade());
            }

            public void SetPrevorius()
            {
                if (_entries.Count == 0) return;

                _currentPosition -= 1;
                OnPageingStade(GetStade());
            }

            public void Reset()
            {
                _currentPosition = 1;
                OnPageingStade(GetStade());

            }

            private void OnPageingStade((bool CanBack, bool CanNext) obj) => PageingStade?.Invoke(obj);

            public (bool CanBack, bool CanNext) GetStade()
                => (_currentPosition > 0, _currentPosition < GetPages());

            private int GetPages()
            {
                int pages = _entries.Count / PageCount;
                int rest = _entries.Count % PageCount;

                if (rest > 0)
                    pages++;

                return pages;
            }
        }

        private PagingHelper _pagingHelper;

        private int _stop;

        public event Action<(bool CanBack, bool CanNext)> PageingStade
        {
            add => _pagingHelper.PageingStade += value;
            remove => _pagingHelper.PageingStade -= value;
        }

        public IViewFetcher ViewFetcher { get; private set; }

        private readonly ManualResetEvent _stopWatch = new ManualResetEvent(true);

        public PageEntrieList PageEntrieList { get; } = new PageEntrieList();

        public event Action FetchingCompled;

        [Inject]
        public IDownloadManager DownloadManager { get; set; }

        [Inject]
        public IClipboardManager ClipboardManager { get; set; }

        [Inject]
        public IDBSettings DbSettings { get; set; }

        [Inject]
        public IDialogFactory Dialogs { get; set; }

        public BorderHelper BorderHelper { get; set; }

        public (bool CanBack, bool CanNext) PagingStade => _pagingHelper.GetStade();

        public string ActualError
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string FetchedPage
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string Delayed
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

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

        public OnlineViewerModel() 
            => IsFetching = false;

        public void StopFetching()
        {
            if (IsNotFetching) return;

            _stopWatch.Reset();
            Interlocked.Exchange(ref _stop, 1);
            _stopWatch.WaitOne();

            _pagingHelper.Reset();
        }

        public void StartFetching(IViewFetcher viewFetcher)
        {
            if (IsFetching) return;
            IsFetching = true;
            ViewFetcher = viewFetcher;

            Interlocked.Exchange(ref _stop, 0);
            
            PageEntrieList.Clear();
            Task.Run(Fetcher);
        }

        private void Fetcher()
        {
            if(ViewFetcher == null) return;

            try
            {
                LinkedList<PageEntrie> localEntries = new LinkedList<PageEntrie>();
                var fetcher = ViewFetcher;
                var value = DbSettings.FetcherData.TryGetOrDefault(fetcher.Id);
                
                if (!fetcher.IsValidLastValue(ref value))
                {
                    if (_stop == 1) return;
                    var text = Dialogs.GetText(CommonApplication.Current.MainWindow, UIResources.OnlineViewView_Label_LastSelect_Instraction,
                        null, UIResources.OnlineViewView_Label_LastSelect_Caption, true, null)?.Trim();

                    if (!fetcher.IsValidLastValue(ref text)) return;

                    value = text;
                }

                FetcherResult fetcherResult = null;
                if (_stop == 1) return;
                int page = 0;

                do
                {
                    ActualError = null;

                    bool first = fetcherResult == null;
                    fetcherResult = fetcher.GetNext(fetcherResult?.Next, value);

                    if (_stop == 1) return;
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
                        page++;
                        FetchedPage = UIResources.OnlineViewerView_Fetcher_Page + page;

                        if (_stop == 1) return;

                        if (first)
                            DbSettings.FetcherData[fetcher.Id] = fetcherResult.Last;

                        if (_stop == 1) return;

                        foreach (var pageEntry in fetcherResult.Images.Select(fi => new PageEntrie(fi.Image, fi.Link, ClipboardManager, fi.Info, BorderHelper)))
                            localEntries.AddFirst(pageEntry);

                        if (localEntries.Count >= PageCount || fetcherResult.LastArrived)
                        {
                            using (PageEntrieList.Block())
                            {
                                for (int i = 0; i < PageCount; i++)
                                {
                                    if (_stop == 1) return;

                                    if (localEntries.Count == 0) break;
                                    var node = localEntries.Last;
                                    localEntries.RemoveLast();
                                    PageEntrieList.Add(node.Value);
                                }
                            }
                        }

                        if (_stop == 1) return;
                        if(DbSettings.MaxOnlineViewerPage < page) return;
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

                FetchingCompled?.Invoke();
                DownloadManager.Start();
            }
        }

        public override void BuildCompled()
        {
            BorderHelper = new BorderHelper(DbSettings);
            BorderHelper.Reset();
            _pagingHelper = new PagingHelper(PageEntrieList);

            base.BuildCompled();
        }

        public void Dispose() => _stopWatch.Dispose();

        public (IEnumerable<PageEntrie> Images, int Page) GetNext()
        {
            _pagingHelper.SetNext();
            return _pagingHelper.GetActual();
        }

        public (IEnumerable<PageEntrie> Images, int Page) GetPrevorius()
        {
            _pagingHelper.SetPrevorius();
            return _pagingHelper.GetActual();
        }

        public (IEnumerable<PageEntrie> Images, int Page) GetActual()
            => _pagingHelper.GetActual();

        public void Clear()
        {
            PageEntrieList.Clear();
            _pagingHelper.Reset();
        }
    }
}
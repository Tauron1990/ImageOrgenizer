using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.ImageOrganizer.BL.Provider.DownloadImpl;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    [Export(typeof(IDownloadManager))]
    public class DownloadManagerImpl : IDisposable, INotifyBuildCompled, IDownloadManager
    {
        private readonly Timer _task;
        private readonly ManualResetEventSlim _pause = new ManualResetEventSlim(true);
        private readonly AutoResetEvent _shutdownEvent = new AutoResetEvent(false);
        private readonly object _lock = new object();
        private IDownloadDispatcher _downloadDispatcher;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<string, DateTime> _delays = new ConcurrentDictionary<string, DateTime>();
        private bool _shutdown;

        [Inject]
        public IProviderManager ProviderManager { private get; set; }

        [Inject]
        public IOperator Operator { private get; set; }

        [Inject]
        public IBrowserManager BrowserManager { private get; set; }

        public event EventHandler<DownloadChangedEventArgs> DownloadChangedEvent;

        public bool IsPaused { get; private set; }

        public DownloadManagerImpl() => _task = new Timer(Enqueue);

        public void Start()
        {
            if (IsPaused) _pause.Set();
            else StartTime();
        }

        public void Pause()
        {
            IsPaused = true;
            _pause.Reset();
        }

        private void StartTime() => _task.Change(TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);

        private void Enqueue(object sender)
        {
            lock (_lock)
            {
                if (_shutdown)
                {
                    _shutdownEvent.Set();
                    return;
                }

                try
                {
                    var curr = DateTime.Now;
                    foreach (var dateTime in _delays.ToArray())
                        if (curr > dateTime.Value)
                            _delays.TryRemove(dateTime.Key, out _);


                    var items = Operator.GetDownloadItems(new GetDownloadItemInput(false, _delays.Keys));
                    if (items == null || items.Length == 0) return;

                    //Reactivate Downloads
                    //Worker(items);

                }
                finally
                {
                    StartTime();
                }
            }
        }

        private void Worker(IEnumerable<DownloadItem> downloadEntities)
        {
            int dispacherCount = 0;

            foreach (var item in downloadEntities.Where(it => it != null))
            {
                _pause.Wait();

                var browser = BrowserManager.GetBrowser();

                try
                {
                    if(_delays.ContainsKey(item.Provider)) continue;

                    var entry = _downloadDispatcher.Get(item);
                    var provider = ProviderManager.Get(entry.Data.ProviderName);
                    provider.FillInfo(entry, browser,
                        s => _delays[s] = DateTime.Now + TimeSpan.FromMinutes(15),
                        (s, type) => AddDownloadAction(s, type, provider.Id, entry.Data.Name));
                }
                catch (Exception e)
                {
                    Logger.Error(e, e.Message);
                    item.FailedReason = DownloadEntry.FormatException(e);
                    Operator.DownloadFailed(item);
                }
                finally
                {
                    browser.Clear();
                }

                if (dispacherCount == 10)
                {
                    dispacherCount = 0;
                    _downloadDispatcher.Dispatch();
                }
                else
                    dispacherCount++;
            }

            _downloadDispatcher.Dispatch();
        }

        private void AddDownloadAction(string name, DownloadType type, string provider, string imageName)
        {
            void AddDownload(Task<DownloadItem[]> tt)
            {
                foreach (var downloadItem in tt.Result) OnDowloandChangedEvent(new DownloadChangedEventArgs(DownloadAction.DownloadAdded, downloadItem));
            }

            switch (type)
            {
                case DownloadType.UpdateColor:
                    Operator.HasTagType(name).ContinueWith(t =>
                    {
                        if(t.Result) return;

                        Operator.ScheduleDownload(new DownloadItem(type, imageName, DateTime.Now + TimeSpan.FromMinutes(30), -1, DownloadStade.Queued, provider, false, null, name)
                            { AvoidDouble = true}).ContinueWith(AddDownload);
                    });
                    break;
                case DownloadType.UpdateDescription:
                    Operator.HasTag(name).ContinueWith(t =>
                    {
                        if (t.Result) return;
                        Operator.ScheduleDownload(new DownloadItem(type, imageName, DateTime.Now + TimeSpan.FromMinutes(30), -1, DownloadStade.Queued, provider, false, null, name)
                            { AvoidDouble = true }).ContinueWith(AddDownload);
                    });
                    break;
                default:
                    Operator.ScheduleDownload(new DownloadItem(type, imageName, DateTime.Now + TimeSpan.FromMinutes(30), -1, DownloadStade.Queued, provider, false, null, name)
                        { AvoidDouble = true }).ContinueWith(AddDownload);
                    break;
            }
        }

        public void ShutDown()
        {
            _pause.Set();
            lock (_lock)
                _shutdown = true;
            _shutdownEvent.WaitOne();
        }

        public void Dispose()
        {
            try
            {
                _task.Dispose();
                _pause.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        private void OnDowloandChangedEvent(DownloadChangedEventArgs e) => CommonApplication.QueueWorkitemAsync(() => DownloadChangedEvent?.Invoke(this, e));

        public void BuildCompled()
        {
            lock (_lock)
                _downloadDispatcher = new DownloadDispatcher(Operator, OnDowloandChangedEvent);
        }
    }
}
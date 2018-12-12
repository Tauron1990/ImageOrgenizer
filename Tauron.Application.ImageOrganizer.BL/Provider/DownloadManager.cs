using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NLog;
using Tauron.Application.ImageOrganizer.BL.Provider.DownloadImpl;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;
using Timer = System.Timers.Timer;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    [Export(typeof(IDownloadManager))]
    public class DownloadManagerImpl : IDisposable, INotifyBuildCompled, IDownloadManager
    {
        private readonly BlockingCollection<DownloadItem[]> _downloadEntities = new BlockingCollection<DownloadItem[]>(10);
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Timer _task = new Timer(10_000);
        private Task _worker;
        private int _inProgress;
        private readonly ManualResetEventSlim _pause = new ManualResetEventSlim(true);
        private readonly object _lock = new object();
        private IDownloadDispatcher _downloadDispatcher;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); 

        [Inject]
        public IProviderManager ProviderManager { private get; set; }

        [Inject]
        public IOperator Operator { private get; set; }

        public event EventHandler<DownloadChangedEventArgs> DowloandChangedEvent;

        public bool IsPaused { get; private set; }

        public void Start()
        {
            if(IsPaused) _pause.Set();
            if (_task.Enabled) return;

            _task.Elapsed += Enqueue;
            _task.Start();
            _worker = Task.Factory.StartNew(Worker, TaskCreationOptions.LongRunning);
        }

        public void Pause()
        {
            IsPaused = true;
            _pause.Reset();
        }

        private void Enqueue(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_lock)
                {
                    if (_downloadEntities.Count != 0 || _inProgress != 0)
                        return;

                    var items = Operator.GetDownloadItems(false);
                    if (items == null || items.Length == 0) return;

                    _downloadEntities.Add(items.Take(1).ToArray());
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void Worker()
        {
            List<Task> tasks = new List<Task>();

            foreach (var downloadItems in _downloadEntities.GetConsumingEnumerable())
            {
                lock (_lock)
                {
                    Interlocked.Exchange(ref _inProgress, 1);
                    _pause.Wait();

                    try
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                            return;


                        tasks.AddRange(downloadItems.Select(item => Task.Run(() =>
                        {
                            try
                            {
                                var entry = _downloadDispatcher.Get(item);
                                var provider = ProviderManager.Get(entry.Data.ProviderName);
                                provider.FillInfo(entry, (s, type) => AddDownloadAction(s, type, provider.Id, entry.Data.Name));
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, e.Message);
                                item.FailedReason = DownloadEntry.FormatException(e);
                                Operator.DownloadFailed(item);
                            }
                        })));

                        Task.WaitAll(tasks.ToArray());
                        tasks.Clear();

                        _downloadDispatcher.Dispatch();
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _inProgress, 0);
                    }
                }
            }

            lock (_lock)
                _downloadDispatcher.Dispatch();
        }

        private void AddDownloadAction(string name, DownloadType type, string provider, string imageName)
        {
            switch (type)
            {
                case DownloadType.UpdateColor:
                    Operator.HasTagType(name).ContinueWith(t =>
                    {
                        if(t.Result) return;
                        Operator.ScheduleDownload(new DownloadItem(type, imageName, DateTime.Now /*+ TimeSpan.FromHours(1)*/, -1, DownloadStade.Queued, provider, false, null, name));
                    });
                    break;
                case DownloadType.UpdateDescription:
                    Operator.HasTag(name).ContinueWith(t =>
                    {
                        if (t.Result) return;
                        Operator.ScheduleDownload(new DownloadItem(type, imageName, DateTime.Now /*+ TimeSpan.FromHours(1)*/, -1, DownloadStade.Queued, provider, false, null, name));
                    });
                    break;
                default:
                    Operator.ScheduleDownload(new DownloadItem(type, imageName, DateTime.Now /*+ TimeSpan.FromHours(1)*/, -1, DownloadStade.Queued, provider, false, null, name));
                    break;
            }
        }

        public void ShutDown()
        {
            _task.Stop();
            _pause.Set();
            _cancellationTokenSource.Cancel();
            _downloadEntities.CompleteAdding();
            _worker.Wait();
        }

        public void Dispose()
        {
            try
            {
                _cancellationTokenSource.Dispose();
                _task.Dispose();
                _downloadEntities.Dispose();
                _worker.Dispose();
                _pause.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        private void OnDowloandChangedEvent(DownloadChangedEventArgs e) => CommonApplication.QueueWorkitemAsync(() => DowloandChangedEvent?.Invoke(this, e));

        public void BuildCompled()
        {
            lock (_lock)
                _downloadDispatcher = new DownloadDispatcher(Operator, OnDowloandChangedEvent);
        }
    }
}
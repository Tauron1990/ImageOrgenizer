using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ImageOrganizer.BL.Provider.DownloadImpl;
using Tauron.Application.Ioc;
using Timer = System.Timers.Timer;

namespace ImageOrganizer.BL.Provider
{
    [Export(typeof(DownloadManager))]
    public class DownloadManager : IDisposable, INotifyBuildCompled
    {
        private readonly BlockingCollection<DownloadItem> _downloadEntities = new BlockingCollection<DownloadItem>(10);
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Timer _task = new Timer(10_000);
        private Task _worker;
        private int _inProgress;
        private readonly ManualResetEventSlim _pause = new ManualResetEventSlim(true);
        private readonly object _lock = new object();
        private IDownloadDispatcher _downloadDispatcher;

        [Inject]
        public ProviderManager ProviderManager { private get; set; }

        [Inject]
        public Operator Operator { private get; set; }

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
                if (_downloadEntities.Count != 0 || _inProgress != 0)
                    return;

                lock (_lock)
                    _downloadDispatcher.Dispatch();

                var items = Operator.GetDownloadItems(false);
                if(items == null || items.Length == 0) return;

                foreach (var downloadItem in items)
                {
                    if(_downloadEntities.IsAddingCompleted) break;
                    _downloadEntities.Add(downloadItem);
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void Worker()
        {
            foreach (var downloadItem in _downloadEntities.GetConsumingEnumerable())
            {
                lock (_lock)
                {
                    Interlocked.Exchange(ref _inProgress, 1);
                    _pause.Wait();

                    try
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                            return;

                        try
                        {
                            var entry = _downloadDispatcher.Get(downloadItem);
                            var provider = ProviderManager.Get(entry.Data.ProviderName);
                            provider.FillInfo(entry);
                        }
                        catch
                        {
                            Operator.DownloadFailed(downloadItem);
                        }
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

        private void OnDowloandChangedEvent(DownloadChangedEventArgs e) => DowloandChangedEvent?.Invoke(this, e);

        public void BuildCompled() => _downloadDispatcher = new DownloadDispatcher(Operator, OnDowloandChangedEvent);
    }
}
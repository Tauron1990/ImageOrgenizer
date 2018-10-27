using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;
using Timer = System.Timers.Timer;

namespace ImageOrganizer.BL.Provider
{
    [Export(typeof(DownloadManager))]
    public class DownloadManager : IDisposable
    {
        private readonly BlockingCollection<DownloadItem> _downloadEntities = new BlockingCollection<DownloadItem>(10);
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Timer _task = new Timer(10000);
        private Task _worker;
        private int _inProgress;
        private readonly ManualResetEventSlim _pause = new ManualResetEventSlim(true);
        
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
            if (_downloadEntities.Count != 0 || _inProgress != 0)
                return;

            var items = Operator.GetDownloadItems(false);
            if(items.Length == 0) return;

            foreach (var downloadItem in items)
                _downloadEntities.Add(downloadItem);
        }

        private void Worker()
        {
            foreach (var downloadItem in _downloadEntities.GetConsumingEnumerable())
            {
                Interlocked.Exchange(ref _inProgress, 1);
                _pause.Wait();

                try
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        return;

                    var image = downloadItem.DownloadType == DownloadType.DownloadImage ? new ImageData(downloadItem.Image, downloadItem.Provider) : Operator.GetImageData(downloadItem.Image).Result;
                    if (image == null)
                    {
                        Operator.DownloadCompled(downloadItem);
                        continue;
                    }

                    try
                    {
                        var provider = ProviderManager.Get(image.ProviderName);
                        if (provider.FillInfo(image, downloadItem.DownloadType, Operator, out var ok) && ok)
                        {
                            var item = new DownloadItem(DownloadType.UpdateTags, downloadItem.Image, DateTime.Now + TimeSpan.FromDays(30), -1, DownloadStade.Queued, provider.Id, false);
                            OnDowloandChangedEvent(new DownloadChangedEventArgs(DownloadAction.DownloadAdded, item));
                            Operator.ScheduleDownload(item);
                        }

                        if (ok)
                        {
                            OnDowloandChangedEvent(new DownloadChangedEventArgs(DownloadAction.DownloadCompled, downloadItem));
                            Operator.DownloadCompled(downloadItem);
                            Operator.UpdateImage(image);
                        }
                        else
                            Operator.DownloadFailed(downloadItem);
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

        public void Dispose()
        {
            try
            {
                _pause.Set();
                _cancellationTokenSource.Cancel();
                _downloadEntities.CompleteAdding();
                _task.Stop();
                _worker.Wait();

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
    }
}
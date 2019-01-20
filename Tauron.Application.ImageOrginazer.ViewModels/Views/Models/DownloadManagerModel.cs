using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [ExportModel(AppConststands.DownloadManagerModel)]
    public sealed class DownloadManagerModel : ModelBase
    {
        public class NullIferCollection : UIObservableCollection<DownloadItem>
        {
            //private NotifyCollectionChangedEventHandler _handler;

            //public override event NotifyCollectionChangedEventHandler CollectionChanged
            //{
            //    add
            //    {
            //        if (_handler == null)
            //            _handler = value;
            //        else
            //            _handler += value;
            //    }
            //    remove
            //    {
            //        if(_handler == null) return;

            //        // ReSharper disable once DelegateSubtraction
            //        _handler -= value;
            //    }
            //}

            public void Nullifiy(){}// => _handler = null;
        }

        private IClipboardViewer _clipboardViewer;
        private object _lock = new object();
        private bool _isBusy;
        private int _isAttached;
        private int _downloadCount;

        [Inject]
        public IDownloadManager DownloadManager { get; set; }

        [Inject]
        public IOperator Operator { get; set; }

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        [Inject]
        public IClipboardManager ClipboardManager { get; set; }
        
        public NullIferCollection DownloadItems { get; private set; }

        public int DownloadCount
        {
            get => _downloadCount;
            set => SetProperty(ref _downloadCount, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private void FetchDataAsync()
        {
            DownloadItems = new NullIferCollection();
            Task.Run(() =>
            {
                lock (_lock)
                {
                    IsBusy = true;
                    DownloadItems.AddRange(Operator.GetDownloadItems(new GetDownloadItemInput(true, Enumerable.Empty<string>())));
                    Interlocked.Exchange(ref _isAttached, 1);
                    IsBusy = false;
                }
            });
        }

        public void Attach() => FetchDataAsync();

        private void OnDowloandChangedEvent(object sender, DownloadChangedEventArgs e)
        {
            lock (_lock)
            {
                switch (e.DownloadAction)
                {
                    case DownloadAction.DownloadCompled:
                        if(_isAttached == 1)
                            lock (_lock) DownloadItems?.Remove(DownloadItems.FirstOrDefault(ii => ii.Equals(e.DownloadItem)));
                        DownloadCount--;
                        break;
                    case DownloadAction.DownloadAdded:
                        if(_isAttached == 1)
                            lock(_lock) DownloadItems?.Add(e.DownloadItem);
                        DownloadCount++;
                        break;
                    case DownloadAction.DownloadFailed:
                        lock (_lock)
                        {
                            var tempItem = DownloadItems?.FirstOrDefault(it => it.Id == e.DownloadItem.Id);
                            if (tempItem != null)
                                tempItem.FailedReason = e.DownloadItem.FailedReason;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void DeAttach()
        {
            lock (_lock)
            {
                Interlocked.Exchange(ref _isAttached, 0);
                DownloadItems = null;

                //DownloadItems.Nullifiy();
                //var blocker = DownloadItems.BlockChangedMessages();
                //DownloadItems.Clear();
                //UiSynchronize.Synchronize.BeginInvoke(blocker.Dispose);
            }
        }

        public void StartClipBoardListening()
        {
            DownloadCount = Operator.GetDownloadCount();

            DownloadManager.DownloadChangedEvent += OnDowloandChangedEvent;
            DownloadManager.Start();

            //UiSynchronize.Synchronize.Invoke(() =>
            //{
            //    _clipboardViewer = ClipboardManager.CreateViewer(CommonApplication.Current.MainWindow ?? throw new InvalidOperationException("Window is Null"), true, true);
            //    _clipboardViewer.ClipboardChanged += ClipboardViewerOnClipboardChanged;
            //});
        }

        private void ClipboardViewerOnClipboardChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (!ClipboardManager.ContainsText()) return;
                    var text = ClipboardManager.GetText();

                    Task.Run(() => TryQueueDownload(text));

                    break;
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }
            }
        }

        private List<string> _inProgress = new List<string>();

        private void TryQueueDownload(string url)
        {
            lock (_inProgress)
            {
                MainWindowViewModel.ShowDownloadManagerAction();
                if (_inProgress.Contains(url)) return;
                _inProgress.Add(url);
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out _)) return;

            var provider = ProviderManager.Find(url);
            if (string.IsNullOrWhiteSpace(provider)) return;

            DownloadItem item = new DownloadItem(DownloadType.DownloadImage, url, DateTime.Now, -1, DownloadStade.Paused, provider, true, String.Empty, String.Empty)
            {
                AvoidDouble = true
            };

            Operator.ScheduleDownload(item).ContinueWith(t =>
            {
                lock (_inProgress) _inProgress.Remove(url);
                lock (_lock) DownloadItems?.AddRange(t.Result);
            });
        }

        public void Shutdown() => DownloadManager.ShutDown();
    }
}
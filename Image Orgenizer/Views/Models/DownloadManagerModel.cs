using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ImageOrganizer.BL;
using ImageOrganizer.BL.Provider;
using ImageOrganizer.Data.Entities;
using Syncfusion.Windows.Shared;
using Tauron.Application;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace ImageOrganizer.Views.Models
{
    [ExportModel(AppConststands.DownloadManagerModel)]
    public class DownloadManagerModel : ModelBase
    {
        private ClipboardViewer _clipboardViewer;
        private object _lock;
        private bool _isBusy;
        private int _isAttached;
        private int _downloadCount;

        [Inject]
        public DownloadManager DownloadManager { get; set; }

        [Inject]
        public Operator Operator { get; set; }

        [Inject]
        public ProviderManager ProviderManager { get; set; }

        public UIObservableCollection<DownloadItem> DownloadItems { get; } = new UIObservableCollection<DownloadItem>();

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

        public void FetchDataAsync()
        {
            Task.Run(() =>
            {
                lock (_lock)
                {
                    IsBusy = true;
                    DownloadItems.Clear();
                    DownloadItems.AddRange(Operator.GetDownloadItems(true));
                    IsBusy = false;
                }
            });
        }

        public void Attach() => Interlocked.Exchange(ref _isAttached, 1);

        private void OnDowloandChangedEvent(object sender, DownloadChangedEventArgs e)
        {
            lock (_lock)
            {
                switch (e.DownloadAction)
                {
                    case DownloadAction.DownloadCompled:
                        if(_isAttached == 1)
                            DownloadItems.Remove(DownloadItems.FirstOrDefault(ii => ii.Equals(e.DownloadItem)));
                        DownloadCount++;
                        break;
                    case DownloadAction.DownloadAdded:
                        if(_isAttached == 1)
                            DownloadItems.Add(e.DownloadItem);
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
                DownloadItems.Clear();
            }
        }

        public void StartClipBoardListening()
        {
            DownloadCount = Operator.GetDownloadCount();

            DownloadManager.DowloandChangedEvent += OnDowloandChangedEvent;
            DownloadManager.Start();

            UiSynchronize.Synchronize.Invoke(() =>
            {
                _clipboardViewer = new ClipboardViewer(CommonApplication.Current.MainWindow ?? throw new InvalidOperationException("Window is Null"), true, true);
                _clipboardViewer.ClipboardChanged += ClipboardViewerOnClipboardChanged;
            });
        }

        private void ClipboardViewerOnClipboardChanged(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                Task.Run(() => TryQueueDownload(text));
            }
        }

        private void TryQueueDownload(string url)
        {
            if(!Uri.TryCreate(url, UriKind.Absolute, out _)) return;

            var provider = ProviderManager.Find(url);
            if(provider.IsNullOrWhiteSpace()) return;

            DownloadItem item = new DownloadItem(DownloadType.DownloadImage, url, DateTime.Now, -1, DownloadStade.Paused, provider, false)
            {
                AvoidDouble = true
            };

            Operator.ScheduleDownload(item);
            DownloadItems.Add(item);

            MainWindowViewModel.ShowDownloadManagerAction();
        }
    }
}
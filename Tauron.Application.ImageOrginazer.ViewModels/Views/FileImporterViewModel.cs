using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.FileImporter)]
    public class FileImporterViewModel : MainViewControllerBase
    {
        private FileImporterProviderItem _currentProvider;

        private string _target;
        private long _fileCount;
        private bool _isLoadingActive;

        [Inject]
        public IProviderManager ProviderManager { get; set; }

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManager { get; set; }

        [Inject]
        public IOperator Operator { get; set; }

        public override string ProgrammTitle { get; } = UIResources.FileImporter_Programm_Title;
        public override bool IsSidebarEnabled { get; }
        public override bool IsNavigatorEnabled { get; }
        public override bool IsMainControlEnabled { get; }

        public override string ControlButtonLabel { get; } = UIResources.Common_Label_Back;

        public UIObservableCollection<FileImporterProviderItem> ProviderItems { get; } = new UIObservableCollection<FileImporterProviderItem>();

        public FileImporterProviderItem CurrentProvider
        {
            get => _currentProvider;
            set => SetProperty(ref _currentProvider, value);
        }

        public string Target
        {
            get => _target;
            set => SetProperty(ref _target, value, TargetChanged);
        }
        
        public override void OnClick() => MainWindowViewModel.ShowImagesAction();

        [CommandTarget]
        public void SearchFiles()
        {
            var path = Dialogs.ShowOpenFolderDialog(MainWindow, UIResources.Dialog_Title_Open, Environment.SpecialFolder.DesktopDirectory, false, true, out var erg);

            if (erg == false) return;

            Target = path;
        }

        [CommandTarget]
        public void StartImport()
        {
            _cancellationTokenSource?.Cancel();

            var input = new ImporterInput(Target, CurrentProvider.Id);
            input.PostMessage += OperationManager.PostMessage;

            var dis = OperationManager.EnterOperation(true, true, input.OnPause, input.OnStop);
            Operator.ImportFiles(input).ContinueWith(t =>
            {
                dis.Dispose();
                var ex = t.Result;
                if (ex == null)
                    MainWindowViewModel.ShowImagesRefreshAction();
                else
                    Dialogs.ShowMessageBox(MainWindow, $"{ex.GetType()} -- {ex.Message}", "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
            });
        }

        [CommandTarget]
        public bool CanStartImport() => CurrentProvider != null || Target.ExisDirectory();

        public override void BuildCompled()
        {
            ProviderItems.AddRange(ProviderManager.Ids.Select(s => new FileImporterProviderItem(s)));
            CurrentProvider = ProviderItems.First(p => p.Id == AppConststands.ProviderNon);
        }

        private object _startLock = new object();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _fileWorker;
        private UiFileInfo _currentFileInfo;

        public UIObservableCollection<UiFileInfo> UiFileInfos { get; } = new UIObservableCollection<UiFileInfo>();

        public UiFileInfo CurrentFileInfo
        {
            get => _currentFileInfo;
            set => SetProperty(ref _currentFileInfo, value);
        }

        public long FileCount
        {
            get => _fileCount;
            set => SetProperty(ref _fileCount, value);
        }

        public bool IsLoadingActive
        {
            get => _isLoadingActive;
            set => SetProperty(ref _isLoadingActive, value);
        }

        [EventTarget]
        public void DataGridClick()
        {
            if(CurrentFileInfo == null) return;

            try
            {
                Process.Start(CurrentFileInfo.FullName);
            }
            catch (Exception e)
            {
                if (e.IsCriticalApplicationException())
                    throw;
            }
        }

        // ReSharper disable once RedundantDelegateCreation
        private void TargetChanged() => Task.Run(new Action(StartAsync));

        private void StartAsync()
        {
            lock (_startLock)
            {
                if (_fileWorker != null)
                {
                    _cancellationTokenSource.Cancel();
                    _fileWorker.Wait();
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();

                _fileWorker?.Dispose();
                _fileWorker = new Task(FillFiles);
                _fileWorker.Start();
            }
        }

        private void FillFiles()
        {
            IsLoadingActive = true;
            try
            {
                var token = _cancellationTokenSource.Token;
                string[] files = Target.GetFiles();

                FileCount = files.Length;
                
                token.ThrowIfCancellationRequested();
                UiFileInfos.Clear();

                using (UiFileInfos.BlockChangedMessages())
                {
                    foreach (var file in files)
                    {
                        token.ThrowIfCancellationRequested();
                        UiFileInfos.Add(new UiFileInfo(new FileInfo(file)));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                FileCount = 0;
                UiFileInfos.Clear();
            }
            finally
            {
                IsLoadingActive = false;
            }
        }
    }
}
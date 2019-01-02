using System;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.ImageOrganizer.UI.Video;
using Tauron.Application.ImageOrginazer.ViewModels.Core;
using Tauron.Application.Ioc;
using Tauron.Application.Models;
using IVideoSourceProvider = Tauron.Application.ImageOrganizer.UI.IVideoSourceProvider;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.PreviewWindowName)]
    public class PreviewWindowModel : ViewModelBase, IDisposable, IVideoSourceProvider
    {
        private readonly ImageData _data;
        private VideoManager _videoManager;
        private bool _error;
        private string _errorText;
        private bool _operationRunning;

        public ImageOrganizer.UI.Video.IVideoSourceProvider VideoSource { get; set; }

        public event Action LockEvent;
        public event Action<IVideoSourceProvider> UnlockEvent;

        //public bool Error
        //{
        //    get => _error;
        //    set => SetProperty(ref _error, value);
        //}

        //public string ErrorText
        //{
        //    get => _errorText;
        //    set => SetProperty(ref _errorText, value);

        //}

        [Inject]
        public IOperator Operator { get; set; }

        [Inject]
        public PreviewWindowModel([Inject] ImageData data)
        {
            _data = data;
            _videoManager = new VideoManager();
        }

        public PreviewWindowModel() { }

        public void BeginLoad()
        {
            if (_videoManager == null)
            {
                Dialogs.ShowMessageBox(MainWindow, "Inject Error (No Data)", "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
                return;
            }

            OnUnlockEvent(this);
            _videoManager.ShowImage(() => _data, VideoSource, Operator);

            //Error = _videoManager.ViewError;
            //ErrorText = _videoManager.ErrorMessage;
        }

        public void Dispose()
        {
            OnLockEvent();
            _videoManager?.Dispose();
        }

        private void OnLockEvent() => LockEvent?.Invoke();

        private void OnUnlockEvent(IVideoSourceProvider obj) => UnlockEvent?.Invoke(obj);
    }
}
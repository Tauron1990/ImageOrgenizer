using System;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.UI.Video;
using Tauron.Application.ImageOrginazer.ViewModels.Core;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.PreviewWindowName)]
    public class PreviewWindowModel : ViewModelBase, IDisposable
    {
        private readonly ImageData _data;
        private VideoManager _videoManager;
        private bool _error;
        private string _errorText;
        private bool _operationRunning;

        public IVideoSourceProvider VideoSourceProvider { get; set; }

        public bool Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);

        }

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
                Error = true;
                ErrorText = "Inject Error (No Data)";
                return;
            }

            _videoManager.ShowImage(() => _data, VideoSourceProvider, Operator);

            Error = _videoManager.ViewError;
            ErrorText = _videoManager.ErrorMessage;
        }

        public void Dispose() => _videoManager?.Dispose();
    }
}
using System;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.UI.Video;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;

namespace Tauron.Application.ImageOrginazer.ViewModels.Core
{
    public class VideoManager : IDisposable
    {
        private static DirectoryInfo _basePath;

        public bool ViewError { get; private set; }
        public string ErrorMessage { get; private set; }
        public ImageData ImageData { get; private set; }

        private Stream _currentMedia;
        private IDisposable _vlcMedia;
        
        public void ShowImage([NotNull] Func<ImageData> dataFunc, [NotNull] IVideoSourceProvider sourceProvider, [NotNull] IOperator op)
        {
            if (dataFunc == null) throw new ArgumentNullException(nameof(dataFunc));
            if (sourceProvider == null) throw new ArgumentNullException(nameof(sourceProvider));
            if (op == null) throw new ArgumentNullException(nameof(op));

            ViewError = false;
            var data = dataFunc();
            ImageData = data;

            if (data == null)
            {
                ErrorMessage = UIResources.ImageViewer_Error_NoData;
                ViewError = true;
                sourceProvider.MediaPlayer?.Stop();
                return;
            }

            ViewError = false;

            try
            {
                if (!sourceProvider.ExistPlayer)
                {
                    if (_basePath == null)
                    {
                        string basePath = AppDomain.CurrentDomain.BaseDirectory.CombinePath("libvlc");

                        basePath = basePath.CombinePath(Environment.Is64BitProcess ? "win-x64" : "win-x86");

                        _basePath = new DirectoryInfo(basePath);
                    }

                    sourceProvider.CreatePlayer(_basePath);
                }

                var player = sourceProvider.MediaPlayer;

                _vlcMedia?.Dispose();
                _vlcMedia = null;

                _currentMedia?.Dispose();
                _currentMedia = op.GetFile(data.Name);

                if (_currentMedia != null)
                    _vlcMedia = player.Play(_currentMedia);
                else
                {
                    ViewError = true;
                    ErrorMessage = UIResources.ImageViewer_Error_DataInvalid;
                }
            }
            catch (Exception e)
            {
                if (e.IsCriticalApplicationException())
                    throw;

                ErrorMessage = $"{e.GetType()} -- {e.Message} -- {e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}";
                ViewError = true;
            }
        }

        public void StreamDispose()
        {
            _currentMedia?.Dispose();
            _currentMedia = null;
        }

        public void LockDispose()
        {
            _vlcMedia?.Dispose();
            _vlcMedia = null;

            StreamDispose();
        }

        public void Dispose()
        {
            _vlcMedia?.Dispose();
            _currentMedia?.Dispose();
        }
    }
}
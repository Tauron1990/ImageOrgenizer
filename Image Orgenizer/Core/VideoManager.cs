using System;
using System.IO;
using ImageOrganizer.BL;
using ImageOrganizer.Resources;
using JetBrains.Annotations;
using Tauron;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;

namespace ImageOrganizer.Core
{
    public class VideoManager
    {
        private const string RepeatOption = "--repeat";

        public bool ViewError { get; private set; }
        public string ErrorMessage { get; private set; }
        public ImageData ImageData { get; private set; }

        public void ShowImage([NotNull] Func<ImageData> dataFunc, [NotNull] VlcVideoSourceProvider sourceProvider, [NotNull] Operator op)
        {
            if (dataFunc == null) throw new ArgumentNullException(nameof(dataFunc));
            if (sourceProvider == null) throw new ArgumentNullException(nameof(sourceProvider));
            if (op == null) throw new ArgumentNullException(nameof(op));

            ViewError = false;
            var data = dataFunc();

            if (data == null)
            {
                ErrorMessage = UIResources.ImageViewer_Error_NoData;
                ViewError = true;
                sourceProvider.MediaPlayer?.Stop();
                return;
            }

            ImageData = data;
            ViewError = false;

            try
            {
                if (sourceProvider.MediaPlayer == null)
                {
                    string basePath = AppDomain.CurrentDomain.BaseDirectory.CombinePath("libvlc");

                    basePath = basePath.CombinePath(Environment.Is64BitProcess ? "win-x64" : "win-x86");

                    sourceProvider.CreatePlayer(new DirectoryInfo(basePath));
                }

                var player = sourceProvider.MediaPlayer;
                player.Audio.IsMute = true;

                var stream = op.GetFile(data.Name);

                if (stream != null)
                {
                    player.Play(stream, RepeatOption);
                    
                }
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

                ErrorMessage = $"{e.GetType()} -- {e.Message}";
                ViewError = true;
            }
        }
    }
}
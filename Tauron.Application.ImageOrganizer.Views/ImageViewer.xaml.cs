using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.ImageOrganizer.Views.Core;
using Tauron.Application.Views;
using Vlc.DotNet.Wpf;
using WpfAnimatedGif;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ImageViewer.xaml
    /// </summary>
    [ExportView(AppConststands.ImageViewer)]
    public partial class ImageViewer
    {
        private class ContentManager : IDisposable
        {
            private readonly Action _removeImage;

            private readonly Lazy<VlcControl> _vlcControl = new Lazy<VlcControl>(() => new VlcControl { Background = Brushes.Transparent });

            private VlcControl VlcControl => _vlcControl.Value;

            public ImageSource ImageSource { get; set; }

            public VlcVideoSourceProvider GetSourceProvicer() => _vlcControl.IsValueCreated ? VlcControl.SourceProvider : null;

            public ContentManager(Action removeImage) => _removeImage = removeImage;

            public void Dispose()
            {
                _removeImage();
                ImageSource = null;

                if(_vlcControl.IsValueCreated)
                {
                    typeof(VlcVideoSourceProvider)
                        .InvokeMember("RemoveVideo", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, VlcControl.SourceProvider, null);
                }
            }
        }
        private ContentManager _contentManager;

        public ImageViewer() => InitializeComponent();

        private void ImageViewer_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is IVideoSourceProvider temp)) return;
            
            temp.LockEvent += OnLockEvent;
            temp.UnlockEvent += OnUnlockEvent;
        }

        private void OnUnlockEvent(IVideoSourceProvider obj)
        {
            _contentManager = new ContentManager(RemoveImage);
            obj.VideoSource = new VlcScourceInterface(_contentManager.GetSourceProvicer, NewImage, CleanImage);
        }

        private void CleanImage(ImageSource obj)
        {
        }

        private void NewImage(ImageSource obj)
        {
            switch (obj)
            {
                case BitmapFrame frame when frame.Decoder is GifBitmapDecoder:
                    ImageControl.Source = frame;
                    ImageBehavior.SetAnimatedSource(ImageControl, frame);
                    break;
                default:
                    ImageBehavior.SetAnimatedSource(ImageControl, null);
                    if (ImageControl.Source != obj)
                        ImageControl.Source = obj;
                    break;
            }
        }

        private void RemoveImage() => ImageControl.Source = null;

        private void OnLockEvent()
        {
            _contentManager.Dispose();
            _contentManager = null;
        }
    }
}
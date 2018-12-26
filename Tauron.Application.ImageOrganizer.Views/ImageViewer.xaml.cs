using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Tauron.Application.ImageOrganizer.Views.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Views;
using Tauron.Application.Views;
using Vlc.DotNet.Wpf;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ImageViewer.xaml
    /// </summary>
    [ExportView(AppConststands.ImageViewer)]
    public partial class ImageViewer
    {
        private VlcControl _vlcControl;

        public ImageViewer() => InitializeComponent();

        private void ImageViewer_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is ImageViewerViewModel temp)) return;
            
            temp.LockEvent += OnLockEvent;
            temp.UnlockEvent += OnUnlockEvent;
        }

        private void OnUnlockEvent(ImageViewerViewModel obj)
        {
            _vlcControl = new VlcControl { Background = Brushes.Transparent };
            obj.SourceProvider = new VlcScourceInterface(_vlcControl.SourceProvider);
            ContentControl.Content = _vlcControl;
        }

        private void OnLockEvent()
        {
            ContentControl.Content = null;
            typeof(VlcVideoSourceProvider)
                .InvokeMember("RemoveVideo", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, _vlcControl.SourceProvider, null);
            _vlcControl = null;
        }
    }
}
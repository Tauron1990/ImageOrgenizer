using System.Windows;
using Tauron.Application.ImageOrganizer.Views.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Views;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ImageViewer.xaml
    /// </summary>
    [ExportView(AppConststands.ImageViewer)]
    public partial class ImageViewer
    {
        public ImageViewer() => InitializeComponent();

        private void ImageViewer_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is ImageViewerViewModel temp)) return;

            temp.SourceProvider = new VlcScourceInterface(VlcControl.SourceProvider);
        }
    }
}
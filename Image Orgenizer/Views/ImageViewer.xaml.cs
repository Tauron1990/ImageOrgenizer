using System;
using System.IO;
using System.Windows;
using Tauron;
using Tauron.Application.Views;

namespace ImageOrganizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ImageViewer.xaml
    /// </summary>
    [ExportView(AppConststands.ImageViewer)]
    public partial class ImageViewer
    {
        public ImageViewer()
        {
            InitializeComponent();
        }

        private void ImageViewer_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var temp = e.NewValue as ImageViewerViewModel;
            if (temp == null) return;

            temp.SourceProvider = VlcControl.SourceProvider;
        }
    }
}
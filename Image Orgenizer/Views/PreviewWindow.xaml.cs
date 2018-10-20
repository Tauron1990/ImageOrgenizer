using System;
using System.Windows;
using Tauron.Application.Views;

namespace ImageOrganizer.Views
{
    /// <summary>
    /// Interaktionslogik für PreviewWindow.xaml
    /// </summary>
    [ExportView(AppConststands.PreviewWindowName)]
    public partial class PreviewWindow
    {
        public PreviewWindow()
        {
            InitializeComponent();
        }

        private void PreviewWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var model = ((PreviewWindowModel) DataContext);
            model.VideoSourceProvider = VlcControl.SourceProvider;
            model.BeginLoad();
        }

        private void OnClosed(object sender, EventArgs e) => VlcControl.Dispose();
    }
}

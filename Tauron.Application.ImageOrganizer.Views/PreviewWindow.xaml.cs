using System;
using System.Windows;
using Tauron.Application.ImageOrganizer.Views.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Views;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    /// Interaktionslogik für PreviewWindow.xaml
    /// </summary>
    [ExportWindow(AppConststands.PreviewWindowName)]
    public partial class PreviewWindow
    {
        public PreviewWindow()
        {
            InitializeComponent();
        }

        private void PreviewWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var model = (PreviewWindowModel) DataContext;
            model.VideoSourceProvider = new VlcScourceInterface(VlcControl.SourceProvider);
            model.BeginLoad();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            VlcControl.Dispose();
            ((IDisposable)DataContext).Dispose();
        }
    }
}

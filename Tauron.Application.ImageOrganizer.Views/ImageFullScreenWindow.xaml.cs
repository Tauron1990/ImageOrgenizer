using System.Windows.Input;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ImageFullScreenWindow.xaml
    /// </summary>
    [ExportWindow(AppConststands.ImageFullScreen)]
    public partial class ImageFullScreenWindow
    {
        public ImageFullScreenWindow()
        {
            InitializeComponent();
        }

        private void ImageFullScreenWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
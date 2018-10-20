using System.Windows.Input;
using Tauron.Application.Views;

namespace ImageOrganizer
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    [ExportWindow(AppConststands.MainWindowName)]
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigatorText_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            e.Handled = true;
            ((MainWindowViewModel) DataContext).EditModeStop();
        }
    }
}
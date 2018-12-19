using System.Windows;
using System.Windows.Input;
using Syncfusion.Windows.Controls;
using Tauron.Application.ImageOrginazer.ViewModels;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Models;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    [ExportWindow(AppConststands.MainWindowName)]
    public partial class MainWindow
    {
        private LockScreenManagerModel _lockScreen;

        public LockScreenManagerModel LockScreen => _lockScreen ?? (_lockScreen = (LockScreenManagerModel) ModelBase.ResolveModel(AppConststands.LockScreenModel));

        public MainWindow()
        {
            EventManager.RegisterClassHandler(typeof(SfChromelessWindow), SfChromelessWindow.KeyDownEvent, new KeyEventHandler(((sender, args) => LockScreen.OnLockscreenReset())), true);
            EventManager.RegisterClassHandler(typeof(SfChromelessWindow), SfChromelessWindow.MouseDownEvent, new MouseButtonEventHandler(((sender, args) => LockScreen.OnLockscreenReset())), true);

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
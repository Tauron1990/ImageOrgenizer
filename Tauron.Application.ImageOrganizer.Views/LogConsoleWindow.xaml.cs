using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    /// Interaktionslogik für LogConsoleWindow.xaml
    /// </summary>
    [ExportWindow(AppConststands.LogConsoleWindowName)]
    public partial class LogConsoleWindow
    {
        public LogConsoleWindow()
        {
            InitializeComponent();
        }
    }
}

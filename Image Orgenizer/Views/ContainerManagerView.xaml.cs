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

namespace ImageOrganizer.Views
{
    /// <summary>
    /// Interaktionslogik für ContainerManagerView.xaml
    /// </summary>
    [ExportView(AppConststands.ContainerManager)]
    public partial class ContainerManagerView : UserControl
    {
        public ContainerManagerView()
        {
            InitializeComponent();
        }
    }
}

using System.Windows.Controls;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
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

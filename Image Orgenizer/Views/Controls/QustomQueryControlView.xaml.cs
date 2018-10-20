using System.Windows.Controls;
using Tauron.Application.Views;

namespace ImageOrganizer.Views.Controls
{
    /// <summary>
    /// Interaktionslogik für QustomQueryControlView.xaml
    /// </summary>
    [ExportView(AppConststands.CustomQueryControl)]
    public partial class QustomQueryControlView : UserControl
    {
        public QustomQueryControlView()
        {
            InitializeComponent();
        }
    }
}

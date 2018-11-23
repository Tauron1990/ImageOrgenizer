using System.Windows.Controls;
using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    ///     Interaktionslogik für FileImporterView.xaml
    /// </summary>
    [ExportView(AppConststands.FileImporter)]
    public partial class FileImporterView : UserControl
    {
        public FileImporterView()
        {
            InitializeComponent();
        }
    }
}
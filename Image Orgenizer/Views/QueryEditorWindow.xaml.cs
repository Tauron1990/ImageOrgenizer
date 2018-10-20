using System.Windows;
using Tauron.Application.Views;

namespace ImageOrganizer.Views
{
    /// <summary>
    /// Interaktionslogik für QueryEditorWindow.xaml
    /// </summary>
    [ExportWindow(AppConststands.QueryEditorName)]
    public partial class QueryEditorWindow
    {
        public QueryEditorWindow() => InitializeComponent();

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}

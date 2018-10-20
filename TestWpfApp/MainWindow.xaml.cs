using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Syncfusion.Licensing;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace TestWpfApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            SyncfusionLicenseProvider.RegisterLicense("MjcwMTFAMzEzNjJlMzMyZTMwUy9NTnVGSU9scHNKNW0rSU51VG1ON3ZBQ2ozWGpqZXUwKzJ2RitJM1pzTT0=");
            InitializeComponent();
        }

        private void SfDataGrid_OnRowValidating(object sender, RowValidatingEventArgs e)
        {
            if (TestGrid.IsAddNewIndex(e.RowIndex))
            {
                if (!(e.RowData is INotifyDataErrorInfo data)) return;

                e.IsValid = !data.HasErrors;
                //if (!e.IsValid)
                //    TestGrid.GetAddNewRowController().CancelAddNew();
            }
        }

        private void TestGrid_OnAddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
        {
            ((TestContext) DataContext).OnAddNew(sender, e);
        }
    }
}

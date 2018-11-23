using System;
using Syncfusion.UI.Xaml.Grid;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Views.Controls.Converter
{
    [Export(typeof(ISpecificControlConverter))]
    public class DataGridConverter : ISpecificControlConverter
    {
        private class DataGrid : IDataGrid
        {
            private readonly SfDataGrid _dataGrid;

            public DataGrid(SfDataGrid dataGrid) => _dataGrid = dataGrid;

            public bool IsAddNewIndex(int rowIndex) => _dataGrid.IsAddNewIndex(rowIndex);
        }

        public Type ControlType => typeof(SfDataGrid);
        public object Convert(object input)
        {
            if(input is SfDataGrid grid)
                return new DataGrid(grid);
            return null;
        }
    }
}
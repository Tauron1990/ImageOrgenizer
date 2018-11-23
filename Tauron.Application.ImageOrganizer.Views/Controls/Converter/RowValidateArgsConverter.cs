using System;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.Grid;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Views.Controls.Converter
{
    [Export(typeof(ISpecificControlConverter))]
    public class RowValidateArgsConverter : ISpecificControlConverter
    {
        private class RowValidatingArgs : IRowValidatingArgs
        {
            private readonly RowValidatingEventArgs _eventArgs;

            public RowValidatingArgs(RowValidatingEventArgs eventArgs) => _eventArgs = eventArgs;

            public Dictionary<string, string> ErrorMessages => _eventArgs.ErrorMessages;
            public int RowIndex => _eventArgs.RowIndex;
            public object RowData => _eventArgs.RowData;
            public bool IsValid
            {
                get => _eventArgs.IsValid;
                set => _eventArgs.IsValid = value;
            }
        }

        public Type ControlType => typeof(RowValidatingEventArgs);

        public object Convert(object input)
        {
            if (input is RowValidatingEventArgs args)
                return new RowValidatingArgs(args);

            return null;
        }
    }
}
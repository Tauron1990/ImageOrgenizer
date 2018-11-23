using System;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.Grid;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Views.Controls.Converter
{
    [Export(typeof(ISpecificControlConverter))]
    public class RecordDeletedArgsConverter : ISpecificControlConverter
    {
        private class RecordDeletedArgs : IRecordDeletedArgs
        {
            private readonly RecordDeletedEventArgs _args;

            public RecordDeletedArgs(RecordDeletedEventArgs args) => _args = args;

            public IEnumerable<object> Items => _args.Items;
        }

        public Type ControlType => typeof(RecordDeletedEventArgs);
        public object Convert(object input)
        {
            if(input is RecordDeletedEventArgs args)
                return new RecordDeletedArgs(args);
            return null;
        }
    }
}
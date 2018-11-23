using System;
using Syncfusion.UI.Xaml.Grid;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Views.Controls.Converter
{
    [Export(typeof(ISpecificControlConverter))]
    public class AddRowInitialingConverter : ISpecificControlConverter
    {
        private class AddNewRowInitating : IAddRowInitiatingArgs
        {
            private readonly AddNewRowInitiatingEventArgs _args;

            public AddNewRowInitating(AddNewRowInitiatingEventArgs args) => _args = args;

            public object NewObject
            {
                get => _args.NewObject;
                set => _args.NewObject = value;
            }
        }

        public Type ControlType => typeof(AddNewRowInitiatingEventArgs);
        public object Convert(object input)
        {
            if(input is AddNewRowInitiatingEventArgs args) 
                return new AddNewRowInitating(args);
            return null;
        }
    }
}
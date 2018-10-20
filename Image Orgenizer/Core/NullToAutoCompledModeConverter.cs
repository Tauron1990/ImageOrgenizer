using System.Windows.Data;
using System.Windows.Markup;
using Syncfusion.Windows.Controls.Input;
using Tauron.Application.Converter;

namespace ImageOrganizer.Core
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class NullToAutoCompledModeConverter :ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateCommonConverter<string[], AutoCompleteMode>(strings => strings == null ? AutoCompleteMode.None : AutoCompleteMode.SuggestAppend);
    }
}
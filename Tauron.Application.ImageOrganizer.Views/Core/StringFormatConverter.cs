using System.Windows.Data;
using System.Windows.Markup;
using Tauron.Application.Converter;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class StringFormatConverter : ValueConverterFactoryBase
    {
        public string Format { get; set; }

        protected override IValueConverter Create() 
            => CreateStringConverter<object>(t => string.Format(Format, t));
    }
}
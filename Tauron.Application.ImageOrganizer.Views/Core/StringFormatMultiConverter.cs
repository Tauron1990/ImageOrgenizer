using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    [MarkupExtensionReturnType(typeof(IMultiValueConverter))]
    public class StringFormatMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) 
            => string.Format(UIResources.ResourceManager.GetString((string) parameter) ?? string.Empty, values);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    [MarkupExtensionReturnType(typeof(IMultiValueConverter))]
    public class ProgressValueConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is int value) || !(values[1] is int maximum)) return 0d;

            if (value == 0 || maximum == 0)
                return 0d;

            double percent = 100d / maximum * value;

            return percent / 100d;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
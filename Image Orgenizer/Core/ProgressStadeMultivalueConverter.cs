using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Shell;

namespace ImageOrganizer.Core
{
    [MarkupExtensionReturnType(typeof(IMultiValueConverter))]
    public class ProgressStadeMultivalueConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool operationRunning = (bool) values[0];
                bool showProgress = (bool) values[1];
                bool isIntermediate = (bool) values[2];

                if (!operationRunning) return TaskbarItemProgressState.None;

                if (showProgress)
                    return TaskbarItemProgressState.Normal;

                return isIntermediate ? TaskbarItemProgressState.Indeterminate : TaskbarItemProgressState.None;
            }
            catch (InvalidCastException)
            {
                return TaskbarItemProgressState.None;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
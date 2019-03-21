using System.Windows.Data;
using Tauron.Application.Converter;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public class NullableLongToInt : ValueConverterFactoryBase
    {
        private class Converter : ValueConverterBase<int, long?>
        {
            protected override long? Convert(int value) => value;
            protected override bool CanConvertBack => true;

            protected override int ConvertBack(long? value)
            {
                if (value == null) return 0;
                if (value.Value > int.MaxValue) return int.MaxValue;
                return (int) value.Value;
            }
        }

        protected override IValueConverter Create() => new Converter();
    }
}
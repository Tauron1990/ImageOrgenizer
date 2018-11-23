using System.Windows.Data;
using System.Windows.Media;
using Tauron.Application.Converter;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public class IfThenElseImageConverter : ValueConverterFactoryBase
    {
        public ImageSource IfTrue { get; set; }

        public ImageSource IfFalse { get; set; }

        protected override IValueConverter Create()
        {
            return CreateCommonConverter<bool, ImageSource>(b => b ? IfTrue : IfFalse);
        }
    }
}
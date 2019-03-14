using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tauron.Application.Converter;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public class BitmapSourceConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateCommonConverter(new Func<byte[], ImageSource>(ba => BitmapFrame.Create(new MemoryStream(ba))));
    }
}
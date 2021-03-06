﻿using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Tauron.Application.Converter;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public sealed class ColorConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateStringConverter<Color>(c => c.ToString(CultureInfo.CurrentUICulture));
    }
}
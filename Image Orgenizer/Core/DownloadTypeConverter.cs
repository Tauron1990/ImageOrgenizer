using System.Windows.Data;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Resources;
using Tauron.Application.Converter;

namespace ImageOrganizer.Core
{
    public class DownloadTypeConverter : ValueConverterFactoryBase
    { 
        protected override IValueConverter Create() => CreateStringConverter<DownloadType>(type => DownloadTypeLocals.ResourceManager.GetString(type.ToString()));
    }
}
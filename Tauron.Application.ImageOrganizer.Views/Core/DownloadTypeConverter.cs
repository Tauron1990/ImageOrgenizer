using System.Windows.Data;
using Tauron.Application.Converter;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public class DownloadTypeConverter : ValueConverterFactoryBase
    { 
        protected override IValueConverter Create() => CreateStringConverter<DownloadType>(type => DownloadTypeLocals.ResourceManager.GetString(type.ToString()));
    }
}
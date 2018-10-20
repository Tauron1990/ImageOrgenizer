using System.Windows.Data;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Resources;
using Tauron.Application.Converter;

namespace ImageOrganizer.Core
{
    public class DonwlodadStadeConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateStringConverter<DownloadStade>(stade => DownloadStadeLocals.ResourceManager.GetString(stade.ToString()));
    }
}
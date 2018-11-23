using System.Windows.Data;
using Tauron.Application.Converter;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    public class DonwlodadStadeConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateStringConverter<DownloadStade>(stade => DownloadStadeLocals.ResourceManager.GetString(stade.ToString()));
    }
}
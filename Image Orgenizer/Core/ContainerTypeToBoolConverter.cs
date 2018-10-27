using System.Windows.Data;
using System.Windows.Markup;
using ImageOrganizer.BL;
using ImageOrganizer.Views;
using Tauron.Application.Converter;

namespace ImageOrganizer.Core
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class ContainerTypeToBoolConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateCommonConverter<ContainerTypeUi, bool>(ui => ui != null &&  ui.Type == ContainerType.Compose);
    }
}
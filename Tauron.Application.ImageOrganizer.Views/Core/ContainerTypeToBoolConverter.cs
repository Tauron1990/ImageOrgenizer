using System.Windows.Data;
using System.Windows.Markup;
using Tauron.Application.Converter;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrginazer.ViewModels.Views;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class ContainerTypeToBoolConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => CreateCommonConverter<ContainerTypeUi, bool>(ui => ui != null &&  ui.Type == ContainerType.Compose);
    }
}
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrginazer.ViewModels
{
    public static class ViewModelsExtension
    {
        public static ExportResolver AddViewModels(this ExportResolver resolver) => resolver.AddAssembly(typeof(ViewModelsExtension));
    }
}
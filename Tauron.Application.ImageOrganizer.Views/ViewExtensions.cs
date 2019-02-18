using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Views
{
    public static class ViewExtensions
    {
        public static ExportResolver AddViews(this ExportResolver resolver) => resolver.AddAssembly(typeof(ViewExtensions));
    }
}
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL
{
    public static class BlExtension
    {
        public static ExportResolver AddBussinesLayer(this ExportResolver resolver) => resolver.AddAssembly(typeof(BlExtension).Assembly);
    }
}
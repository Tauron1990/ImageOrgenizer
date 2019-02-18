using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL
{
    public static class BLExtension
    {
        public static ExportResolver AddBussinesLayer(this ExportResolver resolver) => resolver.AddAssembly(typeof(BLExtension).Assembly);
    }
}
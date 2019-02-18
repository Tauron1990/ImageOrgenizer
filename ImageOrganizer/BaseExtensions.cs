using Tauron.Application.Ioc;

namespace ImageOrganizer
{
    public static class BaseExtensions
    {
        public static ExportResolver AddBase(this ExportResolver resolver) => resolver.AddAssembly(typeof(BaseExtensions).Assembly);
    }
}
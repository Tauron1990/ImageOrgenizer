using Tauron.Application.Ioc;

namespace Tauron.Application.IOInterface
{
    public static class ImageIOExtensions
    {
        public static ExportResolver AddIO(this ExportResolver resolver) => resolver.AddAssembly(typeof(ImageIOExtensions));
    }
}
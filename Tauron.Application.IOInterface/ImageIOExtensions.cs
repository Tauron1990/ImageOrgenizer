using Tauron.Application.Ioc;

namespace Tauron.Application.IOInterface
{
    public static class ImageIOExtensions
    {
        public static void AddIO(this ExportResolver resolver) => resolver.AddAssembly(typeof(ImageIOExtensions));
    }
}
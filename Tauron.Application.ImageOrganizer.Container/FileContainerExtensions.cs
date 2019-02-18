using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Container
{
    public static class FileContainerExtensions
    {
        public static ExportResolver AddContainer(this ExportResolver resolver) => resolver.AddAssembly(typeof(FileContainerExtensions).Assembly);
    }
}
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Data
{
    public static class DataExtensions
    {
        public static ExportResolver AddData(this ExportResolver resolver) => resolver.AddAssembly(typeof(DataExtensions).Assembly);
    }
}
using System;
using System.Linq;

namespace ImageOrganizer.Web.Core
{
    public class AggregateServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider[] _providers;

        public AggregateServiceProvider(params IServiceProvider[] providers) => _providers = providers;

        public object GetService(Type serviceType) => _providers.Select(p => p.GetService(serviceType)).FirstOrDefault(s => s != null);
    }
}
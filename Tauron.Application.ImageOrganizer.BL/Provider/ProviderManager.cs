using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    [Export(typeof(IProviderManager))]
    public class ProviderManager : IProviderManager
    {
        private readonly object _lock = new object();

        [Inject]
        private IProvider[] _providers;

        public IEnumerable<string> Ids => _providers.Select(t => t.Id);

        [NotNull]
        public IProvider Get(string id)
        {
            lock (_lock)
                return _providers.FirstOrDefault(provider => provider.Id == id) ?? _providers.First(p => p.Id == AppConststands.ProviderNon);
        }

        public string Find(string url) => _providers.Where(p => p.Id != AppConststands.ProviderNon).FirstOrDefault(p => p.IsValidUrl(url))?.Id;
    }
}
using System.Collections.Generic;
using System.Linq;
using ImageOrganizer.BL.Provider.Impl;
using JetBrains.Annotations;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BL.Provider
{
    [Export(typeof(ProviderManager))]
    public class ProviderManager
    {
        [Inject]
        private IProvider[] _providers;

        public IEnumerable<string> Ids => _providers.Select(t => t.Id);

        [NotNull]
        public IProvider Get(string id) => _providers.FirstOrDefault(provider => provider.Id == id) ?? _providers.First(p => p.Id == NonProvider.ProviderNon);

        public string Find(string url) => _providers.Where(p => p.Id != NonProvider.ProviderNon).FirstOrDefault(p => p.IsValidUrl(url))?.Id ?? NonProvider.ProviderNon;
    }
}
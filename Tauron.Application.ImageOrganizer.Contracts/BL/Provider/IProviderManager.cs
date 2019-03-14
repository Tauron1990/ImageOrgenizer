using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL.Provider {
    public interface IProviderManager
    {
        IEnumerable<string> Ids { get; }
        IProvider Get(string id);
        string Find(string url);

        IEnumerable<IViewFetcher> GetFetchers();
    }
}
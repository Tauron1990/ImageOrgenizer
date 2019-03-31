using System.Collections.Generic;
using Tauron.Application.ImageOrganizer.BL.Provider;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    public interface IFetcherCache
    {
        string FetcherId { get; }
        bool Fetching { get; }

        int UIPage { get; }

        IList<FetcherImage> Images { get; }

        string Last { get; }

        string Next { get; }

        int FetcherPage { get; }

        void Clear();

        void Read();

        void Start(string fetcherId);

        void Feed(FetcherResult result, string last, int fetcherPage);

        void SetUIPage(int page);
    }
}
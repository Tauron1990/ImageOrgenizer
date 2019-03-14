using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public sealed class FetcherResult
    {
        public IEnumerable<FetcherImage> Images { get; }

        public bool Sucseeded { get; }

        public string Error { get; }

        public bool Delay { get; }

        public string Next { get; }

        public string Last { get; }

        public bool LastArrived { get; }

        public FetcherResult(IEnumerable<FetcherImage> images, bool sucseeded, string error, bool delay, string next, string last, bool lastArrived)
        {
            Images = images;
            Sucseeded = sucseeded;
            Error = error;
            Delay = delay;
            Next = next;
            Last = last;
            LastArrived = lastArrived;
        }
    }
}
using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class GetDownloadItemInput
    {
        public bool FetchAll { get; }

        public IEnumerable<string> Delays { get; }

        public GetDownloadItemInput(bool fetchAll, IEnumerable<string> delays)
        {
            FetchAll = fetchAll;
            Delays = delays;
        }
    }
}
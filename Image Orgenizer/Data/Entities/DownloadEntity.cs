using System;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.Data.Entities
{
    public class DownloadEntity : GenericBaseEntity<int>
    {
        public string Image { get; set; }
        
        public DownloadType DownloadType { get; set; }

        public DateTime Schedule { get; set; }

        public DownloadStade DownloadStade { get; set; }

        public int FailedCount { get; set; }

        public string Provider { get; set; }

        public bool RemoveImageOnFail { get; set; }
    }
}
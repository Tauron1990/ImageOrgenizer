using System;
using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL
{
    public interface IDBSettings
    {
        event Action<string, ProfileData, bool> ProfileChanged;

        event Action Initilized;

        IDictionary<string, ProfileData> ProfileDatas { get; }

        string LastProfile { get; set; }

        string DownloadManagerGridStade { get; set; }

        ContainerType ContainerType { get; set; }

        string CustomMultiPath { get; set; }

        void Initialize();
    }
}
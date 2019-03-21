using System;
using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.BL
{
    public interface IDBSettings
    {
        #region Basic

        event Action Initilized;

        void Initialize();

        #endregion


        #region Profiles

        event Action<string, ProfileData, bool> ProfileChanged;
        
        IDictionary<string, ProfileData> ProfileDatas { get; }

        #endregion

        #region Fetcher

        IDictionary<string, string> FetcherData { get; }

        string BlacklistTags { get; set; }

        string WhitelistTags { get; set; }

        int MaxOnlineViewerPage { get; set; }

        #endregion

        #region Database

        ContainerType ContainerType { get; set; }

        string CustomMultiPath { get; set; }

        string LastProfile { get; set; }

        string DownloadManagerGridStade { get; set; }

        #endregion
    }
}
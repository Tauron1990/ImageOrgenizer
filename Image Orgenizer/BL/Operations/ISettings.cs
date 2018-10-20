using System.Collections.Generic;

namespace ImageOrganizer.BL.Operations
{
    public interface ISettings
    {
        IDictionary<string, ProfileData> ProfileDatas { get; }

        string LastProfile { get; set; }

        string DonwloadManagerGridStade { get; set; }

        ContainerType ContainerType { get; set; }

        string CustomMultiPath { get; set; }
    }
}
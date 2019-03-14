using BITSReference10_2;
using JetBrains.Annotations;

namespace SharpBits.Base
{
    [PublicAPI]
    public class BitsFileInfo
    {
        internal BitsFileInfo(_BG_FILE_INFO fileInfo) => BgFileInfo = fileInfo;

        public BitsFileInfo(string remoteName, string localName) => BgFileInfo = new _BG_FILE_INFO {RemoteName = remoteName, LocalName = localName};

        public string RemoteName => BgFileInfo.RemoteName;

        public string LocalName => BgFileInfo.LocalName;

        internal _BG_FILE_INFO BgFileInfo { get; }
    }
}

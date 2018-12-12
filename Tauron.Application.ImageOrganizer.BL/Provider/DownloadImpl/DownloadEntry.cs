
using System;

namespace Tauron.Application.ImageOrganizer.BL.Provider.DownloadImpl
{
    public class DownloadEntry : IDownloadEntry
    {
        private readonly Func<string, TagData> _getTagFunc;

        public ImageData Data { get; }
        public DownloadItem Item { get; }

        public bool Failed { get; private set; }

        public string FailedReason { get; private set; }

        public bool Update { get; private set; }

        public bool Changed { get; private set; }

        public (string Name, byte[] Data) File { get; private set; }

        public DownloadEntry(ImageData data, DownloadItem item, Func<string, TagData> getTagFunc)
        {
            _getTagFunc = getTagFunc;
            Data = data;
            Item = item;
        }

        public void AddFile(string name, byte[] data) => File = (name, data);

        public void MarkFailed(Exception e)
        {
            Failed = true;
            FailedReason = FormatException(e);
        }

        internal static string FormatException(Exception e) => $"{e.GetType()} -- {e.Message} -- {e.TargetSite?.Name}";

        public void NeedUpdate() => Update = true;
        public void MarkChanged() => Changed = true;
        public void MarkFailed(string message)
        {
            FailedReason = message;
            Failed = true;
        }

        public TagData GetTag(string name) => _getTagFunc(name);
    }
}
using System;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IDownloadEntry
    {
        TagData GetTag(string name);
        ImageData Data { get; }
        DownloadItem Item { get; }
        void AddFile(string name, byte[] data);
        void MarkFailed(Exception e);
        void NeedUpdate();
        void MarkChanged();
        void MarkFailed(string message);
    }
}
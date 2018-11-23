namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IDownloadEntry
    {
        ImageData Data { get; }
        DownloadItem Item { get; }
        void AddFile(string name, byte[] data);
        void MarkFailed();
        void NeedUpdate();
        void MarkChanged();
    }
}
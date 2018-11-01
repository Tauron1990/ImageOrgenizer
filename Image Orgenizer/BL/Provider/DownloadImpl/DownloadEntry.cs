
namespace ImageOrganizer.BL.Provider.DownloadImpl
{
    public class DownloadEntry : IDownloadEntry
    {
        public ImageData Data { get; }
        public DownloadItem Item { get; }

        public bool Failed { get; private set; }

        public bool Update { get; private set; }

        public (string Name, byte[] Data) File { get; private set; }

        public DownloadEntry(ImageData data, DownloadItem item)
        {
            Data = data;
            Item = item;
        }

        public void AddFile(string name, byte[] data) => File = (name, data);

        public void MarkFailed() => Failed = true;

        public void NeedUpdate() => Update = true;
    }
}
namespace ImageOrganizer.BL.Provider
{
    public interface IDownloadDispatcher
    {
        void Dispatch();
        IDownloadEntry Get(DownloadItem item);
    }
}
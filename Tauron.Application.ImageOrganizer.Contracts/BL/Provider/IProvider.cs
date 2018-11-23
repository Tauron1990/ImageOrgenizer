namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IProvider
    {
        string Id { get; }

        bool IsValid(string file);
        bool IsValidUrl(string url);

        void FillInfo(IDownloadEntry entry);
        void ShowUrl(string name);
    }
}
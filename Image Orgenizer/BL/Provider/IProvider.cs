using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.BL.Provider
{
    public interface IProvider
    {
        string Id { get; }

        bool IsValid(string file);
        bool IsValidUrl(string url);

        bool FillInfo(ImageData image, DownloadType downloadItemDownloadType, Operator op, out bool ok);
    }
}
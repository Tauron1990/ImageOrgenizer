namespace Tauron.Application.ImageOrganizer.BL.Services
{
    public interface IDownloadService
    {
        void DownloadFailed(DownloadItem item);
        bool AddFile(AddFileInput input);
        void DownloadCompled(DownloadItem downloadItem);
        void UpdateTagType(TagTypeData[] tagTypeDatas);
        void UpdateTag(UpdateTagInput updateTagInput);
        void UpdateImage(ImageData[] imageDatas);
        DownloadItem[] ScheduleDownload(DownloadItem[] downloadItems);
        ImageData GetImageData(string image);
        TagData GetTag(string tag);
        DownloadItem[] GetDownloadItems(GetDownloadItemInput getDownloadItemInput);
        bool HasTagType(string name);
        bool HasTag(string name);
    }
}
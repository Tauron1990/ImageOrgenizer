using Tauron.Application.Common.MVVM.Dynamic;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    [Export(typeof(IDownloadService))]
    [CreateRuleCall]
    public abstract class DownloadService : IDownloadService
    {
        [BindRule]
        public abstract void DownloadFailed(DownloadItem item);
        [BindRule]
        public abstract bool AddFile(AddFileInput input);
        [BindRule]
        public abstract void DownloadCompled(DownloadItem downloadItem);
        [BindRule]
        public abstract void UpdateTagType(TagTypeData[] tagTypeDatas);
        [BindRule]
        public abstract void UpdateTag(UpdateTagInput updateTagInput);
        [BindRule]
        public abstract void UpdateImage(ImageData[] imageDatas);
        [BindRule]
        public abstract DownloadItem[] ScheduleDownload(DownloadItem[] downloadItems);
        [BindRule]
        public abstract ImageData GetImageData(string image);
        [BindRule]
        public abstract TagData GetTag(string tag);
        [BindRule]
        public abstract DownloadItem[] GetDownloadItems(GetDownloadItemInput getDownloadItemInput);
        [BindRule]
        public abstract bool HasTagType(string name);
        [BindRule]
        public abstract bool HasTag(string name);
        [BindRule]
        public abstract int GetDownloadCount();
        [BindRule]
        public abstract bool ScheduleRedownload(string name);
        [BindRule]
        public abstract void StartDownloads();
    }
}
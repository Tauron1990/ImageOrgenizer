using System;
using System.IO;
using System.Threading.Tasks;

namespace Tauron.Application.ImageOrganizer.BL
{
    public interface IOperator
    {
        Task<PagerOutput> GetNextImages(PagerInput input);
        void IncreaseViewCount(IncreaseViewCountInput name);
        TagElement GetTagFilterElement(string name);
        bool UpdateDatabase(string database);
        Stream GetFile(string fileName);
        Task<Exception> ImportFiles(ImporterInput input);
        DownloadItem[] GetDownloadItems(GetDownloadItemInput input);
        Task<ImageData> GetImageData(string name);
        void DownloadCompled(DownloadItem item);
        Task<DownloadItem[]> ScheduleDownload(params DownloadItem[] item);
        void DownloadFailed(DownloadItem item);
        Task<bool> AddFile(AddFileInput input);
        Task<bool> ScheduleRedownload(string name);
        void DeleteImage(string name);
        Task<ImageData[]> UpdateImage(params ImageData[] data);
        Task StartDownloads();
        int GetDownloadCount();
        Task<PagerOutput> GetRandomImages(PagerInput pager);
        void MarkFavorite(ImageData data);
        RawSqlResult ExecuteRawSql(string input);
        AllDataResult GetAllData(DataType type);
        Task<bool> RemoveImage(ImageData data);
        Task<TagData> UpdateTag(UpdateTagInput data);
        Task<TagData> GetTag(string name);
        Task<bool> RemoveTag(TagData data);
        Task<TagTypeData[]> UpdateTagType(TagTypeData[] data);
        Task<TagTypeData> GetTagTypeData(string name);
        Task<bool> RemoveTagType(TagTypeData data);
        Task Defrag(DefragInput input);
        void Recuvery(RecuveryInput input);
        Task<SwitchContainerOutput> SwitchContainer(SwitchContainerInput input);
        ProfileData SearchLocation(string name);
        void SpecialUpdateImage(ImageData data);
        bool ReplaceImage(ReplaceImageInput input);
        Task<bool> HasTag(string name);
        Task<bool> HasTagType(string name);
        int GetImageCount();

        void RunOperatorTask(Action action);
    }
}
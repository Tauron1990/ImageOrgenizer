using System;
using System.IO;
using System.Threading.Tasks;
using ImageOrganizer.BL.Operations;
using Tauron.Application;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.BusinessLayer;
using Tauron.Application.Ioc;
using TaskScheduler = Tauron.Application.TaskScheduler;

namespace ImageOrganizer.BL
{
    [Export(typeof(Operator))]
    public class Operator : INotifyBuildCompled, IDisposable
    {
        private readonly TaskScheduler _taskScheduler = new TaskScheduler(UiSynchronize.Synchronize);
        private IIOBusinessRule<string, TagFilterElement> _getTagFilterElement;
        private IIBusinessRule<IncreaseViewCountInput> _increaseViewCountRule;
        private IIOBusinessRule<string, Stream> _getFile;
        private IIOBusinessRule<PagerInput, PagerOutput> _pagerRule;
        private IIBusinessRule<string> _updateDatabaseRule;
        private IIOBusinessRule<ImporterInput, Exception> _importFiles;
        private IIOBusinessRule<bool, DownloadItem[]> _getDonwloadItems;
        private IIOBusinessRule<string, ImageData> _getImageData;
        private IIBusinessRule<DownloadItem> _downloadCompled;
        private IIBusinessRule<DownloadItem> _scheduleDownload;
        private IIBusinessRule<DownloadItem> _downloadFailed;
        private IIBusinessRule<AddFileInput> _addFileRule;
        private IIOBusinessRule<string, bool> _scheduleRedownload;
        private IIBusinessRule<string> _delteImage;
        private IIOBusinessRule<ImageData, ImageData> _updateImage;
        private IBusinessRule _startDownloads;
        private IOBussinesRule<int> _getDownloadCount;
        private IIOBusinessRule<PagerInput, PagerOutput> _getRandomImages;
        private IIBusinessRule<ImageData> _markFavorite;
        private IIOBusinessRule<string, RawSqlResult> _executeRawSql;
        private IIOBusinessRule<DataType, AllDataResult> _getAllData;
        private IIOBusinessRule<ImageData, bool> _removeImage;
        private IIOBusinessRule<TagData, TagData> _updateTagData;
        private IIOBusinessRule<string, TagData> _getTag;
        private IIOBusinessRule<TagData, bool> _removeTag;
        private IIOBusinessRule<TagTypeData, TagTypeData> _updateTagType;
        private IIOBusinessRule<string, TagTypeData> _getTagType;
        private IIOBusinessRule<TagTypeData, bool> _removeTagType;
        private IIBusinessRule<DefragInput> _defrag;
        private IIBusinessRule<RecuveryInput> _recuvery;
        private IIOBusinessRule<SwitchContainerInput, SwitchContainerOutput> _switchContainer;

        [InjectRuleFactory]
        public RuleFactory RuleFactory { private get; set; }
        
        public void Dispose()
        {
            try
            {
                _taskScheduler?.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        void INotifyBuildCompled.BuildCompled()
        {
            _pagerRule = RuleFactory.CreateIioBusinessRule<PagerInput, PagerOutput>(RuleNames.Pager);
            _increaseViewCountRule = RuleFactory.CreateIiBusinessRule<IncreaseViewCountInput>(RuleNames.IncreaseViewCount);
            _getTagFilterElement = RuleFactory.CreateIioBusinessRule<string, TagFilterElement>(RuleNames.GetFilterTag);
            _updateDatabaseRule = RuleFactory.CreateIiBusinessRule<string>(RuleNames.UpdateDatabase);
            _getFile = RuleFactory.CreateIioBusinessRule<string, Stream>(RuleNames.GetFile);
            _importFiles = RuleFactory.CreateIioBusinessRule<ImporterInput, Exception>(RuleNames.FileImporter);
            _getDonwloadItems = RuleFactory.CreateIioBusinessRule<bool, DownloadItem[]>(RuleNames.GetDownloadItems);
            _getImageData = RuleFactory.CreateIioBusinessRule<string, ImageData>(RuleNames.GetImageData);
            _downloadCompled = RuleFactory.CreateIiBusinessRule<DownloadItem>(RuleNames.DownloadCompled);
            _scheduleDownload = RuleFactory.CreateIiBusinessRule<DownloadItem>(RuleNames.ScheduleDonwnload);
            _downloadFailed = RuleFactory.CreateIiBusinessRule<DownloadItem>(RuleNames.DownloadFailed);
            _addFileRule = RuleFactory.CreateIiBusinessRule<AddFileInput>(RuleNames.AddFile);
            _scheduleRedownload = RuleFactory.CreateIioBusinessRule<string, bool>(RuleNames.ScheduleRedownload);
            _delteImage = RuleFactory.CreateIiBusinessRule<string>(RuleNames.DeleteImage);
            _updateImage = RuleFactory.CreateIioBusinessRule<ImageData, ImageData>(RuleNames.UpdateImage);
            _startDownloads = RuleFactory.CreateBusinessRule(RuleNames.StartDownloads);
            _getDownloadCount = RuleFactory.CreateOBussinesRule<int>(RuleNames.GetDownloadCount);
            _getRandomImages = RuleFactory.CreateIioBusinessRule<PagerInput, PagerOutput>(RuleNames.RandomPager);
            _markFavorite = RuleFactory.CreateIiBusinessRule<ImageData>(RuleNames.MarkFavorite);
            _executeRawSql = RuleFactory.CreateIioBusinessRule<string, RawSqlResult>(RuleNames.ExecuteRawSql);
            _getAllData = RuleFactory.CreateIioBusinessRule<DataType, AllDataResult>(RuleNames.GetAllData);
            _removeImage = RuleFactory.CreateIioBusinessRule<ImageData, bool>(RuleNames.RemoveImage);
            _updateTagData = RuleFactory.CreateIioBusinessRule<TagData, TagData>(RuleNames.UpdateTag);
            _getTag = RuleFactory.CreateIioBusinessRule<string, TagData>(RuleNames.GetTag);
            _removeTag = RuleFactory.CreateIioBusinessRule<TagData, bool>(RuleNames.RemoveTag);
            _updateTagType = RuleFactory.CreateIioBusinessRule<TagTypeData, TagTypeData>(RuleNames.UpdateTagType);
            _getTagType = RuleFactory.CreateIioBusinessRule<string, TagTypeData>(RuleNames.GetTagType);
            _removeTagType = RuleFactory.CreateIioBusinessRule<TagTypeData, bool>(RuleNames.RemoveTagType);
            _defrag = RuleFactory.CreateIiBusinessRule<DefragInput>(RuleNames.Defrag);
            _recuvery = RuleFactory.CreateIiBusinessRule<RecuveryInput>(RuleNames.Recuvery);
            _switchContainer = RuleFactory.CreateIioBusinessRule<SwitchContainerInput, SwitchContainerOutput>(RuleNames.SwitchContainer);

            _taskScheduler.Start();
        }

        public Task<PagerOutput> GetNextImages(PagerInput input) => QueuePrivate(() => _pagerRule.Action(input));

        public void IncreaseViewCount(IncreaseViewCountInput name) => QueuePrivate(() => _increaseViewCountRule.Action(name));

        public TagFilterElement GetTagFilterElement(string name) => QueuePrivate(() => _getTagFilterElement.Action(name)).Result;

        public void UpdateDatabase(string database) => QueuePrivate(() => _updateDatabaseRule.Action(database)).Wait();

        public Stream GetFile(string fileName) => QueuePrivate(() => _getFile.Action(fileName)).Result;

        public Task<Exception> ImportFiles(ImporterInput input) => QueuePrivate(() => _importFiles.Action(input));

        public DownloadItem[] GetDownloadItems(bool fetchall) => QueuePrivate(() => _getDonwloadItems.Action(fetchall)).Result;

        public Task<ImageData> GetImageData(string name) => QueuePrivate(() => _getImageData.Action(name));

        public void DownloadCompled(DownloadItem item) => QueuePrivate(() => _downloadCompled.Action(item));

        public void ScheduleDownload(DownloadItem item) => QueuePrivate(() => _scheduleDownload.Action(item));

        public void DownloadFailed(DownloadItem item) => QueuePrivate(() => _downloadFailed.Action(item));

        public void AddFile(AddFileInput input) => QueuePrivate(() => _addFileRule.Action(input));

        public Task<bool> ScheduleRedownload(string name) => QueuePrivate(() => _scheduleRedownload.Action(name));

        public void DeleteImage(string name) => QueuePrivate(() => _delteImage.Action(name));

        public Task<ImageData> UpdateImage(ImageData data) => QueuePrivate(() => _updateImage.Action(data));

        public void StartDownloads() => QueuePrivate(_startDownloads.Action);

        public int GetDownloadCount() => QueuePrivate(() => _getDownloadCount.Action()).Result;

        public Task<PagerOutput> GetRandomImages(PagerInput pager) => QueuePrivate(() => _getRandomImages.Action(pager));

        public void MarkFavorite(ImageData data) => QueuePrivate(() => _markFavorite.Action(data));

        public RawSqlResult ExecuteRawSql(string input) => QueuePrivate(() => _executeRawSql.Action(input)).Result;

        public AllDataResult GetAllData(DataType type) => QueuePrivate(() => _getAllData.Action(type)).Result;

        public Task<bool> RemoveImage(ImageData data) => QueuePrivate(() => _removeImage.Action(data));

        public Task<TagData> UpdateTag(TagData data) => QueuePrivate(() => _updateTagData.Action(data));

        public Task<TagData> GetTag(string name) => QueuePrivate(() => _getTag.Action(name));

        public Task<bool> RemoveTag(TagData data) => QueuePrivate(() => _removeTag.Action(data));

        public Task<TagTypeData> UpdateTagType(TagTypeData data) => QueuePrivate(() => _updateTagType.Action(data));

        public Task<TagTypeData> GetTagTypeData(string name) => QueuePrivate(() => _getTagType.Action(name));

        public Task<bool> RemoveTagType(TagTypeData data) => QueuePrivate(() => _removeTagType.Action(data));

        public Task Defrag(DefragInput input) => QueuePrivate(() => _defrag.Action(input));

        public void Recuvery(RecuveryInput input) => QueuePrivate(() => _recuvery.Action(input)).Wait();

        public Task<SwitchContainerOutput> SwitchContainer(SwitchContainerInput input) => QueuePrivate(() => _switchContainer.Action(input));

        private Task<T> QueuePrivate<T>(Func<T> func)
        {
            ITask task = new UserResultTask<T>(func, false);
            _taskScheduler.QueueTask(task);
            return (Task<T>) task.Task;
        }

        private Task QueuePrivate(Action func)
        {
            ITask task = new UserTask(func, false);
            _taskScheduler.QueueTask(task);
            return task.Task;
        }
    }
}
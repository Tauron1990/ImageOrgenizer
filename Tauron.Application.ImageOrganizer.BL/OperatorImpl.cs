using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.BusinessLayer;
using Tauron.Application.ImageOrganizer.BL.Operations;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL
{
    [Export(typeof(IOperator))]
    [DebuggerStepThrough]
    public class OperatorImpl : INotifyBuildCompled, IDisposable, IOperator
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly TaskScheduler _taskScheduler = new TaskScheduler(UiSynchronize.Synchronize, "Operator Thread");
        private IIOBusinessRule<string, TagElement> _getTagFilterElement;
        private IIBusinessRule<IncreaseViewCountInput> _increaseViewCountRule;
        private IIOBusinessRule<string, Stream> _getFile;
        private IIOBusinessRule<PagerInput, PagerOutput> _pagerRule;
        private IIOBusinessRule<string, bool> _updateDatabaseRule;
        private IIOBusinessRule<ImporterInput, Exception> _importFiles;
        private IIOBusinessRule<GetDownloadItemInput, DownloadItem[]> _getDonwloadItems;
        private IIOBusinessRule<string, ImageData> _getImageData;
        private IIBusinessRule<DownloadItem> _downloadCompled;
        private IIOBusinessRule<DownloadItem[], DownloadItem[]> _scheduleDownload;
        private IIBusinessRule<DownloadItem> _downloadFailed;
        private IIOBusinessRule<AddFileInput, bool> _addFileRule;
        private IIOBusinessRule<string, bool> _scheduleRedownload;
        private IIBusinessRule<string> _delteImage;
        private IIOBusinessRule<ImageData[], ImageData[]> _updateImage;
        private IBusinessRule _startDownloads;
        private IOBussinesRule<int> _getDownloadCount;
        private IIOBusinessRule<PagerInput, PagerOutput> _getRandomImages;
        private IIBusinessRule<ImageData> _markFavorite;
        private IIOBusinessRule<string, RawSqlResult> _executeRawSql;
        private IIOBusinessRule<DataType, AllDataResult> _getAllData;
        private IIOBusinessRule<ImageData, bool> _removeImage;
        private IIOBusinessRule<UpdateTagInput, TagData> _updateTagData;
        private IIOBusinessRule<string, TagData> _getTag;
        private IIOBusinessRule<TagData, bool> _removeTag;
        private IIOBusinessRule<TagTypeData[], TagTypeData[]> _updateTagType;
        private IIOBusinessRule<string, TagTypeData> _getTagType;
        private IIOBusinessRule<TagTypeData, bool> _removeTagType;
        private IIBusinessRule<DefragInput> _defrag;
        private IIBusinessRule<RecuveryInput> _recuvery;
        private IIOBusinessRule<SwitchContainerInput, SwitchContainerOutput> _switchContainer;
        private IIOBusinessRule<string, ProfileData> _searchLocation;
        private IIBusinessRule<ImageData> _specialUpdateImage;
        private IIOBusinessRule<ReplaceImageInput, bool> _replaceImage;
        private IIOBusinessRule<string, bool> _hasTag;
        private IIOBusinessRule<string, bool> _hasTagType;

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
            _getTagFilterElement = RuleFactory.CreateIioBusinessRule<string, TagElement>(RuleNames.GetFilterTag);
            _updateDatabaseRule = RuleFactory.CreateIioBusinessRule<string, bool>(RuleNames.UpdateDatabase);
            _getFile = RuleFactory.CreateIioBusinessRule<string, Stream>(RuleNames.GetFile);
            _importFiles = RuleFactory.CreateIioBusinessRule<ImporterInput, Exception>(RuleNames.FileImporter);
            _getDonwloadItems = RuleFactory.CreateIioBusinessRule<GetDownloadItemInput, DownloadItem[]>(RuleNames.GetDownloadItems);
            _getImageData = RuleFactory.CreateIioBusinessRule<string, ImageData>(RuleNames.GetImageData);
            _downloadCompled = RuleFactory.CreateIiBusinessRule<DownloadItem>(RuleNames.DownloadCompled);
            _scheduleDownload = RuleFactory.CreateIioBusinessRule<DownloadItem[], DownloadItem[]>(RuleNames.ScheduleDonwnload);
            _downloadFailed = RuleFactory.CreateIiBusinessRule<DownloadItem>(RuleNames.DownloadFailed);
            _addFileRule = RuleFactory.CreateIioBusinessRule<AddFileInput, bool>(RuleNames.AddFile);
            _scheduleRedownload = RuleFactory.CreateIioBusinessRule<string, bool>(RuleNames.ScheduleRedownload);
            _delteImage = RuleFactory.CreateIiBusinessRule<string>(RuleNames.DeleteImage);
            _updateImage = RuleFactory.CreateIioBusinessRule<ImageData[], ImageData[]>(RuleNames.UpdateImage);
            _startDownloads = RuleFactory.CreateBusinessRule(RuleNames.StartDownloads);
            _getDownloadCount = RuleFactory.CreateOBussinesRule<int>(RuleNames.GetDownloadCount);
            _getRandomImages = RuleFactory.CreateIioBusinessRule<PagerInput, PagerOutput>(RuleNames.RandomPager);
            _markFavorite = RuleFactory.CreateIiBusinessRule<ImageData>(RuleNames.MarkFavorite);
            _executeRawSql = RuleFactory.CreateIioBusinessRule<string, RawSqlResult>(RuleNames.ExecuteRawSql);
            _getAllData = RuleFactory.CreateIioBusinessRule<DataType, AllDataResult>(RuleNames.GetAllData);
            _removeImage = RuleFactory.CreateIioBusinessRule<ImageData, bool>(RuleNames.RemoveImage);
            _updateTagData = RuleFactory.CreateIioBusinessRule<UpdateTagInput, TagData>(RuleNames.UpdateTag);
            _getTag = RuleFactory.CreateIioBusinessRule<string, TagData>(RuleNames.GetTag);
            _removeTag = RuleFactory.CreateIioBusinessRule<TagData, bool>(RuleNames.RemoveTag);
            _updateTagType = RuleFactory.CreateIioBusinessRule<TagTypeData[], TagTypeData[]>(RuleNames.UpdateTagType);
            _getTagType = RuleFactory.CreateIioBusinessRule<string, TagTypeData>(RuleNames.GetTagType);
            _removeTagType = RuleFactory.CreateIioBusinessRule<TagTypeData, bool>(RuleNames.RemoveTagType);
            _defrag = RuleFactory.CreateIiBusinessRule<DefragInput>(RuleNames.Defrag);
            _recuvery = RuleFactory.CreateIiBusinessRule<RecuveryInput>(RuleNames.Recuvery);
            _switchContainer = RuleFactory.CreateIioBusinessRule<SwitchContainerInput, SwitchContainerOutput>(RuleNames.SwitchContainer);
            _searchLocation = RuleFactory.CreateIioBusinessRule<string, ProfileData>(RuleNames.SearchLocation);
            _specialUpdateImage = RuleFactory.CreateIiBusinessRule<ImageData>(RuleNames.SpecialUpdateImage);
            _replaceImage = RuleFactory.CreateIioBusinessRule<ReplaceImageInput, bool>(RuleNames.ReplaceImage);
            _hasTag = RuleFactory.CreateIioBusinessRule<string, bool>(RuleNames.HasTag);
            _hasTagType = RuleFactory.CreateIioBusinessRule<string, bool>(RuleNames.HasTagType);

            _taskScheduler.Start();
        }

        public Task<PagerOutput> GetNextImages(PagerInput input) => QueuePrivate(() => _pagerRule.Action(input), _pagerRule);

        public void IncreaseViewCount(IncreaseViewCountInput name) => QueuePrivate(() => _increaseViewCountRule.Action(name), _increaseViewCountRule);

        public TagElement GetTagFilterElement(string name) => QueuePrivate(() => _getTagFilterElement.Action(name), _getTagFilterElement).Result;

        public bool UpdateDatabase(string database) => QueuePrivate(() => _updateDatabaseRule.Action(database), _updateDatabaseRule).Result;

        public Stream GetFile(string fileName) => _getFile.Action(fileName);

        public Task<Exception> ImportFiles(ImporterInput input) => QueuePrivate(() => _importFiles.Action(input), _importFiles);

        public DownloadItem[] GetDownloadItems(GetDownloadItemInput input) => QueuePrivate(() => _getDonwloadItems.Action(input), _getDonwloadItems).Result;

        public Task<ImageData> GetImageData(string name) => QueuePrivate(() => _getImageData.Action(name), _getImageData);

        public void DownloadCompled(DownloadItem item) => QueuePrivate(() => _downloadCompled.Action(item), _downloadCompled);

        public Task<DownloadItem[]> ScheduleDownload(params DownloadItem[] item) => QueuePrivate(() => _scheduleDownload.Action(item), _scheduleDownload);

        public void DownloadFailed(DownloadItem item) => QueuePrivate(() => _downloadFailed.Action(item), _downloadFailed);

        public Task<bool> AddFile(AddFileInput input) => QueuePrivate(() => _addFileRule.Action(input), _addFileRule);

        public Task<bool> ScheduleRedownload(string name) => QueuePrivate(() => _scheduleRedownload.Action(name), _scheduleRedownload);

        public void DeleteImage(string name) => QueuePrivate(() => _delteImage.Action(name), _delteImage);

        public Task<ImageData[]> UpdateImage(params ImageData[] data) => QueuePrivate(() => _updateImage.Action(data), _updateImage);

        public Task StartDownloads() => QueuePrivate(_startDownloads.Action, _startDownloads);

        public int GetDownloadCount() => QueuePrivate(() => _getDownloadCount.Action(), _getDownloadCount).Result;

        public Task<PagerOutput> GetRandomImages(PagerInput pager) => QueuePrivate(() => _getRandomImages.Action(pager), _getRandomImages);

        public void MarkFavorite(ImageData data) => QueuePrivate(() => _markFavorite.Action(data), _markFavorite);

        public RawSqlResult ExecuteRawSql(string input) => QueuePrivate(() => _executeRawSql.Action(input), _executeRawSql).Result;

        public AllDataResult GetAllData(DataType type) => QueuePrivate(() => _getAllData.Action(type), _getAllData).Result;

        public Task<bool> RemoveImage(ImageData data) => QueuePrivate(() => _removeImage.Action(data), _removeImage);

        public Task<TagData> UpdateTag(UpdateTagInput data) => QueuePrivate(() => _updateTagData.Action(data), _updateTagData);

        public Task<TagData> GetTag(string name) => QueuePrivate(() => _getTag.Action(name), _getTag);

        public Task<bool> RemoveTag(TagData data) => QueuePrivate(() => _removeTag.Action(data), _removeTag);

        public Task<TagTypeData[]> UpdateTagType(TagTypeData[] data) => QueuePrivate(() => _updateTagType.Action(data), _updateTagType);

        public Task<TagTypeData> GetTagTypeData(string name) => QueuePrivate(() => _getTagType.Action(name), _getTagType);

        public Task<bool> RemoveTagType(TagTypeData data) => QueuePrivate(() => _removeTagType.Action(data), _removeTagType);

        public Task Defrag(DefragInput input) => QueuePrivate(() => _defrag.Action(input), _defrag);

        public void Recuvery(RecuveryInput input) => QueuePrivate(() => _recuvery.Action(input), _recuvery).Wait();

        public Task<SwitchContainerOutput> SwitchContainer(SwitchContainerInput input) => QueuePrivate(() => _switchContainer.Action(input), _switchContainer);

        public ProfileData SearchLocation(string name) => QueuePrivate(() => _searchLocation.Action(name), _searchLocation).Result;

        public void SpecialUpdateImage(ImageData data) => QueuePrivate(() => _specialUpdateImage.Action(data), _specialUpdateImage);

        public bool ReplaceImage(ReplaceImageInput input) => QueuePrivate(() => _replaceImage.Action(input), _replaceImage).Result;

        public Task<bool> HasTag(string name) => QueuePrivate(() => _hasTag.Action(name), _hasTag);

        public Task<bool> HasTagType(string name) => QueuePrivate(() => _hasTagType.Action(name), _hasTagType);

        [DebuggerStepThrough]
        private Task<T> QueuePrivate<T>(Func<T> func, IRuleBase rule)
        {
            ITask task = new UserResultTask<T>(() =>
            {
                _logger.Info($"Enter Rule {rule.GetType()}");
                var result = func();
                TryLogError(rule);
                return result;
            }, false);
            _taskScheduler.QueueTask(task);
            return (Task<T>) task.Task;
        }

        [DebuggerStepThrough]
        private Task QueuePrivate(Action func, IRuleBase rule)
        {
            ITask task = new UserTask(() =>
            {
                _logger.Info($"Enter Rule {rule.GetType()}");
                func();
                TryLogError(rule);
            }, false);
            _taskScheduler.QueueTask(task);
            return task.Task;
        }

        private void TryLogError(IRuleBase rule)
        {
            var error = rule.Errors?.FirstOrDefault();
            if(error == null) return;

            _logger.Error($"Rule:{rule.GetType()} -- {error}");
        }
    }
}
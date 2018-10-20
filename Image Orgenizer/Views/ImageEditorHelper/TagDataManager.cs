using System.Threading.Tasks;
using ImageOrganizer.BL;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class TagDataManager : IOperationManager<TagData, TagDataItem>
    {
        private readonly Operator _operator;

        public TagDataManager(Operator @operator) => _operator = @operator;

        public TagDataItem CreatEditorItem(TagData rawData) => new TagDataItem(rawData);

        public Task<TagData> SendToDatabase(TagDataItem item) => _operator.UpdateTag(item.Create());

        public Task<TagData> FetchFromDatabase(TagDataItem item) => _operator.GetTag(item.Name);

        public Task<bool> RemoveFromDatabase(TagDataItem item) => _operator.RemoveTag(item.Create());
    }
}
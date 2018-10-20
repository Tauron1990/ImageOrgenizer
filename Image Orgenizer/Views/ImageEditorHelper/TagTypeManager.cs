using System.Threading.Tasks;
using ImageOrganizer.BL;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class TagTypeManager : IOperationManager<TagTypeData, TagTypeDataItem>
    {
        private readonly Operator _operator;

        public TagTypeManager(Operator @operator) => _operator = @operator;

        public TagTypeDataItem CreatEditorItem(TagTypeData rawData) => new TagTypeDataItem(rawData);

        public Task<TagTypeData> SendToDatabase(TagTypeDataItem item) => _operator.UpdateTagType(item.Create());

        public Task<TagTypeData> FetchFromDatabase(TagTypeDataItem item) => _operator.GetTagTypeData(item.Name);

        public Task<bool> RemoveFromDatabase(TagTypeDataItem item) => _operator.RemoveTagType(item.Create());
    }
}
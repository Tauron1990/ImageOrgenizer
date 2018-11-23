using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagTypeManager : IOperationManager<TagTypeData, TagTypeDataItem>
    {
        private readonly IOperator _operator;

        public TagTypeManager(IOperator @operator) => _operator = @operator;

        public TagTypeDataItem CreatEditorItem(TagTypeData rawData) => new TagTypeDataItem(rawData);

        public Task<TagTypeData> SendToDatabase(TagTypeDataItem item) => _operator.UpdateTagType(item.Create());

        public Task<TagTypeData> FetchFromDatabase(TagTypeDataItem item) => _operator.GetTagTypeData(item.Name);

        public Task<bool> RemoveFromDatabase(TagTypeDataItem item) => _operator.RemoveTagType(item.Create());
    }
}
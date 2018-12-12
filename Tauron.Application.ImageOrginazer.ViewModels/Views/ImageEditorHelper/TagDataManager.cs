using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagDataManager : IOperationManager<TagData, TagDataItem>
    {
        private readonly IOperator _operator;

        public TagDataManager(IOperator @operator) => _operator = @operator;

        public TagDataItem CreatEditorItem(TagData rawData) => new TagDataItem(rawData);

        public Task<TagData> SendToDatabase(TagDataItem item) => _operator.UpdateTag(new UpdateTagInput(item.Create(), false));

        public Task<TagData> FetchFromDatabase(TagDataItem item) => _operator.GetTag(item.Name);

        public Task<bool> RemoveFromDatabase(TagDataItem item) => _operator.RemoveTag(item.Create());
    }
}
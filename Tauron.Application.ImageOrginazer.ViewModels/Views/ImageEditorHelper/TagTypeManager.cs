using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagTypeManager : IOperationManager<TagTypeData, TagTypeDataItem>
    {
        private readonly IEditorService _operator;

        public TagTypeManager(IEditorService @operator) => _operator = @operator;

        public TagTypeDataItem CreatEditorItem(TagTypeData rawData) => new TagTypeDataItem(rawData);

        public Task<TagTypeData> SendToDatabase(TagTypeDataItem item) => Task.Run(() => _operator.UpdateTagType(new []{item.Create()})).ContinueWith(t => t.Result[0]);

        public Task<TagTypeData> FetchFromDatabase(TagTypeDataItem item) => Task.Run(() => _operator.GetTagTypeData(item.Name));

        public Task<bool> RemoveFromDatabase(TagTypeDataItem item) => Task.Run(() => _operator.RemoveTagType(item.Create()));
    }
}
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagDataManager : IOperationManager<TagData, TagDataItem>
    {
        private readonly IEditorService _operator;

        public TagDataManager(IEditorService @operator) => _operator = @operator;

        public TagDataItem CreatEditorItem(TagData rawData) => new TagDataItem(rawData);

        public Task<TagData> SendToDatabase(TagDataItem item) => Task.Run(() => _operator.UpdateTag(new UpdateTagInput(item.Create(), false)));

        public Task<TagData> FetchFromDatabase(TagDataItem item) => Task.Run(() => _operator.GetTag(item.Name));

        public Task<bool> RemoveFromDatabase(TagDataItem item) => Task.Run(() => _operator.RemoveTag(item.Create()));
    }
}
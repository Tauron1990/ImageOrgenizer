using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class ImageDataManager : IOperationManager<ImageData, ImageDataItem>
    {
        private readonly IEditorService _operator;

        public ImageDataManager(IEditorService @operator) => _operator = @operator;

        public ImageDataItem CreatEditorItem(ImageData rawData) => new ImageDataItem(rawData, _operator);

        public Task<ImageData> SendToDatabase(ImageDataItem item) => Task.Run(() => _operator.UpdateImage(new []{ item.Create() })).ContinueWith(t => t.Result[0]);

        public Task<ImageData> FetchFromDatabase(ImageDataItem item) => Task.Run(() => _operator.GetImageData(item.Name));

        public Task<bool> RemoveFromDatabase(ImageDataItem item) => Task.Run(() => _operator.RemoveImage(item.Create()));
    }
}
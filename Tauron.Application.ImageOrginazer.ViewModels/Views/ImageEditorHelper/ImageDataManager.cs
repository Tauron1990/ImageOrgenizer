using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class ImageDataManager : IOperationManager<ImageData, ImageDataItem>
    {
        private readonly IOperator _operator;

        public ImageDataManager(IOperator @operator) => _operator = @operator;

        public ImageDataItem CreatEditorItem(ImageData rawData) => new ImageDataItem(rawData, _operator);

        public Task<ImageData> SendToDatabase(ImageDataItem item) => _operator.UpdateImage(item.Create()).ContinueWith(t => t.Result[0]);

        public Task<ImageData> FetchFromDatabase(ImageDataItem item) => _operator.GetImageData(item.Name);

        public Task<bool> RemoveFromDatabase(ImageDataItem item) => _operator.RemoveImage(item.Create());
    }
}
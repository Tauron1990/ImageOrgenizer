using System.Threading.Tasks;
using ImageOrganizer.BL;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class ImageDataManager : IOperationManager<ImageData, ImageDataItem>
    {
        private readonly Operator _operator;

        public ImageDataManager(Operator @operator) => _operator = @operator;

        public ImageDataItem CreatEditorItem(ImageData rawData) => new ImageDataItem(rawData);

        public Task<ImageData> SendToDatabase(ImageDataItem item) => _operator.UpdateImage(item.Create()).ContinueWith(t => t.Result[0]);

        public Task<ImageData> FetchFromDatabase(ImageDataItem item) => _operator.GetImageData(item.Name);

        public Task<bool> RemoveFromDatabase(ImageDataItem item) => _operator.RemoveImage(item.Create());
    }
}
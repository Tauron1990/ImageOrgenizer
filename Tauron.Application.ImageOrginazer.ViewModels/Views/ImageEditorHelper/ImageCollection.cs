using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class ImageCollection : LiveDatCollection<ImageData, ImageDataItem>
    {
        public ImageCollection(IOperator op) : base(new ImageDataManager(op))
        {
        }
    }
}
using ImageOrganizer.BL;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class ImageCollection : LiveDatCollection<ImageData, ImageDataItem>
    {
        public ImageCollection(Operator op) : base(new ImageDataManager(op))
        {
        }
    }
}
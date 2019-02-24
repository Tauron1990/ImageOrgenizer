using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class ImageCollection : LiveDatCollection<ImageData, ImageDataItem>
    {
        public ImageCollection(IEditorService op) : base(new ImageDataManager(op))
        {
        }
    }
}
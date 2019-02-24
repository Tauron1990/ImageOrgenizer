
namespace Tauron.Application.ImageOrganizer.BL.Services
{
    public interface IEditorService
    {
        void SpecialUpdateImage(ImageData image);
        ImageData[] UpdateImage(ImageData[] imageData);
        ImageData GetImageData(string itemName);
        bool RemoveImage(ImageData imageData);
        TagData UpdateTag(UpdateTagInput tagInput);
        TagData GetTag(string itemName);
        bool RemoveTag(TagData tagData);
        TagTypeData[] UpdateTagType(TagTypeData[] tagTypeDatas);
        TagTypeData GetTagTypeData(string name);
        bool RemoveTagType(TagTypeData tagTypeData);
        AllDataResult GetAllData(DataType dataType);
    }
}
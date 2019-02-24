using System.IO;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    public interface IImageService
    {
        Stream GetFile(string imageDataName);
        RawSqlResult ExecuteRawSql(string sqlText);
        PagerOutput GetNextImages(PagerInput pagerInput);
        void IncreaseViewCount(IncreaseViewCountInput input);
        PagerOutput GetRandomImages(PagerInput pagerInput);
        int GetImageCount();
        bool ReplaceImage(ReplaceImageInput replaceImageInput);
        void DeleteImage(string imageName);
        void MarkFavorite(ImageData currentImage);
        ProfileData SearchLocation(string searchText);
        bool UpdateDatabase(string database);
        TagElement GetTagFilterElement(string name);
    }
}
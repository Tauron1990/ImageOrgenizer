using System.IO;
using Tauron.Application.Common.MVVM.Dynamic;
using Tauron.Application.ImageOrganizer.BL.Operations;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    [Export(typeof(IImageService))]
    [CreateRuleCall]
    public abstract class ImageService : IImageService
    {
        [BindRule]
        public abstract Stream GetFile(string imageDataName);
        
        [BindRule]
        public abstract RawSqlResult ExecuteRawSql(string sqlText);

        [BindRule(RuleNames.Pager)]
        public abstract PagerOutput GetNextImages(PagerInput pagerInput);

        [BindRule]
        public abstract void IncreaseViewCount(IncreaseViewCountInput input);

        [BindRule(RuleNames.RandomPager)]
        public abstract PagerOutput GetRandomImages(PagerInput pagerInput);

        [BindRule]
        public abstract int GetImageCount();

        [BindRule]
        public abstract bool ReplaceImage(ReplaceImageInput replaceImageInput);

        [BindRule]
        public abstract void DeleteImage(string imageName);

        [BindRule]
        public abstract void MarkFavorite(ImageData currentImage);

        [BindRule]
        public abstract ProfileData SearchLocation(string searchText);

        [BindRule]
        public abstract bool UpdateDatabase(string database);

        [BindRule(RuleNames.GetFilterTag)]
        public abstract TagElement GetTagFilterElement(string name);
    }
}
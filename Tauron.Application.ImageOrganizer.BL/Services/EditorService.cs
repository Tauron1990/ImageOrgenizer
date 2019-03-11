using Tauron.Application.Common.MVVM.Dynamic;
using Tauron.Application.ImageOrganizer.BL.Operations;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    [Export(typeof(IEditorService))]
    [CreateRuleCall]
    public abstract class EditorService : IEditorService
    {
        [BindRule]
        public abstract void SpecialUpdateImage(ImageData image);

        [BindRule]
        public abstract ImageData[] UpdateImage(ImageData[] imageData);

        [BindRule]
        public abstract ImageData GetImageData(string itemName);

        [BindRule]
        public abstract bool RemoveImage(ImageData imageData);

        [BindRule]
        public abstract TagData UpdateTag(UpdateTagInput tagInput);

        [BindRule]
        public abstract TagData GetTag(string itemName);

        [BindRule]
        public abstract bool RemoveTag(TagData tagData);

        [BindRule]
        public abstract TagTypeData[] UpdateTagType(TagTypeData[] tagTypeDatas);

        [BindRule(RuleNames.GetTagType)]
        public abstract TagTypeData GetTagTypeData(string name);

        [BindRule]
        public abstract bool RemoveTagType(TagTypeData tagTypeData);

        [BindRule(RuleNames.GetAllData)]
        public abstract AllDataResult GetAllData(DataType dataType);
    }
}
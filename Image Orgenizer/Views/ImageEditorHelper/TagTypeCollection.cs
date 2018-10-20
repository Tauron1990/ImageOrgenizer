using ImageOrganizer.BL;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class TagTypeCollection : LiveDatCollection<TagTypeData, TagTypeDataItem>
    {
        public TagTypeCollection(Operator op) : base(new TagTypeManager(op))
        {
        }
    }
}

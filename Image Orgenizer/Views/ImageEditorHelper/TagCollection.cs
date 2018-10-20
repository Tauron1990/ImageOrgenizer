using ImageOrganizer.BL;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class TagCollection : LiveDatCollection<TagData, TagDataItem>
    {
        public TagCollection(Operator op) : base(new TagDataManager(op))
        {
        }
    }
}
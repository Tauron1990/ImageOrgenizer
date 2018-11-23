using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagTypeCollection : LiveDatCollection<TagTypeData, TagTypeDataItem>
    {
        public TagTypeCollection(IOperator op) : base(new TagTypeManager(op))
        {
        }
    }
}

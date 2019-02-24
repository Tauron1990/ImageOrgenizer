using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagTypeCollection : LiveDatCollection<TagTypeData, TagTypeDataItem>
    {
        public TagTypeCollection(IEditorService op) : base(new TagTypeManager(op))
        {
        }
    }
}

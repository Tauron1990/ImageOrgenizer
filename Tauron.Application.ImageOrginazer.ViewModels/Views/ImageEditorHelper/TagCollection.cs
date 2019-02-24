using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.BL.Services;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagCollection : LiveDatCollection<TagData, TagDataItem>
    {
        public TagCollection(IEditorService op) : base(new TagDataManager(op))
        {
        }
    }
}
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagCollection : LiveDatCollection<TagData, TagDataItem>
    {
        public TagCollection(IOperator op) : base(new TagDataManager(op))
        {
        }
    }
}
//TODO Convert Color

using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class TagTypeDataItem : EditorItemBase, IEditorItem<TagTypeData>
    {
        public TagTypeDataItem()
        {
            IsNew = true;
            Color = "Black";
        }

        public TagTypeDataItem(TagTypeData data) => Update(data);

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Color
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool Equals(IEditorItem<TagTypeData> other) => Name == (other as TagTypeDataItem)?.Name;

        public bool Equals(TagTypeData other) => Name == other?.Name;

        public TagTypeData Create() => new TagTypeData(Name, Color);

        public void Update(Task<TagTypeData> data) => data.ContinueWith(t => Update(t.Result));
        
        private void Update(TagTypeData data)
        {
            using (EnterUpdateMode())
            {
                Name = data.Name;
                Color = data.Color;
            }
        }
    }
}
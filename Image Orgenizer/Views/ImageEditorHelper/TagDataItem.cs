using System.Collections.Generic;
using System.Threading.Tasks;
using ImageOrganizer.BL;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class TagDataItem : EditorItemBase, IEditorItem<TagData>
    {
        public TagDataItem() => IsNew = true;

        public TagDataItem(TagData data) => Update(data);

        public TagTypeData Type
        {
            get => GetValue<TagTypeData>();
            set => SetValue(value);
        }

        public string Description
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public List<string> Tags { get; set; }

        public bool Equals(IEditorItem<TagData> other) => Name == (other as TagDataItem)?.Name;

        public bool Equals(TagData other) => Name == other?.Name;

        public TagData Create() => new TagData(Type, Description, Name);

        public void Update(Task<TagData> data) => data.ContinueWith(t => Update(t.Result));

        private void Update(TagData data)
        {
            using (EnterUpdateMode())
            {
                IsNew = false;
                Tags = null;
                Type = data.Type;
                Name = data.Name;
                Description = data.Description;
            }
        }
    }
}
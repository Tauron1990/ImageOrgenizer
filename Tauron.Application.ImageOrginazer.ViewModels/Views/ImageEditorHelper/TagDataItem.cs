//TODO Convert AutoCompled

using System.Collections.Generic;
using System.Threading.Tasks;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.UI;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ImageEditorHelper
{
    public class  TagDataItem : EditorItemBase, IEditorItem<TagData>
    {
        private List<string> _tags;
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

        public List<string> Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                InternalAutoCompledMode = InternalAutoCompleteMode.Suggest;

                OnPropertyChanged();
                OnPropertyChanged(nameof(InternalAutoCompledMode));
            }
        }

        public InternalAutoCompleteMode InternalAutoCompledMode { get; set; }

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
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Media;
using ImageOrganizer.BL;
using Tauron;

namespace ImageOrganizer.Views.ImageEditorHelper
{
    public class TagTypeDataItem : EditorItemBase, IEditorItem<TagTypeData>
    {
        public TagTypeDataItem()
        {
            IsNew = true;
            Color = Colors.Black;
        }

        public TagTypeDataItem(TagTypeData data) => Update(data);

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public Color Color
        {
            get => GetValue<Color>();
            set => SetValue(value);
        }

        public bool Equals(IEditorItem<TagTypeData> other) => Name == (other as TagTypeDataItem)?.Name;

        public bool Equals(TagTypeData other) => Name == other?.Name;

        public TagTypeData Create() => new TagTypeData(Name, Color.ToString(CultureInfo.InvariantCulture));

        public void Update(Task<TagTypeData> data) => data.ContinueWith(t => Update(t.Result));
        
        private void Update(TagTypeData data)
        {
            using (EnterUpdateMode())
            {
                Name = data.Name;
                try
                {
                    Color = ColorConverter.ConvertFromString(data.Color).CastObj<Color>();
                }
                catch(FormatException)
                {
                    Color = Colors.Black;
                }
            }
        }
    }
}
using System.Windows.Input;
using ImageOrganizer.Data.Entities;
using JetBrains.Annotations;

namespace ImageOrganizer.BL
{
    public sealed class TagElement
    {
        public TagElement(string name, TagTypeData type, string description)
        {
            Name = name;
            Type = type;
            Description = description;
        }

        public TagElement(TagEntity entity)
            : this(entity.Id, new TagTypeData(entity.Type), entity.Description)
        {
        }

        public TagElement(TagData data)
            : this(data.Name, data.Type, data.Description)
        {
        }

        public string Name { get; }

        public TagTypeData Type { get; }

        public string Description { get; }

        [CanBeNull]
        public ICommand Click { get; set; }
    }
}
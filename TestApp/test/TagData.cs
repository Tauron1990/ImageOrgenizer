
namespace ImageOrganizer.BL
{
    public class TagData
    {
        public TagData(TagTypeData type, string description, string name)
        {
            Type = type;
            Description = description;
            Name = name;
        }



        public TagTypeData Type { get; }

        public string Description { get; }

        public string Name { get; }

    }
}
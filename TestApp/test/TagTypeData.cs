namespace ImageOrganizer.BL
{
    public class TagTypeData
    {
        public TagTypeData(string name, string color)
        {
            Name = name;
            Color = color;
        }

        public string Name { get; }

        public string Color { get; }
    }
}
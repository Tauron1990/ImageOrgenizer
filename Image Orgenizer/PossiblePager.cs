using JetBrains.Annotations;

namespace ImageOrganizer
{
    [UsedImplicitly]
    public class PossiblePager
    {
        public string Name { get; }

        public string DisplayName { get; }

        public PossiblePager(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }
}
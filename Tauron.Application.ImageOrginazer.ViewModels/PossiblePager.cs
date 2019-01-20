using JetBrains.Annotations;

namespace Tauron.Application.ImageOrginazer.ViewModels
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
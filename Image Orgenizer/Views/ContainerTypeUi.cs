using ImageOrganizer.BL;
using JetBrains.Annotations;

namespace ImageOrganizer.Views
{
    public class ContainerTypeUi
    {
        public ContainerType Type { get; }

        [UsedImplicitly]
        public string Display { get; }

        public ContainerTypeUi(ContainerType type, string display)
        {
            Type = type;
            Display = display;
        }
    }
}
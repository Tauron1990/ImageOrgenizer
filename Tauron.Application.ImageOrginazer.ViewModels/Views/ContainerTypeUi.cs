using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
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
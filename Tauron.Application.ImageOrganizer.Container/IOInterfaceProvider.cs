using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.Core.IO;

namespace Tauron.Application.ImageOrganizer.Container
{
    public class IOInterfaceProvider
    {
        private static IIOInterface _interface;

        [NotNull]
        public static IIOInterface IOInterface => _interface ?? (_interface = CommonApplication.Current.Container.Resolve<IIOInterface>());
    }
}
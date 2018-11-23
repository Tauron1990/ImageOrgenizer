using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.Container.Fluent;
using Tauron.Application.ImageOrganizer.Container.Fluent.Impl;

namespace Tauron.Application.ImageOrganizer.Container
{
    [PublicAPI]
    public static class ContainerFactory
    {
        public static IStart Begin() => new Start();
    }
}
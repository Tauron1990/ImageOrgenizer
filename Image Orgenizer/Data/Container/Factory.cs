using ImageOrganizer.Data.Container.Fluent;
using ImageOrganizer.Data.Container.Fluent.Impl;
using JetBrains.Annotations;

namespace ImageOrganizer.Data.Container
{
    [PublicAPI]
    public static class Factory
    {
        public static IStart Begin() => new Start();
    }
}
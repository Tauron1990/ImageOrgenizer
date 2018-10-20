using ImageOrganizer.Data.Container.Fluent;
using ImageOrganizer.Data.Container.Fluent.Impl;
using ImageOrganizer.Data.Container.MultiFile;
using ImageOrganizer.Data.Container.SingleFile;
using JetBrains.Annotations;

namespace ImageOrganizer.Data.Container
{
    [PublicAPI]
    public static class FluentExtensions
    {
        public static IInitializable UseSingle(this IStart start) => new Initializable(() => new ContainerFile());

        public static IInitializable UseMulti(this IStart start) => new Initializable(() => new MultiContainerFile());

        public static IComposeConfig UseCompose(this IStart start) => new ComposeConfig();

        public static IComposeConfig AddSingle(this IComposeConfig config)
        {
            ((ComposeConfig)config).Add("Single", () => (new ContainerFile(), string.Empty));
            return config;
        }

        public static IComposeConfig AddMulti(this IComposeConfig config, string custom)
        {
            ((ComposeConfig)config).Add("Multi" + custom, () => (new MultiContainerFile(), custom));
            return config;
        }
    }
}
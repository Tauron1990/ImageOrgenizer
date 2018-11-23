using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.Container.Fluent;
using Tauron.Application.ImageOrganizer.Container.Fluent.Impl;
using Tauron.Application.ImageOrganizer.Container.MultiFile;
using Tauron.Application.ImageOrganizer.Container.SingleFile;

namespace Tauron.Application.ImageOrganizer.Container
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
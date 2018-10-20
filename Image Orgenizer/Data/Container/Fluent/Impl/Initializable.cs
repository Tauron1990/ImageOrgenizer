using System;

namespace ImageOrganizer.Data.Container.Fluent.Impl
{
    public class Initializable : IInitializable
    {
        private readonly Func<IContainerFile> _builder;

        public Initializable(Func<IContainerFile> builder) => _builder = builder;

        public IContainerFile Initialize(string name)
        {
            var container = _builder();
            container.Initialize(name);
            return container;
        }
    }
}
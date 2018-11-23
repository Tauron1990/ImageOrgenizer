namespace Tauron.Application.ImageOrganizer.Container.Fluent
{
    public interface IInitializable
    {
        IContainerFile Initialize(string name);
    }
}
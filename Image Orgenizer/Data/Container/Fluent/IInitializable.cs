namespace ImageOrganizer.Data.Container.Fluent
{
    public interface IInitializable
    {
        IContainerFile Initialize(string name);
    }
}
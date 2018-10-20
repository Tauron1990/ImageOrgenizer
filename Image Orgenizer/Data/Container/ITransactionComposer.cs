namespace ImageOrganizer.Data.Container
{
    public interface ITransactionComposer
    {
        bool IsCompatible(IContainerTransaction transaction);
    }
}
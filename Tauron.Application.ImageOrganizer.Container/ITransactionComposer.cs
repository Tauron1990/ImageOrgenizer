namespace Tauron.Application.ImageOrganizer.Container
{
    public interface ITransactionComposer
    {
        bool IsCompatible(IContainerTransaction transaction);
    }
}
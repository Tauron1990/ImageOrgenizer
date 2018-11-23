using System;

namespace Tauron.Application.ImageOrganizer.Container.Compse
{
    public interface IComposerExpose : IContainerFile, ITransactionComposer
    {
        void AddFileCore(byte[] file, string name, IContainerTransaction transaction);
        void Compled(IContainerTransaction transaction);
        void ReadIndex();
        void RemoveImpl(string name, IContainerTransaction transaction);
        void StartRecuvery(Action<RecuverElement> recuveryElement);
    }
}
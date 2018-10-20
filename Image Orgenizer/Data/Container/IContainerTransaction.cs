using System;
using System.Transactions;
using JetBrains.Annotations;

namespace ImageOrganizer.Data.Container
{
    [PublicAPI]
    public interface IContainerTransaction : IDisposable
    {
        TTechno TryCast<TTechno>();

        void Commit();

        void Rollback();
    }
}
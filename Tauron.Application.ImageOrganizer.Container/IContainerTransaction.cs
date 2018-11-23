using System;
using JetBrains.Annotations;

namespace Tauron.Application.ImageOrganizer.Container
{
    [PublicAPI]
    public interface IContainerTransaction : IDisposable
    {
        TTechno TryCast<TTechno>();

        void Commit();

        void Rollback();
    }
}
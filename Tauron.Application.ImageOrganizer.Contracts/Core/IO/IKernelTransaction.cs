using System;

namespace Tauron.Application.ImageOrganizer.Core.IO
{
    public interface IKernelTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
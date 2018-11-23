using Alphaleonis.Win32.Filesystem;
using Tauron.Application.ImageOrganizer.Core.IO;

namespace Tauron.Application.IOInterface
{
    public sealed class KernalTransactionImpl : IKernelTransaction
    {
        public KernelTransaction Transaction { get; } = new KernelTransaction();

        public void Dispose() => Transaction.Dispose();

        public void Commit() => Transaction.Commit();

        public void Rollback() => Transaction.Rollback();
    }
}
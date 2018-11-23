using System.IO;
using Alphaleonis.Win32.Filesystem;
using Tauron.Application.ImageOrganizer.Core.IO;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;

namespace Tauron.Application.IOInterface
{
    public class FileInfoImpl : IFileInfo
    {
        private readonly FileInfo _fileInfo;

        public FileInfoImpl(IKernelTransaction transaction, string path)
        {
            KernelTransaction kernelTransaction = null;
            if (transaction is KernalTransactionImpl trans)
                kernelTransaction = trans.Transaction;

            _fileInfo = new FileInfo(kernelTransaction, path);
        }

        public Stream Open(FileMode mode, FileAccess fileAccess) => _fileInfo.Open(mode, fileAccess);

        public void Delete(bool ignoreReadonly)
        {
            _fileInfo.Delete(true);
        }

        public bool Exists => _fileInfo.Exists;
        public Stream Open(FileMode mode, FileAccess fileAccess, FileShare fileShare) => _fileInfo.Open(mode, fileAccess, fileShare);
    }
}
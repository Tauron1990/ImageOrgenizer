using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using Crc32C;
using Tauron.Application.ImageOrganizer.Core.IO;
using Tauron.Application.Ioc;

namespace Tauron.Application.IOInterface
{
    [Export(typeof(IIOInterface))]
    public class IOInterfaceImpl : IIOInterface
    {
        public IKernelTransaction CreateTransaction() => new KernalTransactionImpl();

        private KernelTransaction Convert(IKernelTransaction transaction)
        {
            if (transaction is KernalTransactionImpl impl) return impl.Transaction;

            return null;
        }

        public IFileInfo GetFileInfo(IKernelTransaction transaction, string path) => new FileInfoImpl(transaction, path);

        public uint Crc32Compute(byte[] bytes) => Crc32CAlgorithm.Compute(bytes);

        public IEnumerable<string> EnumerateFiles(string path, InternalDirectoryEnumerationOptions options, InternalPathFormat format) => Directory.EnumerateFiles(path, (DirectoryEnumerationOptions)(int)options, Convert(format));

        private PathFormat Convert(InternalPathFormat internalPathFormat)
        {
            switch (internalPathFormat)
            {
                case InternalPathFormat.RelativePath:
                    return PathFormat.RelativePath;
                case InternalPathFormat.FullPath:
                    return PathFormat.FullPath;
                case InternalPathFormat.LongFullPath:
                    return PathFormat.LongFullPath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(internalPathFormat), internalPathFormat, null);
            }
        }

        public long GetSize(string path) => File.GetSize(path);

        public void Delete(string path, bool recursive, bool ignoreReadOnly) => Directory.Delete(path, recursive, ignoreReadOnly);

        public ICopyMoveResult MoveTransacted(IKernelTransaction trans, string start, string dest) => new CopyMoveResultInterface(Directory.MoveTransacted(Convert(trans), start, dest));
        public void DeleteTransacted(IKernelTransaction trans, string location, bool recursive, bool ignoreReadOnly) => Directory.DeleteTransacted(Convert(trans), location, recursive, ignoreReadOnly);
        public void DeleteTransacted(IKernelTransaction trans, string location, bool recursive) => Directory.DeleteTransacted(Convert(trans), location, recursive);
    }
}
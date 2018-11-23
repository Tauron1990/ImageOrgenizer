using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.Core.IO
{
    public interface IIOInterface
    {
        IKernelTransaction CreateTransaction();
        IFileInfo GetFileInfo(IKernelTransaction transaction, string path);
        uint Crc32Compute(byte[] bytes);
        IEnumerable<string> EnumerateFiles(string path, InternalDirectoryEnumerationOptions options, InternalPathFormat format);
        long GetSize(string path);
        void Delete(string path, bool recursive, bool ignoreReadOnly);
        ICopyMoveResult MoveTransacted(IKernelTransaction trans, string start, string dest);
        void DeleteTransacted(IKernelTransaction trans, string location, bool recursive, bool ignoreReadOnly);
        void DeleteTransacted(IKernelTransaction trans, string location, bool recursive);
    }
}
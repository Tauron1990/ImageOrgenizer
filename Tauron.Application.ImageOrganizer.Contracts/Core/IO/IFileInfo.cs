using System.IO;

namespace Tauron.Application.ImageOrganizer.Core.IO
{
    public interface IFileInfo
    {
        Stream Open(FileMode mode, FileAccess fileAccess);
        void Delete(bool ignoreReadonly);
        bool Exists { get; }
        Stream Open(FileMode mode, FileAccess fileAccess, FileShare fileShare);
    }
}
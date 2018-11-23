using System.Collections.Generic;
using System.IO;

namespace Tauron.Application.ImageOrganizer.BL.Operations.Helper
{
    public sealed class NaturalFileInfoNameComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo a, FileInfo b) => a?.Name.CompareNumeric(b?.Name) ?? 0;
    }
}
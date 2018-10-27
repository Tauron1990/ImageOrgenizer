﻿using System.Collections.Generic;
using System.IO;

namespace ImageOrganizer.BL.Operations.Helper
{
    public sealed class NaturalFileInfoNameComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo a, FileInfo b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a.Name, b.Name);
        }
    }
}
using System;

namespace Tauron.Application.ImageOrganizer.Core.IO
{
    [Flags]
    public enum InternalDirectoryEnumerationOptions
    {
        None = 0,
        Files = 1,
        Folders = 2,
        FilesAndFolders = Folders | Files,
        AsLongPath = 4,
        SkipReparsePoints = 8,
        ContinueOnException = 16,
        Recursive = 32,
        BasicSearch = 64,
        LargeCache = 128, 
    }
}
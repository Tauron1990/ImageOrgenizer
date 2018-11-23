using System;

namespace Tauron.Application.ImageOrganizer.Core.IO
{
    public interface ICopyMoveResult
    {
        TimeSpan Duration { get; }

        string Destination { get; }

        int ErrorCode { get; }

        string ErrorMessage { get; }

        bool IsCanceled { get; }

        bool IsCopy { get; }

        bool IsDirectory { get; }

        bool IsEmulatedMove { get; }

        bool IsFile { get; }

        bool IsMove { get; }

        string Source { get; }

        bool TimestampsCopied { get; }

        long TotalBytes { get; }

        string TotalBytesUnitSize { get; }

        long TotalFiles { get; }

        long TotalFolders { get; }
    }
}
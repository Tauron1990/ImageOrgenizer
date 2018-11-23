using System;
using Alphaleonis.Win32.Filesystem;
using Tauron.Application.ImageOrganizer.Core.IO;

namespace Tauron.Application.IOInterface
{
    public class CopyMoveResultInterface : ICopyMoveResult
    {
        private readonly CopyMoveResult _result;

        public CopyMoveResultInterface(CopyMoveResult result) => _result = result;

        public TimeSpan Duration => _result.Duration;
        public string Destination => _result.Destination;
        public int ErrorCode => _result.ErrorCode;
        public string ErrorMessage => _result.ErrorMessage;
        public bool IsCanceled => _result.IsCanceled;
        public bool IsCopy => _result.IsCopy;
        public bool IsDirectory => _result.IsDirectory;
        public bool IsEmulatedMove => _result.IsEmulatedMove;
        public bool IsFile => _result.IsFile;
        public bool IsMove => _result.IsMove;
        public string Source => _result.Source;
        public bool TimestampsCopied => _result.TimestampsCopied;
        public long TotalBytes => _result.TotalBytes;
        public string TotalBytesUnitSize => _result.TotalBytesUnitSize;
        public long TotalFiles => _result.TotalFiles;
        public long TotalFolders => _result.TotalFolders;
    }
}
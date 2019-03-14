using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BITSReference10_2;
using JetBrains.Annotations;

namespace SharpBits.Base
{
    [PublicAPI]
    public class BitsFiles: List<BitsFile>, IDisposable
    {
        private IEnumBackgroundCopyFiles _fileList;
        private readonly BitsJob _job;
        private bool _disposed;

        internal BitsFiles(BitsJob job, IEnumBackgroundCopyFiles fileList)
        {
            _fileList = fileList;
            _job = job;
            Refresh();
        }

        private void Refresh()
        {
            _fileList.Reset();
            Clear();
            _fileList.GetCount(out var count);
            for (int i = 0; i < count; i++)
            {
                uint fetchedCount = 0;
                _fileList.Next(1, out IBackgroundCopyFile currentFile, ref fetchedCount);
                if (fetchedCount == 1)
                {
                    Add(new BitsFile(_job, currentFile));
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //TODO: release COM resource
                    Marshal.ReleaseComObject(_fileList);
                    _fileList = null;
                }
            }
            _disposed = true;
        }


        #endregion
    }
}

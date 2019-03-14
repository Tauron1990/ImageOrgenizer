using System;
using System.Runtime.InteropServices;
using BITSReference10_2;
using JetBrains.Annotations;

namespace SharpBits.Base
{
    [PublicAPI]
    public sealed class BitsFile
    {
        private IBackgroundCopyFile _file;
        private FileProgress _progress;
        private bool _disposed;
        private readonly BitsJob _job;

        internal BitsFile(BitsJob job, IBackgroundCopyFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file), "Parameter IBackgroundCopyFile cannot be a null reference");
            _job = job;
        }

        #region public properties
        public string LocalName
        {
            get
            {
                string name = string.Empty;
                try
                {
                    _file.GetLocalName(out name);
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return name;
            }
        }

        public string RemoteName
        {
            get
            {
                string name = string.Empty;
                try
                {
                    _file.GetRemoteName(out name);
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return name;
            }
            set //supported in IBackgroundCopyFile2
            {
                try
                {
                    if (_file2 != null)
                    {
                        _file2.SetRemoteName(value);
                    }
                    else
                    {
                        throw new NotSupportedException("IBackgroundCopyFile2");
                    }
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
            }
        }

        public FileProgress Progress
        {
            get
            {
                if (null != _progress) return _progress;
                try
                {
                    _file.GetProgress(out var fileProgress);
                    _progress = new FileProgress(fileProgress);
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return _progress;
            }
        }
        #endregion

        #region IDisposable Members


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Marshal.ReleaseComObject(_file);
                    _file = null;
                }
            }
            _disposed = true;
        }

        #endregion
    }
}

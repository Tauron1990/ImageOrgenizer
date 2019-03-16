using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using BITSReference10_2;

namespace SharpBits.Base
{
    public partial class BitsJob
    {
        private IBackgroundCopyJob3 job3;

        #region public properties

        #region IBackgroundCopyJob3
        public void ReplaceRemotePrefix(string oldPrefix, string newPrefix)
        {
            try
            {
                if (job3 != null)   // only supported from IBackgroundCopyJob3 and above
                {
                    job3.ReplaceRemotePrefix(oldPrefix, newPrefix);
                }
                else
                {
                    throw new NotSupportedException("IBackgroundCopyJob3");
                }
            }
            catch (COMException exception)
            {
                _manager.PublishException(this, exception);
            }
        }

        public void AddFileWithRanges(string remoteName, string localName, Collection<FileRange> fileRanges)
        {
            try
            {
                if (job3 != null && fileRanges != null) // only supported from IBackgroundCopyJob3 and above
                {
                    BG_FILE_RANGE[] ranges = new BG_FILE_RANGE[fileRanges.Count];
                    for (int i = 0; i < fileRanges.Count; i++)
                    {
                        ranges[i] = fileRanges[i]._BG_FILE_RANGE;
                    }
                    this.job3.AddFileWithRanges(remoteName, localName, (uint)fileRanges.Count, ranges); 
                }
                else
                {
                    throw new NotSupportedException("IBackgroundCopyJob3");
                }
            }
            catch (COMException exception)
            {
                _manager.PublishException(this, exception);
            }
        }

        public FileACLFlags FileACLFlags
        {
            get
            {
                FILE_ACL_FLAGS flags = 0;
                try
                {
                    if (this.job3 != null)// only supported from IBackgroundCopyJob3 and above
                    {
                        this.job3.GetFileACLFlags(out flags);
                    }
                    else
                    {
                        throw new NotSupportedException("IBackgroundCopyJob3");
                    }
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return (FileACLFlags)flags;
            }
            set
            {
                try
                {
                    if (this.job3 != null)// only supported from IBackgroundCopyJob3 and above
                    {
                        this.job3.SetFileACLFlags((FILE_ACL_FLAGS)value);
                    }
                    else
                    {
                        throw new NotSupportedException("IBackgroundCopyJob3");
                    }
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }
        }
        #endregion

        #endregion
    }
}
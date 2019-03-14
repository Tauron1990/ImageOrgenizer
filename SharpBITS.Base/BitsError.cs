using System;
using System.Threading;
using System.Runtime.InteropServices;
using BITSReference10_2;
using JetBrains.Annotations;

namespace SharpBits.Base
{
    [PublicAPI]
    public class BitsError
    {
        private readonly IBackgroundCopyError _error;
        private readonly BitsJob _job;

        internal BitsError(BitsJob job, IBackgroundCopyError error)
        {
            _error = error ?? throw new ArgumentNullException(nameof(error), "Parameter IBackgroundCopyError cannot be a null reference");
            _job = job;
        }

        public string Description
        {
            get  
            {
                string description = string.Empty;
                try
                {
                    _error.GetErrorDescription(Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID), out description);
                }
                catch (COMException exception)
                {                    
                    _job.PublishException(exception);
                }
                return description;
            }
        }

        public string ContextDescription
        {
            get
            {
                string description = string.Empty;
                try
                {
                    _error.GetErrorContextDescription(Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID), out description);
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return description;
            }
        }

        public string Protocol
        {
            get
            {
                string protocol = string.Empty;
                try
                {
                    _error.GetProtocol(out protocol);
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return protocol;
            }
        }

        public BitsFile File
        {
            get
            {
                try
                {
                    _error.GetFile(out var errorFile);
                    return new BitsFile(_job, errorFile);
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return null;    //couldn't create new job
            }
        }

        public ErrorContext ErrorContext
        {
            get 
            {
                try
                {
                    _error.GetError(out var context, out _);
                    return (ErrorContext)context;
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return ErrorContext.UnknownError;
            }
        }

        public int ErrorCode
        {
            get 
            {
                int errorCode = 0;
                try
                {
                    _error.GetError(out _, out errorCode);
                    return errorCode;
                }
                catch (COMException exception)
                {
                    _job.PublishException(exception);
                }
                return errorCode;
            }
        }

    }
}

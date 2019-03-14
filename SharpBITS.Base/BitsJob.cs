using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using BITSReference10_2;

namespace SharpBits.Base
{
    public partial class BitsJob: IDisposable
    {

        #region member fields
        private readonly BitsManager _manager;
        private bool _disposed;
        private JobTimes _jobTimes;
        private ProxySettings _proxySettings;
        private BitsError _error;
        private JobProgress _progress;
        private GUID? _guid;
        //notification
        internal readonly IBackgroundCopyCallback NotificationTarget;
        private EventHandler<JobNotificationEventArgs> _onJobModified;
        private EventHandler<JobNotificationEventArgs> _onJobTransfered;
        private EventHandler<JobErrorNotificationEventArgs> _onJobErrored;
        #endregion

        #region .ctor
        internal BitsJob(BitsManager manager, IBackgroundCopyJob job)
        {
            _manager = manager;
            Job = job;
            job2 = Job as IBackgroundCopyJob2;
            job3 = Job as IBackgroundCopyJob3;
            job4 = Job as IBackgroundCopyJob4;

            //store existing notification handler and route message to this as well
            //otherwise it may break system download jobs
            if (NotificationInterface != null)
                NotificationTarget = NotificationInterface; //pointer to the existing one;
            NotificationInterface = manager.NotificationHandler;   //notification interface will be disabled when NotifyCmd is set
        }
        #endregion

        #region public properties

        #region IBackgroundCopyJob
        /// <summary>
        /// Display Name, max 256 chars
        /// </summary>
        public string DisplayName
        {
            get
            {
                try
                {
                    Job.GetDisplayName(out var displayName);
                    return displayName;
                }
                catch (COMException exception)
                {                    
                    _manager.PublishException(this, exception);
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    Job.SetDisplayName(value);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }
        }

        /// <summary>
        /// Description, max 1024 chars
        /// </summary>
        public string Description
        {
            get
            {
                try
                {
                    Job.GetDescription(out var description);
                    return description;
                }
                catch (COMException exception)
                {                    
                    _manager.PublishException(this, exception);
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    Job.SetDescription(value);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }
        }

        /// <summary>
        /// SID of the job owner
        /// </summary>
        public string Owner
        {
            get
            {
                try
                {
                    Job.GetOwner(out var owner);
                    return owner;
                }
                catch (COMException exception)
                {                    
                    _manager.PublishException(this, exception);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// resolved owner name from job owner SID
        /// </summary>
        public string OwnerName
        {
            get 
            {
                try
                {
                    return Utils.GetName(Owner);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Job priority
        /// can not be set for jobs already in state Canceled or Acknowledged
        /// </summary>
        public JobPriority Priority
        {
            get
            {
                BG_JOB_PRIORITY priority = BG_JOB_PRIORITY.BG_JOB_PRIORITY_NORMAL;
                try
                {
                    Job.GetPriority(out priority);
                }
                catch (COMException exception)
                {                    
                    _manager.PublishException(this, exception);
                }
                return (JobPriority)priority;
            }
            set
            {
                try
                {
                    Job.SetPriority((BG_JOB_PRIORITY)value);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }

        }

        public JobProgress Progress
        {
            get
            {
                try
                {
                    Job.GetProgress(out var jobProgress);
                    _progress = new JobProgress(jobProgress);
                }
                catch (COMException exception)
                {                    
                    _manager.PublishException(this, exception);
                }
                return _progress;
            }
        }

        public BitsFiles EnumFiles()
        {
            try
            {
                Job.EnumFiles(out var fileList);
                Files = new BitsFiles(this, fileList);
            }
            catch (COMException exception)
            {                
                _manager.PublishException(this, exception);
            }
            return Files;
        }

        public BitsFiles Files { get; private set; }

        public ulong ErrorCount
        {
            get
            {
                uint count = 0;
                try
                {
                    Job.GetErrorCount(out count);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return count;
            }
        }

        public BitsError Error
        {
            get
            {
                try
                {
                    JobState state = State;
                    if (state == JobState.Error || state == JobState.TransientError)
                    {

                        if (null == _error)
                        {
                            Job.GetError(out var error);
                            if (null != error)
                            {
                                _error = new BitsError(this, error);
                            }
                        }
                    }
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return _error;
            }
        }

        public uint MinimumRetryDelay
        {
            get
            {
                uint seconds = 0;
                try
                {
                    Job.GetMinimumRetryDelay(out seconds);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return seconds;
            }
            set
            {
                try
                {
                    Job.SetMinimumRetryDelay(value);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }
        }

        public uint NoProgressTimeout
        {
            get
            {
                uint seconds = 0;
                try
                {
                    Job.GetNoProgressTimeout(out seconds);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return seconds;
            }
            set
            {
                try
                {
                    Job.SetNoProgressTimeout(value);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }

        }

        public GUID JobId
        {
            get
            {
                try
                {
                    if (_guid == null)
                    {
                        Job.GetId(out var guid);

                        _guid = guid;
                    }
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return _guid == null ? new GUID() : _guid.Value;
            }
        }

        public JobState? State
        {
            get
            {
                try
                {
                    Job.GetState(out var state);
                    return (JobState)state;
                }
                catch (COMException exception)
                {                    
                    _manager.PublishException(this, exception);
                }
                return null;
            }
        }

        public JobTimes JobTimes
        {
            get
            {
                try
                {
                    Job.GetTimes(out var times);
                    _jobTimes = new JobTimes(times);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return _jobTimes;
            }
        }

        public JobType? JobType
        {
            get
            {
                try
                {
                    Job.GetType(out var jobType);

                    return (JobType) jobType;
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return null;
            }
        }

        public ProxySettings ProxySettings
        {
            get 
            {
                if (null == _proxySettings)
                {
                    this._proxySettings = new ProxySettings(this.Job);
                }
                return this._proxySettings;
            }
        }

        public void Suspend()
        {
            try
            {
                this.Job.Suspend();
            }
            catch (COMException exception)
            {                
                _manager.PublishException(this, exception);
            }
        }

        public void Resume()
        {
            try
            {
                this.Job.Resume();
            }
            catch (COMException exception)
            {
                _manager.PublishException(this, exception);
            }
        }

        public void Cancel()
        {
            try
            {
                this.Job.Cancel();
            }
            catch (COMException exception)
            {
                _manager.PublishException(this, exception);
            }
        }

        public void Complete()
        {
            try
            {
                this.Job.Complete();
            }
            catch (COMException exception)
            {
                _manager.PublishException(this, exception);
            }
        }

        public void TakeOwnership()
        {
            try
            {
                this.Job.TakeOwnership();
            }
            catch (COMException exception) 
            {
                _manager.PublishException(this, exception);
            }
        }

        public void AddFile(string remoteName, string localName)
        {
            try
            {
                this.Job.AddFile(remoteName, localName);
            }
            catch (COMException exception)
            {
                _manager.PublishException(this, exception);
            }
        }

        public void AddFile(BitsFileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");
            this.AddFile(fileInfo.RemoteName, fileInfo.LocalName);
        }

        internal void AddFiles(BG_FILE_INFO[] files)
        {
            try
            {
                uint count = Convert.ToUInt32(files.Length);
                this.Job.AddFileSet(count, files);
            }
            catch (COMException exception)
            {
                _manager.PublishException(this, exception);
            }
        }

        public void AddFiles(Collection<BitsFileInfo> files)
        {
            BG_FILE_INFO[] fileArray = new BG_FILE_INFO[files.Count];
            for (int i = 0; i < files.Count; i++)
            {
                fileArray[i] = files[i].BgFileInfo;
            }
            this.AddFiles(fileArray);
        }

        public NotificationFlags NotificationFlags
        {
            get
            {
                BG_JOB_NOTIFICATION_TYPE flags = 0;
                try
                {
                    Job.GetNotifyFlags(out flags);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return (NotificationFlags)flags;
            }
            set
            {
                try
                {
                    Job.SetNotifyFlags((BG_JOB_NOTIFICATION_TYPE)value);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }
        }

        internal IBackgroundCopyCallback NotificationInterface
        {
            get
            {
                object notificationInterface = null;
                try
                {
                    Job.GetNotifyInterface(out notificationInterface);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
                return notificationInterface as IBackgroundCopyCallback;
            }
            set
            {
                try
                {
                    Job.SetNotifyInterface(value);
                }
                catch (COMException exception)
                {
                    _manager.PublishException(this, exception);
                }
            }
        }

        #endregion

        #endregion

        #region notification
        internal void JobTransferred(object sender, NotificationEventArgs e)
        {
            if (this._onJobTransfered != null)
                this._onJobTransfered(sender, new JobNotificationEventArgs());
        }

        internal void JobModified(object sender, NotificationEventArgs e)
        {
            if (this._onJobModified != null)
                this._onJobModified(sender, new JobNotificationEventArgs());
        }

        internal void JobError(object sender, ErrorNotificationEventArgs e)
        {
            if (null != this._onJobErrored)
                this._onJobErrored(sender, new JobErrorNotificationEventArgs(e.Error));
        }
        #endregion

        #region public events
        public event EventHandler<JobNotificationEventArgs> OnJobModified
        {
            add { this._onJobModified += value; }
            remove { this._onJobModified -= value; }
        }

        public event EventHandler<JobNotificationEventArgs> OnJobTransferred
        {
            add { this._onJobTransfered += value; }
            remove { this._onJobTransfered -= value; }
        }

        public event EventHandler<JobErrorNotificationEventArgs> OnJobError
        {
            add { this._onJobErrored += value; }
            remove { this._onJobErrored -= value; }
        }
        #endregion

        #region internal
        internal IBackgroundCopyJob Job { get; private set; }

        internal void PublishException(COMException exception)
        {
            this._manager.PublishException(this, exception);
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    if (this.NotificationTarget != null)
                        this.NotificationInterface = this.NotificationTarget;
                    if (this.Files != null)
                        this.Files.Dispose();
                    this.Job = null;
                }
            }
            _disposed = true;
        }
        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BITSReference10_2;

namespace SharpBits.Base
{
    /// <summary>
    /// Use the IBackgroundCopyManager interface to create transfer jobs, 
    /// retrieve an enumerator object that contains the jobs in the queue, 
    /// and to retrieve individual jobs from the queue.
    /// </summary>
    public class BitsManager : IDisposable
    {
        private IBackgroundCopyManager manager;
        private BitsNotification notificationHandler;
        private BitsJobs jobs;
        private EventHandler<NotificationEventArgs> onJobModified;
        private EventHandler<NotificationEventArgs> onJobTransfered;
        private EventHandler<ErrorNotificationEventArgs> onJobErrored;
        private EventHandler<NotificationEventArgs> onJobAdded;
        private EventHandler<NotificationEventArgs> onJobRemoved;
        private EventHandler<BitsInterfaceNotificationEventArgs> onInterfaceError;
        internal JobOwner currentOwner;
        private bool disposed;

        public BitsManager()
        {
            // Set threading apartment
            System.Threading.Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA);
            int hResult = NativeMethods.CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, RpcAuthnLevel.Connect, RpcImpLevel.Impersonate, IntPtr.Zero, EoAuthnCap.None, IntPtr.Zero); 

            this.manager = new BackgroundCopyManager10_2();
            this.jobs = new BitsJobs(this); // will be set correctly later after initialization
            this.notificationHandler = new BitsNotification(this);
            this.notificationHandler.OnJobErrorEvent += new EventHandler<ErrorNotificationEventArgs>(notificationHandler_OnJobErrorEvent);
            this.notificationHandler.OnJobModifiedEvent += new EventHandler<NotificationEventArgs>(notificationHandler_OnJobModifiedEvent);
            this.notificationHandler.OnJobTransferredEvent += new EventHandler<NotificationEventArgs>(notificationHandler_OnJobTransferredEvent);
        }

        #region event handler for notication interface
        private void notificationHandler_OnJobTransferredEvent(object sender, NotificationEventArgs e)
        {
            // route the event to the job
            if (this.jobs.ContainsKey(e.Job.JobId))
            {
                BitsJob job = this.jobs[e.Job.JobId];
                job.JobTransferred(sender, e);
            }
            //publish the event to other subscribers
            if (this.onJobTransfered != null)
                this.onJobTransfered(sender, e);
        }

        private void notificationHandler_OnJobModifiedEvent(object sender, NotificationEventArgs e)
        {
            // route the event to the job
            if (this.jobs.ContainsKey(e.Job.JobId))
            {
                BitsJob job = this.jobs[e.Job.JobId];
                job.JobModified(sender, e);
            }
            //publish the event to other subscribers
            if (this.onJobModified != null)
                this.onJobModified(sender, e);
        }

        private void notificationHandler_OnJobErrorEvent(object sender, ErrorNotificationEventArgs e)
        {
            // route the event to the job
            if (this.jobs.ContainsKey(e.Job.JobId))
            {
                BitsJob job = this.jobs[e.Job.JobId];
                job.JobError(sender, e);
            }
            //publish the event to other subscribers
            if (this.onJobErrored != null)
                this.onJobErrored(sender, e);
        }
        #endregion

        public BitsJobs EnumJobs()
        {
            return this.EnumJobs(JobOwner.CurrentUser);
        }

        public BitsJobs EnumJobs(JobOwner jobOwner)
        {
            currentOwner = jobOwner;
            IEnumBackgroundCopyJobs jobList = null;

            this.manager.EnumJobs((UInt32)jobOwner, out jobList);
            if (this.jobs == null)
            {
                this.jobs = new BitsJobs(this, jobList);
            }
            else
            {
                this.jobs.Update(jobList);
            }

            return this.jobs;
        }

        /// <summary>
        /// Creates a new transfer job.
        /// </summary>
        /// <param name="displayName">Null-terminated string that contains a display name for the job. 
        /// Typically, the display name is used to identify the job in a user interface. 
        /// Note that more than one job may have the same display name. Must not be NULL. 
        /// The name is limited to 256 characters, not including the null terminator.</param>
        /// <param name="jobType"> Type of transfer job, such as JobType.Download. For a list of transfer types, see the JobType enumeration</param>
        /// <returns></returns>
        public BitsJob CreateJob(string displayName, JobType jobType)
        {
            Guid guid;
            IBackgroundCopyJob pJob;            
            this.manager.CreateJob(displayName, (BG_JOB_TYPE)jobType, out guid, out pJob);
            BitsJob job;
            lock (this.jobs)
            {
                job = new BitsJob(this, pJob);
                this.jobs.Add(guid, job);
            }
            if (null != this.onJobAdded)
                this.onJobAdded(this, new NotificationEventArgs(job));
            return job; 
        }

        public BitsJobs Jobs
        {
            get { return this.jobs; }
        }

        #region convert HResult into meaningful error message
        public string GetErrorDescription(int hResult)
        {
            string description;
            this.manager.GetErrorDescription(hResult, Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID), out description);
            return description;
        }
        #endregion

        #region notification interface

        #region internal notification handling
        internal BitsNotification NotificationHandler
        {
            get { return this.notificationHandler; }
        }

        internal void NotifyOnJobRemoval(BitsJob job)
        {
            if (null != this.onJobRemoved)
                this.onJobRemoved(this, new NotificationEventArgs(job));
        }

        internal void PublishException(BitsJob job, COMException exception)
        {
            if (this.onInterfaceError != null)
            {
                string description = this.GetErrorDescription(exception.ErrorCode);
                this.onInterfaceError(this, new BitsInterfaceNotificationEventArgs(job, exception, description));
            }

        }
        #endregion

        #region public events
        public event EventHandler<NotificationEventArgs> OnJobModified
        {
            add { this.onJobModified += value; }
            remove { this.onJobModified -= value; }
        }

        public event EventHandler<NotificationEventArgs> OnJobTransferred
        {
            add { this.onJobTransfered += value; }
            remove { this.onJobTransfered -= value; }
        }

        public event EventHandler<ErrorNotificationEventArgs> OnJobError
        {
            add { this.onJobErrored += value; }
            remove { this.onJobErrored -= value; }
        }

        public event EventHandler<NotificationEventArgs> OnJobAdded
        {
            add { this.onJobAdded += value; }
            remove { this.onJobAdded -= value; }
        }

        public event EventHandler<NotificationEventArgs> OnJobRemoved
        {
            add { this.onJobRemoved += value; }
            remove { this.onJobRemoved -= value; }
        }

        public event EventHandler<BitsInterfaceNotificationEventArgs> OnInterfaceError
        {
            add { this.onInterfaceError += value; }
            remove { this.onInterfaceError -= value; }
        }
        #endregion

        #endregion

        internal IBackgroundCopyManager BackgroundCopyManager
        {
            get { return this.manager; }
        }

        #region util methods
        public static BitsVersion BitsVersion
        {
            get { return Utils.BITSVersion; }
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
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (BitsJob job in this.Jobs.Values)
                    {
                        job.Dispose();
                    }

                    this.jobs.Clear();
                    this.jobs.Dispose();
                    Marshal.ReleaseComObject(manager);
                    manager = null;
                }
            }
            disposed = true;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Tauron.Application.ImageOrganizer.Container.Compse;

namespace Tauron.Application.ImageOrganizer.Container
{
    public abstract class ContainerFileBase<TTransaction> : IComposerExpose
        where TTransaction : class 
    {
        protected class SyncHelper
        {
            private readonly HashSet<string> _content;

            public SyncHelper(IEnumerable<string> content) => _content = new HashSet<string>(content);

            public IEnumerable<string> Filter(IEnumerable<string> expected) => expected.Where(ex => _content.Add(ex));

            public static IEnumerable<string> Filter(IEnumerable<string> @base, IEnumerable<string> expected) => new SyncHelper(@base).Filter(expected);
        }

        private bool _isInitialized;

        protected IContainerTransaction CurrentTransaction { get; private set; }

        protected void CheckInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Not Initialized");
        }

        public virtual void Initialize(string name)
        {
            _isInitialized = true;
        }

        protected virtual TTransaction ExtractTransation(IContainerTransaction transaction)
        {
            CurrentTransaction = transaction;
            if (transaction == null) return null;

            var real = transaction.TryCast<TTransaction>();
            if (real == null) throw new NotSupportedException();

            return real;
        }

        protected abstract void AddFileCore(byte[] file, string name, TTransaction transaction);
        protected abstract void Compled(TTransaction transaction);
        protected abstract void ReadIndex();
        protected abstract void RemoveImpl(string name, TTransaction transaction);

        public virtual void AddFile(string file, string name, IContainerTransaction transaction)
        {
            CheckInitialized();
            if(!File.Exists(file)) return;

            try
            {
                var trans = ExtractTransation(transaction);

                AddFileCore(File.ReadAllBytes(file), name, trans);
                Compled(trans);
            }
            catch
            {
                transaction?.Rollback();
                ReadIndex();
                throw;
            }
        }
        public virtual void AddFile(byte[] file, string name, IContainerTransaction transaction)
        {
            CheckInitialized();
            try
            {
                var trans = ExtractTransation(transaction);
                AddFileCore(file, name, trans);
                Compled(trans);
            }
            catch 
            {
                transaction?.Rollback();
                ReadIndex();
                throw;
            }
        }
        public virtual void AddFiles(string[] files, Func<string, string> nameSelector, Controller controller = null)
        {
            CheckInitialized();
            if (controller == null)
                controller = new Controller();

            var trans = ExtractTransation(controller.Transaction);

            ManualResetEvent pause = new ManualResetEvent(true);
            int isSet = 1;
            int stop = 0;

            void OnStop()
            {
                Interlocked.Exchange(ref stop, 1);
                // ReSharper disable AccessToDisposedClosure
                pause.Set();
            }

            void OnPause()
            {
                if (isSet == 1)
                {
                    Interlocked.Decrement(ref isSet);
                    pause.Reset();
                }
                else
                {
                    Interlocked.Increment(ref isSet);
                    pause.Set();
                }
            }
            // ReSharper restore AccessToDisposedClosure

            try
            {
                controller.Stop += OnStop;
                controller.Pause += OnPause;

                int count = 0;

                foreach (var file in files)
                {
                    if (!File.Exists(file)) continue;

                    pause.WaitOne();
                    if (stop == 1) return;

                    string name = nameSelector(file);
                    AddFileCore(File.ReadAllBytes(file), name, trans);

                    count++;
                    controller.OnPostMessage($"{name} ({count}/{files.Length})", count, files.Length + 1, false);
                }

                Compled(trans);
            }
            catch
            {
                controller.Transaction?.Rollback();
                ReadIndex();
                throw;
            }
            finally
            {
                controller.Stop -= OnStop;
                controller.Pause -= OnPause;
                pause.Dispose();
            }
        }

        public abstract Stream GetStream(string name);
        public abstract bool Conatins(string name);

        public virtual void Remove(string name, IContainerTransaction transaction)
        {
            var trans = ExtractTransation(transaction);
            try
            {
                RemoveImpl(name, trans);
                Compled(trans);
            }
            catch
            {
                transaction.Rollback();
                ReadIndex();
                throw;
            }
        }
        
        public abstract IContainerTransaction CreateTransaction();

        public virtual void Sync(IEnumerable<string> expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, IContainerTransaction transaction)
        {
            CheckInitialized();
            SyncImpl(expectedContent as string[] ?? expectedContent.ToArray(), onErrorFound, onMessage, ExtractTransation(transaction));
        }
        public virtual void Recuvery(string targetDirectory, Action<RecuveryMessage> onMessage)
        {
            void ElementRecuvered(RecuverElement element)
            {
                onMessage(new RecuveryMessage(element.Name, element.Current));
                string fullPath = targetDirectory.CombinePath(element.Name);
                if(File.Exists(fullPath)) return;

                File.WriteAllBytes(fullPath, element.Data);
            }

            StartRecuveryImpl(ElementRecuvered);
        }

        public abstract string[] GetContainerNames();
        public abstract string[] GetAllContentNames();
        public abstract long ComputeSize();


        public abstract bool IsCompatible(IContainerTransaction transaction);
        public abstract void SyncImpl(string[] expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, TTransaction kernelTransaction);
        public abstract void StartRecuveryImpl(Action<RecuverElement> recuveryElement);

        void IComposerExpose.AddFileCore(byte[] file, string name, IContainerTransaction transaction) => AddFileCore(file, name, ExtractTransation(transaction));
        void IComposerExpose.Compled(IContainerTransaction transaction) => Compled(ExtractTransation(transaction));
        void IComposerExpose.ReadIndex() => ReadIndex();
        void IComposerExpose.RemoveImpl(string name, IContainerTransaction transaction) => RemoveImpl(name, ExtractTransation(transaction));
        void IComposerExpose.StartRecuvery(Action<RecuverElement> recuveryElement) => StartRecuveryImpl(recuveryElement);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tauron.Application.ImageOrganizer.Container.Resources;

namespace Tauron.Application.ImageOrganizer.Container.Compse
{
    public class ComposeFile : ContainerFileBase<ComposeTransaction>
    {
        private class ValueHolder
        {
            public IComposerExpose ContainerFile { get; }

            private string CustomInit { get; }

            public ValueHolder(IComposerExpose containerFile, string customInit)
            {
                ContainerFile = containerFile;
                CustomInit = customInit;
            }

            public void Init(string name) => ContainerFile.Initialize(string.IsNullOrWhiteSpace(CustomInit) ? name : CustomInit);
        }

        private readonly List<ValueHolder> _files = new List<ValueHolder>();

        public void Add(IComposerExpose expose, string custom)
        {
            _files.Add(new ValueHolder(expose, custom));
        }

        public override void Initialize(string name)
        {
            List<Exception> ex = null;
            for (var index = 0; index < _files.Count; index++)
            {
                var file = _files[index];
                try
                {
                    file.Init(name);
                }
                catch (Exception e)
                {
                    if (index == 0)
                        throw;

                    if (ex == null)
                        ex = new List<Exception>();
                    ex.Add(e);
                }
            }

            if(_files.Count == ex?.Count) 
                throw new AggregateException(ex);

            base.Initialize(name);
        }

        protected override void AddFileCore(byte[] file, string name, ComposeTransaction transaction) => Iterate(cf => cf.AddFileCore(file, name, transaction));
        protected override void Compled(ComposeTransaction transaction) => Iterate(cf => cf.Compled(transaction));
        protected override void ReadIndex() => Iterate(cf => cf.ReadIndex());
        protected override void RemoveImpl(string name, ComposeTransaction transaction) => Iterate(cf => cf.RemoveImpl(name, transaction));

        public override Stream GetStream(string name) => _files.Select(f => f.ContainerFile.GetStream(name)).FirstOrDefault(s => s != null);

        public override bool Conatins(string name) => _files.Any(f => f.ContainerFile.Conatins(name));

        public override IContainerTransaction CreateTransaction()
        {
            List<IContainerTransaction> transactions = new List<IContainerTransaction>();

            foreach (var composerExpose in _files.Select(vh => vh.ContainerFile))
            {
                if(transactions.Any(composerExpose.IsCompatible)) continue;

                transactions.Add(composerExpose.CreateTransaction());
            }

            return new ComposeTransaction(transactions.ToArray());
        }

        public override string[] GetContainerNames()
        {
            CheckInitialized();
           return _files.SelectMany(f => f.ContainerFile.GetContainerNames()).ToArray();
        }

        public override string[] GetAllContentNames() => _files.SelectMany(vh => vh.ContainerFile.GetAllContentNames()).Distinct().ToArray();

        public override long ComputeSize()
        {
            CheckInitialized();
            return _files.First().ContainerFile.ComputeSize();
        }

        public override bool IsCompatible(IContainerTransaction transaction) => false;
        public override void SyncImpl(string[] expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, ComposeTransaction kernelTransaction)
        {
            HashSet<string> toSync = new HashSet<string>();

            void CheckError((string Name, ErrorType Error) error)
            {
                if(error.Error == ErrorType.DataMissing)
                    if (Conatins(error.Name))
                    {
                        toSync.Add(error.Name);
                        return;
                    }

                onErrorFound(error);
            }
            

            for (int i = 0; i < _files.Count; i++)
            {
                var container = _files[i];

                var i1 = i;
                container.ContainerFile.Sync(expectedContent, CheckError, s => onMessage(ContainerResources.ContainerSync_Compose.SFormat(i1, s)), kernelTransaction);
            }

            onMessage(ContainerResources.ContainerSync_Compose_Syncing);
            foreach (var name in toSync)
            {
                var container = _files.Select(vh => vh.ContainerFile).First(c => c.Conatins(name));

                foreach (var file in _files.Select(vh => vh.ContainerFile).Where(cf => !cf.Conatins(name)))
                    file.AddFile(ReadAll(container.GetStream(name)), name, kernelTransaction);
            }
        }
        public override void StartRecuveryImpl(Action<RecuverElement> recuveryElement)
        {
            foreach (var file in _files)
                file.ContainerFile.StartRecuvery(recuveryElement);
        }

        protected override ComposeTransaction ExtractTransation(IContainerTransaction transaction)
        {
            switch (transaction)
            {
                case null:
                    return null;
                case NTFSTransaction _:
                    return new ComposeTransaction(new []{transaction});
                case ComposeTransaction trans:
                    return trans;
                default:
                    throw new NotSupportedException();
            }
        }

        private void Iterate(Action<IComposerExpose> runner)
        {
            for (var index = 0; index < _files.Count; index++)
            {
                var containerFile = _files[index];
                try
                {
                    runner(containerFile.ContainerFile);
                }
                catch (Exception e)
                {
                    if (index == 0 || e.IsCriticalApplicationException()) throw;
                }
            }
        }
        public static byte[] ReadAll(Stream stream)
        {
            using (stream)
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);

                    return mem.ToArray();
                }
            }
        }
    }
}
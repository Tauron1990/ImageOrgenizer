using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageOrganizer.Resources;
using Tauron;

namespace ImageOrganizer.Data.Container.Compse
{
    public class ComposeFile : ContainerFileBase<ComposeTransaction>
    {
        private class ValueHolder
        {
            public IComposerExpose ContainerFile { get; }

            public string CustomInit { get; }

            public ValueHolder(IComposerExpose containerFile, string customInit)
            {
                ContainerFile = containerFile;
                CustomInit = customInit;
            }

            public void Init(string name)
            {
                if(string.IsNullOrWhiteSpace(CustomInit))
                    ContainerFile.Initialize(name);
                else
                    ContainerFile.Initialize(name);
            }
        }

        private readonly List<ValueHolder> _files = new List<ValueHolder>();

        public void Add(IComposerExpose expose, string custom)
        {
            _files.Add(new ValueHolder(expose, custom));
        }

        public override void Initialize(string name)
        {
            foreach (var file in _files)
                file.Init(name);
            base.Initialize(name);
        }

        protected override void AddFileCore(byte[] file, string name, ComposeTransaction transaction) => Iterate(cf => cf.AddFileCore(file, name, transaction));
        protected override void Compled(ComposeTransaction transaction) => Iterate(cf => cf.Compled(transaction));
        protected override void ReadIndex() => Iterate(cf => cf.ReadIndex());
        protected override void RemoveImpl(string name, ComposeTransaction transaction) => Iterate(cf => cf.RemoveImpl(name, transaction));

        public override Stream GetStream(string name) => _files.Select(f => f.ContainerFile.GetStream(name)).FirstOrDefault(s => s != null);

        public override bool Conatins(string name) => _files.Any(f => f.ContainerFile.Conatins(name));

        public override IContainerTransaction CreateTransaction() => new ComposeTransaction(_files.Select(c => c.ContainerFile.CreateTransaction()).ToArray());

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
                container.ContainerFile.Sync(expectedContent, CheckError, s => onMessage(UIResources.ContainerSync_Compose.SFormat(i1, s)), CurrentTransaction);
            }

            onMessage(UIResources.ContainerSync_Compose_Syncing);
            foreach (var name in toSync)
            {
                var container = _files.Select(vh => vh.ContainerFile).First(c => c.Conatins(name));

                foreach (var file in _files.Select(vh => vh.ContainerFile).Where(cf => !cf.Conatins(name)))
                    file.AddFile(ReadAll(container.GetStream(name)), name, CurrentTransaction);
            }
        }
        public override void StartRecuveryImpl(Action<RecuverElement> recuveryElement)
        {
            foreach (var file in _files)
                file.ContainerFile.StartRecuvery(recuveryElement);
        }

        private void Iterate(Action<IComposerExpose> runner)
        {
            foreach (var containerFile in _files)
                runner(containerFile.ContainerFile);
        }
        private byte[] ReadAll(Stream stream)
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
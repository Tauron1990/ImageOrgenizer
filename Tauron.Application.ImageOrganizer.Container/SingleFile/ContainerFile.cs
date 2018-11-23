using System;
using System.IO;
using System.Linq;
using System.Security;
using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.Container.Resources;
using Tauron.Application.ImageOrganizer.Core.IO;

namespace Tauron.Application.ImageOrganizer.Container.SingleFile
{
    [PublicAPI]
    public class ContainerFile : ContainerFileBase<IKernelTransaction>
    {
        private string _containerName;
        private IndexFile _indexFile;
        private ContentReader _contentReader;
        private IIOInterface _io;

        public override void Initialize(string name)
        {
            _io = IOInterfaceProvider.IOInterface;
            _containerName = name;
            _indexFile = new IndexFile(name, _io);
            _contentReader = new ContentReader(name, _io);
            base.Initialize(name);
        }

        protected override void AddFileCore(byte[] file, string name, IKernelTransaction transaction)
        {
            var location = _contentReader.AddFile(file, name, transaction);
            _indexFile.Add(name, location.Position, location.Length);
        }

        protected override void Compled(IKernelTransaction transaction) => _indexFile.SaveIndexFile(transaction);
        protected override void ReadIndex() => _indexFile.Read();
        protected override void RemoveImpl(string name, IKernelTransaction transaction)
        {
            var data = _indexFile.Remove(name);
            _contentReader.Remove(data.Position, data.Lenght, transaction);
        }

        public override Stream GetStream(string name)
        {
            CheckInitialized();
            var data = _indexFile.GetLocation(name);
            return _contentReader.Open(data.Position, data.Lenght);
        }

        public override bool Conatins(string name)
        {
            CheckInitialized();
            return _indexFile.Contains(name);
        }
        
        public override IContainerTransaction CreateTransaction() => new NTFSTransaction();

        public override string[] GetContainerNames() => new[] {_contentReader.FileLocation, _indexFile.Name};
        public override string[] GetAllContentNames() => _indexFile.GetAllNames().ToArray();

        public override long ComputeSize() => _io.GetSize(_contentReader.FileLocation) + _io.GetSize(_indexFile.Name);

        public override bool IsCompatible(IContainerTransaction transaction) => transaction is NTFSTransaction;

        public override void SyncImpl(string[] expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, IKernelTransaction kernelTransaction)
        {
            onMessage(ContainerResources.ContainerSync_Missing);
            foreach (var missing in SyncHelper.Filter(_indexFile.GetAllNames(), expectedContent))
                onErrorFound((missing, ErrorType.DataMissing));

            onMessage(ContainerResources.ContainerSync_Delting);
            foreach (var missing in SyncHelper.Filter(expectedContent, _indexFile.GetAllNames()))
            {
                Remove(missing, CurrentTransaction);
                onErrorFound((missing, ErrorType.Deleted));
            }
        }

        public override void StartRecuveryImpl(Action<RecuverElement> recuveryElement)
        {
            try
            {
                int current = 0;
                foreach (var data in _contentReader.Recuvery())
                {
                    current++;
                    recuveryElement(new RecuverElement(data.Name, data.Data, current));
                }
            }
            catch (Exception e) when(e is IOException || e is SecurityException)
            {}
        }

        //public static void DeleteContainer(string name)
        //{
        //    string file1 = name + IndexFile.IndexExtension;
        //    string file2 = name + ContentReader.ContentExtension;

        //    file1.DeleteFile();
        //    file2.DeleteFile();
        //}
    }
}
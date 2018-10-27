using System;
using System.IO;
using System.Linq;
using System.Security;
using Alphaleonis.Win32.Filesystem;
using ImageOrganizer.Resources;
using JetBrains.Annotations;
using File = Alphaleonis.Win32.Filesystem.File;

namespace ImageOrganizer.Data.Container.SingleFile
{
    [PublicAPI]
    public class ContainerFile : ContainerFileBase<KernelTransaction>
    {
        private string _containerName;
        private IndexFile _indexFile;
        private ContentReader _contentReader;

        public override void Initialize(string name)
        {
            _containerName = name;
            _indexFile = new IndexFile(name);
            _contentReader = new ContentReader(name);
            base.Initialize(name);
        }

        protected override void AddFileCore(byte[] file, string name, KernelTransaction transaction)
        {
            var location = _contentReader.AddFile(file, name, transaction);
            _indexFile.Add(name, location.Position, location.Length);
        }

        protected override void Compled(KernelTransaction transaction) => _indexFile.SaveIndexFile(transaction);
        protected override void ReadIndex() => _indexFile.Read();
        protected override void RemoveImpl(string name, KernelTransaction transaction)
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

        public override long ComputeSize() => File.GetSize(_contentReader.FileLocation) + File.GetSize(_indexFile.Name);

        public override bool IsCompatible(IContainerTransaction transaction) => transaction is NTFSTransaction;

        public override void SyncImpl(string[] expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, KernelTransaction kernelTransaction)
        {
            onMessage(UIResources.ContainerSync_Missing);
            foreach (var missing in SyncHelper.Filter(_indexFile.GetAllNames(), expectedContent))
                onErrorFound((missing, ErrorType.DataMissing));

            onMessage(UIResources.ContainerSync_Delting);
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
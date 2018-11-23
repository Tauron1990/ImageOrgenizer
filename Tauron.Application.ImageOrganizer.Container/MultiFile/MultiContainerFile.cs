using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Tauron.Application.ImageOrganizer.Container.Resources;
using Tauron.Application.ImageOrganizer.Core.IO;

namespace Tauron.Application.ImageOrganizer.Container.MultiFile
{
    public class MultiContainerFile : ContainerFileBase<IKernelTransaction>
    {
        private string _containerName;
        private MultiFileIndex _index;
        private HashSet<string> _names;
        private IIOInterface _io;

        public override void Initialize(string name)
        {
            _io = IOInterfaceProvider.IOInterface;
            name = name.Replace('.', '-');
            _containerName = name;
            name.CreateDirectoryIfNotExis();
            _index = new MultiFileIndex(_containerName, _io);
            _names = new HashSet<string>(_index.GetAllNames());

            base.Initialize(name);
        }

        protected override void AddFileCore(byte[] file, string name, IKernelTransaction transaction)
        {
            string fileName;

            do
            {
                fileName = Guid.NewGuid().ToString("D");
            } while (!_names.Add(fileName));

            _index.Add(name, fileName);
            fileName = _containerName.CombinePath(fileName);

            var info = _io.GetFileInfo(transaction, fileName + ".bin");

            using (var stream = info.Open(FileMode.Create, FileAccess.Write))
                stream.Write(file, 0, file.Length);

            using (var writer = new BinaryWriter(_io.GetFileInfo(transaction, fileName + ".info").Open(FileMode.Create, FileAccess.Write)))
            {
                writer.Write(name);
                writer.Write(_io.Crc32Compute(file));
            }
        }

        protected override void Compled(IKernelTransaction transaction) => _index.Save(transaction);
        protected override void ReadIndex() => _index.Read();
        protected override void RemoveImpl(string name, IKernelTransaction transaction)
        {
            string file = _index.Remove(name);
            var info = _io.GetFileInfo(transaction, file);
            info.Delete(true);
        }

        public override Stream GetStream(string name)
        {
            using (var stream = new FileStream(_containerName.CombinePath(_index.GetName(name) + ".bin"), FileMode.Open))
            {
                var mem = new MemoryStream();
                stream.CopyTo(mem);
                mem.Position = 0;
                return mem;
            }
        }

        public override bool Conatins(string name) => _index.Contains(name);

        public override IContainerTransaction CreateTransaction() => new NTFSTransaction();
        public override void SyncImpl(string[] expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, IKernelTransaction transaction)
        {
            onMessage(ContainerResources.ContainerSync_Missing);
            foreach (var missing in SyncHelper.Filter(_index.GetAllNames(), expectedContent))
                onErrorFound((missing, ErrorType.DataMissing));

            onMessage(ContainerResources.ContainerSync_Delting);
            foreach (var missing in SyncHelper.Filter(expectedContent, _index.GetAllNames()))
            {
                Remove(missing, CurrentTransaction);
                onErrorFound((missing, ErrorType.Deleted));
            }
        }
        public override void StartRecuveryImpl(Action<RecuverElement> recuveryElement)
        {
            var current = 0;
            foreach (var file in _io.EnumerateFiles(_containerName, InternalDirectoryEnumerationOptions.ContinueOnException, InternalPathFormat.FullPath).Where(p => p.EndsWith(".bin")))
            {
                try
                {
                    string textName = file.Remove(file.Length - 3, 3) + "info";
                    string name;
                    uint crc;
                    using (var reader = new BinaryReader(File.OpenRead(textName)))
                    {
                        name = reader.ReadString();
                        crc = reader.ReadUInt32();
                    }

                    byte[] data = File.ReadAllBytes(file);

                    if(_io.Crc32Compute(data) != crc) continue;

                    current++;
                    recuveryElement(new RecuverElement(name, data, current));
                }
                catch (IOException) { }
                catch(SecurityException) { }
            }
        }

        public override string[] GetContainerNames() => new[] {_containerName };

        public override string[] GetAllContentNames() => _index.GetAllNames().ToArray();

        public override long ComputeSize() => _io.EnumerateFiles(_containerName, InternalDirectoryEnumerationOptions.ContinueOnException, InternalPathFormat.FullPath).Select(_io.GetSize).Sum();

        public override bool IsCompatible(IContainerTransaction transaction) => transaction is NTFSTransaction;
    }
}
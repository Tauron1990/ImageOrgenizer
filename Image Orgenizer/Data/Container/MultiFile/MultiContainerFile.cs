using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Alphaleonis.Win32.Filesystem;
using ImageOrganizer.Resources;
using Tauron;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using Directory = Alphaleonis.Win32.Filesystem.Directory;

namespace ImageOrganizer.Data.Container.MultiFile
{
    public class MultiContainerFile : ContainerFileBase<KernelTransaction>
    {
        private string _containerName;
        private MultiFileIndex _index;
        private HashSet<string> _names;

        public override void Initialize(string name)
        {
            name = name.Replace('.', '-');
            _containerName = name;
            if (!System.IO.Directory.Exists(name))
                System.IO.Directory.CreateDirectory(name);
            _index = new MultiFileIndex(_containerName);
            _names = new HashSet<string>(_index.GetAllNames());

            base.Initialize(name);
        }

        protected override void AddFileCore(byte[] file, string name, KernelTransaction transaction)
        {
            string fileName;

            do
            {
                fileName = Guid.NewGuid().ToString("D");
            } while (!_names.Add(fileName));

            _index.Add(name, fileName);
            fileName = _containerName.CombinePath(fileName);

            var info = new FileInfo(transaction, fileName + ".bin");

            using (var stream = info.Open(FileMode.Create, FileAccess.Write))
                stream.Write(file, 0, file.Length);

            using (var writer = new BinaryWriter(new FileInfo(transaction, fileName + ".info").Open(FileMode.Create, FileAccess.Write)))
            {
                writer.Write(name);
                writer.Write(Crc32C.Crc32CAlgorithm.Compute(file));
            }
        }

        protected override void Compled(KernelTransaction transaction) => _index.Save(transaction);
        protected override void ReadIndex() => _index.Read();
        protected override void RemoveImpl(string name, KernelTransaction transaction)
        {
            string file = _index.Remove(name);
            var info = new FileInfo(transaction, file);
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
        public override void SyncImpl(string[] expectedContent, Action<(string Name, ErrorType Error)> onErrorFound, Action<string> onMessage, KernelTransaction transaction)
        {
            onMessage(UIResources.ContainerSync_Missing);
            foreach (var missing in SyncHelper.Filter(_index.GetAllNames(), expectedContent))
                onErrorFound((missing, ErrorType.DataMissing));

            onMessage(UIResources.ContainerSync_Delting);
            foreach (var missing in SyncHelper.Filter(expectedContent, _index.GetAllNames()))
            {
                Remove(missing, CurrentTransaction);
                onErrorFound((missing, ErrorType.Deleted));
            }
        }
        public override void StartRecuveryImpl(Action<RecuverElement> recuveryElement)
        {
            var current = 0;
            foreach (var file in Directory.EnumerateFiles(_containerName, DirectoryEnumerationOptions.ContinueOnException, PathFormat.FullPath).Where(p => p.EndsWith(".bin")))
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

                    if(Crc32C.Crc32CAlgorithm.Compute(data) != crc) continue;

                    current++;
                    recuveryElement(new RecuverElement(name, data, current));
                }
                catch (IOException) { }
                catch(SecurityException) { }
            }
        }

        public override string[] GetContainerNames() => new[] {_containerName };

        public override string[] GetAllContentNames() => _index.GetAllNames().ToArray();

        public override long ComputeSize() => Directory.EnumerateFiles(_containerName, DirectoryEnumerationOptions.ContinueOnException, PathFormat.FullPath).Select(File.GetSize).Sum();

        public override bool IsCompatible(IContainerTransaction transaction) => transaction is NTFSTransaction;
    }
}
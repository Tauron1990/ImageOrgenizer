using System.Collections.Generic;
using System.IO;
using Tauron.Application.ImageOrganizer.Core.IO;

namespace Tauron.Application.ImageOrganizer.Container.MultiFile
{
    public class MultiFileIndex
    {
        private const string FileName = "index.itx";

        private readonly Dictionary<string, string> _files;
        private readonly IIOInterface _io;

        public string Name { get; }

        public MultiFileIndex(string dic, IIOInterface io)
        {
            _io = io;
            _files = new Dictionary<string, string>();
            Name = dic.CombinePath(FileName);
            Read();
        }

        public void Read()
        {
            _files.Clear();
            if(!Name.ExisFile()) return;

            try
            {
                using (var file = new FileStream(Name, FileMode.Open))
                    using (var reader = new BinaryReader(file))
                    {
                        int lenght = reader.ReadInt32();
                        for (int i = 0; i < lenght; i++)
                            _files[reader.ReadString()] = reader.ReadString();

                    }
            }
            catch (IOException)
            {
            }
        }

        public void Save(IKernelTransaction transaction)
        {
            var info = _io.GetFileInfo(transaction, Name);

            using (var file = info.Open(FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(file))
                {
                    writer.Write(_files.Count);
                    foreach (var entry in _files)
                    {
                        writer.Write(entry.Key);
                        writer.Write(entry.Value);
                    }
                }
            }
        }

        public IEnumerable<string> GetAllNames() => _files.Keys;

        public void Add(string name, string fileName) => _files.Add(name, fileName);

        public string GetName(string name) => _files[name];

        public bool Contains(string name) => _files.ContainsKey(name);

        public string Remove(string name)
        {
            var fileName = _files[name];
            _files.Remove(name);

            return fileName;
        }
    }
}
using System.Collections.Generic;
using System.IO;
using Alphaleonis.Win32.Filesystem;
using Tauron;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;

namespace ImageOrganizer.Data.Container.MultiFile
{
    public class MultiFileIndex
    {
        private const string FileName = "index.itx";

        private readonly Dictionary<string, string> _files;
        public string Name { get; }

        public MultiFileIndex(string dic)
        {
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

        public void Save(KernelTransaction transaction)
        {
            var info = new FileInfo(transaction, Name);

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
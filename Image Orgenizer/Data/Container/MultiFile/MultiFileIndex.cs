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
        private readonly string _name;

        public MultiFileIndex(string dic)
        {
            _files = new Dictionary<string, string>();
            _name = dic.CombinePath(FileName);
            Read();
        }

        public void Read()
        {
            _files.Clear();
            if(!_name.ExisFile()) return;

            try
            {
                using (var file = new FileStream(_name, FileMode.Open))
                    using (var reader = new BinaryReader(file))
                        for (int i = 0; i < reader.ReadInt32(); i++)
                            _files[reader.ReadString()] = reader.ReadString();
            }
            catch (IOException)
            {
            }
        }

        public void Save(KernelTransaction transaction)
        {
            var info = new FileInfo(transaction, _name);

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

        public IEnumerable<string> GetAllNames() => _files.Values;

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
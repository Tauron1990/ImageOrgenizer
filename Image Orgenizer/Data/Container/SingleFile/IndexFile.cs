using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alphaleonis.Win32.Filesystem;
using LZ4;
using Tauron;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;

namespace ImageOrganizer.Data.Container.SingleFile
{
    public class IndexFile
    {
        private class IndexEntry
        {
            public long Length { get; set; }
            public long Position { get; set; }
        }

        private const string IndexExtension = ".itx";

        private SortedList<string, IndexEntry> IndexEntries { get; } = new SortedList<string, IndexEntry>(StringComparer.Ordinal);
        public string Name { get; }

        public IndexFile(string name)
        {
            Name = name + IndexExtension;
            Read();
        }

        public void Read()
        {
            if (!Name.ExisFile()) return;

            using (var file = new FileStream(Name, FileMode.Open))
            {

                using (var lz4 = new LZ4Stream(file, LZ4StreamMode.Decompress, LZ4StreamFlags.HighCompression))
                {
                    using (var reader = new BinaryReader(lz4))
                    {
                        long l = reader.ReadInt64();
                        for (int i = 0; i < l; i++)
                        {
                            IndexEntries.Add(reader.ReadString(), new IndexEntry
                            {
                                Position = reader.ReadInt64(),
                                Length = reader.ReadInt64()
                            });
                        }
                    }
                }
            }
        }

        public void SaveIndexFile( KernelTransaction taransaction)
        {
            FileInfo info = new FileInfo(taransaction, Name);
            using (var file = info.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                WriteToStream(file);
        }

        private void WriteToStream(FileStream file)
        {
            using (var lz4 = new LZ4Stream(file, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression))
            {
                using (var writer = new BinaryWriter(lz4))
                {
                    writer.Write((long)IndexEntries.Count);

                    foreach (var indexEntry in IndexEntries)
                    {
                        writer.Write(indexEntry.Key);
                        writer.Write(indexEntry.Value.Position);
                        writer.Write(indexEntry.Value.Length);
                    }
                }
            }
        }

        public void Add(string name, long position, long length) => IndexEntries.Add(name, new IndexEntry{ Position = position, Length = length});

        public (long Position, long Lenght) GetLocation(string name)
        {
            var entry = IndexEntries[name];
            return (entry.Position, entry.Length);
        }

        public (long Position, long Lenght) Remove(string name)
        {
            var entry = IndexEntries[name];
            IndexEntries.Remove(name);

            foreach (var indexEntry in IndexEntries
                                            .OrderBy(e => e.Value.Position)
                                            .Where(e => e.Value.Position > entry.Position)
                                            .Select(e => e.Value)
                                            .ToArray())
            {
                indexEntry.Position -= entry.Length;
            }

            return (entry.Position, entry.Length);
        }

        public bool Contains(string name) => IndexEntries.ContainsKey(name);

        public IEnumerable<string> GetAllNames() => IndexEntries.Keys;
    }
}
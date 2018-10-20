using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Alphaleonis.Win32.Filesystem;
using Crc32C;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;

namespace ImageOrganizer.Data.Container.SingleFile
{
    public class ContentReader
    {
        private class FileStreamPointer
        {
            private long _lastPosition;
            private readonly FileStream _fileStream;

            public bool End { get; private set; }

            public FileStreamPointer(FileStream fileStream, long position)
            {
                _fileStream = fileStream;
                _lastPosition = position;
            }

            public int Read(byte[] buffer)
            {
                _fileStream.Position = _lastPosition;
                var temp = _fileStream.Read(buffer, 0, buffer.Length);
                _lastPosition = _fileStream.Position;

                End = _fileStream.Length == _fileStream.Position;

                return temp;
            }

            public void Write(byte[] buffer, int length)
            {
                _fileStream.Position = _lastPosition;
                _fileStream.Write(buffer, 0, length);
                _lastPosition = _fileStream.Position;
            }

        }

        private const string ContentExtension = ".bin";


        private readonly string _fileLocation;

        public ContentReader(string name) => _fileLocation = name + ContentExtension;

        public Substream Open(long position, long length)
        {
            var tempStream = new Substream(OpenForRead(), position + HeaderHelper.HeaderLength + 4, length);
            using (var reader = new BinaryReader(tempStream, Encoding.UTF8, true))
                tempStream.SetLength(reader.ReadInt32());
            return tempStream;
        }

        private FileStream OpenContainerWrite(FileShare share, KernelTransaction transaction = null)
        {
            var info = new FileInfo(transaction, _fileLocation);
            return info.Exists
                ? info.Open(FileMode.Open, FileAccess.Write, share)
                : info.Open(FileMode.CreateNew, FileAccess.Write, share);
        }

        private FileStream OpenForRead() => new FileStream(_fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read);

        public void Remove(long position, long length, KernelTransaction trans)
        {
            var info = new FileInfo(trans, _fileLocation);
            using (var stream = info.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                byte[] buffer = new byte[short.MaxValue * 2];

                FileStreamPointer input = new FileStreamPointer(stream, position + length);
                FileStreamPointer output = new FileStreamPointer(stream, position);

                while (!input.End)
                {
                    var readCount = input.Read(buffer);
                    output.Write(buffer, readCount);
                }

                stream.SetLength(stream.Length - length);
            }
        }

        public (long Position, long Length) AddFile(byte[] file, string name, KernelTransaction transaction)
        {
            long length;
            long position;

            using (var container = OpenContainerWrite(FileShare.Read, transaction))
            {
                using (var target = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(target, Encoding.UTF8, true))
                    {
                        writer.Write(HeaderHelper.EntryHeader);
                        writer.Write(Crc32CAlgorithm.Compute(file));
                        writer.Write(file.Length);
                        writer.Write(file);
                        writer.Write(name);
                    }

                    target.Position = 0;
                    container.Seek(0, SeekOrigin.End);

                    position = container.Position;
                    length = target.Length;

                    target.CopyTo(container);
                }
            }

            return (position, length);
        }

        public IEnumerable<(string Name, byte[] Data)> Recuvery()
        {

            using (var stream = OpenForRead())
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    while (true)
                    {
                        var pos = HeaderHelper.Find(stream);
                        if (pos == -1) yield break;

                        stream.Position = pos;
                        var crc = reader.ReadUInt32();
                        var bufferLength = reader.ReadInt32();
                        var buffer = reader.ReadBytes(bufferLength);

                        if (crc == Crc32CAlgorithm.Compute(buffer))
                            yield return (reader.ReadString(), buffer);
                        else
                            stream.Position = pos;
                    }
                }
            }
        }
    }
}
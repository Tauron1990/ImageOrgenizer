using System;
using System.IO;

namespace ImageOrganizer.Data.Container.SingleFile
{
    public class Substream : Stream
    {
        private Stream _baseStream;
        private long _length;
        private readonly long _offset;

        public Substream(Stream baseStream, long offset, long length)
        {
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            if (!baseStream.CanRead || !baseStream.CanSeek)
                throw new ArgumentException("stream");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            var streamLen = baseStream.Length;
            if (offset > streamLen)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + length > streamLen)
                throw new ArgumentOutOfRangeException(nameof(length));
            _baseStream = baseStream;
            _offset = offset;
            _length = length;
            _baseStream.Position = _offset;
        }

        public override bool CanSeek => true;

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override long Position
        {
            get => _baseStream.Position - _offset;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override long Length => _length;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _baseStream.Dispose();
            _baseStream = null;
        }

        public override void SetLength(long value)
        {
            if(Position > value || _length < value) 
                throw new NotSupportedException();

            _length = value;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return _baseStream.Seek(_offset + offset, origin) - _offset;
                case SeekOrigin.End:
                    return _baseStream.Seek(_length + _offset + offset, SeekOrigin.Begin) - _offset;
                case SeekOrigin.Current:
                    return _baseStream.Seek(offset, origin) - _offset;
                default:
                    throw new ArgumentException("origin");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = (int) Math.Min(
                count,
                _length - (_baseStream.Position - _offset)
            );
            return read > 0 ? _baseStream.Read(buffer, offset, read) : 0;
        }

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void Flush()
        {
        }
    }

}
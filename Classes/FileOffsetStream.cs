using System;
using System.IO;

namespace PSXPrev.Classes
{
    // Stream wrapper for tracking the farthest position visited in the file.
    public sealed class FileOffsetStream : Stream
    {
        private readonly Stream _stream;
        private long _maxPosition;

        public FileOffsetStream(Stream stream)
        {
            _stream = stream;
            _maxPosition = stream.Position;
        }


        public long FarthestPosition
        {
            get
            {
                _maxPosition = Math.Max(_maxPosition, _stream.Position);
                return _maxPosition;
            }
        }

        public override long Position
        {
            get => _stream.Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override long Length => _stream.Length;

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;


        public override long Seek(long offset, SeekOrigin origin)
        {
            _maxPosition = Math.Max(_maxPosition, _stream.Position);
            var newPosition = _stream.Seek(offset, origin);
            _maxPosition = Math.Max(_maxPosition, newPosition);
            return newPosition;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _maxPosition = Math.Max(_maxPosition, _stream.Position);
            }
            base.Dispose(disposing);
        }

        public override int ReadByte() => _stream.ReadByte();

        public override void WriteByte(byte value) => _stream.WriteByte(value);

        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public override void Flush() => _stream.Flush();

        public override void SetLength(long value) => _stream.SetLength(value);
    }
}

using System;
using System.IO;

namespace PSXPrev.Common.Parsers
{
    // Stream wrapper for tracking the farthest position visited in the file.
    // This class assumes that the underlying stream is never moved independent of this wrapper.
    public sealed class FileOffsetStream : Stream
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;
        private long _maxPosition;

        public FileOffsetStream(Stream stream, bool leaveOpen = false)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
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

        public override int ReadByte() => _stream.ReadByte();

        public override void WriteByte(byte value) => _stream.WriteByte(value);

        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public override void Flush() => _stream.Flush();

        public override void SetLength(long value) => _stream.SetLength(value);


        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    try
                    {
                        _maxPosition = Math.Max(_maxPosition, _stream.Position);
                    }
                    finally
                    {
                        if (_leaveOpen)
                        {
                            _stream.Flush();
                        }
                        else
                        {
                            _stream.Close();
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}

using System;
using System.IO;

namespace PSXPrev.Common.Parsers
{
    // Stream wrapper for tracking the farthest position visited in the file.
    // This class assumes that the underlying stream is never moved independent of this wrapper.
    public sealed class FileOffsetStream : Stream
    {
        private Stream _stream;
        private readonly bool _leaveOpen;
        private long _maxPosition;

        public FileOffsetStream(Stream stream, bool leaveOpen = false)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException("Stream must be able to Read and Seek", nameof(stream));
            }

            _maxPosition = stream.Position;
        }


        public long FarthestPosition
        {
            get
            {
                if (_stream != null)
                {
                    _maxPosition = Math.Max(_maxPosition, _stream.Position);
                }
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

        public override bool CanWrite => false;


        public void ResetFarthestPosition()
        {
            _maxPosition = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var oldPosition = _stream.Position;
            var newPosition = _stream.Seek(offset, origin);
            _maxPosition = Math.Max(_maxPosition, Math.Max(newPosition, oldPosition));
            return newPosition;
        }

        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Stream is not writable");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Stream is not writable");
        }

        public override void Flush()
        {
        }


        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && _stream != null)
                {
                    try
                    {
                        _maxPosition = Math.Max(_maxPosition, _stream.Position);
                    }
                    finally
                    {
                        if (!_leaveOpen)
                        {
                            _stream.Close();
                        }
                    }
                }
            }
            finally
            {
                _stream = null;
                base.Dispose(disposing);
            }
        }
    }
}

using System;
using System.IO;

namespace PSXPrev.Common.Parsers
{
    public sealed class BinCDStream : Stream
    {
        // 150 skips common data like PS1 logo model
        public const int SectorsFirstIndex = 150;
        public const int SectorRawSize = 2352;
        public const int SectorUserStart = 24;
        public const int SectorUserSize = 2048;

        // File config
        private readonly long _fileSectorsStart;
        private readonly int _fileSectorRawSize;
        private readonly int _fileSectorUserStart;
        private readonly int _fileSectorUserSize;

        private Stream _stream;
        private readonly bool _leaveOpen;
        private readonly byte[] _buffer;
        private readonly long _length;
        private readonly int _sectorCount; // Total number of sectors in the file after _fileSectorsStart

        private int _sectorIndex;          // Index of current sector
        private int _sectorOffset;       // Index inside current sector
        private int _lastReadSectorIndex;  // Index of last sector that was read into the buffer

        public BinCDStream(string file)
            : this(File.OpenRead(file), false)
        {
        }

        public BinCDStream(Stream stream, bool leaveOpen = false)
            : this(stream, SectorsFirstIndex, SectorRawSize, SectorUserStart, SectorUserSize, leaveOpen)
        {
        }

        // Set sectorsFirstIndex to 0 to read information like PS1 logo model
        // Set sectorUserSize to 2032 and sectorUserStart to 40 for Star Ocean 2
        public BinCDStream(string file, int sectorsFirstIndex)
            : this(File.OpenRead(file), sectorsFirstIndex, false)
        {
        }

        public BinCDStream(Stream stream, int sectorsFirstIndex, bool leaveOpen = false)
            : this(stream, sectorsFirstIndex, SectorRawSize, SectorUserStart, SectorUserSize, leaveOpen)
        {
        }

        public BinCDStream(string file, int sectorsFirstIndex, int sectorRawSize, int sectorUserStart, int sectorUserSize)
            : this(File.OpenRead(file), sectorsFirstIndex, sectorRawSize, sectorUserStart, sectorUserSize, false)
        {
        }

        public BinCDStream(Stream stream, int sectorsFirstIndex, int sectorRawSize, int sectorUserStart, int sectorUserSize, bool leaveOpen = false)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException("Stream must be able to Read and Seek", nameof(stream));
            }

            _fileSectorsStart    = sectorsFirstIndex * sectorRawSize;
            _fileSectorRawSize   = sectorRawSize;
            _fileSectorUserStart = sectorUserStart;
            _fileSectorUserSize  = sectorUserSize;
            _buffer = new byte[sectorUserSize];

            _sectorCount = Math.Max(0, SectorIndexFromRawPosition(stream.Length));
            _length = SectorIndexToUserPosition(_sectorCount);
            _sectorIndex = 0;
            _sectorOffset = 0;
            _lastReadSectorIndex = -1; // No sectors read into buffer
        }


        public static bool IsBINFile(string file) => IsBINFile(file, SectorRawSize);

        public static bool IsBINFile(string file, int sectorRawSize)
        {
            try
            {
                var size = new FileInfo(file).Length;
                return size > 0 && (size % sectorRawSize) == 0;
            }
            catch
            {
                // Failed to get file info? Clearly this isn't a raw PS1 bin file...
            }
            return false;
        }


        public override long Position
        {
            get => SectorIndexToUserPosition(_sectorIndex) + _sectorOffset;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override long Length => _length;

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => false;


        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Current:
                    offset += Position;
                    break;
                case SeekOrigin.End:
                    offset += _length;
                    break;
            }
            if (offset < 0)
            {
                throw new IOException("An attempt was made to move the file pointer before the beginning of the file.");
            }

            // Seek is somewhat performance critical code, so we're implementing SectorFromUserPosition by hand.
            _sectorIndex = (int)Math.DivRem(offset, _fileSectorUserSize, out var longSectorOffset);
            _sectorOffset = (int)longSectorOffset;
            return offset;
        }

        public override int ReadByte()
        {
            if (!PrepareSectorForRead())
            {
                return -1; // End of stream
            }
            return _buffer[_sectorOffset++];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("Count is less than 0", nameof(count));
            }
            var bytesRead = 0;
            while (count > 0)
            {
                if (!PrepareSectorForRead())
                {
                    break; // End of stream
                }
                var sectorLeft = _fileSectorUserSize - _sectorOffset;
                var sectorRead = Math.Min(sectorLeft, count);
                Buffer.BlockCopy(_buffer, _sectorOffset, buffer, offset + bytesRead, sectorRead);
                bytesRead += sectorRead;
                count -= sectorRead;
                _sectorOffset += sectorRead;
            }
            return bytesRead;
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
                if (disposing && !_leaveOpen && _stream != null)
                {
                    _stream.Close();
                }
            }
            finally
            {
                _stream = null;
                base.Dispose(disposing);
            }
        }


        private bool PrepareSectorForRead()
        {
            // Increment the sector if we've reached the end
            if (_sectorOffset == _fileSectorUserSize)
            {
                _sectorIndex++;
                _sectorOffset = 0;
            }
            // Check if we've reached EOF
            if (_sectorIndex >= _sectorCount)
            {
                return false; // Nothing more to read
            }
            // Read the sector into the buffer if we haven't already done so.
            if (_sectorIndex != _lastReadSectorIndex)
            {
                // We always need to seek, since we need to skip non-user data between sectors
                var rawPosition = SectorIndexToRawPosition(_sectorIndex);
                _lastReadSectorIndex = -1; // Set to invalid sector in-case an exception occurs during Seek/Read
                _stream.Seek(rawPosition, SeekOrigin.Begin);
                _stream.Read(_buffer, 0, _fileSectorUserSize);
                _lastReadSectorIndex = _sectorIndex;
            }
            return true;
        }

        private int SectorOffsetFromUserPosition(long position)
        {
            return (int)GeomMath.PositiveModulus(position, _fileSectorUserSize);
            //return (int)(position % _fileSectorUserSize);
        }

        private int SectorIndexFromUserPosition(long position)
        {
            return (int)GeomMath.FloorDiv(position, _fileSectorUserSize);
            //return (int)(position / _fileSectorUserSize);
        }

        private long SectorIndexToUserPosition(int sectorIndex)
        {
            return ((long)sectorIndex * _fileSectorUserSize);
        }

        // Returns sectorIndex and outputs sectorOffset
        private int SectorFromUserPosition(long position, out int sectorOffset)
        {
            var sectorIndex = (int)GeomMath.FloorDivRem(position, _fileSectorUserSize, out var longSectorOffset);
            sectorOffset = (int)longSectorOffset;
            return sectorIndex;
        }

        /*private int SectorOffsetFromRawPosition(long rawPosition)
        {
            return (int)GeomMath.PositiveModulus(rawPosition - _fileSectorsStart, _fileSectorRawSize) - _fileSectorUserStart;
            //return (int)((rawPosition - _fileSectorsStart) % _fileSectorRawSize) - _fileSectorUserStart;
        }*/

        private int SectorIndexFromRawPosition(long rawPosition)
        {
            return (int)GeomMath.FloorDiv(rawPosition - _fileSectorsStart, _fileSectorRawSize);
            //return (int)((rawPosition - _fileSectorsStart) / _fileSectorRawSize);
        }

        private long SectorIndexToRawPosition(int sectorIndex)
        {
            return _fileSectorsStart + ((long)sectorIndex * _fileSectorRawSize) + _fileSectorUserStart;
        }

        /*private long UserPositionToRawPosition(long position)
        {
            var sectorIndex = SectorIndexFromUserPosition(position);
            // Shorthand verion: Add the difference between raw/user sector sizes and use position as-is.
            //var rawAdditional = (long)sectorIndex * (_fileSectorRawSize - _fileSectorUserSize);
            //return _fileSectorsStart + rawAdditional + position;
            // Longer verion: Easier to read.
            var rawPosition = SectorIndexToRawPosition(sectorIndex);
            var sectorOffset = SectorOffsetFromUserPosition(position);
            return rawPosition + sectorOffset + _fileSectorUserStart;
        }

        private long UserPositionFromRawPosition(long rawPosition)
        {
            var sectorIndex = SectorIndexFromRawPosition(rawPosition);
            var sectorOffset = GeomMath.Clamp(SectorOffsetFromRawPosition(rawPosition), 0, _fileSectorUserSize);
            return SectorIndexToUserPosition(sectorIndex) + sectorOffset;
        }*/
    }
}

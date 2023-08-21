using System;
using System.IO;

namespace PSXPrev.Common.Parsers
{
    public class BinCDStream : Stream
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

        private readonly Stream _stream;
        private readonly bool _leaveOpen;
        private readonly byte[] _buffer;
        private readonly long _length;
        private readonly int _sectorCount; // Total number of sectors in the file after _fileSectorsStart

        private int _sectorIndex;          // Index of current sector
        private int _sectorPosition;       // Index inside current sector
        private int _lastReadSectorIndex;  // Index of last sector that was read into the buffer

        public BinCDStream(string file, bool leaveOpen = false)
            : this(File.OpenRead(file), leaveOpen)
        {
        }

        public BinCDStream(Stream stream, bool leaveOpen = false)
            : this(stream, SectorsFirstIndex, SectorRawSize, SectorUserStart, SectorUserSize, leaveOpen)
        {
        }

        // Set sectorsFirstIndex to 0 to read information like PS1 logo model
        // Set sectorUserSize to 2032 and sectorUserStart to 40 for Star Ocean 2
        public BinCDStream(string file, int sectorsFirstIndex, int sectorRawSize, int sectorUserStart, int sectorUserSize, bool leaveOpen = false)
            : this(File.OpenRead(file), sectorsFirstIndex, sectorRawSize, sectorUserStart, sectorUserSize, leaveOpen)
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
            _sectorPosition = 0;
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


        public int SectorIndex => _sectorIndex;

        public int SectorCount => _sectorCount;

        public override long Position
        {
            get => SectorIndexToUserPosition(_sectorIndex) + _sectorPosition;
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
            offset = GeomMath.Clamp(offset, 0, _length);

            _sectorIndex = SectorIndexFromUserPosition(offset);
            _sectorPosition = SectorPositionFromUserPosition(offset);
            return Position;
        }

        public override int ReadByte()
        {
            if (!PrepareSectorForRead())
            {
                return -1; // End of stream
            }
            return _buffer[_sectorPosition++];
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
                var sectorLeft = _fileSectorUserSize - _sectorPosition;
                var sectorRead = Math.Min(sectorLeft, count);
                Buffer.BlockCopy(_buffer, _sectorPosition, buffer, offset + bytesRead, sectorRead);
                bytesRead += sectorRead;
                count -= sectorRead;
                _sectorPosition += sectorRead;
            }
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Stream is not writable");
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Stream is not writable");
        }


        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !_leaveOpen)
                {
                    _stream.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


        private bool PrepareSectorForRead()
        {
            // Increment the sector if we've reached the end
            if (_sectorPosition == _fileSectorUserSize)
            {
                _sectorIndex++;
                _sectorPosition = 0;
            }
            // Check if we've reached EOF
            if (_sectorIndex == _sectorCount)
            {
                return false; // Nothing more to read
            }
            // Read the sector into the buffer if we haven't already done so.
            if (_sectorIndex != _lastReadSectorIndex)
            {
                // We always need to seek, since we need to skip non-user data between sectors
                var rawPosition = SectorIndexToRawPosition(_sectorIndex);
                _stream.Seek(rawPosition, SeekOrigin.Begin);
                _stream.Read(_buffer, 0, _fileSectorUserSize);
                _lastReadSectorIndex = _sectorIndex;
            }
            return true;
        }


        private int SectorPositionFromUserPosition(long position)
        {
            return (int)(position % _fileSectorUserSize);
        }

        private int SectorIndexFromUserPosition(long position)
        {
            return (int)(position / _fileSectorUserSize);
        }

        private long SectorIndexToUserPosition(int sectorIndex)
        {
            return ((long)sectorIndex * _fileSectorUserSize);
        }

        /*private int SectorPositionFromRawPosition(long rawPosition)
        {
            return (int)((rawPosition - _fileSectorsStart) % _fileSectorRawSize) - _fileSectorUserStart;
        }*/

        private int SectorIndexFromRawPosition(long rawPosition)
        {
            return (int)((rawPosition - _fileSectorsStart) / _fileSectorRawSize);
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
            var sectorPosition = SectorPositionFromUserPosition(position);
            return rawPosition + SECTOR_USER_START + sectorPosition;
        }

        private long UserPositionFromRawPosition(long rawPosition)
        {
            var sectorIndex = SectorIndexFromRawPosition(rawPosition);
            if (sectorIndex < 0)
            {
                return 0;
            }
            var sectorPosition = GeomMath.Clamp(SectorPositionFromRawPosition(rawPosition), 0, _fileSectorUserSize);
            return SectorIndexToUserPosition(sectorIndex) + sectorPosition;
        }*/
    }
}

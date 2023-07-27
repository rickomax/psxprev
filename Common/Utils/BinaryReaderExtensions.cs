using System;
using System.IO;

// Big Endian alternative methods taken from:
// <https://github.com/zeroKilo/CrocExplorerWV/tree/master/CrocExplorerWV>

namespace PSXPrev.Common.Utils
{
    public static class BinaryReaderExtensions
    {
        public static short ReadInt16BE(this BinaryReader reader) => (short)reader.ReadUInt16BE();

        public static ushort ReadUInt16BE(this BinaryReader reader)
        {
            uint value = reader.ReadByte();
            value = (value << 8) | reader.ReadByte();
            return (ushort)value;
        }

        public static int ReadInt32BE(this BinaryReader reader) => (int)reader.ReadUInt32BE();

        public static uint ReadUInt32BE(this BinaryReader reader)
        {
            uint value = reader.ReadByte();
            for (var i = 1; i < 4; i++)
            {
                value = (value << 8) | reader.ReadByte(); // JIT should loop unroll this
            }
            return value;
        }

        public static long ReadInt64BE(this BinaryReader reader) => (long)reader.ReadUInt64BE();

        public static ulong ReadUInt64BE(this BinaryReader reader)
        {
            ulong value = reader.ReadByte();
            for (var i = 1; i < 8; i++)
            {
                value = (value << 8) | reader.ReadByte(); // JIT should loop unroll this
            }
            return value;
        }

        public static float ReadSingleBE(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
            // This doesn't exist yet in the current .NET version we're using.
            //return BitConverter.Int32BitsToSingle(reader.ReadInt32());
        }

        public static double ReadDoubleBE(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
            // Alternative:
            //return BitConverter.Int64BitsToDouble(reader.ReadInt64());
        }
    }
}

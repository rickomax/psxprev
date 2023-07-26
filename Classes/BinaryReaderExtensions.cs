using System.IO;

// Big Endian alternative methods taken from:
// <https://github.com/zeroKilo/CrocExplorerWV/tree/master/CrocExplorerWV>

namespace PSXPrev.Classes
{
    public static class BinaryReaderExtensions
    {
        public static short ReadInt16BE(this BinaryReader reader) => (short)reader.ReadUInt16BE();

        public static ushort ReadUInt16BE(this BinaryReader reader)
        {
            uint value;
            value  = ((uint)reader.ReadByte() <<  8);
            value |= ((uint)reader.ReadByte() <<  0);
            //uint value = reader.ReadByte();
            //value = (value << 8) | reader.ReadByte();
            return (ushort)value;
        }

        public static int ReadInt32BE(this BinaryReader reader) => (int)reader.ReadUInt32BE();

        public static uint ReadUInt32BE(this BinaryReader reader)
        {
            uint value;
            value  = ((uint)reader.ReadByte() << 24);
            value |= ((uint)reader.ReadByte() << 16);
            value |= ((uint)reader.ReadByte() <<  8);
            value |= ((uint)reader.ReadByte() <<  0);
            //uint value = reader.ReadByte();
            //value = (value << 8) | reader.ReadByte();
            //value = (value << 8) | reader.ReadByte();
            //value = (value << 8) | reader.ReadByte();
            return value;
        }
    }
}

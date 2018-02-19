using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace PSXPrev
{
    public class BigEndianReader : BinaryReader
    {
        private byte[] _a16 = new byte[2];
        private byte[] _a32 = new byte[4];
        private byte[] a64 = new byte[8];

        public BigEndianReader(Stream stream) : base(stream, Encoding.Unicode)
        {
        }

        public override Int16 ReadInt16()
        {
            _a16 = base.ReadBytes(2);
            Array.Reverse(_a16);
            return BitConverter.ToInt16(_a16, 0);
        }

        public override int ReadInt32()
        {
            _a32 = base.ReadBytes(4);
            Array.Reverse(_a32);
            return BitConverter.ToInt32(_a32, 0);
        }

        public override Int64 ReadInt64()
        {
            a64 = base.ReadBytes(8);
            Array.Reverse(a64);
            return BitConverter.ToInt64(a64, 0);
        }

        public override UInt16 ReadUInt16()
        {
            _a16 = base.ReadBytes(2);
            Array.Reverse(_a16);
            return BitConverter.ToUInt16(_a16, 0);
        }

        public override UInt32 ReadUInt32()
        {
            _a32 = base.ReadBytes(4);
            Array.Reverse(_a32);
            return BitConverter.ToUInt32(_a32, 0);
        }

        public override Single ReadSingle()
        {
            _a32 = base.ReadBytes(4);
            Array.Reverse(_a32);
            return BitConverter.ToSingle(_a32, 0);
        }

        public override UInt64 ReadUInt64()
        {
            a64 = base.ReadBytes(8);
            Array.Reverse(a64);
            return BitConverter.ToUInt64(a64, 0);
        }

        public override Double ReadDouble()
        {
            a64 = base.ReadBytes(8);
            Array.Reverse(a64);
            return BitConverter.ToUInt64(a64, 0);
        }

        public string ReadStringToNull()
        {
            string result = "";
            char c;
            for (int i = 0; i < base.BaseStream.Length; i++)
            {
                if ((c = (char) base.ReadByte()) == 0)
                {
                    break;
                }
                result += c.ToString(CultureInfo.InvariantCulture);
            }
            return result;
        }
    }
}
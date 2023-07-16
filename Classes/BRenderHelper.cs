using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//taken from:
//https://github.com/zeroKilo/CrocExplorerWV/tree/master/CrocExplorerWV

namespace PSXPrev.Classes
{
    public static class BRenderHelper
    {
        public static uint ReadU32BE(Stream s)
        {
            ulong result = 0;
            result = (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            return (uint)result;
        }

        public static ushort ReadU16BE(Stream s)
        {
            ulong result = 0;
            result = (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            return (ushort)result;
        }

        public static uint ReadU32LE(Stream s)
        {
            ulong result = 0;
            result = (byte)s.ReadByte();
            result |= (ulong)((byte)s.ReadByte() << 8);
            result |= (ulong)((byte)s.ReadByte() << 16);
            result |= (ulong)((byte)s.ReadByte() << 24);
            return (uint)result;
        }

        public static ushort ReadU16LE(Stream s)
        {
            ulong result = 0;
            result = (byte)s.ReadByte();
            result |= (ulong)((byte)s.ReadByte() << 8);
            return (ushort)result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSXPrev
{
    public class MissingTriangle
    {
        [ReadOnly(true), DisplayName("Ilen")]
        public int Ilen { get; set; }

        [ReadOnly(true), DisplayName("Olen")]
        public int Olen { get; set; }

        [ReadOnly(true), DisplayName("Flags")]
        public uint Flags { get; set; }

        [ReadOnly(true), DisplayName("Mode")]
        public int Mode { get; set; }

        public override string ToString()
        {
            return string.Format("olen:{0:X}, ilen:{1:X}, mode:{2:X}, flags:{3:X}", Olen, Ilen, Mode, Flags);
        }
    }
}

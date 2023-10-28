using System.IO;

namespace PSXPrev.Common.Parsers
{
    // Not supported yet, since we have no way to store loose palettes.
    public class CLTParser : FileOffsetScanner
    {
        public const string FormatNameConst = "CLT";

        public CLTParser(TextureAddedAction textureAdded)
            : base(textureAdded: textureAdded)
        {
        }

        public override string FormatName => FormatNameConst;

        protected override void Parse(BinaryReader reader)
        {
            if (!ReadCLT(reader))
            {
                //foreach (var texture in TextureResults)
                //{
                //    texture.Dispose();
                //}
                //TextureResults.Clear();
            }
        }

        private bool ReadCLT(BinaryReader reader)
        {
            var header = reader.ReadUInt32();
            var id       = (header      ) & 0xff;
            var version  = (header >>  8) & 0xff;
            var reserved = (header >> 16);
            // How we originally ignored version:
            if (id != 0x12 || version != 0x00 || (!Limits.IgnoreTIMVersion && reserved != 0))
            //if (id != 0x12 || (!Limits.IgnoreTIMVersion && (version != 0x00 || reserved != 0)))
            {
                return false;
            }

            var flag = reader.ReadUInt32();
            var pmode = (flag & 0x3);
            // Reduce false positives, since the hibits of flag should be all zeroes.
            if (!Limits.IgnoreTIMVersion && (flag & ~0x3u) != 0)
            {
                return false;
            }

            var clutPosition = reader.BaseStream.Position;
            var clutSize = reader.ReadUInt32(); // Size of image data starting at this field
            var clutDx = reader.ReadUInt16(); // Frame buffer coordinates
            var clutDy = reader.ReadUInt16();
            var clutWidth  = reader.ReadUInt16();
            var clutHeight = reader.ReadUInt16();
            if (clutSize < 12 + clutHeight * clutWidth * 2)
            {
                return false;
            }

            // Noted in jpsxdec/CreateTim that some files can claim an unpaletted pmode but still use a palette.
            // This may be why the CLT format allows pmode 2 and 3, even though PXL doesn't.
            var bpp = TIMParser.InferBppFromClut(pmode, clutWidth);

            var palettes = TIMParser.ReadPalettes(reader, bpp, clutWidth, clutHeight, out var hasSemiTransparency, false);
            reader.BaseStream.Seek(clutPosition + clutSize, SeekOrigin.Begin);
            if (palettes != null)
            {
                // We can't do anything with this yet...
                return true;
            }

            return false;
        }
    }
}  

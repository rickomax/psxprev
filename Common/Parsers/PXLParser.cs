using System.IO;

namespace PSXPrev.Common.Parsers
{
    // Not supported yet, since we have no way to assign loose palettes to textures.
    public class PXLParser : FileOffsetScanner
    {
        public PXLParser(TextureAddedAction textureAdded)
            : base(textureAdded: textureAdded)
        {
        }

        public override string FormatName => "PXL";

        protected override void Parse(BinaryReader reader)
        {
            if (!ReadPXL(reader))
            {
                //foreach (var texture in TextureResults)
                //{
                //    texture.Dispose();
                //}
                //TextureResults.Clear();
            }
        }

        private bool ReadPXL(BinaryReader reader)
        {
            var header = reader.ReadUInt32();
            var id       = (header      ) & 0xff;
            var version  = (header >>  8) & 0xff;
            var reserved = (header >> 16);
            // How we originally ignored version:
            if (id != 0x11 || version != 0x00 || (!Limits.IgnoreTIMVersion && reserved != 0))
            //if (id != 0x11 || (!Limits.IgnoreTIMVersion && (version != 0x00 || reserved != 0)))
            {
                return false;
            }

            var flag = reader.ReadUInt32();
            var pmode = (flag & 0x1);
            // Reduce false positives, since the hibits of flag should be all zeroes.
            if (!Limits.IgnoreTIMVersion && (flag & ~0x1u) != 0)
            {
                return false;
            }

            var bpp = TIMParser.GetBppFromMode(pmode);

            // Use a pre-allocated empty palette until we have a real palette selected.
            var palettes = Texture.GetEmptyPalettes(bpp);
            var hasSemiTransparency = true; // Always allocate a semi-transparent map

            var imagePosition = reader.BaseStream.Position;
            var imageSize = reader.ReadUInt32(); // Size of image data starting at this field
            var dx = reader.ReadUInt16(); // Frame buffer coordinates
            var dy = reader.ReadUInt16();
            var stride = reader.ReadUInt16(); // Stride in units of 2 bytes
            var height = reader.ReadUInt16();
            if (imageSize < 12 + height * stride * 2)
            {
                return false;
            }

            var texture = TIMParser.ReadTexture(reader, bpp, stride, height, dx, dy, 0, palettes, hasSemiTransparency, false);
            reader.BaseStream.Seek(imagePosition + imageSize, SeekOrigin.Begin);
            if (texture != null)
            {
                texture.NeedsPalette = true; // Marks this texture as needing a user-assigned palette
                TextureResults.Add(texture);
                return true;
            }

            return false;
        }
    }
}  

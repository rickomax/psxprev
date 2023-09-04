using System.Collections.Generic;
using System.IO;

namespace PSXPrev.Common.Parsers
{
    // Blitz Games: .SPT texture library format
    // Not to be confused with model sprites that always face the camera.
    public class SPTParser : FileOffsetScanner
    {
        public const string FormatNameConst = "SPT";

        private readonly List<SPTHeader> _sprites = new List<SPTHeader>();

        public SPTParser(TextureAddedAction textureAdded)
            : base(textureAdded: textureAdded)
        {
        }

        public override string FormatName => FormatNameConst;

        protected override void Parse(BinaryReader reader)
        {
            _sprites.Clear();

            if (!ParseSPT(reader))
            {
                foreach (var texture in TextureResults)
                {
                    texture.Dispose();
                }
                TextureResults.Clear();
            }
        }

        private bool ParseSPT(BinaryReader reader)
        {
            var lastSprite = false;

            while (!lastSprite && _sprites.Count < (int)Limits.MaxSPTSprites)
            {
                var imageTop = reader.ReadUInt32();
                var clutTop  = reader.ReadUInt32();
                var width  = reader.ReadByte();
                var height = reader.ReadByte();
                var u = reader.ReadInt16();
                var v = reader.ReadInt16();
                var flags = reader.ReadUInt16();
                var nameCRC = reader.ReadUInt32();

                lastSprite  = (flags & 0x1) != 0;
                var clut256 = (flags & 0x2) != 0;

                if (width == 0 || height == 0 || nameCRC == 0 || imageTop < 20 || clutTop < 20)
                {
                    return false;
                }
                if ((!clut256 && width % 4 != 0) || (clut256 && width % 2 != 0))
                {
                    return false; // Stride can't match width
                }
                if ((flags & ~0x3f) != 0)
                {
                    return false; // Invalid flags
                }
                if (_offset + imageTop >= reader.BaseStream.Length || _offset + clutTop >= reader.BaseStream.Length)
                {
                    return false;
                }

                _sprites.Add(new SPTHeader
                {
                    ImageTop = imageTop,
                    CLUTTop = clutTop,
                    Width = width,
                    Height = height,
                    //U = u,
                    //V = v,
                    Flags = flags,
                    NameCRC = nameCRC,
                });
            }
            if (!lastSprite)
            {
                return false; // Too many sprites
            }

            var dataTop = reader.BaseStream.Position - _offset;

            for (var i = 0; i < _sprites.Count; i++)
            {
                var s = _sprites[i];

                if (s.ImageTop < dataTop || s.CLUTTop < dataTop)
                {
                    return false;
                }

                var clut256 = (s.Flags & 0x2) != 0;

                var pmode     = TIMParser.GetModeFromBpp(!clut256 ? 4 : 8);
                var clutWidth = TIMParser.GetClutWidth(pmode);
                var stride    = TIMParser.GetStride(pmode, s.Width);

                reader.BaseStream.Seek(_offset + s.CLUTTop, SeekOrigin.Begin);
                var palettes = TIMParser.ReadPalettes(reader, pmode, clutWidth, 1, out var hasSemiTransparency, false, false);
                if (palettes == null)
                {
                    return false;
                }
                var maskMagenta = TexturePalette.FromComponents(248, 0, 248, false);
                hasSemiTransparency |= TexturePalette.MaskColor(palettes, maskMagenta, true, true); // todo: Should we be ignoring stp bit?


                reader.BaseStream.Seek(_offset + s.ImageTop, SeekOrigin.Begin);
                var unknown1 = reader.ReadUInt32();
                // Sprites don't define X/Y, that's determined by packing.
                var texture = TIMParser.ReadTexture2(reader, stride, s.Width, s.Height, 0, 0, 0, pmode, 0, palettes, hasSemiTransparency, true);
                if (texture == null)
                {
                    return false;
                }
                // Assign loose texture settings
                texture.LookupID = s.NameCRC;

                TextureResults.Add(texture);
            }

            // Each header entry is a valid start to an SPT file.
            // So we need to prevent the same SPT file from producing (N-0)+(N-1)+(N-2)....+(N-N+1) textures.
            MinOffsetIncrement = dataTop;

            return true;
        }


        private class SPTHeader
        {
            public uint ImageTop;
            public uint CLUTTop;
            public byte Width;
            public byte Height;
            //public short U; // Unused
            //public short V;
            public ushort Flags;
            public uint NameCRC;
        }
    }
}

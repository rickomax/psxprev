using System;
using System.Collections.Generic;
using System.IO;

namespace PSXPrev.Common.Parsers
{
    // Blitz Games: .SPT texture library format
    // Not to be confused with model sprites that always face the camera.
    public class SPTParser : FileOffsetScanner
    {
        public const string FormatNameConst = "SPT";

        private static readonly ushort MaskMagenta = TexturePalette.FromComponents(248, 0, 248, false);

        private readonly List<SPTHeader> _sprites = new List<SPTHeader>();

        public SPTParser(TextureAddedAction textureAdded)
            : base(textureAdded: textureAdded)
        {
        }

        public override string FormatName => FormatNameConst;

        public override long MinAlignment => 2048; // Prevent eating up gigabytes of memory in 20 seconds flat

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
            //var splitStart = -1;

            while (!lastSprite && _sprites.Count < (int)Limits.MaxSPTSprites)
            {
                var imageTop = reader.ReadUInt32();
                var clutTop  = reader.ReadUInt32();
                var width  = reader.ReadByte();
                var height = reader.ReadByte();
                // Only used for UI/HUD images, negative values usually used, which I guess point the image towards its center.
                // You can see this often used for split images to show each of their relative offsets.
                var originX = reader.ReadInt16();
                var originY = reader.ReadInt16();
                var flags = reader.ReadUInt16();
                var nameCRC = reader.ReadUInt32();

                lastSprite    = (flags & 0x01) != 0;
                //var clut256   = (flags & 0x02) != 0;
                // This appears in a row for image columns until the final images.
                // For example, an image that's split into four coloumns will have this flag set for the first 3 images.
                // HOWEVER, this wasn't always the case. Action Man 2 uses this flag just about everywhere, so it likely
                // had a different meaning back then.
                //var hsplit    = (flags & 0x04) != 0;
                //var bitStream = (flags & 0x08) != 0; // Unknown
                //var alpha     = (flags & 0x10) != 0; // Unknown
                //var colorKey  = (flags & 0x20) != 0; // Unknown, can't be magenta masking since some sprites are missing it
                // Can be used together with hsplit, as seen in Chicken Run.
                //var vsplit    = (flags & 0x40) != 0;


                //if (splitStart == -1 && (hsplit || vsplit))
                //{
                //    splitStart = _sprites.Count;
                //}

                // nameCRC has been zero before, so we can't use that to rule out false positives anymore...
                if (width == 0 || height == 0 || imageTop < 20 || clutTop < 20)
                {
                    return false;
                }
                // Sadly, some textures have uneven widths (i.e. 26 for 4bpp). So we can't use this to rule out false positives.
                //if ((!clut256 && width % 4 != 0) || (clut256 && width % 2 != 0))
                //{
                //    //return false; // Stride can't match width
                //}
                if ((flags & ~0x7fu) != 0)
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
                    OriginX = originX,
                    OriginY = originY,
                    Flags = flags,
                    NameCRC = nameCRC,
                });

                //if (!hsplit && !vsplit)
                //{
                //    splitStart = -1; // End current split sprite (if there was one)
                //}
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

                var clut256   = (s.Flags & 0x02) != 0;
                var alpha     = (s.Flags & 0x10) != 0; // Unknown
                var colorKey  = (s.Flags & 0x20) != 0; // Unknown, can't be magenta masking since some sprites are missing it

                var bpp = !clut256 ? 4 : 8;
                var clutWidth = TIMParser.GetClutWidth(bpp);

                reader.BaseStream.Seek(_offset + s.CLUTTop, SeekOrigin.Begin);
                var palettes = TIMParser.ReadPalettes(reader, bpp, clutWidth, 1, out var hasSemiTransparency, false, false);
                if (palettes == null)
                {
                    return false;
                }

                var origPalettes = MaskPalette(colorKey, alpha, palettes, ref hasSemiTransparency);


                reader.BaseStream.Seek(_offset + s.ImageTop, SeekOrigin.Begin);
                var pixelCount = reader.ReadUInt32();
                if (pixelCount != s.Width * s.Height)
                {
                    var breakHere = 0;
                }
                // Sprites don't define X/Y, that's determined by packing.
                var texture = TIMParser.ReadTexturePacked(reader, bpp, s.Width, s.Height, 0, palettes, hasSemiTransparency, true);
                if (texture == null)
                {
                    return false;
                }
                // Assign loose texture settings
                texture.LookupID = s.NameCRC;
                texture.OriginalPalettes = origPalettes;
#if DEBUG
                texture.DebugData = new[] { $"pixelCount: 0x{pixelCount:x}", $"0x{s.Flags:x04}", $"origin: {s.OriginX},{s.OriginY}" };
#endif
                TextureResults.Add(texture);
            }

            // Each header entry is a valid start to an SPT file.
            // So we need to prevent the same SPT file from producing (N-0)+(N-1)+(N-2)....+(N-N+1) textures.
            MinOffsetIncrement = dataTop;

            return true;
        }

        private static ushort[][] MaskPalette(bool colorKey, bool alpha, ushort[][] palettes, ref bool? hasSemiTransparency)
        {
            var palette = palettes[0];
            var paletteSize = palette.Length;
            ushort[][] origPalettes = null;
            //var maskColor = palette[0]; // Only used if colorKey is true
            for (var c = 0; c < paletteSize; c++)
            {
                var color = palette[c];
                var newColor = color;
                // todo: Should we be ignoring stp bit?
                // Not sure what to think any more about colorKey, with how magenta is masked...
                /*if (colorKey && TexturePalette.Equals(color, maskColor, true))
                {
                    newColor = TexturePalette.Transparent;
                }
                else*/ if (TexturePalette.CloseTo8(color, MaskMagenta, 1, true))
                {
                    // Magenta seems to always be masked. Sometimes these colors are 1 bit off
                    // (sometimes more, but we should ignore those cases since they can conflict)
                    newColor = TexturePalette.Transparent;
                }
                // Never countered this flag, so not sure how its used...
                else if (/*alpha ||*/ color == TexturePalette.Transparent)
                {
                    newColor = TexturePalette.SetStp(color, true);
                    hasSemiTransparency = true;
                }
                else
                {
                    newColor = TexturePalette.SetStp(color, true);
                    hasSemiTransparency = true;
                }

                if (color != newColor)
                {
                    if (origPalettes == null)
                    {
                        origPalettes = new ushort[][] { (ushort[])palette.Clone() };
                    }
                    palette[c] = newColor;
                }
            }

            return origPalettes ?? palettes;
        }


        private struct SPTHeader
        {
            public uint ImageTop;
            public uint CLUTTop;
            public byte Width;
            public byte Height;
            public short OriginX; // Unused
            public short OriginY;
            public ushort Flags;
            public uint NameCRC;
        }
    }
}

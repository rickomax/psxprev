using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class TIMParser : FileOffsetScanner
    {
        public TIMParser(TextureAddedAction textureAdded)
            : base(textureAdded: textureAdded)
        {
        }

        public override string FormatName => "TIM";

        protected override void Parse(BinaryReader reader)
        {
            var id = reader.ReadUInt16();
            if (id == 0x10)
            {
                var version = reader.ReadUInt16();
                if (Limits.IgnoreTIMVersion || version == 0x00)
                {
                    var texture = ParseTim(reader);
                    if (texture != null)
                    {
                        TextureResults.Add(texture);
                    }
                }
            }
        }

        private Texture ParseTim(BinaryReader reader)
        {
            var flag = reader.ReadUInt32();
            var pmode = (flag & 0x7);
            if (pmode == 4 || pmode > 4)
            {
                return null; // Mixed format not supported (check now to speed up TIM scanning), or invalid pmode
            }
            var hasClut = (flag & 0x8) != 0;
            // Reduce false positives, since the hiword of flag should be all zeroes.
            //if (!Limits.IgnoreTIMVersion && (flag & 0xffff0000) != 0)
            //{
            //    return null;
            //}

            System.Drawing.Color[] palette = null;
            bool[] semiTransparentPalette = null;
            if (hasClut)
            {
                var clutBnum = reader.ReadUInt32(); // Size of clut data starting at this field
                var clutDx = reader.ReadUInt16();
                var clutDy = reader.ReadUInt16();
                var clutWidth = reader.ReadUInt16();
                var clutHeight = reader.ReadUInt16();

                // Noted in jpsxdec/CreateTim that some files can claim an unpaletted pmode but still use a palette.
                if (pmode == 2)
                {
                    pmode = GetModeFromClut(clutWidth, clutHeight);
                }
                else if (pmode == 3)
                {
                    pmode = 1; // 8bpp (256clut)
                }

                palette = ReadPalette(reader, pmode, clutWidth, clutHeight, out semiTransparentPalette, false);
            }

            if (pmode < 2 && palette == null)
            {
                return null; // No palette for clut format (check now to speed up TIM scanning)
            }

            var imgBnum = reader.ReadUInt32(); // Size of image data starting at this field
            var imgDx = reader.ReadUInt16();
            var imgDy = reader.ReadUInt16();
            var imgStride = reader.ReadUInt16(); // Stride in units of 2 bytes
            var imgHeight = reader.ReadUInt16();

            return ReadTexture(reader, imgStride, imgHeight, imgDx, imgDy, pmode, palette, semiTransparentPalette, false);
        }

        public static System.Drawing.Color[] ReadPalette(BinaryReader reader, uint pmode, uint clutWidth, uint clutHeight, out bool[] semiTransparentPalette, bool allowOutOfBounds)
        {
            semiTransparentPalette = null;

            if (clutWidth == 0 || clutHeight == 0 || clutWidth > 256 || clutHeight > 256)
            {
                return null;
            }

            // HMD: Support models with invalid image data, but valid model data.
            var clutDataSize = (clutHeight * clutWidth * 2);
            if (allowOutOfBounds && clutDataSize + reader.BaseStream.Position > reader.BaseStream.Length)
            {
                return null;
            }

            var count = clutWidth * clutHeight;
            System.Drawing.Color[] palette = null;
            // We should probably allocate the full 16clut or 256clut in-case an image pixel has bad data.
            switch (pmode)
            {
                case 0:
                    palette = new System.Drawing.Color[16];
                    break;
                case 1:
                    palette = new System.Drawing.Color[256];
                    break;
            }
            if (palette != null)
            {
                for (var c = 0; c < palette.Length; c++)
                {
                    System.Drawing.Color color;
                    if (c >= count)
                    {
                        // Use default masking black as fallback color.
                        color = System.Drawing.Color.FromArgb(255, 0, 0, 0);
                    }
                    else
                    {
                        var data = reader.ReadUInt16();
                        var r = (data) & 0x1f;
                        var g = (data >> 5) & 0x1f;
                        var b = (data >> 10) & 0x1f;
                        var stp = ((data >> 15) & 0x1) == 1; // Semi-transparency: 0-Off, 1-On
                        var a = 255;

                        // Note: stpMode (not stp) is defined on a per polygon basis. We can't apply alpha now, only during rendering.
                        if (stp)
                        {
                            if (semiTransparentPalette == null)
                            {
                                semiTransparentPalette = new bool[palette.Length];
                            }
                            semiTransparentPalette[c] = true;
                        }
                        else if (r == 0 && g == 0 && b == 0)
                        {
                            a = 0; // Transparent when black and !stp
                        }

                        color = System.Drawing.Color.FromArgb(a, r * 8, g * 8, b * 8);
                    }
                    palette[c] = color;
                }
            }
            return palette;
        }

        public static Texture ReadTexture(BinaryReader reader, ushort stride, ushort height, ushort dx, ushort dy, uint pmode, System.Drawing.Color[] palette, bool[] semiTransparentPalette, bool allowOutOfBounds)
        {
            if ((pmode == 0 || pmode == 1) && palette == null)
            {
                return null; // No palette for clut format
            }
            if (pmode == 4 || pmode > 4)
            {
                return null; // Mixed format not supported, or invalid pmode
            }

            var textureBpp = GetBpp(pmode);
            var textureWidth = stride * 16 / textureBpp;
            var textureHeight = height;

            if (stride == 0 || height == 0 || textureWidth > (int)Limits.MaxTIMResolution || height > Limits.MaxTIMResolution)
            {
                return null;
            }

            // HMD: Support models with invalid image data, but valid model data.
            var textureDataSize = (textureHeight * textureWidth * textureBpp / 8);
            if (allowOutOfBounds && textureDataSize + reader.BaseStream.Position > reader.BaseStream.Length)
            {
                return null;
            }

            var texturePageX = dx / 64;
            if (texturePageX > 16)
            {
                return null;
            }
            var textureOffsetX = texturePageX * 64;

            var texturePageY = dy / 256; // Changed from 255
            if (texturePageY > 2)
            {
                return null;
            }
            var textureOffsetY = texturePageY * 256;

            var texturePage = (texturePageY * 16) + texturePageX;
            var textureX = (dx - textureOffsetX) * 16 / textureBpp;// Math.Min(16, textureBpp); // todo: Or is this the same as textureWidth?
            var textureY = (dy - textureOffsetY);


            var texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, texturePage);
            try
            {
                var bitmap = texture.Bitmap;
                var semiTransparentMap = semiTransparentPalette != null ? texture.SetupSemiTransparentMap() : null;

                switch (pmode)
                {
                    case 0: // 4bpp (16clut)
                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < stride; x++)
                            {
                                var data = reader.ReadUInt16();
                                var index1 = (data) & 0xf;
                                var index2 = (data >> 4) & 0xf;
                                var index3 = (data >> 8) & 0xf;
                                var index4 = (data >> 12) & 0xf;

                                var color1 = palette[index1];
                                var color2 = palette[index2];
                                var color3 = palette[index3];
                                var color4 = palette[index4];

                                bitmap.SetPixel((x * 4) + 0, y, color1);
                                bitmap.SetPixel((x * 4) + 1, y, color2);
                                bitmap.SetPixel((x * 4) + 2, y, color3);
                                bitmap.SetPixel((x * 4) + 3, y, color4);

                                if (semiTransparentPalette != null)
                                {
                                    if (semiTransparentPalette[index1])
                                    {
                                        semiTransparentMap.SetPixel((x * 4) + 0, y, Texture.SemiTransparentFlag);
                                    }
                                    if (semiTransparentPalette[index2])
                                    {
                                        semiTransparentMap.SetPixel((x * 4) + 1, y, Texture.SemiTransparentFlag);
                                    }
                                    if (semiTransparentPalette[index3])
                                    {
                                        semiTransparentMap.SetPixel((x * 4) + 2, y, Texture.SemiTransparentFlag);
                                    }
                                    if (semiTransparentPalette[index4])
                                    {
                                        semiTransparentMap.SetPixel((x * 4) + 3, y, Texture.SemiTransparentFlag);
                                    }
                                }
                            }
                        }
                        break;

                    case 1: // 8bpp (256clut)
                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < stride; x++)
                            {
                                var data = reader.ReadUInt16();
                                var index1 = (data) & 0xff;
                                var index2 = (data >> 8) & 0xff;

                                var color1 = palette[index1];
                                var color2 = palette[index2];

                                bitmap.SetPixel((x * 2) + 0, y, color1);
                                bitmap.SetPixel((x * 2) + 1, y, color2);

                                if (semiTransparentPalette != null)
                                {
                                    if (semiTransparentPalette[index1])
                                    {
                                        semiTransparentMap.SetPixel((x * 2) + 0, y, Texture.SemiTransparentFlag);
                                    }
                                    if (semiTransparentPalette[index2])
                                    {
                                        semiTransparentMap.SetPixel((x * 2) + 1, y, Texture.SemiTransparentFlag);
                                    }
                                }
                            }
                        }
                        break;

                    case 2: // 16bpp (5/5/5)
                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < stride; x++)
                            {
                                var data = reader.ReadUInt16();
                                var r = (data) & 0x1f;
                                var g = (data >> 5) & 0x1f;
                                var b = (data >> 10) & 0x1f;
                                var stp = ((data >> 15) & 0x1) == 1; // Semi-transparency: 0-Off, 1-On
                                var a = 255;

                                // Note: stpMode (not stp) is defined on a per polygon basis. We can't apply alpha now, only during rendering.
                                if (stp)
                                {
                                    if (semiTransparentMap == null)
                                    {
                                        semiTransparentMap = texture.SetupSemiTransparentMap();
                                    }
                                    semiTransparentMap.SetPixel(x, y, Texture.SemiTransparentFlag);
                                }
                                else if (r == 0 && g == 0 && b == 0)
                                {
                                    a = 0; // Transparent when black and !stp
                                }

                                var color1 = System.Drawing.Color.FromArgb(a, r * 8, g * 8, b * 8);

                                bitmap.SetPixel(x, y, color1);
                            }
                        }
                        break;

                    case 3: // 24bpp
                        var padding = (stride * 2) - (textureWidth * 3);

                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < textureWidth; x++)
                            {
                                var r = reader.ReadByte();
                                var g = reader.ReadByte();
                                var b = reader.ReadByte();

                                var color1 = System.Drawing.Color.FromArgb(255, r, g, b);

                                bitmap.SetPixel(x, y, color1);
                            }
                            // todo: Is there padding at the end of rows?
                            //       It's probably padding to 2-bytes if there is any, rather than 4-bytes.
                            for (var p = 0; p < padding; p++)
                            {
                                reader.ReadByte();
                            }
                        }
                        break;

                    case 4: // Mixed (not supported yet)
                        texture.Dispose();
                        texture = null;
                        break;
                }
            }
            catch
            {
                texture.Dispose(); // Cleanup on failure to parse
                throw;
            }

            return texture;
        }

        public static uint GetModeFromClut(ushort clutWidth, ushort clutHeight)
        {
            // NOTE: Width*height always seems to be 16 or 256.
            //       Specifically width was 16 or 256 and height was 1.
            //       With that, it's safe to assume the dimensions tell us the color count.
            //       Because this data could potentionally give us something other than 16 or 256,
            //       assume anything greater than 16 will allocate a 256clut and only read w*h colors.

            // todo: Which is correct?
            //return (clutWidth * clutHeight <= 16 ? 0u : 1u);
            return (clutWidth * clutHeight < 256 ? 0u : 1u);
        }

        public static uint GetModeFromNoClut()
        {
            return 2u;
        }

        public static int GetBpp(uint pmode)
        {
            switch (pmode)
            {
                case 0: return  4; // 4bpp (16clut)
                case 1: return  8; // 8bpp (256clut)
                case 2: return 16; // 16bpp (5/5/5)
                case 3: return 24; // 24bpp
                case 4: return  0; // Mixed
            }
            return -1;
        }
    }
}

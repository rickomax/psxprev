using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

            int[][] palettes = null;
            bool[][] semiTransparentPalettes = null;
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
                    pmode = GetModeFromClut(clutWidth);
                }
                else if (pmode == 3)
                {
                    pmode = 1; // 8bpp (256clut)
                }

                palettes = ReadPalettes(reader, pmode, clutWidth, clutHeight, out semiTransparentPalettes, false);
            }

            if (pmode < 2 && palettes == null)
            {
                return null; // No palette for clut format (check now to speed up TIM scanning)
            }

            var imgBnum = reader.ReadUInt32(); // Size of image data starting at this field
            var imgDx = reader.ReadUInt16();
            var imgDy = reader.ReadUInt16();
            var imgStride = reader.ReadUInt16(); // Stride in units of 2 bytes
            var imgHeight = reader.ReadUInt16();

            return ReadTexture(reader, imgStride, imgHeight, imgDx, imgDy, pmode, 0, palettes, semiTransparentPalettes, false);
        }

        public static int[] ReadPalette(BinaryReader reader, uint pmode, uint clutWidth, out bool[] semiTransparentPalette, bool allowOutOfBounds)
        {
            semiTransparentPalette = null;

            if (clutWidth == 0 || clutWidth > 256)
            {
                return null;
            }
            if (pmode >= 2)
            {
                return null; // Not a clut format
            }

            // HMD: Support models with invalid image data, but valid model data.
            var clutDataSize = (clutWidth * 2);
            if (allowOutOfBounds && clutDataSize + reader.BaseStream.Position > reader.BaseStream.Length)
            {
                return null;
            }

            // We should probably allocate the full 16clut or 256clut in-case an image pixel has bad data.
            var count = pmode == 0 ? 16 : 256; // clutWidth;
            int[] palette = null;

            for (var c = 0; c < count; c++)
            {
                int argb;
                if (c >= clutWidth)
                {
                    // Use default masking black as fallback color.
                    // No need to assign empty color
                    //argb = (0 << 24) | (0 << 16) | (0 << 8) | 0;
                }
                else
                {
                    var data = reader.ReadUInt16();
                    var r = ((data      ) & 0x1f) << 3;
                    var g = ((data >>  5) & 0x1f) << 3;
                    var b = ((data >> 10) & 0x1f) << 3;
                    var stp = ((data >> 15) & 0x1) == 1; // Semi-transparency: 0-Off, 1-On
                    var a = 255;

                    // Note: stpMode (not stp) is defined on a per polygon basis. We can't apply alpha now, only during rendering.
                    if (stp)
                    {
                        if (semiTransparentPalette == null)
                        {
                            semiTransparentPalette = new bool[count];
                        }
                        semiTransparentPalette[c] = true;
                    }
                    else if (r == 0 && g == 0 && b == 0)
                    {
                        a = 0; // Transparent when black and !stp
                    }

                    if (data != 0)
                    {
                        argb = (a << 24) | (r << 16) | (g << 8) | b;

                        if (palette == null)
                        {
                            palette = new int[count];
                        }
                        palette[c] = argb;
                    }
                }
            }

            if (palette == null)
            {
                palette = pmode == 0 ? Texture.EmptyPalette16 : Texture.EmptyPalette256;
            }

            return palette;
        }

        public static int[][] ReadPalettes(BinaryReader reader, uint pmode, uint clutWidth, uint clutHeight, out bool[][] semiTransparentPalettes, bool allowOutOfBounds, bool firstOnly = false)
        {
            semiTransparentPalettes = null;

            if (clutWidth == 0 || clutHeight == 0 || clutWidth > 256 || clutHeight > 256)
            {
                return null;
            }
            if (pmode >= 2)
            {
                return null; // Not a clut format
            }

            // HMD: Support models with invalid image data, but valid model data.
            var clutDataSize = (clutHeight * clutWidth * 2);
            if (allowOutOfBounds && clutDataSize + reader.BaseStream.Position > reader.BaseStream.Length)
            {
                return null;
            }

            var count = firstOnly ? 1 : clutHeight;
            var palettes = new int[count][];
            semiTransparentPalettes = null;

            for (var i = 0; i < clutHeight; i++)
            {
                if (i < count)
                {
                    palettes[i] = ReadPalette(reader, pmode, clutWidth, out var semiTransparentPalette, allowOutOfBounds);
                    if (semiTransparentPalette != null)
                    {
                        if (semiTransparentPalettes == null)
                        {
                            semiTransparentPalettes = new bool[count][];
                        }
                        semiTransparentPalettes[i] = semiTransparentPalette;
                    }
                }
                else
                {
                    // Skip past this clut
                    reader.BaseStream.Seek(clutWidth * 2, SeekOrigin.Current);
                }
            }
            if (semiTransparentPalettes != null)
            {
                for (var i = 0; i < count; i++)
                {
                    if (semiTransparentPalettes[i] == null)
                    {
                        semiTransparentPalettes[i] = pmode == 0 ? Texture.EmptySemiTransparentPalette16 : Texture.EmptySemiTransparentPalette256;
                    }
                }
            }

            return palettes;
        }

        public static Texture ReadTexture(BinaryReader reader, ushort stride, ushort height, ushort dx, ushort dy, uint pmode, int clutIndex, int[][] palettes, bool[][] semiTransparentPalettes, bool allowOutOfBounds)
        {
            if ((pmode == 0 || pmode == 1) && palettes == null)
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

            var texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, texturePage, clutIndex, palettes, semiTransparentPalettes);

            BitmapData bmpData = null;
            BitmapData stpData = null;
            try
            {
                var bitmap = texture.Bitmap;
                if (pmode <= 2 || semiTransparentPalettes != null)
                {
                    texture.SetupSemiTransparentMap();
                }

                var rect = new Rectangle(0, 0, textureWidth, textureHeight);
                var pixelFormat = texture.Bitmap.PixelFormat; //Texture.GetPixelFormat(textureBpp);
                bmpData = texture.Bitmap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
                if (pmode <= 2 || semiTransparentPalettes != null)
                {
                    stpData = texture.SemiTransparentMap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
                }
                
                switch (pmode)
                {
                    case 0: // 4bpp (16clut)
                        Read4BppTexture(reader, texture, stride, bmpData, stpData);
                        break;

                    case 1: // 8bpp (256clut)
                        Read8BppTexture(reader, texture, stride, bmpData, stpData);
                        break;

                    case 2: // 16bpp (5/5/5)
                        Read16BppTexture(reader, texture, stride, bmpData, stpData);
                        break;

                    case 3: // 24bpp
                        Read24BppTexture(reader, texture, stride, bmpData);
                        break;
                }

                if (bmpData != null)
                {
                    texture.Bitmap.UnlockBits(bmpData);
                }
                if (stpData != null)
                {
                    texture.SemiTransparentMap.UnlockBits(stpData);
                }
            }
            catch
            {
                // We can't put this in a finally block, since we don't want to Dispose of texture first
                if (bmpData != null)
                {
                    texture.Bitmap.UnlockBits(bmpData);
                }
                if (stpData != null)
                {
                    texture.SemiTransparentMap.UnlockBits(stpData);
                }
                texture.Dispose(); // Cleanup on failure to parse
                throw;
            }

            return texture;
        }

        // Gets pixel data and extra padding if data is non-null, and performs a lot of safety checks since we're using unsafe.
        // writeStride should be expected stride for writing (without any padding).
        private static IntPtr GetPixelData(Texture texture, BitmapData data, int writeStride, PixelFormat expectedFormat, out int padding, bool nonNull)
        {
            if (data != null)
            {
                padding = data.Stride - writeStride;

                var width  = texture.Width;
                var height = texture.Height;

                // Don't use Debug.Assert, since that won't execute in release builds.
                // Ensure we have the correct format
                Trace.Assert(data.PixelFormat == expectedFormat, "Unexpected pixel data format in unsafe context");
                // Ensure the dimensions are the same
                Trace.Assert(data.Width == width && data.Height == height, "Unexpected pixel data dimensions in unsafe context");
                // Ensure stride isn't smaller than our expected write stride
                Trace.Assert(padding >= 0, "Unexpected pixel data stride in unsafe context");
                // Ensure there's enough data to write to without going out of bounds
                Trace.Assert(data.Height * data.Stride >= height * (writeStride + padding), "Unexpected pixel data size in unsafe context");
                // Ensure our pointer is non-null
                Trace.Assert(data.Scan0 != IntPtr.Zero, "Unexpected pixel data null pointer in unsafe context");

                return data.Scan0;
            }
            else if (nonNull)
            {
                throw new ArgumentNullException("bmpData");
            }
            padding = 0;
            return IntPtr.Zero;
        }

        private static unsafe void Read4BppTexture(BinaryReader reader, Texture texture, ushort stride, BitmapData bmpData, BitmapData stpData)
        {
            var width  = texture.Width;
            var height = texture.Height;

            // This expects 4bpp Textures to use 4bpp indexed format
            var writeStride = width / 2;
            var expectedFormat = PixelFormat.Format4bppIndexed;
            var p = (byte*)GetPixelData(texture, bmpData, writeStride, expectedFormat, out var bmpPadding, true);
            var s = (byte*)GetPixelData(texture, stpData, writeStride, expectedFormat, out var stpPadding, false);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width / 2; x++)
                {
                    // Swap order of 4-bit indices in bytes
                    var data = reader.ReadByte();
                    var index1 = (data >> 4) & 0xf;
                    var index2 = (data     ) & 0xf;
                    data = (byte)((index2 << 4) | index1);

                    *p++ = data;
                    if (s != null)
                    {
                        *s++ = data;
                    }
                }

                p += bmpPadding;
                if (s != null)
                {
                    s += stpPadding;
                }
            }
        }

        private static unsafe void Read8BppTexture(BinaryReader reader, Texture texture, ushort stride, BitmapData bmpData, BitmapData stpData)
        {
            var width  = texture.Width;
            var height = texture.Height;

            // This expects 8bpp Textures to use 8bpp indexed format
            var writeStride = width;
            var expectedFormat = PixelFormat.Format8bppIndexed;
            var p = (byte*)GetPixelData(texture, bmpData, writeStride, expectedFormat, out var bmpPadding, true);
            var s = (byte*)GetPixelData(texture, stpData, writeStride, expectedFormat, out var stpPadding, false);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var data = reader.ReadByte();
                    *p++ = data;
                    if (s != null)
                    {
                        *s++ = data;
                    }
                }

                p += bmpPadding;
                if (s != null)
                {
                    s += stpPadding;
                }
            }
        }

        private static unsafe void Read16BppTexture(BinaryReader reader, Texture texture, ushort stride, BitmapData bmpData, BitmapData stpData)
        {
            var width  = texture.Width;
            var height = texture.Height;

            // This expects 16bpp Textures to use 32bpp format
            var writeStride = width * 4;
            var expectedFormat = PixelFormat.Format32bppArgb;
            var p = (int*)GetPixelData(texture, bmpData, writeStride, expectedFormat, out var bmpPadding, true);
            var s = (int*)GetPixelData(texture, stpData, writeStride, expectedFormat, out var stpPadding, false);

            var noStpArgb = Texture.NoSemiTransparentFlag.ToArgb();
            var stpArgb   = Texture.SemiTransparentFlag.ToArgb();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var data = reader.ReadUInt16();
                    var r = ((data      ) & 0x1f) << 3;
                    var g = ((data >>  5) & 0x1f) << 3;
                    var b = ((data >> 10) & 0x1f) << 3;
                    var stp = ((data >> 15) & 0x1) == 1; // Semi-transparency: 0-Off, 1-On
                    var a = 255;

                    // Note: stpMode (not stp) is defined on a per polygon basis. We can't apply alpha now, only during rendering.
                    if (!stp && r == 0 && g == 0 && b == 0)
                    {
                        a = 0; // Transparent when black and !stp
                    }

                    var argb = (a << 24) | (r << 16) | (g << 8) | b;
                    *p++ = argb;
                    if (s != null)
                    {
                        argb = stp ? stpArgb : noStpArgb;
                        *s++ = argb;
                    }
                }

                p = (int*)(((byte*)p) + bmpPadding);
                if (s != null)
                {
                    s = (int*)(((byte*)s) + stpPadding);
                }
            }
        }

        private static unsafe void Read24BppTexture(BinaryReader reader, Texture texture, ushort stride, BitmapData bmpData)
        {
            var width  = texture.Width;
            var height = texture.Height;
            var readPadding = (stride * 2) - (width * 3);

            // This expects 24bpp Textures to use 32bpp format
            var writeStride = width * 4;
            var expectedFormat = PixelFormat.Format32bppArgb;
            var p = (int*)GetPixelData(texture, bmpData, writeStride, expectedFormat, out var bmpPadding, true);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var r = reader.ReadByte();
                    var g = reader.ReadByte();
                    var b = reader.ReadByte();
                    var a = 255;

                    var argb = (a << 24) | (r << 16) | (g << 8) | b;
                    *p++ = argb;
                }

                p = (int*)(((byte*)p) + bmpPadding);

                // todo: Is there padding at the end of rows?
                //       It's probably padding to 2-bytes if there is any, rather than 4-bytes.
                for (var pad = 0; pad < readPadding; pad++)
                {
                    reader.ReadByte();
                }
            }
        }

        public static uint GetModeFromClut(ushort clutWidth)
        {
            // NOTE: Width always seems to be 16 or 256.
            //       Specifically width was 16 or 256 and height was 1.
            //       With that, it's safe to assume the dimensions tell us the color count.
            //       Because this data could potentionally give us something other than 16 or 256,
            //       assume anything greater than 16 will allocate a 256clut and only read w colors.

            // Note that height is different, and is used to count the number of cluts.

            // todo: Which is correct?
            //return (clutWidth <= 16 ? 0u : 1u);
            return (clutWidth < 256 ? 0u : 1u);
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

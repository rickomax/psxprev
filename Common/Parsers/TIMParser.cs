using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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
            if (!ReadTIM(reader))
            {
                //foreach (var texture in TextureResults)
                //{
                //    texture.Dispose();
                //}
                //TextureResults.Clear();
            }
        }

        private bool ReadTIM(BinaryReader reader)
        {
            var header = reader.ReadUInt32();
            var id       = (header      ) & 0xff;
            var version  = (header >>  8) & 0xff;
            var reserved = (header >> 16);
            // How we originally ignored version:
            if (id != 0x10 || version != 0x00 || (!Limits.IgnoreTIMVersion && reserved != 0))
            //if (id != 0x10 || (!Limits.IgnoreTIMVersion && (version != 0x00 || reserved != 0)))
            {
                return false;
            }

            var flag = reader.ReadUInt32();
            var hasClut = (flag & 0x8) != 0;
            var pmode   = (flag & 0x7);
            if (pmode == 4 || pmode > 4)
            {
                // As far as I can tell, mixed format (pmode 4) doesn't actually exist.
                // There's no tools or library functions that support it.
                return false; // Mixed format not supported (check now to speed up TIM scanning), or invalid pmode
            }
            // Reduce false positives, since the hibits of flag should be all zeroes.
            //if (!Limits.IgnoreTIMVersion && (flag & ~0xfu) != 0)
            //{
            //    return false;
            //}

            var bpp = GetBppFromMode(pmode);
            ushort[][] palettes = null;
            bool? hasSemiTransparency = null;
            if (hasClut)
            {
                var clutPosition = reader.BaseStream.Position;
                var clutSize = reader.ReadUInt32(); // Size of clut data starting at this field
                var clutDx = reader.ReadUInt16(); // Frame buffer coordinates
                var clutDy = reader.ReadUInt16();
                var clutWidth  = reader.ReadUInt16();
                var clutHeight = reader.ReadUInt16();
                if (clutSize < 12 + clutHeight * clutWidth * 2)
                {
                    return false;
                }

                // Noted in jpsxdec/CreateTim that some files can claim an unpaletted pmode but still use a palette.
                bpp = InferBppFromClut(pmode, clutWidth);

                palettes = ReadPalettes(reader, bpp, clutWidth, clutHeight, out hasSemiTransparency, false);
                reader.BaseStream.Seek(clutPosition + clutSize, SeekOrigin.Begin);
            }

            if (bpp <= 8 && palettes == null)
            {
                return false; // No palette for clut format (check now to speed up TIM scanning)
            }

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

            var texture = ReadTexture(reader, bpp, stride, height, dx, dy, 0, palettes, hasSemiTransparency, false);
            reader.BaseStream.Seek(imagePosition + imageSize, SeekOrigin.Begin);
            if (texture != null)
            {
                TextureResults.Add(texture);
                return true;
            }

            return false;
        }

        private bool ReadTextureData(BinaryReader reader, uint pmode, bool hasClut)
        {
            int bpp;
            ushort[][] palettes = null;
            bool? hasSemiTransparency = null;
            if (hasClut)
            {
                var clutPosition = reader.BaseStream.Position;
                var clutSize = reader.ReadUInt32(); // Size of clut data starting at this field
                var clutDx = reader.ReadUInt16(); // Frame buffer coordinates
                var clutDy = reader.ReadUInt16();
                var clutWidth  = reader.ReadUInt16();
                var clutHeight = reader.ReadUInt16();
                if (clutSize < 12 + clutHeight * clutWidth * 2)
                {
                    return false;
                }

                // Noted in jpsxdec/CreateTim that some files can claim an unpaletted pmode but still use a palette.
                bpp = InferBppFromClut(pmode, clutWidth);

                palettes = ReadPalettes(reader, bpp, clutWidth, clutHeight, out hasSemiTransparency, false);
                reader.BaseStream.Seek(clutPosition + clutSize, SeekOrigin.Begin);
            }
            else
            {
                bpp = GetBppFromMode(pmode);
            }

            if (bpp <= 8 && palettes == null)
            {
                return false; // No palette for clut format (check now to speed up TIM scanning)
            }

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

            var texture = ReadTexture(reader, bpp, stride, height, dx, dy, 0, palettes, hasSemiTransparency, false);
            reader.BaseStream.Seek(imagePosition + imageSize, SeekOrigin.Begin);
            if (texture != null)
            {
                TextureResults.Add(texture);
                return true;
            }

            return false;
        }

        public static ushort[] ReadPalette(BinaryReader reader, int bpp, uint clutWidth, out bool hasSemiTransparency, bool allowOutOfBounds)
        {
            hasSemiTransparency = false;

            if (clutWidth == 0 || clutWidth > 256)
            {
                return null;
            }
            if (bpp > 8)
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
            var paletteSize = bpp == 4 ? 16 : 256; // clutWidth;
            var palette = new ushort[paletteSize];

            for (var c = 0; c < paletteSize; c++)
            {
                if (c >= clutWidth)
                {
                    // Use default masking black as fallback color.
                    // No need to assign empty color
                    //palette[c] = 0;
                }
                else
                {
                    var color = reader.ReadUInt16();
                    var stp = ((color >> 15) & 0x1) == 1; // Semi-transparency: 0-Off, 1-On

                    // Note: stpMode (not stp) is defined on a per polygon basis. We can't apply alpha now, only during rendering.
                    hasSemiTransparency |= stp;

                    if (color != 0)
                    {
                        palette[c] = color;
                    }
                }
            }

            return palette;
        }

        public static ushort[][] ReadPalettes(BinaryReader reader, int bpp, ushort clutWidth, ushort clutHeight, out bool? hasSemiTransparency, bool allowOutOfBounds, bool firstOnly = false)
        {
            hasSemiTransparency = false;

            if (clutWidth == 0 || clutHeight == 0 || clutWidth > 256 || clutHeight > 256)
            {
                return null;
            }
            if (bpp > 8)
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
            var palettes = new ushort[count][];

            for (var i = 0; i < clutHeight; i++)
            {
                if (i < count)
                {
                    palettes[i] = ReadPalette(reader, bpp, clutWidth, out var stp, allowOutOfBounds);
                    hasSemiTransparency |= stp;
                }
                else
                {
                    // Skip past this clut
                    reader.BaseStream.Seek(clutWidth * 2, SeekOrigin.Current);
                }
            }

            return palettes;
        }

        public static Texture ReadTexture(BinaryReader reader, int bpp, ushort stride, ushort height, ushort dx, ushort dy, int clutIndex, ushort[][] palettes, bool? hasSemiTransparency, bool allowOutOfBounds, Func<ushort, ushort> maskPixel16 = null)
        {
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

            var page = (texturePageY * 16) + texturePageX;
            var x = (dx - textureOffsetX) * 16 / bpp;// Math.Min(16, bpp); // todo: Or is this the same as textureWidth?
            var y = (dy - textureOffsetY);
            var width = stride * 16 / bpp;

            return ReadTextureInternal(reader, bpp, stride, width, height, x, y, page, clutIndex,
                palettes, hasSemiTransparency, allowOutOfBounds, maskPixel16);
        }

        public static Texture ReadTexturePacked(BinaryReader reader, int bpp, int width, int height, int clutIndex, ushort[][] palettes, bool? hasSemiTransparency, bool allowOutOfBounds, Func<ushort, ushort> maskPixel16 = null)
        {
            var stride = GetStride(bpp, (uint)width);
            return ReadTextureInternal(reader, bpp, stride, width, height, 0, 0, 0, clutIndex, palettes, hasSemiTransparency, allowOutOfBounds, maskPixel16);
        }

        private static Texture ReadTextureInternal(BinaryReader reader, int bpp, ushort stride, int width, int height, int x, int y, int page, int clutIndex, ushort[][] palettes, bool? hasSemiTransparency, bool allowOutOfBounds, Func<ushort, ushort> maskPixel16 = null)
        {
            if (bpp <= 8 && palettes == null)
            {
                return null; // No palette for clut format
            }

            if (stride == 0 || width == 0 || height == 0 || width > (int)Limits.MaxTIMResolution || height > (int)Limits.MaxTIMResolution)
            {
                return null;
            }

            // HMD: Support models with invalid image data, but valid model data.
            var textureDataSize = (height * stride * 2);
            if (allowOutOfBounds && textureDataSize + reader.BaseStream.Position > reader.BaseStream.Length)
            {
                return null;
            }

            var texture = new Texture(width, height, x, y, bpp, page, clutIndex, palettes, hasSemiTransparency);

            BitmapData bmpData = null;
            BitmapData stpData = null;
            try
            {
                var bitmap = texture.Bitmap;
                if (bpp <= 16 || (hasSemiTransparency ?? true))
                {
                    texture.SetupSemiTransparentMap();
                }

                var rect = new Rectangle(0, 0, width, height);
                var pixelFormat = texture.Bitmap.PixelFormat; //Texture.GetPixelFormat(textureBpp);
                bmpData = texture.Bitmap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
                if (texture.SemiTransparentMap != null)
                {
                    stpData = texture.SemiTransparentMap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
                }

                switch (bpp)
                {
                    case  4: // 4bpp (16clut)
                        Read4BppTexture(reader, texture, stride, bmpData, stpData);
                        break;

                    case  8: // 8bpp (256clut)
                        Read8BppTexture(reader, texture, stride, bmpData, stpData);
                        break;

                    case 16: // 16bpp (5/5/5/stp)
                        Read16BppTexture(reader, texture, stride, bmpData, stpData, maskPixel16);
                        break;

                    case 24: // 24bpp
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
            var readPadding = (stride * 2) - ((width + 1) / 2);

            // This expects 4bpp Textures to use 4bpp indexed format
            var writeStride = (width + 1) / 2;
            var expectedFormat = PixelFormat.Format4bppIndexed;
            var p = (byte*)GetPixelData(texture, bmpData, writeStride, expectedFormat, out var bmpPadding, true);
            var s = (byte*)GetPixelData(texture, stpData, writeStride, expectedFormat, out var stpPadding, false);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < (width + 1) / 2; x++)
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

                for (var pad = 0; pad < readPadding; pad++)
                {
                    reader.ReadByte();
                }
            }
        }

        private static unsafe void Read8BppTexture(BinaryReader reader, Texture texture, ushort stride, BitmapData bmpData, BitmapData stpData)
        {
            var width  = texture.Width;
            var height = texture.Height;
            var readPadding = (stride * 2) - width;

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

                for (var pad = 0; pad < readPadding; pad++)
                {
                    reader.ReadByte();
                }
            }
        }

        private static unsafe void Read16BppTexture(BinaryReader reader, Texture texture, ushort stride, BitmapData bmpData, BitmapData stpData, Func<ushort, ushort> maskPixel)
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
                    var color = reader.ReadUInt16();
                    if (maskPixel != null)
                    {
                        color = maskPixel(color);
                    }
                    var r = ((color      ) & 0x1f) << 3;
                    var g = ((color >>  5) & 0x1f) << 3;
                    var b = ((color >> 10) & 0x1f) << 3;
                    var stp = ((color >> 15) & 0x1) == 1; // Semi-transparency: 0-Off, 1-On
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

        public static int GetBppFromMode(uint pmode)
        {
            switch (pmode)
            {
                case 0: return  4; // 4bpp (16clut)
                case 1: return  8; // 8bpp (256clut)
                case 2: return 16; // 16bpp (5/5/5/stp)
                case 3: return 24; // 24bpp
            }
            return -1;
        }

        public static int GetBppFromClut(ushort clutWidth)
        {
            // NOTE: Width always seems to be 16 or 256.
            //       Specifically width was 16 or 256 and height was 1.
            //       With that, it's safe to assume the dimensions tell us the color count.
            //       Because this data could potentionally give us something other than 16 or 256,
            //       assume anything greater than 16 will allocate a 256clut and only read w colors.

            // Note that height is different, and is used to count the number of cluts.

            // todo: Which is correct?
            //return (clutWidth <= 16 ? 4 : 8);
            return (clutWidth < 256 ? 4 : 8);
        }

        public static int InferBppFromClut(uint pmode, ushort clutWidth)
        {
            // Noted in jpsxdec/CreateTim that some files can claim an unpaletted pmode but still use a palette.
            switch (pmode)
            {
                default: return GetBppFromMode(pmode);
                case 2: return GetBppFromClut(clutWidth);
                case 3: return 8; // 8bpp (256clut)
            }
        }

        public static int GetBppFromNoClut()
        {
            return 16;
        }

        public static ushort GetClutWidth(int bpp)
        {
            switch (bpp)
            {
                case 4: return 16;
                case 8: return 256;
            }
            return 0;
        }

        public static ushort GetStride(int bpp, uint width)
        {
            switch (bpp)
            {
                case  4: return (ushort)((width + 3) / 4);
                case  8: return (ushort)((width + 1) / 2);
                case 16: return (ushort)width;
                case 24: return (ushort)((width * 3 + 1) / 2);
            }
            return 0;
        }
    }
}

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PSXPrev.Common.Utils;

namespace PSXPrev.Forms.Utils
{
    public static class ClipboardUtils
    {
        public static void SetImageWithTransparency(Image image, Color? opaqueBackground = null, string tempFilePath = null)
        {
#if ENABLE_CLIPBOARD
            using (var opaqueImage = image.CreateOpaqueImage(opaqueBackground ?? Color.White))
            {
                SetImageWithTransparency(image, opaqueImage, tempFilePath);
            }
#endif
        }

        public static void SetImageWithTransparency(Image image, Image opaqueImage, string tempFilePath = null)
        {
#if ENABLE_CLIPBOARD
            Clipboard.Clear();

            using (var pngStream = new MemoryStream())
            using (var dibStream = new MemoryStream())
            using (var dibV5Stream = new MemoryStream())
            {
                var data = new DataObject();

                // Standard bitmap: Without transparency support
                data.SetData(DataFormats.Bitmap, true, opaqueImage);

                // Png: Gimp will prefer this over Dib and Bitmap
                image.Save(pngStream, ImageFormat.Png);
                data.SetData(DataFormatsEx.Png, false, pngStream);

                // Dib(V5): This is (incorrectly) accepted as ARGB by many applications, like Paint.NET
                var dibImageData = GetDibImageData(image);
                WriteDib(dibStream, image, dibImageData);
                data.SetData(DataFormats.Dib, false, dibStream);

                WriteDibV5(dibV5Stream, image, dibImageData);
                data.SetData(DataFormatsEx.DibV5, false, dibV5Stream);

                // FileDrop: For additional support
                if (tempFilePath != null)
                {
                    image.Save(tempFilePath, ImageFormat.Png);
                    data.SetFileDropList(new StringCollection { tempFilePath });
                }

                // Pass copy: true so that we can safely dispose of the MemoryStreams after the operation.
                Clipboard.SetDataObject(data, copy: true);
            }
#endif
        }

#if ENABLE_CLIPBOARD
        private static class DataFormatsEx
        {
            public const string DibV5 = "Format17";
            public const string Png = "PNG";
        }

        private static void WriteDib(Stream stream, Image image, byte[] imageData)
        {
            var bmi = new BITMAPINFOHEADER
            {
                biSize = Marshal.SizeOf<BITMAPINFOHEADER>(),
                biWidth = image.Width,
                biHeight = image.Height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BI_RGB,
                biSizeImage = imageData.Length,
            };

            WriteStruct(stream, bmi);
            stream.Write(imageData, 0, imageData.Length);
        }

        private static void WriteDibV5(Stream stream, Image image, byte[] imageData)
        {
            var bmV5 = new BITMAPV5HEADER
            {
                bV5Size = Marshal.SizeOf<BITMAPV5HEADER>(),
                bV5Width = image.Width,
                bV5Height = image.Height,
                bV5Planes = 1,
                bV5BitCount = 32,
                bV5Compression = BI_RGB,
                bV5SizeImage = imageData.Length,
                bV5RedMask   = 0x00ff0000,
                bV5GreenMask = 0x0000ff00,
                bV5BlueMask  = 0x000000ff,
                bV5AlphaMask = 0xff000000,
                bV5CSType = LCS_sRGB,
                bV5Intent = 4,
            };

            WriteStruct(stream, bmV5);
            stream.Write(imageData, 0, imageData.Length);
        }

        private static void WriteStruct<T>(Stream stream, T value) where T : struct
        {
            var buffer = new byte[Marshal.SizeOf<T>()];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), true);
                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        private static byte[] GetDibImageData(Image image)
        {
            // PixelFormat.Format32bppPArgb makes things work with Discord,
            // otherwise we get a black background. (This was written in 2019, so things may have changed)
            using (var imagePremult = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb))
            {
                using (var graphics = Graphics.FromImage(imagePremult))
                {
                    graphics.CompositingMode = CompositingMode.SourceOver;

                    graphics.Clear(Color.Transparent);

                    graphics.DrawImage(image, 0, 0);// new Rectangle(0, 0, imagePremult.Width, imagePremult.Height));
                }
                // Bitmap format has its lines reversed
                imagePremult.RotateFlip(RotateFlipType.Rotate180FlipX);

                var rect = new Rectangle(0, 0, imagePremult.Width, imagePremult.Height);
                var bmpData = imagePremult.LockBits(rect, ImageLockMode.ReadOnly, imagePremult.PixelFormat);
                try
                {
                    var imageData = new byte[bmpData.Height * bmpData.Stride];
                    Marshal.Copy(bmpData.Scan0, imageData, 0, imageData.Length);
                    return imageData;
                }
                finally
                {
                    imagePremult.UnlockBits(bmpData);
                }
            }

        }

        private const uint LCS_sRGB = 0x73524742;
        private const uint BI_RGB = 0;

        [StructLayout(LayoutKind.Sequential)]
        internal struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPV5HEADER
        {
            public int bV5Size;
            public int bV5Width;
            public int bV5Height;
            public ushort bV5Planes;
            public ushort bV5BitCount;
            public uint bV5Compression;
            public int bV5SizeImage;
            public int bV5XPelsPerMeter;
            public int bV5YPelsPerMeter;
            public uint bV5ClrUsed;
            public uint bV5ClrImportant;
            public uint bV5RedMask;
            public uint bV5GreenMask;
            public uint bV5BlueMask;
            public uint bV5AlphaMask;
            public uint bV5CSType;
            public CIEXYZTRIPLE bV5Endpoints;
            public uint bV5GammaRed;
            public uint bV5GammaGreen;
            public uint bV5GammaBlue;
            public uint bV5Intent;
            public uint bV5ProfileData;
            public uint bV5ProfileSize;
            public uint bV5Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CIEXYZTRIPLE
        {
            public CIEXYZ ciexyzRed;
            public CIEXYZ ciexyzGreen;
            public CIEXYZ ciexyzBlue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CIEXYZ
        {
            public uint ciexyzX; //FXPT2DOT30
            public uint ciexyzY; //FXPT2DOT30
            public uint ciexyzZ; //FXPT2DOT30
        }
#endif
    }
}
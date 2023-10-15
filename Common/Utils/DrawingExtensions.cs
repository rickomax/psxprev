using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PSXPrev.Common.Utils
{
    public static class DrawingExtensions
    {
        // Needed to clone a bitmap while preserving its pixel format.
        public static Bitmap DeepClone(this Bitmap bitmap)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, bitmap);
                stream.Seek(0, SeekOrigin.Begin);
                return (Bitmap)formatter.Deserialize(stream);
            }
        }

        public static Bitmap CreateNewFormat(this Image image, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            var newBitmap = new Bitmap(image.Width, image.Height, pixelFormat);
            try
            {
                using (var graphics = Graphics.FromImage(newBitmap))
                {
                    // Use SourceCopy to overwrite image alpha with alpha stored in textures.
                    graphics.CompositingMode = CompositingMode.SourceCopy;

                    graphics.DrawImage(image, 0, 0);
                }
                return newBitmap;
            }
            catch
            {
                newBitmap?.Dispose();
                throw;
            }
        }

        public static Bitmap CreateOpaqueImage(this Image image, System.Drawing.Color background, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            var newBitmap = new Bitmap(image.Width, image.Height, pixelFormat);
            try
            {
                using (var graphics = Graphics.FromImage(newBitmap))
                {
                    // Overlay image on-top of background color
                    graphics.CompositingMode = CompositingMode.SourceOver;

                    graphics.Clear(background);

                    graphics.DrawImage(image, 0, 0);
                }
                return newBitmap;
            }
            catch
            {
                newBitmap.Dispose();
                throw;
            }
        }

        public static Bitmap CreateCroppedImage(this Image image, Rectangle srcRect, System.Drawing.Color? background = null, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            var newBitmap = new Bitmap(srcRect.Width, srcRect.Height, pixelFormat);
            try
            {
                using (var graphics = Graphics.FromImage(newBitmap))
                {
                    // Use SourceCopy to overwrite image alpha with alpha stored in textures.
                    graphics.CompositingMode = CompositingMode.SourceCopy;

                    if (background.HasValue)
                    {
                        graphics.Clear(background.Value);
                    }
                    
                    graphics.DrawImage(image, 0, 0, srcRect, GraphicsUnit.Pixel);
                }
                return newBitmap;
            }
            catch
            {
                newBitmap?.Dispose();
                throw;
            }
        }
    }
}

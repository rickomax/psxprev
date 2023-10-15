using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace PSXPrev.Common.Renderer
{
    public class TextureBinder : IDisposable
    {
        // Constants for sharing multiple texture pages in a single texture
        private const bool SharePages = true;
        public const int PageColumns = SharePages ? 4 : 1;
        public const int PageRows    = SharePages ? 8 : 1;
        public const int PagesPerTexture = PageColumns * PageRows;
        public const int SemiTransparencyX = PageColumns * VRAM.PageSize;


        private int[] _textureIds;
        private bool[] _texturesInitialized;
        private readonly Bitmap _defaultBitmap; // Used when creating a texture for the first time

        public bool IsDisposed { get; private set; }

        public TextureBinder()
        {
            var width  = PageColumns * VRAM.PageSize;
            var height = PageRows    * VRAM.PageSize;
            _defaultBitmap = new Bitmap(width * 2, height);
            using (var graphics = Graphics.FromImage(_defaultBitmap))
            {
                // Use SourceCopy to overwrite image alpha with alpha stored in NoSemiTransparentFlag.
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.SmoothingMode = SmoothingMode.None;

                // Clear texture data to background color.
                graphics.Clear(VRAM.BackgroundColor);

                // Clear semi-transparent information to its default.
                graphics.FillRectangle(Texture.NoSemiTransparentBrush, SemiTransparencyX, 0, width, height);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (_textureIds != null)
                {
                    GL.DeleteTextures(_textureIds.Length, _textureIds);
                    _textureIds = null;
                    _texturesInitialized = null;
                }
                _defaultBitmap.Dispose();
            }
        }

        public int GetTextureID(int index) => GetTextureID((uint)index);

        public int GetTextureID(uint index)
        {
            var textureIndex = index / PagesPerTexture;
            if (_textureIds == null || _textureIds.Length < textureIndex + 1)
            {
                var oldCount = _textureIds?.Length ?? 0;
                var newCount = textureIndex + 1;
                Array.Resize(ref _textureIds,          (int)newCount);
                Array.Resize(ref _texturesInitialized, (int)newCount);
                for (var i = oldCount; i < newCount; i++)
                {
                    _textureIds[i] = GL.GenTexture();
                }
            }
            return _textureIds[textureIndex];
        }

        public void UpdateTexture(Bitmap bitmap, int index) => UpdateTexture(bitmap, (uint)index);

        public void UpdateTexture(Bitmap bitmap, uint index)
        {
            var textureId = GetTextureID(index);
            if (textureId == 0)
            {
                return;
            }

            GL.ActiveTexture(TextureUnit.Texture0 + Shader.TextureUnit_MainTexture);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // If this is the first update to this textureId, then we need to create it before we can call TexSubImage2D.
            var textureIndex = index / PagesPerTexture;
            if (!_texturesInitialized[textureIndex])
            {
                // We could just pass in IntPtr.Zero for the data,
                // but this is only a good idea if we KNOW that all pages will be written to.
                // And TextureBinder has been redesigned to support any number of additional textures,
                // which may not write all pages, unlike what VRAM does for the first 32 pages.
                var defRect = new Rectangle(0, 0, _defaultBitmap.Width, _defaultBitmap.Height);
                var defData = _defaultBitmap.LockBits(defRect, ImageLockMode.ReadOnly, GdiPixelFormat.Format32bppArgb);
                try
                {
                    // Make sure to restore PixelStore settings here, since we change them below
                    GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
                    GL.PixelStore(PixelStoreParameter.UnpackRowLength, defData.Width);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, defData.Width, defData.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, defData.Scan0);
                }
                finally
                {
                    _defaultBitmap.UnlockBits(defData);
                }

                // Set parameters so that the texture looks sharp, and has no smoothing/blurring between pixels
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                _texturesInitialized[textureIndex] = true;
            }

            // Copy the VRAM page color and semi-transparency columns into their respective squares in the larger texture image.
            var x = GetColumnX(index);
            var y = GetRowY(index);
            var stpX = SemiTransparencyX + x;
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, GdiPixelFormat.Format32bppArgb);
            try
            {
                // Setting this is necessary so that we skip pixels from the semi-transparency/color
                // sections when writing the opposite section.
                GL.PixelStore(PixelStoreParameter.UnpackRowLength, bitmap.Width);

                // Write color column page and skip semi-transparency column pixels
                GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, VRAM.PageSize, VRAM.PageSize, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

                // Write semi-transparency column page and skip color column pixels
                GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, VRAM.PageSemiTransparencyX);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, stpX, y, VRAM.PageSize, VRAM.PageSize, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        public static bool IsTexturePageShared(int indexA, int indexB) => IsTexturePageShared((uint)indexA, (uint)indexB);

        public static bool IsTexturePageShared(uint indexA, uint indexB)
        {
            return (indexA / PagesPerTexture) == (indexB / PagesPerTexture);
        }

        public static Vector2 ConvertUV(Vector2 uv, bool isTiled, int index) => ConvertUV(uv, isTiled, (uint)index);

        public static Vector2 ConvertUV(Vector2 uv, bool isTiled, uint index)
        {
            if (isTiled)
            {
                return new Vector2(uv.X / (float)PageColumns,
                                   uv.Y / (float)PageRows);
            }
            else
            {
                index %= PagesPerTexture;
                return new Vector2(((index % PageColumns) + uv.X) / (float)PageColumns,
                                   ((index / PageColumns) + uv.Y) / (float)PageRows);
            }
            //index %= PagesPerTexture;
            //return new Vector2(((isTiled ? 0 : index % PageColumns) + uv.X) / (float)PageColumns,
            //                   ((isTiled ? 0 : index / PageColumns) + uv.Y) / (float)PageRows);
        }

        public static Vector4 ConvertTiledArea(Vector4 tiledArea, int index) => ConvertTiledArea(tiledArea, (uint)index);

        public static Vector4 ConvertTiledArea(Vector4 tiledArea, uint index)
        {
            index %= PagesPerTexture;
            return new Vector4(((index % PageColumns) + tiledArea.X) / (float)PageColumns,
                               ((index / PageColumns) + tiledArea.Y) / (float)PageRows,
                               tiledArea.Z / (float)PageColumns,
                               tiledArea.W / (float)PageRows);
        }

        private static int GetColumnX(uint index)
        {
            //index %= TexturePagesPerTexture; // Not necessary because of the other modulus
            return (int)(index % PageColumns) * VRAM.PageSize;
        }

        private static int GetRowY(uint index)
        {
            index %= PagesPerTexture;
            return (int)(index / PageColumns) * VRAM.PageSize;
        }
    }
}

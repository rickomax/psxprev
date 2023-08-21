using System;
using System.ComponentModel;
using System.Drawing;
using PSXPrev.Common.Renderer;

namespace PSXPrev.Common
{
    public class Texture : IDisposable
    {
        public static readonly System.Drawing.Color NoSemiTransparentFlag = System.Drawing.Color.FromArgb(255, 0, 0, 0);
        public static readonly System.Drawing.Color SemiTransparentFlag = System.Drawing.Color.FromArgb(255, 255, 255, 255);

        private readonly WeakReference<RootEntity> _ownerEntity = new WeakReference<RootEntity>(null);

        public Texture(Bitmap bitmap, int x, int y, int bpp, int texturePage, bool isVRAMPage = false)
        {
            Bitmap = bitmap;
            X = x;
            Y = y;
            Bpp = bpp;
            TexturePage = texturePage;
            IsVRAMPage = isVRAMPage;
        }

        public Texture(int width, int height, int x, int y, int bpp, int texturePage, bool isVRAMPage = false)
            : this(new Bitmap(width, height), x, y, bpp, texturePage, isVRAMPage)
        {
        }

        public Texture(Texture fromTexture)
            : this(fromTexture.Width, fromTexture.Height, fromTexture.X, fromTexture.Y, fromTexture.Bpp, fromTexture.TexturePage, fromTexture.IsVRAMPage)
        {
            try
            {
                TextureName = fromTexture.TextureName;

                // Copy main bitmap
                using (var graphics = Graphics.FromImage(Bitmap))
                {
                    // Use SourceCopy to overwrite image alpha with alpha stored in textures.
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                    graphics.DrawImage(fromTexture.Bitmap, 0, 0);
                }

                if (fromTexture.SemiTransparentMap != null)
                {
                    // Copy semi-transparent bitmap
                    SetupSemiTransparentMap();
                    using (var graphics = Graphics.FromImage(SemiTransparentMap))
                    {
                        // Use SourceCopy to overwrite image alpha with alpha stored in textures.
                        graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                        graphics.DrawImage(fromTexture.SemiTransparentMap, 0, 0);
                    }
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        [DisplayName("Name")]
        public string TextureName { get; set; }

        [DisplayName("Format"), ReadOnly(true)]
        public string FormatName { get; set; }

        [DisplayName("X")]
        public int X { get; set; }

        [DisplayName("Y")]
        public int Y { get; set; }

        [DisplayName("VRAM Page")]
        public int TexturePage { get; set; }

        [ReadOnly(true), DisplayName("BPP")]
        public int Bpp { get; set; }

        [ReadOnly(true), DisplayName("Width")]
        public int Width => Bitmap.Width;

        [ReadOnly(true), DisplayName("Height")]
        public int Height => Bitmap.Height;

        // Usable area of the texture (only different from Width/Height when IsVRAMPage is true).
        [Browsable(false)]
        public int RenderWidth => IsVRAMPage ? VRAM.PageSize : Width;

        [Browsable(false)]
        public int RenderHeight => IsVRAMPage ? VRAM.PageSize : Height;

        [Browsable(false)]
        public bool IsVRAMPage { get; set; }

        [Browsable(false)]
        public Bitmap Bitmap { get; set; }

        [Browsable(false)]
        public Bitmap SemiTransparentMap { get; set; }

        // The owner model that created this texture (if any).
        [Browsable(false)]
        public RootEntity OwnerEntity
        {
            get => _ownerEntity.TryGetTarget(out var owner) ? owner : null;
            set => _ownerEntity.SetTarget(value);
        }

        public override string ToString()
        {
            var name = TextureName ?? nameof(Texture);
            return $"{name} {X},{Y} {Width}x{Height} {Bpp}bpp";
        }


        public Bitmap SetupSemiTransparentMap()
        {
            if (SemiTransparentMap == null)
            {
                SemiTransparentMap = new Bitmap(Width, Height);
                using (var graphics = Graphics.FromImage(SemiTransparentMap))
                {
                    graphics.Clear(Texture.NoSemiTransparentFlag);
                }
            }
            return SemiTransparentMap;
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
            SemiTransparentMap?.Dispose();
        }
    }
}
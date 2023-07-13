using System;
using System.ComponentModel;
using System.Drawing;

namespace PSXPrev.Classes
{
    public class Texture : IDisposable
    {
        public static readonly System.Drawing.Color NoSemiTransparentFlag = System.Drawing.Color.FromArgb(255, 0, 0, 0);
        public static readonly System.Drawing.Color SemiTransparentFlag = System.Drawing.Color.FromArgb(255, 255, 255, 255);

        public Texture(int width, int height, int x, int y, int bpp, int texturePage, bool isVRAMPage = false)
        {
            Bitmap = new Bitmap(width, height);
            X = x;
            Y = y;
            Bpp = bpp;
            TexturePage = texturePage;
            IsVRAMPage = isVRAMPage;
        }

        [DisplayName("Name")]
        public string TextureName { get; set; }

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

        [Browsable(false)]
        public bool IsVRAMPage { get; set; }

        [Browsable(false)]
        public Bitmap Bitmap { get; set; }

        [Browsable(false)]
        public Bitmap SemiTransparentMap { get; set; }

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
using System.ComponentModel;
using System.Drawing;

namespace PSXPrev.Classes
{
    public class Texture
    {
        public Texture(int width, int height, int x, int y, int bpp, int texturePage)
        {
            Bitmap = new Bitmap(width, height);
            X = x;
            Y = y;
            Bpp = bpp;
            TexturePage = texturePage;
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
        public Bitmap Bitmap { get; set; }
    }
}
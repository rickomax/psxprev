using System;
using System.ComponentModel;
using System.Drawing;
using PSXPrev.Common.Renderer;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common
{
    public class Texture : IDisposable
    {
        public static readonly System.Drawing.Color NoSemiTransparentFlag = System.Drawing.Color.FromArgb(0, 0, 0, 0);
        public static readonly System.Drawing.Color SemiTransparentFlag = System.Drawing.Color.FromArgb(255, 255, 255, 255);

        public static readonly int[] EmptyPalette16  = new int[16];
        public static readonly int[] EmptyPalette256 = new int[256];

        public static readonly bool[] EmptySemiTransparentPalette16  = new bool[16];
        public static readonly bool[] EmptySemiTransparentPalette256 = new bool[256];


        private readonly WeakReference<RootEntity> _ownerEntity = new WeakReference<RootEntity>(null);

        public Texture(Bitmap bitmap, int x, int y, int bpp, int texturePage, int clutIndex, int[][] palettes, bool[][] semiTransparentPalettes, bool isVRAMPage = false)
        {
            Bitmap = bitmap;
            X = x;
            Y = y;
            Bpp = bpp;
            TexturePage = texturePage;
            CLUTIndex = clutIndex;
            Palettes = palettes;
            SemiTransparentPalettes = semiTransparentPalettes;
            IsVRAMPage = isVRAMPage;
        }

        public Texture(int width, int height, int x, int y, int bpp, int texturePage, int clutIndex, int[][] palettes, bool[][] semiTransparentPalettes, bool isVRAMPage = false)
            : this(new Bitmap(width, height, GetPixelFormat(bpp)), x, y, bpp, texturePage, clutIndex, palettes, semiTransparentPalettes, isVRAMPage)
        {
            if (SemiTransparentPalettes != null)
            {
                var stpPalette = SemiTransparentPalettes[0];
                if (!IsEmptySemiTransparentPalette(stpPalette))
                {
                    SetupSemiTransparentMap();
                }
            }
            SetCLUTIndex(clutIndex, true);
            // Throw away the palettes if we only have one, since we'll never need them again
            if (Palettes != null && Palettes.Length == 1)
            {
                Palettes = null;
            }
            if (SemiTransparentPalettes != null && SemiTransparentPalettes.Length == 1)
            {
                SemiTransparentPalettes = null;
            }
        }

        // preserveFormat has no effect if HasPalette is false, otherwise the copy will be much slower when true.
        public Texture(Texture fromTexture, bool preserveFormat)
        {
            X = fromTexture.X;
            Y = fromTexture.Y;
            TexturePage = fromTexture.TexturePage;
            IsVRAMPage = fromTexture.IsVRAMPage;
            TextureName = fromTexture.TextureName;
            FormatName = fromTexture.FormatName;
            try
            {
                // Preserve format only needs to do something special if we're paletted
                if (preserveFormat && fromTexture.HasPalette)
                {
                    Bpp = fromTexture.Bpp;
                    CLUTIndex = fromTexture.CLUTIndex;
                    Palettes = fromTexture.Palettes;
                    SemiTransparentPalettes = fromTexture.SemiTransparentPalettes;

                    Bitmap = fromTexture.Bitmap.DeepClone();

                    if (fromTexture.SemiTransparentMap != null)
                    {
                        SemiTransparentMap = fromTexture.SemiTransparentMap.DeepClone();
                    }
                }
                else
                {
                    Bpp = fromTexture.HasPalette ? 32 : fromTexture.Bpp;

                    Bitmap = new Bitmap(fromTexture.Bitmap);

                    if (fromTexture.SemiTransparentMap != null)
                    {
                        SemiTransparentMap = new Bitmap(fromTexture.SemiTransparentMap);
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

        [DisplayName("CLUT Index"), ReadOnly(true), Browsable(false)]
        public int CLUTIndex { get; set; }

        [DisplayName("CLUT Count"), ReadOnly(true)]
        public int CLUTCount => (Palettes?.Length ?? (HasPalette ? 1 : 0));

        [DisplayName("BPP"), ReadOnly(true)]
        public int Bpp { get; set; }

        [DisplayName("Width"), ReadOnly(true)]
        public int Width => Bitmap.Width;

        [DisplayName("Height"), ReadOnly(true)]
        public int Height => Bitmap.Height;

        // Usable area of the texture (only different from Width/Height when IsVRAMPage is true).
        [Browsable(false)]
        public int RenderWidth => IsVRAMPage ? VRAM.PageSize : Width;

        [Browsable(false)]
        public int RenderHeight => IsVRAMPage ? VRAM.PageSize : Height;

        [Browsable(false)]
        public int[][] Palettes { get; set; }

        [Browsable(false)]
        public bool[][] SemiTransparentPalettes { get; set; }

        [Browsable(false)]
        public int PaletteSize => Palettes?[0].Length ?? 0;

        [Browsable(false)]
        public bool HasPalette => Bpp <= 8;

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


        public void SetCLUTIndex(int clutIndex, bool force = false)
        {
            if (Palettes == null)
            {
                return;
            }
            if (clutIndex < 0 || clutIndex >= Palettes.Length)
            {
                clutIndex = 0; // Default to first CLUT index if we're out of bounds
            }
            if (force || CLUTIndex != clutIndex)
            {
                SetBitmapPalette(Bitmap, Palettes[clutIndex]);
                if (SemiTransparentMap != null && SemiTransparentPalettes != null)
                {
                    SetSemiTransparentMapPalette(SemiTransparentMap, SemiTransparentPalettes[clutIndex]);
                }
                CLUTIndex = clutIndex;
            }
        }

        public Bitmap SetupSemiTransparentMap()
        {
            if (SemiTransparentMap == null)
            {
                SemiTransparentMap = new Bitmap(Width, Height, Bitmap.PixelFormat);
                if (SemiTransparentPalettes != null)
                {
                    SetSemiTransparentMapPalette(SemiTransparentMap, SemiTransparentPalettes[0]);
                }
            }
            return SemiTransparentMap;
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
            SemiTransparentMap?.Dispose();
        }


        public static System.Drawing.Imaging.PixelFormat GetPixelFormat(int bpp)
        {
            switch (bpp)
            {
                case 4: return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                case 8: return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                case 16:
                case 24:
                case 32: return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                default: return 0;
            }
        }

        public static bool IsEmptyPalette(int[] palette)
        {
            return palette == EmptyPalette16 || palette == EmptyPalette256;
        }

        public static bool IsEmptySemiTransparentPalette(bool[] stpPalette)
        {
            return stpPalette == EmptySemiTransparentPalette16 || stpPalette == EmptySemiTransparentPalette256;
        }

        private static void SetBitmapPalette(Bitmap bitmap, int[] palette)
        {
            var count = palette.Length;
            var colorPalette = bitmap.Palette;
            for (var i = 0; i < count; i++)
            {
                colorPalette.Entries[i] = System.Drawing.Color.FromArgb(palette[i]);
            }
            bitmap.Palette = colorPalette;
        }

        private static void SetSemiTransparentMapPalette(Bitmap semiTransparentMap, bool[] stpPalette)
        {
            var count = stpPalette.Length;
            var stpColorPalette = semiTransparentMap.Palette;
            for (var i = 0; i < count; i++)
            {
                stpColorPalette.Entries[i] = stpPalette[i] ? SemiTransparentFlag : NoSemiTransparentFlag;
            }
            semiTransparentMap.Palette = stpColorPalette;
        }
    }
}
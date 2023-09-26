using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using PSXPrev.Common.Renderer;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common
{
    public class Texture : IDisposable
    {
        public static readonly System.Drawing.Color NoSemiTransparentFlag = System.Drawing.Color.FromArgb(0, 0, 0, 0);
        public static readonly System.Drawing.Color SemiTransparentFlag = System.Drawing.Color.FromArgb(255, 255, 255, 255);

        private static readonly ushort[][] EmptyPalettes16  = new ushort[][] { new ushort[16] };
        private static readonly ushort[][] EmptyPalettes256 = new ushort[][] { new ushort[256] };

        private readonly WeakReference<RootEntity> _ownerEntity = new WeakReference<RootEntity>(null);
        private BitmapData _bmpData; // State for locking and unlocking texture to get individual pixels
        private BitmapData _stpData;

        public Texture(Bitmap bitmap, int x, int y, int bpp, int texturePage, int clutIndex, ushort[][] palettes, bool? hasSemiTransparency, bool isVRAMPage = false)
        {
            Bitmap = bitmap;
            X = x;
            Y = y;
            Bpp = bpp;
            TexturePage = texturePage;
            CLUTIndex = clutIndex;
            Palettes = palettes;
            OriginalPalettes = palettes;
            IsVRAMPage = isVRAMPage;
            if (hasSemiTransparency ?? (palettes != null && CheckPalettesForStp(palettes)))
            {
                SetupSemiTransparentMap();
            }
            SetCLUTIndex(clutIndex, true);
        }

        public Texture(int width, int height, int x, int y, int bpp, int texturePage, int clutIndex, ushort[][] palettes, bool? hasSemiTransparency, bool isVRAMPage = false)
            : this(new Bitmap(width, height, GetPixelFormat(bpp)), x, y, bpp, texturePage, clutIndex, palettes, hasSemiTransparency, isVRAMPage)
        {
        }

        // preserveFormat has no effect if HasPalette is false, otherwise the copy will be much slower when true.
        public Texture(Texture fromTexture, bool preserveFormat)
        {
            X = fromTexture.X;
            Y = fromTexture.Y;
            TexturePage = fromTexture.TexturePage;
            IsVRAMPage = fromTexture.IsVRAMPage;
            Name = fromTexture.Name;
            FormatName = fromTexture.FormatName;
            FileOffset = fromTexture.FileOffset;
            ResultIndex = fromTexture.ResultIndex;
            try
            {
                // Preserve format only needs to do something special if we're paletted
                if (preserveFormat && fromTexture.HasPalette)
                {
                    Bpp = fromTexture.Bpp;
                    CLUTIndex = fromTexture.CLUTIndex;
                    Palettes = fromTexture.Palettes;

                    Bitmap = fromTexture.Bitmap.DeepClone();

                    if (fromTexture.SemiTransparentMap != null)
                    {
                        SemiTransparentMap = fromTexture.SemiTransparentMap.DeepClone();
                    }
                }
                else
                {
                    Bpp = fromTexture.HasPalette ? 32 : fromTexture.Bpp;

                    // This expects these constructors to use PixelFormat.Format32bppArgb.
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
        public string Name { get; set; }

        [DisplayName("Format"), ReadOnly(true)]
        public string FormatName { get; set; }

#if DEBUG
        [DisplayName("Debug Data"), ReadOnly(true)]
#else
        [Browsable(false)]
#endif
        public string[] DebugData { get; set; }

        [Browsable(false)]
        public long FileOffset { get; set; }

#if DEBUG
        [DisplayName("Result Index"), ReadOnly(true)]
#else
        [Browsable(false)]
#endif
        public int ResultIndex { get; set; }

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

        [DisplayName("Texture ID")]
        public uint? LookupID { get; set; }

        // Usable area of the texture (only different from Width/Height when IsVRAMPage is true).
        [Browsable(false)]
        public int RenderWidth => IsVRAMPage ? VRAM.PageSize : Width;

        [Browsable(false)]
        public int RenderHeight => IsVRAMPage ? VRAM.PageSize : Height;

        [Browsable(false)]
        public ushort[][] Palettes { get; set; }

        [Browsable(false)]
        public ushort[][] OriginalPalettes { get; set; }

        [Browsable(false)]
        public int PaletteSize => Palettes?[0].Length ?? 0;

        [Browsable(false)]
        public bool HasPalette => Bpp <= 8;

        [Browsable(false)]
        public bool IsVRAMPage { get; set; }

        [Browsable(false)]
        public bool NeedsPacking => LookupID.HasValue; // Texture can be looked up, and requires packing to determine where to draw

        [Browsable(false)]
        public bool IsPacked { get; set; } // The texture's Page, X, and Y were assigned by packing

        [Browsable(false)]
        public bool NeedsPalette { get; set; }

        [Browsable(false)]
        public bool IsPaletteAssigned { get; set; }

        [Browsable(false)]
        public Bitmap Bitmap { get; set; }

        [Browsable(false)]
        public Bitmap SemiTransparentMap { get; set; }

        [Browsable(false)]
        public bool IsLocked => _bmpData != null;

        // The owner model that created this texture (if any).
        [Browsable(false)]
        public RootEntity OwnerEntity
        {
            get => _ownerEntity.TryGetTarget(out var owner) ? owner : null;
            set => _ownerEntity.SetTarget(value);
        }

        public override string ToString()
        {
            var name = Name ?? nameof(Texture);
            return $"{name} {Width}x{Height} {Bpp}bpp";
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
                SetBitmapPalette(Bitmap, SemiTransparentMap, Palettes[clutIndex]);
                CLUTIndex = clutIndex;
            }
        }

        public Bitmap SetupSemiTransparentMap()
        {
            if (SemiTransparentMap == null)
            {
                SemiTransparentMap = new Bitmap(Width, Height, Bitmap.PixelFormat);
                if (HasPalette)
                {
                    var palette = Palettes?[0];
                    if (palette == null)
                    {
                        palette = GetEmptyPalettes(Bpp)[0];
                    }
                    SetBitmapPalette(null, SemiTransparentMap, palette);
                }
                else
                {
                    using (var graphics = Graphics.FromImage(SemiTransparentMap))
                    {
                        graphics.Clear(NoSemiTransparentFlag);
                    }
                }
            }
            return SemiTransparentMap;
        }

        public void Dispose()
        {
            if (IsLocked)
            {
                Unlock();
            }
            Bitmap?.Dispose();
            SemiTransparentMap?.Dispose();
        }

        public void Lock(bool needSemiTransparency = true)
        {
            if (IsLocked)
            {
                throw new InvalidOperationException("Texture is already locked, can't lock");
            }
            var rect = new Rectangle(0, 0, Width, Height);
            _bmpData = Bitmap.LockBits(rect, ImageLockMode.ReadOnly, Bitmap.PixelFormat);

            try
            {
                if (!HasPalette && needSemiTransparency)
                {
                    // We only need to lookup stp information if we don't have a palette
                    _stpData = SemiTransparentMap?.LockBits(rect, ImageLockMode.ReadOnly, Bitmap.PixelFormat);
                }

                int baseStride;
                PixelFormat expectedFormat;
                switch (Bpp)
                {
                    case 4:
                        baseStride = (Width + 1) / 2;
                        expectedFormat = PixelFormat.Format4bppIndexed;
                        break;
                    case 8:
                        baseStride = Width;
                        expectedFormat = PixelFormat.Format8bppIndexed;
                        break;
                    case 16:
                    case 24:
                    case 32:
                        baseStride = Width * 4;
                        expectedFormat = PixelFormat.Format32bppArgb;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid Bpp");
                }
                GetPixelData(this, _bmpData, baseStride, expectedFormat, out _, true);
                GetPixelData(this, _stpData, baseStride, expectedFormat, out _, false);
            }
            catch
            {
                Unlock();
                throw;
            }
        }

        public void Unlock()
        {
            if (!IsLocked)
            {
                throw new InvalidOperationException("Texture is not locked, can't unlock");
            }
            try
            {
                if (_bmpData != null)
                {
                    Bitmap.UnlockBits(_bmpData);
                    _bmpData = null;
                }
            }
            finally
            {
                if (_stpData != null)
                {
                    SemiTransparentMap.UnlockBits(_stpData);
                    _stpData = null;
                }
            }
        }

        private static IntPtr GetPixelData(Texture texture, BitmapData data, int baseStride, PixelFormat expectedFormat, out int padding, bool nonNull)
        {
            if (data != null)
            {
                padding = data.Stride - baseStride;

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
                Trace.Assert(data.Height * data.Stride >= height * (baseStride + padding), "Unexpected pixel data size in unsafe context");
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

        public unsafe System.Drawing.Color GetPixel(int x, int y, out bool stp, out int? paletteIndex)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            else if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            var needsUnlock = false;
            if (!IsLocked)
            {
                Lock();
                needsUnlock = true;
            }

            var p = (byte*)_bmpData.Scan0;
            var s = (byte*)(_stpData?.Scan0 ?? IntPtr.Zero);
            p += _bmpData.Stride * y;
            if (s != null)
            {
                s += _stpData.Stride * y;
            }

            try
            {
                ushort paletteColor;
                switch (Bpp)
                {
                    case 4:
                        p += x / 2;
                        paletteIndex = (*p >> (x % 2 == 0 ? 4 : 0)) & 0xf; // First x is in MSBs
                        paletteColor = Palettes[CLUTIndex][paletteIndex.Value];
                        stp = TexturePalette.GetStp(paletteColor);
                        return TexturePalette.ToColor(paletteColor);

                    case 8:
                        p += x;
                        paletteIndex = *p;
                        paletteColor = Palettes[CLUTIndex][paletteIndex.Value];
                        stp = TexturePalette.GetStp(paletteColor);
                        return TexturePalette.ToColor(paletteColor);

                    case 16:
                    case 24:
                    case 32:
                        if (s != null)
                        {
                            s += (x * 4);
                            stp = *s == SemiTransparentFlag.B;
                        }
                        else
                        {
                            stp = false;
                        }
                        p += (x * 4);
                        var b = *p++;
                        var g = *p++;
                        var r = *p++;
                        var a = *p++;
                        paletteIndex = null;
                        return System.Drawing.Color.FromArgb(a, r, g, b);

                    default:
                        stp = false;
                        paletteIndex = null;
                        return System.Drawing.Color.Black;
                }
            }
            finally
            {
                if (needsUnlock)
                {
                    Unlock();
                }
            }
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

        public static ushort[][] GetEmptyPalettes(int bpp)
        {
            switch (bpp)
            {
                case  4: return EmptyPalettes16;
                case  8: return EmptyPalettes256;
                case 16:
                case 24:
                case 32:
                default: return null;
            }
        }


        public static bool CheckPalettesForStp(ushort[][] palettes)
        {
            var palettesCount = palettes.Length;
            var paletteSize = palettes[0].Length;
            for (var p = 0; p < palettesCount; p++)
            {
                var palette = palettes[p];
                for (var c = 0; c < paletteSize; c++)
                {
                    if (TexturePalette.GetStp(palette[c]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void SetBitmapPalette(Bitmap bitmap, Bitmap semiTransparentMap, ushort[] palette)
        {
            var colorPalette = bitmap?.Palette;
            var stpColorPalette = semiTransparentMap?.Palette;
            var count = palette?.Length ?? colorPalette?.Entries.Length ?? stpColorPalette.Entries.Length;
            for (var i = 0; i < count; i++)
            {
                var color = palette?[i] ?? 0;
                if (colorPalette != null)
                {
                    colorPalette.Entries[i] = TexturePalette.ToColor(color);
                }
                if (stpColorPalette != null)
                {
                    stpColorPalette.Entries[i] = TexturePalette.GetStp(color) ? SemiTransparentFlag : NoSemiTransparentFlag;
                }
            }
            if (bitmap != null)
            {
                bitmap.Palette = colorPalette;
            }
            if (semiTransparentMap != null)
            {
                semiTransparentMap.Palette = stpColorPalette;
            }
        }
    }
}
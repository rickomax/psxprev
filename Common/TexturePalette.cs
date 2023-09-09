using System;
using System.Drawing;

namespace PSXPrev.Common
{
    // 8-postfix functions expect RGB values in the range 0-31 (12-bit color channels divided by 8).
    public static class TexturePalette
    {
        public const ushort Transparent = 0;
        public const ushort Black = 0x8000;


        public static int ToArgb(ushort color, bool noTransparent = false)
        {
            var r = ((color      ) & 0x1f) << 3;
            var g = ((color >>  5) & 0x1f) << 3;
            var b = ((color >> 10) & 0x1f) << 3;
            var a = (!noTransparent && color == 0 ? 0 : 255); // Black color masking
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        public static System.Drawing.Color ToColor(ushort color, bool noTransparent = false)
        {
            return System.Drawing.Color.FromArgb(ToArgb(color, noTransparent));
        }


        public static void ToComponents(ushort color, out byte r, out byte g, out byte b)
        {
            ToComponents(color, out r, out g, out b, out _);
        }

        public static void ToComponents8(ushort color, out byte r, out byte g, out byte b)
        {
            ToComponents8(color, out r, out g, out b, out _);
        }

        public static void ToComponents(ushort color, out byte r, out byte g, out byte b, out bool stp)
        {
            r = (byte)(((color      ) & 0x1f) << 3);
            g = (byte)(((color >>  5) & 0x1f) << 3);
            b = (byte)(((color >> 10) & 0x1f) << 3);
            stp = (color & 0x8000) != 0; // Semi-transparency: 0-Off, 1-On
        }

        public static void ToComponents8(ushort color, out byte r, out byte g, out byte b, out bool stp)
        {
            r = (byte)((color      ) & 0x1f);
            g = (byte)((color >>  5) & 0x1f);
            b = (byte)((color >> 10) & 0x1f);
            stp = (color & 0x8000) != 0; // Semi-transparency: 0-Off, 1-On
        }

        public static ushort FromComponents(int r, int g, int b, bool stp = false)
        {
            ushort color;
            color  = (ushort)((((uint)r >> 3) & 0x1f)      );
            color |= (ushort)((((uint)g >> 3) & 0x1f) <<  5);
            color |= (ushort)((((uint)b >> 3) & 0x1f) << 10);
            color |= (ushort)(stp ? 0x8000 : 0);
            return color;
        }

        public static ushort FromComponents8(int r, int g, int b, bool stp = false)
        {
            ushort color;
            color  = (ushort)(((uint)r & 0x1f)      );
            color |= (ushort)(((uint)g & 0x1f) <<  5);
            color |= (ushort)(((uint)b & 0x1f) << 10);
            color |= (ushort)(stp ? 0x8000 : 0);
            return color;
        }

        public static bool GetStp(ushort color)
        {
            return (color & 0x8000) != 0;
        }

        public static ushort SetStp(ushort color, bool stp)
        {
            return (ushort)((color & ~0x8000u) | (stp ? 0x8000 : 0));
        }

        public static void SetStp(ref ushort color, bool stp)
        {
            color = SetStp(color, stp);
        }

        public static ushort ToggleStp(ushort color)
        {
            return (ushort)(color ^ 0x8000);
        }

        public static void ToggleStp(ref ushort color)
        {
            color = ToggleStp(color);
        }

        public static bool Equals(ushort color, ushort colorOther, bool ignoreStp = false)
        {
            if (ignoreStp)
            {
                // XOR to get non-zero bits that don't match, mask out stp bit, then check if zero.
                return ((color ^ colorOther) & ~0x8000u) == 0;
            }
            else
            {
                return (color == colorOther);
            }
        }

        public static bool CloseTo(ushort color, ushort colorOther, int threshold, bool ignoreStp = false)
        {
            return CloseTo8(color, colorOther, threshold / 8, ignoreStp);
        }

        public static bool CloseTo8(ushort color, ushort colorOther, int threshold8, bool ignoreStp = false)
        {
            if (Equals(color, colorOther, ignoreStp))
            {
                return true;
            }
            else if (!ignoreStp && ((color ^ colorOther) & 0x8000) != 0)
            {
                return false;
            }
            ToComponents8(color,      out var r,      out var g,      out var b);
            ToComponents8(colorOther, out var rOther, out var gOther, out var bOther);
            return Math.Abs(r - rOther) <= threshold8 &&
                   Math.Abs(g - gOther) <= threshold8 &&
                   Math.Abs(b - bOther) <= threshold8;
        }

        public static bool CloseToWrapped(ushort color, ushort colorOther, int threshold, bool ignoreStp = false)
        {
            if (Equals(color, colorOther, ignoreStp))
            {
                return true;
            }
            else if (!ignoreStp && ((color ^ colorOther) & 0x8000) != 0)
            {
                return false;
            }
            // Use 8-postfix to half the number of bit-shifts needed
            ToComponents8(color,      out var r,      out var g,      out var b);
            ToComponents8(colorOther, out var rOther, out var gOther, out var bOther);
            // Bit shift then cast to sbyte to sign-extend, then cast to int to avoid overflow exception.
            return Math.Abs((int)(sbyte)((r - rOther) << 3)) <= threshold &&
                   Math.Abs((int)(sbyte)((g - gOther) << 3)) <= threshold &&
                   Math.Abs((int)(sbyte)((b - bOther) << 3)) <= threshold;
        }

        public static bool CloseToWrapped8(ushort color, ushort colorOther, int threshold8, bool ignoreStp = false)
        {
            return CloseToWrapped(color, colorOther, threshold8 * 8, ignoreStp);
        }

        public static bool InRange(ushort color, ushort colorMin, ushort colorMax, bool ignoreStp = false)
        {
            // If we're not ignoring stp, then check to make sure both stps aren't in the range,
            // If they're not, then compare stp ahead of time.
            if (!ignoreStp && ((colorMin ^ colorMax) & 0x8000) == 0 && ((color ^ colorMin) & 0x8000) != 0)
            {
                return false;
            }
            // Use 8-postfix since it won't make a difference.
            ToComponents8(color,    out var r,    out var g,    out var b);
            ToComponents8(colorMin, out var rMin, out var gMin, out var bMin);
            ToComponents8(colorMax, out var rMax, out var gMax, out var bMax);
            return (r >= rMin && r <= rMax && g >= gMin && g <= gMax && b >= bMin && b <= bMax);
        }

        public static bool Equals(ushort[] paletteA, ushort[] paletteB, bool ignoreStp = false)
        {
            if (paletteA.Length == paletteB.Length)
            {
                var paletteSize = paletteA.Length;
                for (var c = 0; c < paletteSize; c++)
                {
                    if (!Equals(paletteA[c], paletteB[c], ignoreStp))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static bool MaskColors(ushort[][] palettes, ushort[] masks, bool ignoreStp, bool unmaskBlack)
        {
            var semiTransparencySet = false;
            var palettesCount = palettes.Length;
            var paletteSize = palettes[0].Length;
            var maskCount = masks.Length;
            for (var p = 0; p < palettesCount; p++)
            {
                var palette = palettes[p];
                for (var c = 0; c < paletteSize; c++)
                {
                    var color = palette[c];
                    var masked = false;
                    for (var m = 0; m < maskCount; m++)
                    {
                        if (Equals(color, masks[m], ignoreStp))
                        {
                            palette[c] = Transparent;
                            masked = true;
                            break;
                        }
                    }
                    if (!masked && color == Transparent && unmaskBlack)
                    {
                        palette[c] = Black;
                        semiTransparencySet = true;
                    }
                }
            }
            return semiTransparencySet;
        }

        public static bool MaskColor(ushort[][] palettes, ushort mask, bool ignoreStp, bool unmaskBlack)
        {
            var semiTransparencySet = false;
            var palettesCount = palettes.Length;
            var paletteSize = palettes[0].Length;
            for (var p = 0; p < palettesCount; p++)
            {
                var palette = palettes[p];
                for (var c = 0; c < paletteSize; c++)
                {
                    var color = palette[c];
                    if (Equals(color, mask, ignoreStp))
                    {
                        palette[c] = Transparent;
                    }
                    else if (color == Transparent && unmaskBlack)
                    {
                        palette[c] = Black;
                        semiTransparencySet = true;
                    }
                }
            }
            return semiTransparencySet;
        }

        public static bool UnmaskBlack(ushort[][] palettes)
        {
            var semiTransparencySet = false;
            var palettesCount = palettes.Length;
            var paletteSize = palettes[0].Length;
            for (var p = 0; p < palettesCount; p++)
            {
                var palette = palettes[p];
                for (var c = 0; c < paletteSize; c++)
                {
                    if (palette[c] == Transparent)
                    {
                        palette[c] = Black;
                        semiTransparencySet = true;
                    }
                }
            }
            return semiTransparencySet;
        }
    }
}

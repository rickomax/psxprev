using System;
using System.Drawing;

namespace PSXPrev.Common
{
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

        public static void ToComponents(ushort color, out byte r, out byte g, out byte b, out bool stp)
        {
            r = (byte)(((color) & 0x1f) << 3);
            g = (byte)(((color >>  5) & 0x1f) << 3);
            b = (byte)(((color >> 10) & 0x1f) << 3);
            stp = (color & 0x8000) != 0; // Semi-transparency: 0-Off, 1-On
        }

        public static ushort FromComponents(int r, int g, int b, bool stp = false)
        {
            ushort color;
            color  = (ushort)(((r >> 3) & 0x1f));
            color |= (ushort)(((g >> 3) & 0x1f) <<  5);
            color |= (ushort)(((b >> 3) & 0x1f) << 10);
            color |= (ushort)(stp ? 0x8000 : 0);
            return color;
        }

        public static bool GetStp(ushort color)
        {
            return (color & 0x8000) != 0;
        }

        public static ushort SetStp(ushort color, bool stp)
        {
            return (ushort)((color & ~0x8000) | (stp ? 0x8000 : 0));
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

        public static bool Equals(ushort colorA, ushort colorB, bool ignoreStp = false)
        {
            if (ignoreStp)
            {
                // XOR to get non-zero bits that don't match, mask out stp bit, then check if zero.
                return ((colorA ^ colorB) & ~0x8000) == 0;
            }
            else
            {
                return (colorA == colorB);
            }
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

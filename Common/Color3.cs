using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;

namespace PSXPrev.Common
{
    public struct Color3 : IEquatable<Color3>
    {
        public const float DefaultColorTone = 0.5f;
        public static readonly Color3 Red = new Color3(1f, 0f, 0f);
        public static readonly Color3 Orange = new Color3(1f, 0.5f, 0f);
        public static readonly Color3 Yellow = new Color3(1f, 1f, 0f);
        public static readonly Color3 Green = new Color3(0f, 1f, 0f);
        public static readonly Color3 Cyan = new Color3(0f, 1f, 1f);
        public static readonly Color3 Blue = new Color3(0f, 0f, 1f);
        public static readonly Color3 Purple = new Color3(0.5f, 0f, 1f);
        public static readonly Color3 Magenta = new Color3(1f, 0f, 1f);
        public static readonly Color3 Black = new Color3(0f, 0f, 0f);
        public static readonly Color3 White = new Color3(1f, 1f, 1f);
        public static readonly Color3 Grey = new Color3(DefaultColorTone, DefaultColorTone, DefaultColorTone);


        public float R;
        public float G;
        public float B;

        public Color3(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color3(byte r, byte g, byte b)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
        }

        public Color3(Vector3 vector)
            : this(vector.X, vector.Y, vector.Z)
        {
        }

        public Color3(Vector4 vector)
            : this(vector.X, vector.Y, vector.Z)
        {
        }

        public Color3(Color3 color)
            : this(color.R, color.G, color.B)
        {
        }

        public Color3(Color4 color)
            : this(color.R, color.G, color.B)
        {
        }

        public Color3(Color color)
            : this(color.R, color.G, color.B)
        {
        }

        public override string ToString()
        {
            return $"({(int)(R * 255)}, {(int)(G * 255)}, {(int)(B * 255)})";
            //return $"{R}|{G}|{B}";
        }

        public override int GetHashCode()
        {
            // Same as Color4.GetHashCode() without the alpha channel
            return ToRgb();
            // Same as Vector3.GetHashCode()
            /*var hash = R.GetHashCode();
            hash = ((hash * 397) ^ G.GetHashCode());
            return ((hash * 397) ^ B.GetHashCode());*/
        }

        public override bool Equals(object obj)
        {
            return (obj is Color3 other && Equals(other));
        }

        public bool Equals(Color3 other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public int ToRgb()
        {
            return (int)(((uint)(R * 255) << 16) | ((uint)(G * 255) << 8) | (uint)(B * 255));
        }

        public int ToArgb(float alpha = 1f)
        {
            return (int)(((uint)(alpha * 255) << 24) | (uint)ToRgb());
        }


        public static Color3 Lerp(Color3 a, Color3 b, float blend)
        {
            a.R = (blend * (b.R - a.R)) + a.R;
            a.G = (blend * (b.G - a.G)) + a.G;
            a.B = (blend * (b.B - a.B)) + a.B;
            return a;
        }

        public static void Lerp(ref Color3 a, ref Color3 b, float blend, out Color3 result)
        {
            result.R = (blend * (b.R - a.R)) + a.R;
            result.G = (blend * (b.G - a.G)) + a.G;
            result.B = (blend * (b.B - a.B)) + a.B;
        }

        public static bool operator ==(Color3 a, Color3 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Color3 a, Color3 b)
        {
            return !a.Equals(b);
        }

        public static explicit operator Vector3(Color3 color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        public static explicit operator Color3(Vector3 vector)
        {
            return new Color3(vector.X, vector.Y, vector.Z);
        }

        public static explicit operator Vector4(Color3 color)
        {
            return new Vector4(color.R, color.G, color.B, 1f);
        }

        public static explicit operator Color3(Vector4 vector)
        {
            return new Color3(vector.X, vector.Y, vector.Z);
        }

        public static explicit operator Color4(Color3 color)
        {
            return new Color4(color.R, color.G, color.B, 1f);
        }

        public static explicit operator Color3(Color4 color)
        {
            return new Color3(color.R, color.G, color.B);
        }

        public static explicit operator Color(Color3 color)
        {
            var r = (int)(GeomMath.Clamp(color.R, 0f, 1f) * 255);
            var g = (int)(GeomMath.Clamp(color.G, 0f, 1f) * 255);
            var b = (int)(GeomMath.Clamp(color.B, 0f, 1f) * 255);
            return Color.FromArgb(r, g, b);
        }

        public static explicit operator Color3(Color color)
        {
            return new Color3(color.R, color.G, color.B);
        }
    }
}
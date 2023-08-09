using OpenTK;

namespace PSXPrev.Common
{
    public class Color
    {
        public const float DefaultColorTone = 0.5f;
        public static readonly Color Red = new Color(1f, 0f, 0f);
        public static readonly Color Orange = new Color(1f, 0.5f, 0f);
        public static readonly Color Yellow = new Color(1f, 1f, 0f);
        public static readonly Color Green = new Color(0f, 1f, 0f);
        public static readonly Color Cyan = new Color(0f, 1f, 1f);
        public static readonly Color Blue = new Color(0f, 0f, 1f);
        public static readonly Color Purple = new Color(0.5f, 0f, 1f);
        public static readonly Color Magenta = new Color(1f, 0f, 1f);
        public static readonly Color Black = new Color(0f, 0f, 0f);
        public static readonly Color White = new Color(1f, 1f, 1f);
        public static readonly Color Grey = new Color(DefaultColorTone, DefaultColorTone, DefaultColorTone);

        public float R;
        public float G;
        public float B;

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color(Color fromColor)
            : this(fromColor.R, fromColor.G, fromColor.B)
        {
        }

        public Color(Vector3 vector)
            : this(vector.X, vector.Y, vector.Z)
        {
        }

        public Color(System.Drawing.Color color)
            : this(color.R / 255f, color.G / 255f, color.B / 255f)
        {
        }

        public override string ToString()
        {
            return $"{R}|{G}|{B}";
        }


        public static Color Lerp(Color a, Color b, float blend)
        {
            //blend = GeomMath.Clamp(blend, 0f, 1f);
            return new Color(Vector3.Lerp((Vector3)a, (Vector3)b, blend));
        }

        public static explicit operator Vector3(Color color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        public static explicit operator Color(Vector3 vector)
        {
            return new Color(vector.X, vector.Y, vector.Z);
        }

        public static explicit operator Vector4(Color color)
        {
            return new Vector4(color.R, color.G, color.B, 1f);
        }

        public static explicit operator System.Drawing.Color(Color color)
        {
            var r = (int)(GeomMath.Clamp(color.R, 0f, 1f) * 255);
            var g = (int)(GeomMath.Clamp(color.G, 0f, 1f) * 255);
            var b = (int)(GeomMath.Clamp(color.B, 0f, 1f) * 255);
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public static explicit operator Color(System.Drawing.Color color)
        {
            return new Color(color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }
}
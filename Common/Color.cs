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

        public Vector3 Vector => new Vector3(R, G, B);

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color(Vector3 vector)
            : this(vector.X, vector.Y, vector.Z)
        {
        }

        public Color(Color fromColor)
            : this(fromColor.R, fromColor.G, fromColor.B)
        {
        }

        public Color(System.Drawing.Color color)
            : this(color.R / 255f, color.G / 255f, color.B / 255f)
        {
        }

        public override string ToString()
        {
            return R + "|" + G + "|" + B;
        }


        public static Color Lerp(Color a, Color b, float blend)
        {
            //blend = GeomMath.Clamp(blend, 0f, 1f);
            return new Color(Vector3.Lerp(a.Vector, b.Vector, blend));
        }
    }
}
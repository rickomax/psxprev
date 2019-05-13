using OpenTK;

namespace PSXPrev
{
    public struct Line
    {
        public Vector4 P1;
        public Vector4 P2;
        public Color Color;
        public float Width;

        public Line (Vector4 p1, Vector4 p2, Color color, float width = 1f)
        {
            P1 = p1;
            P2 = p2;
            Color = color;
            Width = width;
        }
    }
}

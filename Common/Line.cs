using OpenTK;

namespace PSXPrev.Common
{
    public class Line
    {
        public static readonly Vector3[] EmptyNormals = { Vector3.Zero, Vector3.Zero };
        public static readonly Vector2[] EmptyUv = { Vector2.Zero, Vector2.Zero };
        public static readonly Vector2[] EmptyUvCorrected = { Vector2.Zero, new Vector2(1f/256f) };
        public static readonly Color[] EmptyColors = { Color.Grey, Color.Grey };

        public Vector3[] Vertices { get; set; }
        //public Vector3[] Normals { get; set; } = EmptyNormals;
        //public Vector2[] Uv { get; set; } = EmptyUv;
        public Color[] Colors { get; set; } = EmptyColors;

        public Line(Vector3 vertex0, Vector3 vertex1, Color color0, Color color1 = null)
        {
            Vertices = new[] { vertex0, vertex1 };
            //Normals = EmptyNormals;
            //Uv = EmptyUv;
            Colors = new[] { color0, color1 ?? color0 };
        }


        public Triangle ToTriangle()
        {
            return new Triangle
            {
                Vertices = new[] { Vertices[0], Vertices[1], Vertices[1] },
                Normals = Triangle.EmptyNormals,
                Uv = Triangle.EmptyUv,
                Colors = new[] { Colors[0], Colors[1], Colors[1] },
            };
        }
    }
}

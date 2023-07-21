using OpenTK;

namespace PSXPrev.Classes
{
    public class TiledUV
    {
        public Vector2[] BaseUv { get; set; }
        public float X { get; set; } // Position added to BaseUv after wrapping.
        public float Y { get; set; }
        public float Width  { get; set; } // Denominator of modulus with BaseUv for wrapping.
        public float Height { get; set; }

        public Vector2 Offset => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);
        public Vector4 Area => new Vector4(X, Y, Width, Height);

        public TiledUV(Vector2[] baseUv, float x, float y, float width, float height)
        {
            BaseUv = baseUv;
            X = x;
            Y = y;
            Width  = width;
            Height = height;
        }

        public TiledUV(Vector2[] baseUv, Vector2 offset, Vector2 size)
            : this(baseUv, offset.X, offset.Y, size.X, size.Y)
        {
        }

        public TiledUV(Vector2[] baseUv, Vector4 area)
            : this(baseUv, area.X, area.Y, area.Z, area.W)
        {
        }

        public TiledUV(TiledUV fromTiledUv)
            : this(fromTiledUv.BaseUv, fromTiledUv.X, fromTiledUv.Y, fromTiledUv.Width, fromTiledUv.Height)
        {
        }

        // This function isn't used, since the tiled (display) UV can be calculated with integer math.
        public Vector2[] ConvertBaseUv()
        {
            var uv = new Vector2[BaseUv.Length];
            for (var i = 0; i < uv.Length; i++)
            {
                uv[i] = Convert(BaseUv[i], X, Y, Width, Height);
            }
            return uv;
        }

        public Vector2 Convert(Vector2 uv) => Convert(uv, X, Y, Width, Height);


        public static Vector2 Convert(Vector2 uv, float x, float y, float width, float height)
        {
            return new Vector2((width  == 0f ? uv.X : (x + uv.X % width)),
                               (height == 0f ? uv.Y : (y + uv.Y % height)));
        }
    }
}

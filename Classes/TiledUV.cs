using OpenTK;

namespace PSXPrev.Classes
{
    public class TiledUV
    {
        public Vector2[] BaseUv { get; set; }
        public Vector2 Offset { get; set; } // Position added to BaseUv after wrapping.
        public Vector2 Size { get; set; }   // Denominator of modulus with BaseUv for wrapping.

        public Vector4 Area => new Vector4(Offset.X, Offset.Y, Size.X, Size.Y);

        public TiledUV(Vector2[] baseUv, float x, float y, float width, float height)
        {
            BaseUv = baseUv;
            Offset = new Vector2(x, y);
            Size   = new Vector2(width, height);
        }

        public TiledUV(TiledUV fromTiledUv)
        {
            BaseUv = fromTiledUv.BaseUv;
            Offset = fromTiledUv.Offset;
            Size   = fromTiledUv.Size;
        }

        // This function isn't used, since the tiled (display) UV can be calculated with integer math.
        public Vector2[] ConvertBaseUv()
        {
            var uv = new Vector2[BaseUv.Length];
            for (var i = 0; i < uv.Length; i++)
            {
                uv[i] = Convert(BaseUv[i], Offset.X, Offset.Y, Size.X, Size.Y);
            }
            return uv;
        }

        public Vector2 Convert(Vector2 uv) => Convert(uv, Offset.X, Offset.Y, Size.X, Size.Y);


        public static Vector2 Convert(Vector2 uv, float x, float y, float width, float height)
        {
            return new Vector2((width  == 0f ? uv.X : (x + uv.X % width)),
                               (height == 0f ? uv.Y : (y + uv.Y % height)));
        }
    }
}

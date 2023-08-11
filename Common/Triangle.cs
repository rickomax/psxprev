using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Common
{
    public class Triangle
    {
        public enum PrimitiveTypeEnum
        {
            GsUF3,
            GsUF4,
            TMD_P_F3,
            TMD_P_G3,
            TMD_P_F3G,
            TMD_P_G3G,
            TMD_P_TF3,
            TMD_P_TG3,
            TMD_P_NF3,
            TMD_P_NG3,
            TMD_P_TNF3,
            TMD_P_TNG3,
            TMD_P_F4,
            TMD_P_G4,
            TMD_P_G4G,
            TMD_P_F4G,
            TMD_P_TF4,
            TMD_P_TG4,
            TMD_P_NF4,
            TMD_P_NG4,
            TMD_P_TNF4,
            TMD_P_TNG4,
            _poly_ft3,
            _poly_ft4,
            _poly_gt3,
            _poly_gt4,
            _poly_f3,
            _poly_f4,
            _poly_g3,
            _poly_g4,
            _poly_ft3c,
            _poly_ft4c,
            _poly_gt3c,
            _poly_gt4c,
            _poly_f3c,
            _poly_f4c,
            _poly_g3c,
            _poly_g4c
        }

        public const uint NoAttachment = uint.MaxValue;

        public static readonly Vector3[] EmptyNormals = { Vector3.Zero, Vector3.Zero, Vector3.Zero };
        public static readonly Vector2[] EmptyUv = { Vector2.Zero, Vector2.Zero, Vector2.Zero };
        public static readonly Vector2[] EmptyUvCorrected = { Vector2.Zero, Vector2.Zero, new Vector2(1f/256f) };
        public static readonly Color[] EmptyColors = { Color.Grey, Color.Grey, Color.Grey };
        public static readonly uint[] EmptyAttachableIndices = { NoAttachment, NoAttachment, NoAttachment };


        [ReadOnly(true), DisplayName("Parent")]
        public EntityBase ParentEntity { get; set; }

        [ReadOnly(true)]
        public Vector3[] Vertices { get; set; }

        [ReadOnly(true)]
        public Vector3[] Normals { get; set; } = EmptyNormals;

        [ReadOnly(true), DisplayName("UVs")]
        public Vector2[] Uv { get; set; } = EmptyUv;

        // Defines the area of the texture page that Uv is wrapped around.
        [ReadOnly(true), Browsable(false)]
        public TiledUV TiledUv { get; set; }

        [ReadOnly(true), DisplayName("Tiled Base UVs")]
        public Vector2[] TiledBaseUv => TiledUv?.BaseUv;

        [ReadOnly(true), DisplayName("Tiled Area UVs")]
        public Vector4? TiledArea => TiledUv?.Area;

        [ReadOnly(true), DisplayName("Is Tiled Texture")]
        public bool IsTiled => TiledUv != null;

        [Browsable(false)]
        public bool NeedsTiled => TiledUv?.NeedsTiled ?? false;

        [ReadOnly(true)]
        public Color[] Colors { get; set; } = EmptyColors;

        [Browsable(false)]
        public uint[] OriginalVertexIndices { get; set; }
        
        [Browsable(false)]
        public uint[] OriginalNormalIndices { get; set; }

        [Browsable(false)]
        public uint[] AttachableIndices { get; set; } = EmptyAttachableIndices;

        [Browsable(false)]
        public uint[] AttachedIndices { get; set; }

        // HMD: Attached (shared) normal indices from other model entities.
        [Browsable(false)]
        public uint[] AttachedNormalIndices { get; set; }

        // Cache of already-attached entities/vertices to speed up FixConnections.
        [Browsable(false)]
        public Tuple<EntityBase, Vector3>[] AttachedCache { get; set; }

        [Browsable(false)]
        public float IntersectionDistance { get; set; }

        public Triangle()
        {

        }

        public Triangle(Triangle fromTriangle)
        {
            Vertices = fromTriangle.Vertices;
            Normals = fromTriangle.Normals;
            Uv = fromTriangle.Uv;
            TiledUv = fromTriangle.TiledUv;
            Colors = fromTriangle.Colors;
            OriginalVertexIndices = fromTriangle.OriginalVertexIndices;
            OriginalNormalIndices = fromTriangle.OriginalNormalIndices;
            AttachableIndices = fromTriangle.AttachableIndices;
            AttachedIndices = fromTriangle.AttachedIndices;
            AttachedNormalIndices = fromTriangle.AttachedNormalIndices;
        }


        // A fix to correct UVs where either all U or V coordinates are identical, which causes tearing.
        public void CorrectUVTearing()
        {
            // This fix should be applied even if all 3 UVs are Vector2.Zero.
            // However, it should not be applied if this face is untextured.
            if (Program.FixUVAlignment)
            {
                // This type of tearing occurs only when the UV alignment fix is in place.
                // It happens if all of the U or V coordinates or identical, effectively turning the UV face into a 1D line.
                // When this happens with the alignment fix, the UV line is right on the pixel boundary.
                // Incrementing any one of the 3 U/V coordinates will fix 1D UV tearing.

                var uvs = TiledUv?.BaseUv ?? Uv;

#if true
                // Current Solution B:
                // This works by offsetting every UV coordinate slightly if its on a pixel boundary.
                // If the offsetted UV coordinate is the max, then the offset will be subtracted,
                // otherwise the offset will be added. If the min and max component are equal, then the
                // coordinate will be offset to the center of the pixel.
                // This is preferred over Solution A, because it also solves Wireframe/Vertices fighting.
                // The downside is it will become less accurate if you export a model with a very large single texture.
                var uvMin = uvs[0];
                var uvMax = uvMin;
                for (var i = 1; i < 3; i++)
                {
                    var uv = uvs[i];
                    uvMin = Vector2.ComponentMin(uvMin, uv);
                    uvMax = Vector2.ComponentMax(uvMax, uv);
                }

                var uvScalar = GeomMath.UVScalar;
                var inc = 0.001f / uvScalar; // Offset UV coordinates by 1/1000th of a pixel
                var half = 0.5f / uvScalar;  // Offset UV coordinates to the center of the pixel
                for (var i = 0; i < 3; i++)
                {
                    var uv = uvs[i];
                    // Check if this coordinate is on a pixel boundary
                    if ((float)Math.Floor(uv.X * uvScalar) == (uv.X * uvScalar))
                    {
                        uv.X += (uvMin.X == uvMax.X) ? half : ((uv.X < uvMax.X) ? inc : -inc);
                        //uv.X += (uv.X < uvMax.X || uvMin.X == uvMax.X) ? inc : -inc;
                        //uv.X += (uv.X < 1f) ? inc : -inc;
                    }
                    if ((float)Math.Floor(uv.Y * uvScalar) == (uv.Y * uvScalar))
                    {
                        uv.Y += (uvMin.Y == uvMax.Y) ? half : ((uv.Y < uvMax.Y) ? inc : -inc);
                        //uv.Y += (uv.Y < uvMax.Y || uvMin.Y == uvMax.Y) ? inc : -inc;
                        //uv.Y += (uv.Y < 1f) ? inc : -inc;
                    }
                    uvs[i] = uv;
                }
#else
                // Old Solution A:
                // This works well, until you try to use textures with Wireframe/Vertices draw mode.

                // The reason it's safe to increment by one, is because the area covered by the UV will still only be one pixel.
                // Before:
                // |
                // |_
                // After:
                // |\
                // |_\

                var uvs = TiledUv?.BaseUv ?? Uv;

                // index 2 is arbitrarily chosen.
                if (uvs[0].X == uvs[1].X && uvs[0].X == uvs[2].X)
                {
                    uvs[2].X += 1f / GeomMath.UVScalar;
                }
                if (uvs[0].Y == uvs[1].Y && uvs[0].Y == uvs[2].Y)
                {
                    uvs[2].Y += 1f / GeomMath.UVScalar;
                }
#endif
            }
        }
    }
}
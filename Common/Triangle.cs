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

        public const uint NoJoint = uint.MaxValue;

        public static readonly Vector3[] EmptyNormals = { Vector3.Zero, Vector3.Zero, Vector3.Zero };
        public static readonly Vector2[] EmptyUv = { Vector2.Zero, Vector2.Zero, Vector2.Zero };
        public static readonly Vector2[] EmptyUvCorrected = { Vector2.Zero, Vector2.Zero, new Vector2(1f/256f) };
        public static readonly Color[] EmptyColors = { Color.Grey, Color.Grey, Color.Grey };
        public static readonly uint[] EmptyJoints = { NoJoint, NoJoint, NoJoint };


        [DisplayName("Parent"), ReadOnly(true), TypeConverter(typeof(ExpandableObjectConverter))]
        public EntityBase ParentEntity { get; set; }

        [ReadOnly(true)]
        public Vector3[] Vertices { get; set; }

        [ReadOnly(true)]
        public Vector3[] Normals { get; set; } = EmptyNormals;

        [DisplayName("UVs"), ReadOnly(true)]
        public Vector2[] Uv { get; set; } = EmptyUv;

        // Defines the area of the texture page that Uv is wrapped around.
        [Browsable(false), ReadOnly(true)]
        public TiledUV TiledUv { get; set; }

        [DisplayName("Tiled Base UVs"), ReadOnly(true)]
        public Vector2[] TiledBaseUv => TiledUv?.BaseUv;

        [DisplayName("Tiled Area UVs"), ReadOnly(true)]
        public Vector4? TiledArea => TiledUv?.Area;

        [DisplayName("Is Tiled Texture"), ReadOnly(true)]
        public bool IsTiled => TiledUv != null;

        [Browsable(false)]
        public bool NeedsTiled => TiledUv?.NeedsTiled ?? false;

        [ReadOnly(true)]
        public Color[] Colors { get; set; } = EmptyColors;

        // Used if we're baking joint attachments
        [Browsable(false)]
        public Vector3[] OriginalVertices { get; set; }

        [Browsable(false)]
        public Vector3[] OriginalNormals { get; set; }

        // Used for vertex animations
        [Browsable(false)]
        public uint[] OriginalVertexIndices { get; set; }
        
        [Browsable(false)]
        public uint[] OriginalNormalIndices { get; set; }

        // Used for attached limbs
        // These array values will be modified by RootEntity.PrepareJoints, so don't re-use the arrays unless they're empty.
        [Browsable(false)]
        public uint[] VertexJoints { get; set; }

        [Browsable(false)]
        public uint[] NormalJoints { get; set; }

        // Don't show uint.MaxValue in the property grid when there's no joint. That would be ugly.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DisplayName("Vertex Joint IDs"), ReadOnly(true)]
        public int[] PropertyGrid_VertexJoints => PropertyGrid_GetJoints(VertexJoints);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DisplayName("Normal Joint IDs"), ReadOnly(true)]
        public int[] PropertyGrid_NormalJoints => PropertyGrid_GetJoints(NormalJoints);

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static int[] PropertyGrid_GetJoints(uint[] joints)
        {
            return joints != null ? new int[] { (int)joints[0], (int)joints[1], (int)joints[2] } : null;
        }

        // True if this triangle has non-no vertex joints
        [Browsable(false)]
        public bool HasAttached
        {
            get
            {
                if (VertexJoints != null)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        if (VertexJoints[i] != NoJoint)
                            return true;
                    }
                }
                return false;
            }
        }

        [Browsable(false)]
        public float IntersectionDistance { get; set; }

//#if DEBUG
        [DisplayName("Debug Data"), ReadOnly(true)]
//#else
//        [Browsable(false)]
//#endif
        public string[] DebugData { get; set; }


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
            OriginalVertices = fromTriangle.OriginalVertices;
            OriginalNormals = fromTriangle.OriginalNormals;
            OriginalVertexIndices = fromTriangle.OriginalVertexIndices;
            OriginalNormalIndices = fromTriangle.OriginalNormalIndices;
            VertexJoints = fromTriangle.VertexJoints;
            NormalJoints = fromTriangle.NormalJoints;
        }


        public void TransformPositions(ref Matrix4 worldMatrix, Matrix4[] jointMatrices, out Vector3 vertex0, out Vector3 vertex1, out Vector3 vertex2)
        {
            vertex0 = TransformPosition(0, ref worldMatrix, jointMatrices);
            vertex1 = TransformPosition(1, ref worldMatrix, jointMatrices);
            vertex2 = TransformPosition(2, ref worldMatrix, jointMatrices);
        }

        public Vector3 TransformPosition(int index, ref Matrix4 worldMatrix, Matrix4[] jointMatrices)
        {
            var vertices = OriginalVertices ?? Vertices;
            var jointID = VertexJoints?[index] ?? NoJoint;
            if (jointID != NoJoint && jointMatrices != null)
            {
                return GeomMath.TransformPosition(ref vertices[index], ref jointMatrices[jointID]);
            }
            else
            {
                return GeomMath.TransformPosition(ref vertices[index], ref worldMatrix);
            }
        }

        public void TransformNormals(ref Matrix4 worldMatrix, Matrix4[] jointMatrices, out Vector3 normal0, out Vector3 normal1, out Vector3 normal2)
        {
            normal0 = TransformNormal(0, ref worldMatrix, jointMatrices);
            normal1 = TransformNormal(1, ref worldMatrix, jointMatrices);
            normal2 = TransformNormal(2, ref worldMatrix, jointMatrices);
        }

        public Vector3 TransformNormal(int index, ref Matrix4 worldMatrix, Matrix4[] jointMatrices)
        {
            var normals = OriginalNormals ?? Normals;
            var jointID = NormalJoints?[index] ?? NoJoint;
            if (jointID != NoJoint && jointMatrices != null)
            {
                return GeomMath.TransformNormalNormalized(ref normals[index], ref jointMatrices[jointID]);
            }
            else
            {
                return GeomMath.TransformNormalNormalized(ref normals[index], ref worldMatrix);
            }
        }


        // A fix to correct UVs where either all U or V coordinates are identical, which causes tearing.
        public Triangle CorrectUVTearing()
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
            return this;
        }

        // Shorthand for returning an array, but only if there are non-NoJoints.
        // modelJointID is used to change a joint to NoJoint if equal (because the joint is already attached to this model).
        public static uint[] CreateJoints(uint joint0, uint joint1, uint joint2, uint modelJointID)
        {
            if (joint0 == modelJointID) joint0 = NoJoint;
            if (joint1 == modelJointID) joint1 = NoJoint;
            if (joint2 == modelJointID) joint2 = NoJoint;

            if ((joint0 != NoJoint) || (joint1 != NoJoint) || (joint2 != NoJoint))
            {
                return new[] { joint0, joint1, joint2 };
            }
            return null;// EmptyJoints;
        }
    }
}
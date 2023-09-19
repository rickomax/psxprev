using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Common
{
    public enum AttachJointsMode
    {
        Hide,
        DontAttach,
        Attach,
    }

    public enum TextureUVConversion
    {
        Absolute,     // UVs are already stored with page-size
        TextureSpace, // UVs need to be converted from texture-size to page-size
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TextureLookup : IUVConverter
    {
        [DisplayName("Texture ID")]
        public uint? ID { get; set; } // Required, matches LookupID of texture

        [DisplayName("Expected Format")]
        public string ExpectedFormat { get; set; } // Optional, matches FormatName of texture

        [DisplayName("UV Conversion")]
        public TextureUVConversion UVConversion { get; set; } // Conversion used for UV size

        [DisplayName("Tiled Area Conversion")]
        public TextureUVConversion TiledAreaConversion { get; set; }

        [Browsable(false)]
        public bool UVClamp { get; set; } // Cap U or V at 1f if it goes above.

        [DisplayName("Found Texture"), ReadOnly(true), TypeConverter(typeof(ExpandableObjectConverter))]
        public Texture Texture { get; set; } // Matched texture

        [DisplayName("Enabled")]
        public bool Enabled { get; set; } = true; // Intended for use in property grid to turn off lookup


        public TextureLookup()
        {
        }

        public TextureLookup(TextureLookup fromTextureLookup)
        {
            ID = fromTextureLookup.ID;
            ExpectedFormat = fromTextureLookup.ExpectedFormat;
            UVConversion = fromTextureLookup.UVConversion;
            TiledAreaConversion = fromTextureLookup.TiledAreaConversion;
            UVClamp = fromTextureLookup.UVClamp;
            Texture = fromTextureLookup.Texture;
            Enabled = fromTextureLookup.Enabled;
        }


        public override string ToString()
        {
            var idName = ID?.ToString() ?? "<null>";
            return $"{nameof(TextureLookup)} ID={idName}";
        }


        private Vector2 UVTextureOffset => new Vector2((float)Texture.X / Renderer.VRAM.PageSize,
                                                       (float)Texture.Y / Renderer.VRAM.PageSize);

        private Vector2 UVTextureSize   => new Vector2((float)Texture.Width  / Renderer.VRAM.PageSize,
                                                       (float)Texture.Height / Renderer.VRAM.PageSize);

        public Vector2 ConvertUV(Vector2 uv, bool tiled)
        {
            if (Enabled && Texture != null && Texture.IsPacked)
            {
                if (UVConversion == TextureUVConversion.TextureSpace)
                {
                    uv *= UVTextureSize;
                }
                if (!tiled)
                {
                    uv += UVTextureOffset;

                    if (UVClamp)
                    {
                        if (uv.X > 1f) uv.X = 1f;
                        if (uv.Y > 1f) uv.Y = 1f;
                    }
                }
            }
            return uv;
        }

        public Vector4 ConvertTiledArea(Vector4 tiledArea)
        {
            if (Enabled && Texture != null && Texture.IsPacked)
            {
                if (TiledAreaConversion == TextureUVConversion.TextureSpace)
                {
                    var size = UVTextureSize;
                    tiledArea.X *= size.X;
                    tiledArea.Y *= size.Y;
                    tiledArea.Z *= size.X;
                    tiledArea.W *= size.Y;
                }
                var offset = UVTextureOffset;
                tiledArea.X += offset.X;
                tiledArea.Y += offset.Y;
            }
            return tiledArea;
        }
    }

    public class ModelEntity : EntityBase
    {
        [DisplayName("VRAM Page")]
        public uint TexturePage { get; set; }

        [Browsable(false)]
        public bool MissingTexture => NeedsTextureLookup && TextureLookup.Texture == null;

        // Use default flags for when a reader doesn't assign any.
        [DisplayName("Render Flags")]
        public RenderFlags RenderFlags { get; set; } = RenderFlags.DoubleSided | RenderFlags.Textured;

        [DisplayName("Mixture Rate")]
        public MixtureRate MixtureRate { get; set; }

        [DisplayName("Visible")]
        public bool Visible { get; set; } = true;

        [Browsable(false)]
        public Vector3 SpriteCenter { get; set; }

        [Browsable(false)]
        public bool IsSprite => RenderFlags.HasFlag(RenderFlags.Sprite) || RenderFlags.HasFlag(RenderFlags.SpriteNoPitch);

        // Debug render settings for testing, not for use with PlayStation models.
        // Note: DebugMeshRenderInfo's TexturePage, RenderFlags, MixtureRate, and Visible are ignored.
        [Browsable(false)]
        public Renderer.MeshRenderInfo DebugMeshRenderInfo { get; set; }


        [DisplayName("Triangles"), ReadOnly(true)]
        public int TrianglesCount => Triangles.Length;

        [Browsable(false)]
        public int TotalTriangles
        {
            get
            {
                var count = TrianglesCount;
                if (ChildEntities != null)
                {
                    foreach (ModelEntity subModel in ChildEntities)
                    {
                        count += subModel.TotalTriangles;
                    }
                }
                return count;
            }
        }

        private Triangle[] _triangles;
        [Browsable(false)]
        public Triangle[] Triangles
        {
            get => _triangles;
            set
            {
                if (value != null)
                {
                    for (var i = 0; i < value.Length; i++)
                    {
                        value[i].ParentEntity = this;
                    }
                }
                _triangles = value;
            }
        }

        [Browsable(false)]
        public Texture Texture { get; set; }

        [DisplayName("Texture Lookup")]
        public TextureLookup TextureLookup { get; set; }

        [Browsable(false)]
        public bool NeedsTextureLookup => IsTextured && (TextureLookup?.Enabled ?? false);

        [DisplayName("TMD ID")]
        public uint TMDID { get; set; }

        // This value will be modified by RootEntity.PrepareJoints.
        [Browsable(false)]
        public uint JointID { get; set; } = Triangle.NoJoint;

        // Don't show uint.MaxValue in the property grid when there's no joint. That would be ugly.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DisplayName("Joint ID"), ReadOnly(true)]
        public int PropertyGrid_JointID
        {
            get => (int)JointID;
            set => JointID = (uint)value;
        }

        [Browsable(false)]
        public bool IsJoint => JointID != Triangle.NoJoint;

        [Browsable(false)]
        public string MeshName { get; set; }

        [DisplayName("Has Tiled Texture"), ReadOnly(true)]
        public bool HasTiled
        {
            get
            {
                foreach (var triangle in Triangles)
                {
                    if (triangle.IsTiled)
                        return true;
                }
                return false;
            }
        }

        // Animation speed of texture in UV units per second
        [Browsable(false)]
        public Vector2 TextureAnimation { get; set; }

        [Browsable(false)]
        public bool NeedsTiled
        {
            get
            {
                var needsTextureLookup = NeedsTextureLookup;
                foreach (var triangle in Triangles)
                {
                    if (needsTextureLookup)
                    {
                        if (triangle.IsTiled)
                            return true;
                    }
                    else
                    {
                        if (triangle.NeedsTiled)
                            return true;
                    }
                }
                return false;
            }
        }

        [Browsable(false)]
        public bool IsTextured => RenderFlags.HasFlag(RenderFlags.Textured);

        // Not to be confused with IsTextured, this signifies a model is textured, and has a texture assigned.
        // IsTextured is still useful to help assigning Textures in PreviewForm.
        [Browsable(false)]
        public bool HasTexture => Texture != null && IsTextured;

        //[ReadOnly(true)]
        //public uint PrimitiveIndex { get; set; }

        //[Browsable(false)]
        //public int MeshIndex { get; set; }

        [Browsable(false)]
        public float Interpolator { get; set; }
        [Browsable(false)]
        public Vector3[] InitialVertices { get; set; }
        [Browsable(false)]
        public Vector3[] FinalVertices { get; set; }
        [Browsable(false)]
        public Vector3[] FinalNormals { get; set; }
        [Browsable(false)]
        public Vector3[] InitialNormals { get; set; }

        // The vertices of this model have been attached by FixConnections and are no longer invalid.
        [Browsable(false)]
        public bool IsAttached { get; set; }

        // Joint transforms have been manually applied to triangle vertices and normals.
        [Browsable(false)]
        public bool IsAttachedBaked { get; set; }

        // This model contains at least one attached vertex or normal.
        [Browsable(false)]
        public bool HasAttached { get; set; }

        // This model only contains attached vertices, and should not be used in bounds calculation.
        [Browsable(false)]
        public bool AttachedOnly { get; set; }

        // If true, the model's vertices are not transformed, and the model's vertices need joints to transform properly.
        [Browsable(false)]
        public bool NeedsJointTransform => HasAttached && (!IsAttached || !IsAttachedBaked);


        public ModelEntity()
        {
        }

        public ModelEntity(ModelEntity fromModel, Triangle[] triangles)
            : base(fromModel)
        {
            Triangles = triangles;
            TexturePage = fromModel.TexturePage;
            RenderFlags = fromModel.RenderFlags;
            MixtureRate = fromModel.MixtureRate;
            Visible = fromModel.Visible;
            SpriteCenter = fromModel.SpriteCenter;
            if (fromModel.DebugMeshRenderInfo != null)
            {
                DebugMeshRenderInfo = new Renderer.MeshRenderInfo(fromModel.DebugMeshRenderInfo);
            }
            Texture = fromModel.Texture;
            if (fromModel.TextureLookup != null)
            {
                TextureLookup = new TextureLookup(fromModel.TextureLookup);
            }
            TMDID = fromModel.TMDID;
            JointID = fromModel.JointID;
            MeshName = fromModel.MeshName;
            TextureAnimation = fromModel.TextureAnimation;
            IsAttached = fromModel.IsAttached;
            IsAttachedBaked = fromModel.IsAttachedBaked;
            HasAttached = fromModel.HasAttached;
            AttachedOnly = fromModel.AttachedOnly;
        }


        public override string ToString()
        {
            var name = EntityName ?? GetType().Name;
            var page = IsTextured ? TexturePage.ToString() : "null";
            return $"{name} Triangles={TrianglesCount} TexturePage={page}";
        }

        public override void ComputeBounds(AttachJointsMode attachJointsMode = AttachJointsMode.Hide, Matrix4[] jointMatrices = null)
        {
            var needsJointTransform = attachJointsMode == AttachJointsMode.Attach && NeedsJointTransform;
            if (jointMatrices == null && needsJointTransform)
            {
                jointMatrices = GetRootEntity().JointMatrices;
            }
            base.ComputeBounds(attachJointsMode, jointMatrices);
            var bounds = new BoundingBox();
            var worldMatrix = WorldMatrix;
            foreach (var triangle in Triangles)
            {
                for (var i = 0; i < 3; i++)
                {
                    if (attachJointsMode == AttachJointsMode.Hide && triangle.VertexJoints != null)
                    {
                        if (triangle.VertexJoints[i] != Triangle.NoJoint)
                        {
                            continue;
                        }
                    }

                    Vector3 vertex;
                    if (!needsJointTransform)
                    {
                        Vector3.TransformPosition(ref triangle.Vertices[i], ref worldMatrix, out vertex);
                    }
                    else
                    {
                        vertex = triangle.TransformPosition(i, ref worldMatrix, jointMatrices);
                    }
                    bounds.AddPoint(vertex);
                }
            }
            if (!bounds.IsSet)
            {
                // When a model has all attached limb vertices, the model itself will have no bounds
                // and will always be positioned at (0, 0, 0), even if the root entity has translation.
                // This fixes it by translating the model bounds to match its transform.
                bounds.AddPoint(worldMatrix.ExtractTranslation());
            }
            Bounds3D = bounds;
        }

        public override void FixConnections(bool? bake = null, Matrix4[] tempJointMatrices = null)
        {
            if (!bake.HasValue)
            {
                bake = !Renderer.Scene.JointsSupported;
            }
            if (bake.Value && HasAttached && tempJointMatrices == null)
            {
                tempJointMatrices = GetRootEntity().RelativeAnimatedJointMatrices;
            }
            base.FixConnections(bake, tempJointMatrices);
            if (!HasAttached)
            {
                return;
            }

            if (!bake.Value && IsAttached && IsAttachedBaked)
            {
                UnfixConnections(); // Restore baked connections to their unbaked form
            }
            else if (bake.Value)
            {
                var invTempWorldMatrix = TempWorldMatrix.Inverted();
                foreach (var triangle in Triangles)
                {
                    // If we have cached connections, then use those. It'll make things much faster.
                    if (triangle.VertexJoints != null)
                    {
                        // Backup original values
                        if (triangle.OriginalVertices == null)
                        {
                            triangle.OriginalVertices = (Vector3[])triangle.Vertices.Clone();
                        }

                        for (var i = 0; i < 3; i++)
                        {
                            var jointID = triangle.VertexJoints[i];
                            if (jointID != Triangle.NoJoint)
                            {
                                Vector3.TransformPosition(ref triangle.OriginalVertices[i], ref tempJointMatrices[jointID], out var vertex);
                                Vector3.TransformPosition(ref vertex, ref invTempWorldMatrix, out triangle.Vertices[i]);
                            }
                        }
                    }

                    if (triangle.NormalJoints != null)
                    {
                        // Backup original values
                        if (triangle.OriginalNormals == null)
                        {
                            triangle.OriginalNormals = (Vector3[])triangle.Normals.Clone();
                        }

                        for (var i = 0; i < 3; i++)
                        {
                            var jointID = triangle.NormalJoints[i];
                            if (jointID != Triangle.NoJoint)
                            {
                                // todo: Is the first normalize needed for if the first scale is non-uniform?
                                GeomMath.TransformNormalNormalized(ref triangle.OriginalNormals[i], ref tempJointMatrices[jointID], out var normal);
                                GeomMath.TransformNormalNormalized(ref normal, ref invTempWorldMatrix, out triangle.Normals[i]);
                            }
                        }
                    }
                }
            }
            IsAttached = true;
            IsAttachedBaked = bake.Value;
        }

        public override void UnfixConnections()
        {
            base.UnfixConnections();
            if (HasAttached && IsAttached && IsAttachedBaked)
            {
                foreach (var triangle in Triangles)
                {
                    if (triangle.VertexJoints != null && triangle.OriginalVertices != null)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            triangle.Vertices[i] = triangle.OriginalVertices[i];
                        }
                    }

                    if (triangle.NormalJoints != null && triangle.OriginalNormals != null)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            triangle.Normals[i] = triangle.OriginalNormals[i];
                        }
                    }
                }
            }
            IsAttached = false;
            IsAttachedBaked = false;
        }
    }
}
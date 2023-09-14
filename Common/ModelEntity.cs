using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Common
{
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
        private Triangle[] _triangles;

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

        // This model contains at least one attached vertex.
        [Browsable(false)]
        public bool HasAttached { get; set; }

        // This model only contains attached vertices, and should not be used in bounds calculation.
        [Browsable(false)]
        public bool AttachedOnly { get; set; }

        // HMD: Attachable (shared) geometry can only be used when attachable.SharedID <= attached.SharedID.
        [Browsable(false)]
        public uint SharedID { get; set; }

        // HMD: Attachable (shared) vertices and normals that aren't tied to an existing triangle.
        [Browsable(false)]
        public Dictionary<uint, Vector3> AttachableVertices { get; set; }
        [Browsable(false)]
        public Dictionary<uint, Vector3> AttachableNormals { get; set; }


        public ModelEntity()
        {
        }

        public ModelEntity(ModelEntity fromModel, Triangle[] triangles, bool deepClone = false)
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
            TextureAnimation = fromModel.TextureAnimation;
            IsAttached = fromModel.IsAttached;
            HasAttached = fromModel.HasAttached;
            AttachedOnly = fromModel.AttachedOnly;
            SharedID = fromModel.SharedID;
            if (!deepClone)
            {
                AttachableVertices = fromModel.AttachableVertices;
                AttachableNormals = fromModel.AttachableNormals;
            }
            else
            {
                AttachableVertices = new Dictionary<uint, Vector3>(fromModel.AttachableVertices);
                AttachableNormals = new Dictionary<uint, Vector3>(fromModel.AttachableNormals);
            }
        }


        public override string ToString()
        {
            var name = EntityName ?? GetType().Name;
            var page = IsTextured ? TexturePage.ToString() : "null";
            return $"{name} Triangles={TrianglesCount} TexturePage={page}";
        }

        public void ComputeAttached()
        {
            var hasAttached = false;
            var attachedOnly = (Triangles.Length > 0);
            foreach (var triangle in Triangles)
            {
                if (triangle.AttachedIndices != null)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        if (triangle.AttachedIndices[i] == Triangle.NoAttachment)
                        {
                            attachedOnly = false;
                        }
                        else
                        {
                            hasAttached = true;
                        }
                    }
                }
                else
                {
                    attachedOnly = false;
                }

                if (hasAttached && !attachedOnly)
                {
                    break; // Nothing more to compute
                }
            }
            HasAttached = hasAttached;
            AttachedOnly = attachedOnly;
        }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            var worldMatrix = WorldMatrix;
            foreach (var triangle in Triangles)
            {
                if (triangle.Vertices != null)
                {
                    for (var i = 0; i < triangle.Vertices.Length; i++)
                    {
                        if (!IsAttached && triangle.AttachedIndices != null)
                        {
                            if (triangle.AttachedIndices[i] != Triangle.NoAttachment)
                            {
                                continue;
                            }
                        }
                        Vector3.TransformPosition(ref triangle.Vertices[i], ref worldMatrix, out var vertex);
                        bounds.AddPoint(vertex);
                    }

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

        private Vector3 ConnectVertex(EntityBase subModel, Vector3 vertex, bool transform = true)
        {
            // We only need to transform the vertex if it's not attached to the same model.
            if (subModel != this && transform)
            {
                vertex = Vector3.TransformPosition(vertex, subModel.TempWorldMatrix);
                vertex = Vector3.TransformPosition(vertex, TempWorldMatrix.Inverted());
            }
            return vertex;
        }

        private Vector3 ConnectNormal(EntityBase subModel, Vector3 normal, bool transform = true)
        {
            // We only need to transform the vertex if it's not attached to the same model.
            if (subModel != this && transform)
            {
                // todo: Is the first normalize needed for if the first scale is non-uniform?
                normal = GeomMath.TransformNormalNormalized(normal, subModel.TempWorldMatrix);
                normal = GeomMath.TransformNormalNormalized(normal, TempWorldMatrix.Inverted());
            }
            return normal;
        }

        public override void FixConnections(bool transform = true)
        {
            base.FixConnections(transform);
            if (!HasAttached)
            {
                return;
            }
            IsAttached = true;
            var rootEntity = GetRootEntity();
            if (rootEntity != null)
            {
                foreach (var triangle in Triangles)
                {
                    // If we have cached connections, then use those. It'll make things much faster.
                    if (triangle.AttachedVerticesCache != null)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            var vertexCache = triangle.AttachedVerticesCache[i];
                            var normalCache = triangle.AttachedNormalsCache?[i];
                            if (vertexCache != null)
                            {
                                triangle.Vertices[i] = ConnectVertex(vertexCache.Item1, vertexCache.Item2, transform);
                            }
                            if (normalCache != null)
                            {
                                triangle.Normals[i] = ConnectNormal(normalCache.Item1, normalCache.Item2, transform);
                            }
                        }
                        continue;
                    }

                    // AttachedNormalIndices should only ever be non-null when AttachedIndices is non-null.
                    if (triangle.AttachedIndices == null)
                    {
                        continue;
                    }
                    for (var i = 0; i < 3; i++)
                    {
                        var attachedIndex = triangle.AttachedIndices[i];
                        var attachedNormalIndex = triangle.AttachedNormalIndices?[i] ?? Triangle.NoAttachment;
                        if (attachedIndex != Triangle.NoAttachment)
                        {
                            // In the event that some attached indices are not found,
                            // we don't want to waste time looking for them again. Create an attached cache now.
                            if (triangle.AttachedVerticesCache == null)
                            {
                                triangle.AttachedVerticesCache = new Tuple<EntityBase, Vector3>[3];
                            }
                            foreach (ModelEntity subModel in rootEntity.ChildEntities)
                            {
                                if (subModel != this)
                                {
                                    foreach (var subTriangle in subModel.Triangles)
                                    {
                                        for (var j = 0; j < subTriangle.Vertices.Length; j++)
                                        {
                                            if (subTriangle.AttachableIndices[j] == attachedIndex)
                                            {
                                                var attachedVertex = subTriangle.Vertices[j];
                                                // Cache connection to speed up FixConnections in the future.
                                                triangle.AttachedVerticesCache[i] = new Tuple<EntityBase, Vector3>(subModel, attachedVertex);
                                                triangle.Vertices[i] = ConnectVertex(subModel, attachedVertex, transform);
                                                break;
                                            }
                                        }
                                    }
                                }

                                // HMD: Check for attachable (shared) vertices and normals that aren't associated with an existing triangle.
                                // Shared geometry can only be attached to shared indices defined before it.
                                if (subModel.SharedID <= SharedID)
                                {
                                    if (subModel.AttachableVertices != null && subModel.AttachableVertices.TryGetValue(attachedIndex, out var attachedVertex))
                                    {
                                        // Cache connection to speed up FixConnections in the future.
                                        triangle.AttachedVerticesCache[i] = new Tuple<EntityBase, Vector3>(subModel, attachedVertex);
                                        triangle.Vertices[i] = ConnectVertex(subModel, attachedVertex, transform);
                                    }
                                    if (subModel.AttachableNormals != null && subModel.AttachableNormals.TryGetValue(attachedNormalIndex, out var attachedNormal))
                                    {
                                        if (triangle.AttachedNormalsCache == null)
                                        {
                                            triangle.AttachedNormalsCache = new Tuple<EntityBase, Vector3>[3];
                                        }
                                        triangle.AttachedNormalsCache[i] = new Tuple<EntityBase, Vector3>(subModel, attachedNormal);
                                        triangle.Normals[i] = ConnectNormal(subModel, attachedNormal, transform);
                                    }
                                    // Note: DON'T break when we find a shared attachable. Later-defined attachables have priority.
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void UnfixConnections()
        {
            base.UnfixConnections();
            IsAttached = false;
        }

        public override void ClearConnectionsCache()
        {
            base.ClearConnectionsCache();
            foreach (var triangle in Triangles)
            {
                triangle.AttachedVerticesCache = null;
                triangle.AttachedNormalsCache = null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Common
{
    public class ModelEntity : EntityBase
    {
        private Triangle[] _triangles;

        [DisplayName("VRAM Page")]
        public uint TexturePage { get; set; }

        // Use default flags for when a reader doesn't assign any.
        [DisplayName("Render Flags")]
        public RenderFlags RenderFlags { get; set; } = RenderFlags.DoubleSided | RenderFlags.Textured;

        [DisplayName("Mixture Rate")]
        public MixtureRate MixtureRate { get; set; }

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
        
        [DisplayName("TMD ID")]
        public uint TMDID { get; set; }

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

        [Browsable(false)]
        public bool NeedsTiled
        {
            get
            {
                foreach (var triangle in Triangles)
                {
                    if (triangle.NeedsTiled)
                        return true;
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

        [Browsable(false)]
        public int MeshIndex { get; set; }

        public bool Visible { get; set; } = true;

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

        public ModelEntity(ModelEntity fromModel, Triangle[] triangles)
            : base(fromModel)
        {
            Triangles = triangles;
            TexturePage = fromModel.TexturePage;
            RenderFlags = fromModel.RenderFlags;
            MixtureRate = fromModel.MixtureRate;
            Texture = fromModel.Texture;
            TMDID = fromModel.TMDID;
            Visible = fromModel.Visible;
            SharedID = fromModel.SharedID;
            AttachableVertices = fromModel.AttachableVertices;
            AttachableNormals = fromModel.AttachableNormals;
            //AttachableVertices = new Dictionary<uint, Vector3>(fromModel.AttachableVertices);
            //AttachableNormals = new Dictionary<uint, Vector3>(fromModel.AttachableNormals);
        }


        public override string ToString()
        {
            var name = EntityName ?? GetType().Name;
            var page = IsTextured ? TexturePage.ToString() : "null";
            return $"{name} Triangles={TrianglesCount} TexturePage={page}";
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
                        if (triangle.AttachedIndices != null)
                        {
                            if (triangle.AttachedIndices[i] != Triangle.NoAttachment)
                            {
                                continue;
                            }
                        }
                        var vertex = triangle.Vertices[i];
                        bounds.AddPoint(Vector3.TransformPosition(vertex, worldMatrix));
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

        private Vector3 ConnectVertex(EntityBase subModel, Vector3 vertex)
        {
            // We only need to transform the vertex if it's not attached to the same model.
            if (subModel != this)
            {
                vertex = Vector3.TransformPosition(vertex, subModel.TempWorldMatrix);
                vertex = Vector3.TransformPosition(vertex, TempWorldMatrix.Inverted());
            }
            return vertex;
        }

        public override void FixConnections()
        {
            base.FixConnections();
            var rootEntity = GetRootEntity();
            if (rootEntity != null)
            {
                foreach (var triangle in Triangles)
                {
                    // If we have cached connections, then use those. It'll make things much faster.
                    if (triangle.AttachedCache != null)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            var cache = triangle.AttachedCache[i];
                            if (cache != null)
                            {
                                triangle.Vertices[i] = ConnectVertex(cache.Item1, cache.Item2);
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
                            if (triangle.AttachedCache == null)
                            {
                                triangle.AttachedCache = new Tuple<EntityBase, Vector3>[3];
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
                                                triangle.AttachedCache[i] = new Tuple<EntityBase, Vector3>(subModel, attachedVertex);
                                                triangle.Vertices[i] = ConnectVertex(subModel, attachedVertex);
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
                                        triangle.AttachedCache[i] = new Tuple<EntityBase, Vector3>(subModel, attachedVertex);
                                        triangle.Vertices[i] = ConnectVertex(subModel, attachedVertex);
                                    }
                                    if (subModel.AttachableNormals != null && subModel.AttachableNormals.TryGetValue(attachedNormalIndex, out var attachedNormal))
                                    {
                                        triangle.Normals[i] = attachedNormal;
                                    }
                                    // Note: DON'T break when we find a shared attachable. Later-defined attachables have priority.
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void ClearConnectionsCache()
        {
            base.ClearConnectionsCache();
            foreach (var triangle in Triangles)
            {
                triangle.AttachedCache = null;
            }
        }
    }
}
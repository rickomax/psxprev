using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Classes
{
    public class ModelEntity : EntityBase
    {
        // Default flags for when a reader doesn't assign any.
        public const RenderFlags DefaultRenderFlags = RenderFlags.DoubleSided;


        private Triangle[] _triangles;

        [DisplayName("VRAM Page")]
        public uint TexturePage { get; set; }
        
        [DisplayName("Render Flags")]
        public RenderFlags RenderFlags { get; set; } = DefaultRenderFlags;
        
        [DisplayName("Mixture Rate")]
        public MixtureRate MixtureRate { get; set; }

        [ReadOnly(true), DisplayName("Total Triangles")]
        public int TrianglesCount => Triangles.Length;

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

        [ReadOnly(true), DisplayName("Has Tiled Texture")]
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

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            var worldMatrix = WorldMatrix;
            var hasBounds = false;
            foreach (var triangle in Triangles)
            {
                if (triangle.Vertices != null)
                {
                    for (var i = 0; i < triangle.Vertices.Length; i++)
                    {
                        if (triangle.AttachedIndices != null)
                        {
                            if (triangle.AttachedIndices[i] != uint.MaxValue)
                            {
                                continue;
                            }
                        }
                        var vertex = triangle.Vertices[i];
                        bounds.AddPoint(Vector3.TransformPosition(vertex, worldMatrix));
                        hasBounds = true;
                    }

                }
            }
            if (!hasBounds)
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
                    if (triangle.AttachedCache != null && false)
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
                        var attachedNormalIndex = triangle.AttachedNormalIndices?[i] ?? uint.MaxValue;
                        if (attachedIndex != uint.MaxValue)
                        {
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
                                                if (triangle.AttachedCache == null)
                                                {
                                                    triangle.AttachedCache = new Tuple<EntityBase, Vector3>[3];
                                                }
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
                                        if (triangle.AttachedCache == null)
                                        {
                                            triangle.AttachedCache = new Tuple<EntityBase, Vector3>[3];
                                        }
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
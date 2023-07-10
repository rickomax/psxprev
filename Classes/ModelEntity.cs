using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Classes
{
    public class ModelEntity : EntityBase
    {
        // Default flags for when a reader doesn't assign any.
        public const RenderFlags DefaultRenderFlags = RenderFlags.DoubleSided;

        [DisplayName("VRAM Page")]
        public uint TexturePage { get; set; }
        
        [DisplayName("Render Flags")]
        public RenderFlags RenderFlags { get; set; } = DefaultRenderFlags;
        
        [DisplayName("Mixture Rate")]
        public MixtureRate MixtureRate { get; set; }

        [ReadOnly(true), DisplayName("Total Triangles")]
        public int TrianglesCount => Triangles.Length;

        [Browsable(false)]
        public Triangle[] Triangles { get; set; }

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

        public float Interpolator { get; set; }
        public Vector3[] InitialVertices { get; set; }
        public Vector3[] FinalVertices { get; set; }
        public Vector3[] FinalNormals { get; set; }
        public Vector3[] InitialNormals { get; set; }

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
            bool hasBounds = false;
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

        public override void FixConnections()
        {
            var rootEntity = GetRootEntity();
            if (rootEntity != null)
            {
                foreach (var triangle in Triangles)
                {
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
                                if (subModel == this)
                                {
                                    continue;
                                }
                                foreach (var subTriangle in subModel.Triangles)
                                {
                                    for (var j = 0; j < subTriangle.Vertices.Length; j++)
                                    {
                                        if (subTriangle.AttachableIndices[j] == attachedIndex)
                                        {
                                            var newVertex = Vector3.TransformPosition(subTriangle.Vertices[j], subModel.TempWorldMatrix);
                                            newVertex = Vector3.TransformPosition(newVertex, TempWorldMatrix.Inverted());
                                            triangle.Vertices[i] = newVertex;
                                            break;
                                        }
                                    }
                                }

                                // HMD: Check for attachable vertices and normals that aren't associated with an existing triangle.
                                if (subModel.AttachableVertices != null && subModel.AttachableVertices.TryGetValue(attachedIndex, out var attachedVertex))
                                {
                                    var newVertex = Vector3.TransformPosition(attachedVertex, subModel.TempWorldMatrix);
                                    // WorldMatrix.Inverted() here prevents transforms for this model from being
                                    // applied, which they shouldn't be since attached vertices should only transform
                                    // based on their attached model.
                                    newVertex = Vector3.TransformPosition(newVertex, TempWorldMatrix.Inverted());
                                    triangle.Vertices[i] = newVertex;
                                }
                                if (subModel.AttachableNormals != null && subModel.AttachableNormals.TryGetValue(attachedNormalIndex, out var attachedNormal))
                                {
                                    triangle.Normals[i] = attachedNormal;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
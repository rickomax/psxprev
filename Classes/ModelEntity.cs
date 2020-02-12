using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Classes
{
    public class ModelEntity : EntityBase
    {
        [DisplayName("VRAM Page")]
        public uint TexturePage { get; set; }
        
        [ReadOnly(true), DisplayName("Total Triangles")]
        public int TrianglesCount => Triangles.Length;

        [Browsable(false)]
        public Triangle[] Triangles { get; set; }

        [Browsable(false)]
        public Texture Texture { get; set; }
        
        [DisplayName("TMD ID")]
        public uint TMDID { get; set; }

       //[ReadOnly(true)]
       //public uint PrimitiveIndex { get; set; }

        [Browsable(false)]
        public int MeshIndex { get; set; }

        public bool Visible { get; set; } = true;

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
                            if (triangle.AttachedIndices[i] != uint.MaxValue)
                            {
                                continue;
                            }
                        }
                        var vertex = triangle.Vertices[i];
                        bounds.AddPoint(Vector3.TransformPosition(vertex, worldMatrix));
                    }

                }
            }
            Bounds3D = bounds;
        }

        public override void FixConnectionsRecursively()
        {
            var rootEntity = ParentEntity as RootEntity;
            if (rootEntity != null)
            {
                foreach (var triangle in Triangles)
                {
                    if (triangle.AttachedIndices == null)
                    {
                        continue;
                    }
                    for (var i = 0; i < 3; i++)
                    {
                        var attachedIndex = triangle.AttachedIndices[i];
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
                                            var newVertex = Vector3.TransformPosition(subTriangle.Vertices[j], subModel.WorldMatrix);
                                            newVertex = Vector3.TransformPosition(newVertex, WorldMatrix.Inverted());
                                            triangle.Vertices[i] = newVertex;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            base.FixConnectionsRecursively();
        }
    }
}
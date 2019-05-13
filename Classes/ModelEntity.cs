using System.ComponentModel;
using OpenTK;


namespace PSXPrev
{
    public class ModelEntity : EntityBase
    {
        [DisplayName("VRAM Page")]
        public int TexturePage { get; set; }

        [ReadOnly(true), DisplayName("Normals")]
        public bool HasNormals { get; set; }

        [ReadOnly(true), DisplayName("Colors")]
        public bool HasColors { get; set; }

        [ReadOnly(true), DisplayName("Uvs")]
        public bool HasUvs { get; set; }

        [ReadOnly(true), DisplayName("Relative Addresses")]
        public bool RelativeAddresses { get; set; }

        [ReadOnly(true), DisplayName("Total Triangles")]
        public int TrianglesCount { get => Triangles.Length; }

        [Browsable(false)]
        public Triangle[] Triangles { get; set; }

        [Browsable(false)]
        public Texture Texture { get; set; }
        
        [DisplayName("TMD ID")]
        public int TMDID { get; set; }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            var worldMatrix = WorldMatrix;
            foreach (var triangle in Triangles)
            {
                foreach (var vertex in triangle.Vertices)
                {
                    bounds.AddPoint(Vector3.TransformPosition(vertex, worldMatrix));
                }
            }
            Bounds3D = bounds;
        }
    }
}
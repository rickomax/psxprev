using System.ComponentModel;
using OpenTK;


namespace PSXPrev
{
    public class ModelEntity : EntityBase
    {
        [DisplayName("VRAM Page")]
        public int TexturePage { get; set; }
        
        [DisplayName("Visible")]
        public bool Visible { get; set; }

        [ReadOnly(true), DisplayName("Normals")]
        public bool HasNormals { get; set; }

        [ReadOnly(true), DisplayName("Colors")]
        public bool HasColors { get; set; }

        [ReadOnly(true), DisplayName("Uvs")]
        public bool HasUvs { get; set; }

        [ReadOnly(true)]
        public bool RelativeAddresses { get; set; }

        [ReadOnly(true), DisplayName("Total Triangles")]
        public int TrianglesCount { get => Triangles.Length; }

        [Browsable(false)]
        public Triangle[] Triangles { get; set; }

        //[Browsable(false)]
        //public MissingTriangle[] MissingTriangles { get; set; }

        [Browsable(false)]
        public Texture Texture { get; set; }

         [Browsable(false)]
        public Matrix4 WorldMatrix { get; set; }

        public ModelEntity()
        {
            Visible = true;
            WorldMatrix = Matrix4.Identity;
        }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            foreach (var triangle in Triangles)
            {
                foreach (var vertex in triangle.Vertices)
                {
                    bounds.AddPoint((WorldMatrix * new Vector4(vertex)).Xyz);
                }
            }
            Bounds3D = bounds;
        }
    }
}
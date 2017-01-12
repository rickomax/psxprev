using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;
using PSXPrev.Classes.Mesh;

namespace PSXPrev.Classes.Entities
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

        [Browsable(false)]
        public List<Triangle> Triangles { get; set; }

        [Browsable(false)]
        public Texture.Texture Texture { get; set; }

        [Browsable(false)]
        public Matrix4 WorldMatrix { get; set; }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox.BoundingBox();
            foreach (var triangle in Triangles)
            {
                foreach (var vertex in triangle.Vertices)
                {
                    bounds.AddPoint(vertex);
                }
            }
            Bounds3D = bounds;
        }
    }
}
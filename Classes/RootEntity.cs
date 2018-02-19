using System.ComponentModel;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class RootEntity : EntityBase
    {
        [DisplayName("Name")]
        public string EntityName { get; set; }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            foreach (var entity in ChildEntities)
            {
                var corners = entity.Bounds3D.Corners;
                foreach (var corner in corners)
                {
                    bounds.AddPoint(corner);
                }
            }
            Bounds3D = bounds;
        }

        public override string ToString()
        {
            return EntityName;
        }
    }
}
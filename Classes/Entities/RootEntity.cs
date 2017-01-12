using System.ComponentModel;

namespace PSXPrev.Classes.Entities
{
    public class RootEntity : EntityBase
    {
        [DisplayName("Name")]
        public string EntityName { get; set; }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox.BoundingBox();
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
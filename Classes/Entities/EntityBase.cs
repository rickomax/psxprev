using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace PSXPrev.Classes.Entities
{
    public class EntityBase
    {
        [ReadOnly(true), DisplayName("Bounds")]
        public BoundingBox.BoundingBox Bounds3D { get; protected set; }
     
        [ReadOnly(true), DisplayName("Sub-Models")]
        public string ChildCount
        {
            get
            {
                if (ChildEntities == null)
                {
                    return "0";
                }
                return ChildEntities.Count.ToString(CultureInfo.InvariantCulture);
            }
        }

        [Browsable(false)]
        public List<ModelEntity> ChildEntities { get; set; }

        [Browsable(false)]
        public EntityBase ParentEntity { get; protected set; }

        public virtual void ComputeBounds()
        {
            if (ChildEntities == null)
            {
                return;
            }
            foreach (var entity in ChildEntities)
            {
                entity.ComputeBounds();
            }
        }
    }
}

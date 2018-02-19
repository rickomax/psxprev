using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace PSXPrev.Classes
{
    public class EntityBase
    {
        [ReadOnly(true), DisplayName("Bounds")]
        public BoundingBox Bounds3D { get; protected set; }
     
        [ReadOnly(true), DisplayName("Sub-Models")]
        public string ChildCount => ChildEntities == null ? "0" : ChildEntities.Length.ToString(CultureInfo.InvariantCulture);

        [Browsable(false)]
        public EntityBase[] ChildEntities { get; set; }

        //[Browsable(false)]
        //public EntityBase ParentEntity { get; protected set; }

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

        //public override string ToString()
        //{
        //    return EntityName;
        //}
    }
}

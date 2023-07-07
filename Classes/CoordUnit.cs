using OpenTK;

namespace PSXPrev.Classes
{
    // Hierarchical coordinates for use with HMD models.
    // This is needed so that models can be treated as nested,
    // even when they're stored in a flat list of the root entity.
    public class CoordUnit
    {
        public const uint NoID = uint.MaxValue;

        private Matrix4 _originalLocalMatrix;
        private Vector3 _originalTranslation;
        private Vector3 _originalRotation;
        
        public Matrix4 LocalMatrix { get; set; }
        public Matrix4 OriginalLocalMatrix
        {
            get => _originalLocalMatrix;
            set => LocalMatrix = _originalLocalMatrix = value;
        }

        public Vector3 Translation { get; set; }
        public Vector3 OriginalTranslation
        {
            get => _originalTranslation;
            set => Translation = _originalTranslation = value;
        }

        public Vector3 Rotation { get; set; }
        public Vector3 OriginalRotation
        {
            get => _originalRotation;
            set => Rotation = _originalRotation = value;
        }

        public RotationOrder RotationOrder { get; set; }
        public RotationOrder OriginalRotationOrder => RotationOrder.YXZ; // Observed in Gods98 (PSX) source.
        
        public uint ID { get; set; } = NoID;
        public uint ParentID { get; set; } = NoID;

        public CoordUnit[] Coords { get; set; }

        public uint TMDID => ID + 1;

        public bool HasParent => ParentID != NoID && ParentID != ID;
        public CoordUnit Parent => (HasParent ? Coords?[ParentID] : null);
        
        public Matrix4 WorldMatrix
        {
            get
            {
                var matrix = Matrix4.Identity;
                var coord = this;
                do
                {
                    matrix *= coord.LocalMatrix;
                    coord = coord.Parent;
                } while (coord != null);
                return matrix;
            }
        }

        public CoordUnit()
        {
            OriginalLocalMatrix = Matrix4.Identity;
            // Everything below is unused at the moment:
            OriginalTranslation = Vector3.Zero;
            OriginalRotation = Vector3.Zero;
            RotationOrder = OriginalRotationOrder;
        }

        public void ResetTransform()
        {
            LocalMatrix = OriginalLocalMatrix;
            // Everything below is unused at the moment:
            Translation = OriginalTranslation;
            Rotation = OriginalRotation;
            RotationOrder = OriginalRotationOrder;
        }

        // Check to see if the coord unit has parents that eventually reference themselves.
        public bool HasCircularReference()
        {
            if (ParentID == ID)
            {
                return true; // Parent is self. We can check this even without the coords table.
            }
            else if (Coords != null)
            {
                var coord = this;
                for (var depth = 0; depth < Coords.Length; depth++)
                {
                    coord = coord.Parent;
                    if (coord == null)
                    {
                        return false; // End of parents.
                    }
                }
                return true; // More parents than number of coords means infinite recursion.
            }
            return false; // Can't test because there's no coords table.
        }
    }
}

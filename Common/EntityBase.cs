using System.ComponentModel;
using System.Globalization;
using OpenTK;

namespace PSXPrev.Common
{
    public class EntityBase
    {
        private EntityBase[] _childEntities;
        private Matrix4 _originalLocalMatrix;
        private Matrix4 _localMatrix;
        private Vector3 _translation;
        private Quaternion _rotation;
        private Vector3 _scale;

        [DisplayName("Name")]
        public string EntityName { get; set; }
        
        [ReadOnly(true), DisplayName("Bounds")]
        public BoundingBox Bounds3D { get; set; }

        // Store original transform so that gizmo translations can be reset by the user.
        // This also assigns the current transform (LocalMatrix).
        [Browsable(false)]
        public Matrix4 OriginalLocalMatrix
        {
            get => _originalLocalMatrix;
            set => LocalMatrix = _originalLocalMatrix = value;
        }

        [Browsable(false)]
        public Matrix4 LocalMatrix
        {
            get => _localMatrix;
            set
            {
                _translation = value.ExtractTranslation();
                _rotation = value.ExtractRotation();
                _scale = value.ExtractScale();
                _localMatrix = value;
            }
        }

        // Store each component of LocalMatrix so that changes to one won't minimally affect the other components.
        [Browsable(false)]
        public Vector3 Translation
        {
            get => _translation;
            set => ApplyTransform(value, null, null);
        }
        
        [Browsable(false)]
        public Quaternion Rotation
        {
            get => _rotation;
            set => ApplyTransform(null, value, null);
        }

        [Browsable(false)]
        public Vector3 Scale
        {
            get => _scale;
            set => ApplyTransform(null, null, value);
        }

        [Browsable(false)]
        public Matrix4 WorldMatrix
        {
            get
            {
                var matrix = Matrix4.Identity;
                var entity = this;
                do
                {
                    matrix *= entity.LocalMatrix;
                    entity = entity.ParentEntity;
                } while (entity != null);
                return matrix;
            }
        }

        [Browsable(false)]
        public Matrix4 OriginalWorldMatrix
        {
            get
            {
                var matrix = Matrix4.Identity;
                var entity = this;
                do
                {
                    matrix *= entity.OriginalLocalMatrix;
                    entity = entity.ParentEntity;
                } while (entity != null);
                return matrix;
            }
        }

        [DisplayName("Position X")]
        public float PositionX
        {
            get => Translation.X;
            set => Translation = new Vector3(value, Translation.Y, Translation.Z);
        }

        [DisplayName("Position Y")]
        public float PositionY
        {
            get => Translation.Y;
            set => Translation = new Vector3(Translation.X, value, Translation.Z);
        }

        [DisplayName("Position Z")]
        public float PositionZ
        {
            get => Translation.Z;
            set => Translation = new Vector3(Translation.X, Translation.Y, value);
        }

        [DisplayName("Scale X")]
        public float ScaleX
        {
            get => Scale.X;
            set => Scale = new Vector3(value, Scale.Y, Scale.Z);
        }

        [DisplayName("Scale Y")]
        public float ScaleY
        {
            get => Scale.Y;
            set => Scale = new Vector3(Scale.X, value, Scale.Z);
        }

        [DisplayName("Scale Z")]
        public float ScaleZ
        {
            get => Scale.Z;
            set => Scale = new Vector3(Scale.X, Scale.Y, value);
        }

        [ReadOnly(true), DisplayName("Sub-Models")]
        public string ChildCount => ChildEntities == null ? "0" : ChildEntities.Length.ToString(CultureInfo.InvariantCulture);

        [Browsable(false)]
        public EntityBase[] ChildEntities
        {
            get => _childEntities;
            set
            {
                for (var i = 0; i < value.Length; i++)
                {
                    value[i].EntityName = "Sub-Model " + i;
                }
                _childEntities = value;
            }
        }

        [Browsable(false)]
        public EntityBase ParentEntity { get; set; }

        [Browsable(false)]
        public float IntersectionDistance { get; set; }

        [Browsable(false)]
        public Matrix4 TempMatrix { get; set; } = Matrix4.Identity;


        // todo: fill these in the HMD parsing process
        [Browsable(false)]
        public Matrix4 TempLocalMatrix { get; set; } = Matrix4.Identity;

        [Browsable(false)]
        public Matrix4 TempWorldMatrix => TempMatrix * WorldMatrix;


        protected EntityBase()
        {
            // Also assigns LocalMatrix, Translation, Rotation, and Scale.
            OriginalLocalMatrix = Matrix4.Identity;
        }

        protected EntityBase(EntityBase fromEntity)
        {
            EntityName = fromEntity.EntityName;
            ParentEntity = fromEntity.ParentEntity;
            Bounds3D = fromEntity.Bounds3D;
            TempMatrix = fromEntity.TempMatrix;
            _originalLocalMatrix = fromEntity._originalLocalMatrix;
            _localMatrix = fromEntity._localMatrix;
            _translation = fromEntity._translation;
            _scale = fromEntity._scale;
            _rotation = fromEntity._rotation;
        }


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

        public void ComputeBoundsRecursively()
        {
            var entity = this;
            do
            {
                entity.ComputeBounds();
                entity = entity.ParentEntity;
            } while (entity != null);
        }

        // Reset this and optionally all children's LocalMatrix to OriginalLocalMatrix.
        public void ResetTransform(bool resetChildren)
        {
            LocalMatrix = OriginalLocalMatrix;
            if (resetChildren && ChildEntities != null)
            {
                foreach (var child in ChildEntities)
                {
                    child.ResetTransform(resetChildren);
                }
            }
        }

        // Apply different transform components to LocalMatrix.
        // Null values will use the current transform component for that value.
        private void ApplyTransform(Vector3? translationValues, Quaternion? rotationValues, Vector3? scaleValues)
        {
            if (!translationValues.HasValue)
            {
                translationValues = Translation;
            }
            if (!rotationValues.HasValue)
            {
                rotationValues = Rotation;
            }
            if (!scaleValues.HasValue)
            {
                scaleValues = Scale;
            }
            _translation = translationValues.Value;
            _rotation = rotationValues.Value;
            _scale = scaleValues.Value;
            
            var translation = Matrix4.CreateTranslation(translationValues.Value);
            var rotation = Matrix4.CreateFromQuaternion(rotationValues.Value);
            var scale = Matrix4.CreateScale(scaleValues.Value);
            _localMatrix = scale * rotation * translation;
        }

        public override string ToString()
        {
            var name = EntityName ?? GetType().Name;
            return $"{name} Children={ChildCount}";
        }

        public virtual void FixConnections()
        {
            if (ChildEntities == null)
            {
                return;
            }
            foreach (var child in ChildEntities)
            {
                child.FixConnections();
            }
        }

        public virtual void ClearConnectionsCache()
        {
            if (ChildEntities == null)
            {
                return;
            }
            foreach (var child in ChildEntities)
            {
                child.ClearConnectionsCache();
            }
        }

        public RootEntity GetRootEntity()
        {
            var entity = this;
            while (entity.ParentEntity != null)
            {
                entity = entity.ParentEntity;
            }
            return entity as RootEntity;
        }

        public void ResetAnimationData()
        {
            if (this is ModelEntity modelEntity)
            {
                modelEntity.Interpolator = 0;
                modelEntity.InitialVertices = null;
                modelEntity.FinalVertices = null;
                modelEntity.InitialNormals = null;
                modelEntity.FinalNormals = null;
            }
            if (this is RootEntity rootEntity)
            {
                if (rootEntity.Coords != null)
                {
                    foreach (var coord in rootEntity.Coords)
                    {
                        coord.ResetTransform();
                    }
                }
            }
            TempMatrix = Matrix4.Identity;
            if (ChildEntities != null)
            {
                foreach (var child in ChildEntities)
                {
                    child.ResetAnimationData();
                }
            }
        }
    }
}

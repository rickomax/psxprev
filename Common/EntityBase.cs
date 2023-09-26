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
        public string Name { get; set; }

#if DEBUG
        [DisplayName("Debug Data"), ReadOnly(true)]
#else
        [Browsable(false)]
#endif
        public string[] DebugData { get; set; }

        [DisplayName("Bounds"), ReadOnly(true)]
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
                _rotation = value.ExtractRotationSafe();
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
        public Vector3 WorldOrigin => WorldMatrix.ExtractTranslation();

        [Browsable(false)]
        public Matrix4 WorldMatrix
        {
            get
            {
                var matrix = _localMatrix;
                var entity = ParentEntity;
                while (entity != null)
                {
                    Matrix4.Mult(ref matrix, ref entity._localMatrix, out matrix);
                    entity = entity.ParentEntity;
                }
                return matrix;
            }
        }

        [Browsable(false)]
        public Matrix4 OriginalWorldMatrix
        {
            get
            {
                var matrix = _originalLocalMatrix;
                var entity = ParentEntity;
                while (entity != null)
                {
                    Matrix4.Mult(ref matrix, ref entity._originalLocalMatrix, out matrix);
                    entity = entity.ParentEntity;
                }
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

        [DisplayName("Rotation X")]
        public float RotationX
        {
            get => Rotation.X;
            set => Rotation = new Quaternion(value, Rotation.Y, Rotation.Z, Rotation.W);
        }

        [DisplayName("Rotation Y")]
        public float RotationY
        {
            get => Rotation.Y;
            set => Rotation = new Quaternion(Rotation.X, value, Rotation.Z, Rotation.W);
        }

        [DisplayName("Rotation Z")]
        public float RotationZ
        {
            get => Rotation.Z;
            set => Rotation = new Quaternion(Rotation.X, Rotation.Y, value, Rotation.W);
        }

        [DisplayName("Rotation W")]
        public float RotationW
        {
            get => Rotation.W;
            set => Rotation = new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DisplayName("Sub-Models"), ReadOnly(true)]
        public int PropertyGrid_ChildCount => ChildEntities?.Length ?? 0;

        [Browsable(false)]
        public EntityBase[] ChildEntities
        {
            get => _childEntities;
            set
            {
                for (var i = 0; i < value.Length; i++)
                {
                    value[i].Name = "Sub-Model " + i;
                    if (value[i] is ModelEntity model && !string.IsNullOrEmpty(model.MeshName))
                    {
                        model.Name += " " + model.MeshName;
                    }
                    value[i].ParentEntity = this;
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
            Name = fromEntity.Name;
            ParentEntity = fromEntity.ParentEntity;
            Bounds3D = fromEntity.Bounds3D;
            TempMatrix = fromEntity.TempMatrix;
            _originalLocalMatrix = fromEntity._originalLocalMatrix;
            _localMatrix = fromEntity._localMatrix;
            _translation = fromEntity._translation;
            _scale = fromEntity._scale;
            _rotation = fromEntity._rotation;
        }


        public virtual void ComputeBounds(AttachJointsMode attachJointsMode = AttachJointsMode.Hide, Matrix4[] jointMatrices = null)
        {
            if (ChildEntities == null)
            {
                return;
            }
            foreach (var entity in ChildEntities)
            {
                entity.ComputeBounds(attachJointsMode, jointMatrices);
            }
        }

        /*public void ComputeBoundsRecursively()
        {
            var jointMatrices = GetRootEntity().JointMatrices;
            var entity = this;
            do
            {
                // todo: Doesn't this cause child entities to compute bounds multiple times?
                entity.ComputeBounds(jointMatrices);
                entity = entity.ParentEntity;
            } while (entity != null);
        }*/

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
            var name = Name ?? GetType().Name;
            return $"{name} Children={ChildEntities?.Length ?? 0}";
        }

        public virtual void FixConnections(bool? bake = null, Matrix4[] tempJointMatrices = null)
        {
            if (ChildEntities == null)
            {
                return;
            }
            foreach (var child in ChildEntities)
            {
                child.FixConnections(bake, tempJointMatrices);
            }
        }

        public virtual void UnfixConnections()
        {
            if (ChildEntities == null)
            {
                return;
            }
            foreach (var child in ChildEntities)
            {
                child.UnfixConnections();
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
            TempLocalMatrix = Matrix4.Identity;
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

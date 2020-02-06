using OpenTK;
using System.ComponentModel;
using System.Globalization;

namespace PSXPrev
{
    public class EntityBase
    {
        private EntityBase[] _childEntities;

        [DisplayName("Name")]
        public string EntityName { get; set; }
        
        [ReadOnly(true), DisplayName("Bounds")]
        public BoundingBox Bounds3D { get; set; }

        [Browsable(false)]
        public Matrix4 LocalMatrix { get; set; }

        [Browsable(false)]
        public Matrix4 WorldMatrix
        {
            get
            {
                var matrix = Matrix4.Identity;
                var entity = this;
                do
                {
                    matrix = entity.LocalMatrix * matrix;
                    entity = entity.ParentEntity;
                } while (entity != null);
                return matrix;
            }
        }

        [DisplayName("Position X")]
        public float PositionX
        {
            get => LocalMatrix.ExtractTranslation().X;
            set
            {
                var translationValues = LocalMatrix.ExtractTranslation();
                translationValues.X = value;
                ApplyTranslation(translationValues);
            }
        }

        [DisplayName("Position Y")]
        public float PositionY
        {
            get => LocalMatrix.ExtractTranslation().Y;
            set
            {
                var translationValues = LocalMatrix.ExtractTranslation();
                translationValues.Y = value;
                ApplyTranslation(translationValues);
            }
        }

        [DisplayName("Position Z")]
        public float PositionZ
        {
            get => LocalMatrix.ExtractTranslation().Z;
            set
            {
                var translationValues = LocalMatrix.ExtractTranslation();
                translationValues.Z = value;
                ApplyTranslation(translationValues);
            }
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

        protected EntityBase()
        {
            LocalMatrix = Matrix4.Identity;
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

        private void ApplyTranslation(Vector3 translationValues)
        {
            var translation = Matrix4.CreateTranslation(translationValues);
            var rotation = Matrix4.CreateFromQuaternion(LocalMatrix.ExtractRotation());
            var scale = Matrix4.CreateScale(LocalMatrix.ExtractScale());
            LocalMatrix = translation * rotation * scale;
        }

        public override string ToString()
        {
            return EntityName;
        }
    }
}

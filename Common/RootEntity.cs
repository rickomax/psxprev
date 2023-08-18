using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OpenTK;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common
{
    public class RootEntity : EntityBase
    {
        private readonly List<ModelEntity> _groupedModels = new List<ModelEntity>();

        [Browsable(false)]
        public Coordinate[] Coords { get; set; }

        [DisplayName("Total Triangles"), ReadOnly(false)]
        public int TotalTriangles
        {
            get
            {
                var count = 0;
                if (ChildEntities != null)
                {
                    foreach (ModelEntity subModel in ChildEntities)
                    {
                        count += subModel.TrianglesCount;
                    }
                }
                return count;
            }
        }

        [Browsable(false)]
        public WeakReferenceCollection<Texture> OwnedTextures { get; } = new WeakReferenceCollection<Texture>();

        [Browsable(false)]
        public WeakReferenceCollection<Animation> OwnedAnimations { get; } = new WeakReferenceCollection<Animation>();


        public RootEntity()
        {
        }

        public RootEntity(RootEntity fromRootEntity)
            : base(fromRootEntity)
        {
            Coords = fromRootEntity.Coords;
        }


        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            foreach (var entity in ChildEntities)
            {
                // Not yet, there are some issues with this, like models that are only made of attached vertices.
                if (entity is ModelEntity model && (model.Triangles.Length == 0 || (model.AttachedOnly && !model.IsAttached)))
                {
                    continue; // Don't count empty models in bounds, since they'll always be at (0, 0, 0).
                }
                bounds.AddBounds(entity.Bounds3D);
            }
            if (!bounds.IsSet)
            {
                bounds.AddPoint(WorldMatrix.ExtractTranslation());
            }
            Bounds3D = bounds;
        }

        public List<ModelEntity> GetModelsWithTMDID(uint id)
        {
            _groupedModels.Clear();
            foreach (ModelEntity model in ChildEntities)
            {
                if (model.TMDID == id)
                {
                    _groupedModels.Add(model);
                }
            }
            return _groupedModels;
        }
    }
}
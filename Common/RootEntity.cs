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


        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            foreach (var entity in ChildEntities)
            {
                bounds.AddBounds(entity.Bounds3D);
            }
            Bounds3D = bounds;
        }

        public List<ModelEntity> GetModelsWithTMDID(uint id)
        {
            _groupedModels.Clear();
            foreach (var entityBase in ChildEntities)
            {
                var model = (ModelEntity) entityBase;
                if (model.TMDID == id)
                {
                    _groupedModels.Add(model);
                }
            }
            return _groupedModels;
        }
    }
}
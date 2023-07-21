using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OpenTK;

namespace PSXPrev.Classes
{
    public class RootEntity : EntityBase
    {
        private readonly List<ModelEntity> _groupedModels = new List<ModelEntity>();
        private WeakReferenceCollection<Texture> _ownedTextures;
        private WeakReferenceCollection<Animation> _ownedAnimations;

        [Browsable(false)]
        public CoordUnit[] Coords { get; set; }

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
        public IEnumerable<Texture> OwnedTextures
        {
            get => _ownedTextures ?? Enumerable.Empty<Texture>();
            set => _ownedTextures = new WeakReferenceCollection<Texture>(value);
        }

        [Browsable(false)]
        public IEnumerable<Animation> OwnedAnimations
        {
            get => _ownedAnimations ?? Enumerable.Empty<Animation>();
            set => _ownedAnimations = new WeakReferenceCollection<Animation>(value);
        }


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
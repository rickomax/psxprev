using System.Collections.Generic;
using System.ComponentModel;

namespace PSXPrev
{
    public class RootEntity : EntityBase
    {
        private readonly List<ModelEntity> _groupedModels = new List<ModelEntity>();

        [DisplayName("Name")]
        public string EntityName { get; set; }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            foreach (var entity in ChildEntities)
            {
                bounds.AddPoints(entity.Bounds3D.Corners);
            }
            Bounds3D = bounds;
        }

        public override string ToString()
        {
            return EntityName;
        }

        public List<ModelEntity> GetModelsWithTMDID(int id)
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
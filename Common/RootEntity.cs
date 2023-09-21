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

        [Browsable(false)]
        public ModelEntity[] Joints { get; set; }

        // Gets world joint transform matrices
        private Matrix4[] _jointMatrices;
        [Browsable(false)]
        public Matrix4[] JointMatrices
        {
            get
            {
                if (Joints != null)
                {
                    if (_jointMatrices == null)
                    {
                        _jointMatrices = new Matrix4[Joints.Length];
                    }
                    for (var i = 0; i < Joints.Length; i++)
                    {
                        _jointMatrices[i] = Joints[i]?.WorldMatrix ?? Matrix4.Identity;
                    }
                    return _jointMatrices;
                }
                return null;
            }
        }

        // Gets animated world joint transform matrices, without the root entity's animated transform
        private Matrix4[] _relativeAnimatedJointMatrices;
        [Browsable(false)]
        public Matrix4[] RelativeAnimatedJointMatrices
        {
            get
            {
                if (Joints != null)
                {
                    if (_relativeAnimatedJointMatrices == null)
                    {
                        _relativeAnimatedJointMatrices = new Matrix4[Joints.Length];
                    }
                    for (var i = 0; i < Joints.Length; i++)
                    {
                        _relativeAnimatedJointMatrices[i] = Joints[i]?.TempWorldMatrix ?? Matrix4.Identity;
                    }
                    return _relativeAnimatedJointMatrices;
                }
                return null;
            }
        }

        // Gets animated world joint transform matrices, including the root entity's animated transform
        private Matrix4[] _absoluteAnimatedJointMatrices;
        [Browsable(false)]
        public Matrix4[] AbsoluteAnimatedJointMatrices
        {
            get
            {
                if (Joints != null)
                {
                    if (_absoluteAnimatedJointMatrices == null)
                    {
                        _absoluteAnimatedJointMatrices = new Matrix4[Joints.Length];
                    }
                    var rootTempMatrix = TempMatrix;
                    for (var i = 0; i < Joints.Length; i++)
                    {
                        var model = Joints[i];
                        if (model == null)
                        {
                            _absoluteAnimatedJointMatrices[i] = TempWorldMatrix;
                        }
                        else
                        {
                            var modelTempWorldMatrix = model.TempWorldMatrix;
                            Matrix4.Mult(ref rootTempMatrix, ref modelTempWorldMatrix, out _absoluteAnimatedJointMatrices[i]);
                        }
                    }
                    return _absoluteAnimatedJointMatrices;
                }
                return null;
            }
        }

        [Browsable(false)]
        public Matrix4[] JointMatricesCache => _jointMatrices ?? JointMatrices;

        [Browsable(false)]
        public Matrix4[] RelativeAnimatedJointMatricesCache => _relativeAnimatedJointMatrices ?? RelativeAnimatedJointMatrices;

        [Browsable(false)]
        public Matrix4[] AbsoluteAnimatedJointMatricesCache => _absoluteAnimatedJointMatrices ?? AbsoluteAnimatedJointMatrices;

        [DisplayName("Format"), ReadOnly(true)]
        public string FormatName { get; set; }

        [Browsable(false)]
        public long FileOffset { get; set; }

#if DEBUG
        [DisplayName("Result Index"), ReadOnly(true)]
#else
        [Browsable(false)]
#endif
        public int ResultIndex { get; set; }

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
            FormatName = fromRootEntity.FormatName;
            FileOffset = fromRootEntity.FileOffset;
            ResultIndex = fromRootEntity.ResultIndex;
        }


        public override void ComputeBounds(AttachJointsMode attachJointsMode = AttachJointsMode.Hide, Matrix4[] jointMatrices = null)
        {
            if (jointMatrices == null)
            {
                jointMatrices = JointMatrices;
            }
            base.ComputeBounds(attachJointsMode, jointMatrices);
            var bounds = new BoundingBox();
            foreach (var entity in ChildEntities)
            {
                // Not yet, there are some issues with this, like models that are only made of attached vertices.
                if (entity is ModelEntity model && (model.Triangles.Length == 0 || (model.AttachedOnly && attachJointsMode == AttachJointsMode.Hide)))
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

        private static void PrepareTriangleJoints(Dictionary<uint, uint> jointMapping, uint[] joints, uint modelJointID, ref int attachedCount)
        {
            for (var j = 0; j < 3; j++)
            {
                var jointID = joints[j];
                if (jointID != Triangle.NoJoint)
                {
                    if (jointID == modelJointID)
                    {
                        joints[j] = Triangle.NoJoint;
                    }
                    else
                    {
                        if (!jointMapping.TryGetValue(jointID, out var mappedJointID))
                        {
                            mappedJointID = (uint)jointMapping.Count;
                            jointMapping.Add(jointID, mappedJointID);
                        }
                        joints[j] = mappedJointID;
                        attachedCount++;
                    }
                }
            }
        }

        // Must be called before ComputeBounds or FixConnections
        public void PrepareJoints(bool hasJoints = true)
        {
            var jointMapping = new Dictionary<uint, uint>();
            if (hasJoints)
            {
                foreach (ModelEntity model in ChildEntities)
                {
                    var attachedVertexCount = 0;
                    var attachedNormalCount = 0;
                    foreach (var triangle in model.Triangles)
                    {
                        if (triangle.VertexJoints != null)
                        {
                            PrepareTriangleJoints(jointMapping, triangle.VertexJoints, model.JointID, ref attachedVertexCount);
                        }
                        // Make sure the same array isn't being used for normal and vertex joints.
                        // If it is being reused then it's already remapped, and attempting to remap it again would be bad.
                        if (triangle.NormalJoints != null && triangle.NormalJoints != triangle.VertexJoints)
                        {
                            PrepareTriangleJoints(jointMapping, triangle.NormalJoints, model.JointID, ref attachedNormalCount);
                        }
                    }
                    var vertexCount = model.Triangles.Length * 3;
                    model.HasAttached = attachedVertexCount > 0 || attachedNormalCount > 0;
                    model.AttachedOnly = vertexCount > 0 && attachedVertexCount == vertexCount;// && attachedNormalCount == vertexCount;
                }
            }

            var joints = jointMapping.Count > 0 ? new ModelEntity[jointMapping.Count] : null;
            foreach (ModelEntity model in ChildEntities)
            {
                if (jointMapping.TryGetValue(model.JointID, out var mappedJointID))
                {
                    jointMapping.Remove(model.JointID); // Remove from mapping so that models with the same joint ID are not used
                    model.JointID = mappedJointID;
                    joints[mappedJointID] = model;
                }
                else
                {
                    model.JointID = Triangle.NoJoint;
                }
            }
#if DEBUG
            if (jointMapping.Count > 0)
            {
                var breakHere = 0;
            }
#endif
            Joints = joints;
        }

        public override void FixConnections(bool? bake = null, Matrix4[] tempJointMatrices = null)
        {
            if (!bake.HasValue)
            {
                bake = !Renderer.Shader.JointsSupported;
            }
            if (!bake.Value && tempJointMatrices == null)
            {
                tempJointMatrices = RelativeAnimatedJointMatrices;
            }
            base.FixConnections(bake, tempJointMatrices);
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
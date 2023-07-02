using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Classes
{
    public class AnimationBatch
    {
        private Animation _animation;
        private readonly Scene _scene;

        public AnimationBatch(Scene scene)
        {
            _scene = scene;
        }

        public void SetupAnimationBatch(Animation animation)
        {
            var objectCount = animation.ObjectCount;
            _scene.MeshBatch.Reset(objectCount + 1);
            _scene.BoundsBatch.Reset();
            _scene.SkeletonBatch.Reset();
            _scene.GizmosMeshBatch.Reset(0);
            _animation = animation;
        }

        public bool SetupAnimationFrame(float frameIndex, RootEntity[] checkedEntities, RootEntity selectedRootEntity, ModelEntity selectedModelEntity, bool updateMeshData = false)
        {
            _scene.SkeletonBatch.Reset();
            RootEntity rootEntity = null;
            if (selectedRootEntity != null)
            {
                rootEntity = selectedRootEntity;
            } else if (selectedModelEntity != null)
            {
                rootEntity = selectedModelEntity.GetRootEntity();
            }
            _scene.MeshBatch.SetupMultipleEntityBatch(checkedEntities, selectedModelEntity, selectedRootEntity, _scene.TextureBinder, updateMeshData || _scene.AutoAttach, false, true);
            return ProcessAnimationObject(_animation.RootAnimationObject, frameIndex, rootEntity, Matrix4.Identity);
        }

        private bool ProcessAnimationObject(AnimationObject animationObject, float frameIndex, RootEntity selectedRootEntity, Matrix4 worldMatrix)
        {
            switch (_animation.AnimationType)
            {
                case AnimationType.VertexDiff:
                case AnimationType.NormalDiff:
                    {
                        if (selectedRootEntity != null && animationObject.Parent != null)
                        {
                            var objectId = animationObject.TMDID.GetValueOrDefault();
                            foreach (ModelEntity childModel in selectedRootEntity.ChildEntities)
                            {
                                if (childModel.TMDID == objectId)
                                {
                                    var intFrameIndex = (uint)frameIndex;
                                    if (intFrameIndex > animationObject.AnimationFrames.Count - 1)
                                    {
                                        return false;
                                    }
                                    var animationFrame = animationObject.AnimationFrames[intFrameIndex];
                                    if (intFrameIndex > 0)
                                    {
                                        var lastFrame = animationObject.AnimationFrames[intFrameIndex - 1];
                                        for (uint j = 0; j < animationFrame.Vertices.Length; j++)
                                        {
                                            if (j < lastFrame.Vertices.Length)
                                            {
                                                animationFrame.TempVertices[j] = lastFrame.Vertices[j];
                                            }
                                            else
                                            {
                                                animationFrame.TempVertices[j] = Vector3.Zero;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (uint j = 0; j < animationFrame.Vertices.Length; j++)
                                        {
                                            animationFrame.TempVertices[j] = Vector3.Zero;
                                        }
                                    }
                                    var interpolator = frameIndex % 1;
                                    var initialVertices = _animation.AnimationType == AnimationType.VertexDiff ? animationFrame.TempVertices : null;
                                    var finalVertices = _animation.AnimationType == AnimationType.VertexDiff ? animationFrame.Vertices : null;
                                    var initialNormals = _animation.AnimationType == AnimationType.NormalDiff ? animationFrame.TempVertices : null;
                                    var finalNormals = _animation.AnimationType == AnimationType.NormalDiff ? animationFrame.Vertices : null;
                                    childModel.Interpolator = interpolator;
                                    childModel.InitialVertices = initialVertices;
                                    childModel.FinalVertices = finalVertices;
                                    childModel.InitialNormals = initialNormals;
                                    childModel.FinalNormals = finalNormals;
                                    childModel.TempMatrix = Matrix4.Identity;
                                    //_scene.MeshBatch.BindModelBatch(childModel, childModel.WorldMatrix, _scene.TextureBinder, initialVertices, initialNormals, finalVertices, finalNormals, interpolator);
                                }
                            }
                        }
                        break;
                    }
                case AnimationType.Common:
                    {
                        if (selectedRootEntity != null)
                        {
                            var animationFrames = animationObject.AnimationFrames;
                            var totalFrames = animationFrames.Count;
                            var localMatrix = Matrix4.Identity;
                            for (uint f = 0; f <= frameIndex && f < totalFrames; f++)
                            {
                                if (!animationFrames.ContainsKey(f))
                                {
                                    continue;
                                }

                                var frameMatrix = Matrix4.Identity;
                                var sumFrame = animationFrames[f];
                                if (sumFrame.Rotation != null)
                                {
                                    var r = Matrix4.CreateFromQuaternion(sumFrame.Rotation.Value);
                                    frameMatrix = frameMatrix * r;
                                }
                                else if (sumFrame.EulerRotation != null)
                                {
                                    var r = GeomUtils.CreateR(sumFrame.EulerRotation.Value);
                                    frameMatrix = frameMatrix * r;
                                }

                                if (sumFrame.Scale != null)
                                {
                                    var scale = (Vector3) sumFrame.Scale;
                                    var s = GeomUtils.CreateS(scale.X);
                                    frameMatrix = frameMatrix * s;
                                }

                                if (sumFrame.Translation != null)
                                {
                                    var translation = (Vector3) sumFrame.Translation;
                                    var t = GeomUtils.CreateT(translation);
                                    frameMatrix = frameMatrix * t;
                                }

                                var absoluteMatrixValue = sumFrame.AbsoluteMatrix;
                                if (!absoluteMatrixValue)
                                {
                                    frameMatrix *= localMatrix;
                                }

                                localMatrix = frameMatrix;
                            }

                            //if (parentMatrix != null)
                            //{
                            worldMatrix = worldMatrix * localMatrix;
                            //_scene.SkeletonBatch.AddLine(Vector3.TransformPosition(Vector3.One, parentMatrix.Value), Vector3.TransformPosition(Vector3.One, worldMatrix), animationObject == selectedAnimationObject ? Color.Blue : Color.Red);
                            //}
                            //else
                            //{
                            //    worldMatrix = localMatrix;
                            //}
                            //if (selectedRootEntity != null)
                            //{
                            var objectId = animationObject.TMDID.GetValueOrDefault();
                            if (objectId > 0)
                            {
                                var models = selectedRootEntity.GetModelsWithTMDID(objectId - 1);
                                foreach (var childModel in models)
                                {
                                    childModel.Interpolator = 0;
                                    childModel.InitialVertices = null;
                                    childModel.FinalVertices = null;
                                    childModel.InitialNormals = null;
                                    childModel.FinalNormals = null;
                                    childModel.TempMatrix = worldMatrix;
                                    //_scene.MeshBatch.BindModelBatch(childModel, worldMatrix, _scene.TextureBinder);
                                }
                            }

                            //}
                        }

                        break;
                    }
            }
            foreach (var childAnimationObject in animationObject.Children)
            {
                if (!ProcessAnimationObject(childAnimationObject, frameIndex, selectedRootEntity, worldMatrix))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

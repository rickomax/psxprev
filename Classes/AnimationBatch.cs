using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Linq;

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
            }
            else if (selectedModelEntity != null)
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
                            selectedRootEntity.TempMatrix = Matrix4.Identity;
                            foreach (var objectId in animationObject.TMDID)
                            {
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
                                    }
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
                                    frameMatrix *= r;
                                }
                                else if (sumFrame.EulerRotation != null)
                                {
                                    var r = GeomUtils.CreateR(sumFrame.EulerRotation.Value);
                                    frameMatrix *= r;
                                }
                                if (sumFrame.Scale != null)
                                {
                                    var scale = (Vector3)sumFrame.Scale;
                                    var s = GeomUtils.CreateS(scale);
                                    frameMatrix *= s;
                                }
                                if (sumFrame.Translation != null)
                                {
                                    var translation = (Vector3)sumFrame.Translation;
                                    var t = GeomUtils.CreateT(translation);
                                    frameMatrix *= t;
                                }
                                var absoluteMatrixValue = sumFrame.AbsoluteMatrix;
                                if (!absoluteMatrixValue)
                                {
                                    frameMatrix = localMatrix * frameMatrix;
                                }
                                localMatrix = frameMatrix;
                            }
                            if (animationObject.Parent != null && _scene.ShowSkeleton)
                            {
                                _scene.SkeletonBatch.AddLine(Vector3.TransformPosition(Vector3.One, worldMatrix), Vector3.TransformPosition(Vector3.One, worldMatrix * localMatrix), Color.Blue);
                            }
                            worldMatrix *= localMatrix;
                            if (animationObject.HandlesRoot)
                            {
                                selectedRootEntity.TempMatrix = worldMatrix;
                            }
                            else
                            {
                                foreach (var objectId in animationObject.TMDID)
                                {
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
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case AnimationType.HMD:
                    {
                        if (selectedRootEntity != null && selectedRootEntity.Coords != null)
                        {
                            var animationFrames = animationObject.AnimationFrames;
                            if (animationFrames.Count == 0)
                            {
                                break; // No frames to interpolate
                            }
                            var totalFrames = animationFrames.Values.Max(af => af.FrameTime + Math.Max(1u, af.FrameDuration));

                            // Reset coordinate matrices in-case a frame requires the last state of the matrix.
                            // This is important because there's no guarantee the animation won't skip a frame due to lag.
                            foreach (var tmdid in animationObject.TMDID)
                            {
                                if (tmdid <= 0 || tmdid > selectedRootEntity.Coords.Length)
                                {
                                    continue;
                                }
                                var coord = selectedRootEntity.Coords[tmdid - 1];

                                coord.ResetTransform();
                            }

                            for (uint f = 0; f <= frameIndex && f < totalFrames; f++)
                            {
                                if (!animationFrames.TryGetValue(f, out var srcFrame))
                                {
                                    continue;
                                }
                                /*AnimationFrame dstFrame = null;
                                for (uint fdst = f + 1; fdst < totalFrames; fdst++)
                                {
                                    if (animationFrames.TryGetValue(fdst, out dstFrame))
                                    {
                                        break;
                                    }
                                }
                                if (dstFrame == null)
                                {
                                    dstFrame = srcFrame;
                                }*/

                                // todo: How to properly handle certain None interpolation types is not fully understood.

                                foreach (var tmdid in animationObject.TMDID)
                                {
                                    if (tmdid <= 0 || tmdid > selectedRootEntity.Coords.Length)
                                    {
                                        continue;
                                    }
                                    var coord = selectedRootEntity.Coords[tmdid - 1];

                                    float start = srcFrame.FrameTime;
                                    //float range = dstFrame.FrameTime - srcFrame.FrameTime;
                                    float range = srcFrame.FrameDuration;
                                    float delta = (range != 0f ? Math.Max(0f, Math.Min(1f, (frameIndex - start) / range)) : 1f);

                                    Matrix4 frameMatrix;
                                    if (srcFrame.RotationType == InterpolationType.Linear || srcFrame.ScaleType == InterpolationType.Linear)
                                    {
                                        // Use new matrix.
                                        frameMatrix = Matrix4.Identity;

                                        if (srcFrame.ScaleType == InterpolationType.Linear) // has supported scale
                                        {
                                            var srcS = srcFrame.Scale.Value;
                                            //var dstS = dstFrame.Scale ?? srcS;
                                            var dstS = srcFrame.FinalScale ?? srcS;
                                            var scale = GeomUtils.InterpolateVector(srcS, dstS, delta);
                                            //var scale = HMDHelper.Interpolate(srcFrame.ScaleType, srcFrame.Scale, srcFrame.CurveScales, srcFrame.FinalScale, delta);
                                            var s = GeomUtils.CreateS(scale);
                                            frameMatrix *= s;
                                        }

                                        Vector3 e;
                                        RotationOrder rotOrder;
                                        if (srcFrame.RotationType == InterpolationType.Linear) // has supported rotation
                                        {
                                            var srcR = srcFrame.EulerRotation.Value;
                                            //var dstR = dstFrame.EulerRotation ?? srcR;
                                            var dstR = srcFrame.FinalEulerRotation ?? srcR;
                                            e = GeomUtils.InterpolateVector(srcR, dstR, delta);
                                            //e = HMDHelper.Interpolate(srcFrame.RotationType, srcFrame.EulerRotation, srcFrame.CurveEulerRotations, srcFrame.FinalEulerRotation, delta);
                                            rotOrder = srcFrame.RotationOrder;
                                        }
                                        else
                                        {
                                            // todo: Are we supposed to use the original rotation here?
                                            e = coord.Rotation;
                                            rotOrder = coord.OriginalRotationOrder; // Observed in Gods98 (PSX) source as the default rotation order.
                                        }
                                        var r = GeomUtils.CreateR(e, rotOrder);
                                        frameMatrix *= r;
                                    }
                                    else
                                    {
                                        // Use original coord matrix...?
                                        frameMatrix = coord.LocalMatrix;
                                    }

                                    if (srcFrame.TranslationType == InterpolationType.Linear) // has supported translation
                                    {
                                        var origT = GeomUtils.CreateT(frameMatrix.ExtractTranslation());
                                        var srcT = srcFrame.Translation.Value;
                                        //var dstT = dstFrame.Translation ?? srcT;
                                        var dstT = srcFrame.FinalTranslation ?? srcT;
                                        var translation = GeomUtils.InterpolateVector(srcT, dstT, delta);
                                        //var translation = HMDHelper.Interpolate(srcFrame.TranslationType, srcFrame.Translation, srcFrame.CurveTranslations, srcFrame.FinalTranslation, delta);
                                        var t = GeomUtils.CreateT(translation);
                                        frameMatrix *= origT.Inverted(); // Overwrite old translation
                                        frameMatrix *= t;
                                    }

                                    coord.LocalMatrix = frameMatrix;

                                    // Hierarchy isn't supported for HMD animations. The coordinates do that already.
                                    //worldMatrix *= frameMatrix;
                                    // Not supported by HMD, which modifies coordinates of individual models.
                                    //if (animationObject.HandlesRoot)
                                    //{
                                    //}
                                }
                            }
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

            // HMD: Update the temporary matrices of all models, now that the coordinate system has been updated.
            if (_animation.RootAnimationObject == animationObject && _animation.AnimationType == AnimationType.HMD)
            {
                var coords = selectedRootEntity?.Coords;
                if (coords != null)
                {
                    foreach (var coord in coords)
                    {
                        var models = selectedRootEntity.GetModelsWithTMDID(coord.TMDID);
                        foreach (var childModel in models)
                        {
                            childModel.Interpolator = 0;
                            childModel.InitialVertices = null;
                            childModel.FinalVertices = null;
                            childModel.InitialNormals = null;
                            childModel.FinalNormals = null;
                            childModel.TempMatrix =
                                coord.WorldMatrix * // Transform by new coord matrix
                                childModel.OriginalWorldMatrix.Inverted() * childModel.WorldMatrix * // Preserve gizmo translations
                                childModel.WorldMatrix.Inverted(); // Overwrite original transforms of models
                        }
                    }
                }
            }

            return true;
        }
    }
}

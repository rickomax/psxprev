using OpenTK;

namespace PSXPrev
{
    public class AnimationBatch
    {
        private Animation _animation;
        private readonly Scene _scene;
        private int _animationProcessIndex;
        
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

        public void SetupAnimationFrame(int frame, AnimationObject animationObject = null, RootEntity selectedEntity = null)
        {
            _scene.SkeletonBatch.Reset();
            _animationProcessIndex = 0;
            ProcessAnimationObject(_animation.RootAnimationObject, frame, null, animationObject, selectedEntity);
        }

        private void ProcessAnimationObject(AnimationObject animationObject, int frameIndex, Matrix4? parentMatrix, AnimationObject selectedAnimationObject = null, RootEntity selectedEntity = null)
        {
            var animationFrames = animationObject.AnimationFrames;
            var totalFrames = animationFrames.Count;
            var localMatrix = Matrix4.Identity;
            for (var f = 0; f <= frameIndex && f < totalFrames; f++)
            {
                if (!animationFrames.ContainsKey(f))
                {
                    continue;
                }

                var matrix = Matrix4.Identity;

                var sumFrame = animationFrames[f];

                if (sumFrame.Rotation != null)
                {
                    var r = Matrix4.CreateFromQuaternion(sumFrame.Rotation.Value);
                    matrix = matrix * r;
                }
                else if (sumFrame.EulerRotation != null)
                {
                    var r = GeomUtils.CreateR(sumFrame.EulerRotation.Value);
                    matrix = matrix * r;
                }

                if (sumFrame.Scale != null)
                {
                    var scale = (Vector3)sumFrame.Scale;
                    var s = GeomUtils.CreateS(scale.X);
                    matrix = matrix * s;
                }

                if (sumFrame.Translation != null)
                {
                    var translation = (Vector3)sumFrame.Translation;
                    var t = GeomUtils.CreateT(translation);
                    matrix = matrix * t;
                }

                var absoluteMatrixValue = sumFrame.AbsoluteMatrix;
                if (!absoluteMatrixValue)
                {
                    matrix = matrix * localMatrix;
                }
                localMatrix = matrix;
            }

            Matrix4 worldMatrix;
            if (parentMatrix != null)
            {
                worldMatrix = localMatrix * parentMatrix.Value;
                _scene.SkeletonBatch.AddLine(Vector3.TransformPosition(Vector3.One, parentMatrix.Value), Vector3.TransformPosition(Vector3.One, worldMatrix), animationObject == selectedAnimationObject ? Color.Blue : Color.Red);
            }
            else
            {
                worldMatrix = localMatrix;
            }

            if (selectedEntity != null)
            {
                var objectId = animationObject.TMDID.GetValueOrDefault();
                if (objectId > 0)
                {
                    var models = selectedEntity.GetModelsWithTMDID(objectId-1);
                    foreach (var model in models)
                    {
                        _scene.MeshBatch.BindModelBatch(model, _animationProcessIndex++, worldMatrix, _scene.TextureBinder);
                    }
                }
            }

            foreach (var childObject in animationObject.Children)
            {
                ProcessAnimationObject(childObject, frameIndex, worldMatrix, selectedAnimationObject, selectedEntity);
            }
        }
    }
}
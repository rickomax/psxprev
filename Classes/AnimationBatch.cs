using OpenTK;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class AnimationBatch
    {
        private Animation _animation;
        private Scene _scene;
        private int _animationProcessIndex;

        public AnimationBatch(Scene scene)
        {
            _scene = scene;
        }

        public void SetupAnimationBatch(Animation animation)
        {
            var objectCount = animation.ObjectCount;
            _scene.MeshBatch.Reset(objectCount + 1);
            _animation = animation;
            //_scene.LineBatch.Reset();
        }

        public void SetupAnimationFrame(int frame, EntityBase selectedEntity = null)
        {
            _animationProcessIndex = 0;
            ProcessAnimationObject(_animation.RootAnimationObject, frame, null, selectedEntity);
        }

        private void ProcessAnimationObject(AnimationObject animationObject, int frameIndex, Matrix4? parentMatrix, EntityBase selectedEntity = null)
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
                var sumFrame = animationFrames[f];
                var matrix = Matrix4.Identity;
                var anyMatrix = sumFrame.AbsoluteMatrix;
                if (anyMatrix != null)
                {
                    //var modelMatrix = sumFrame.Matrix;
                    //if (modelMatrix != null)
                    //{
                    //    matrix = (Matrix4)modelMatrix;
                    //}
                    //else
                    //{

                    if (sumFrame.Rotation != null)
                    {
                        var rotation = (Vector3)sumFrame.Rotation;
                        var r = GeomUtils.CreateR(rotation);
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

                    var absoluteMatrixValue = (bool)anyMatrix;
                    if (!absoluteMatrixValue)
                    {
                        matrix = matrix * localMatrix;
                    }
                    localMatrix = matrix;
                }
            }

            Matrix4 worldMatrix;
            if (parentMatrix != null)
            {
                var parentMatrixValue = (Matrix4)parentMatrix;
                worldMatrix = localMatrix * parentMatrixValue;
                //if (animationObject.Visible)
                //{
                //    _scene.LineBatch.AddLine(new Line
                //    {
                //        p1 = Vector4.One * parentMatrixValue,
                //        p2 = Vector4.One * worldMatrix
                //    });
                //}
            }
            else
            {
                worldMatrix = localMatrix;
            }

            if (selectedEntity != null)
            {
                var childEntitiets = selectedEntity.ChildEntities;
                if (animationObject.TMDID > 0 && animationObject.TMDID <= childEntitiets.Length)
                {
                    var model = (ModelEntity) childEntitiets[animationObject.TMDID - 1];
                    _scene.MeshBatch.BindModelBatch(model, _animationProcessIndex++, worldMatrix);
                }
            }

            foreach (var childObject in animationObject.Children)
            {
                ProcessAnimationObject(childObject, frameIndex, worldMatrix, selectedEntity);
            }
        }
    }
}
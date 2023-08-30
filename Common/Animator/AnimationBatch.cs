using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using PSXPrev.Common.Renderer;

namespace PSXPrev.Common.Animator
{
    public class AnimationBatch
    {
        // How close to be near the end of a looping animation while staying at the end (and not being modulused).
        private const double EPSILON = 0.0001d;

        private readonly Scene _scene;
        private Animation _animation;
        private WeakReference<RootEntity> _lastRootEntity; // Optimize: Use weak reference because we only need to check reference equals.
        private double _time;
        private double _loopDelayTime;
        private double _playbackFrameTime;
        private double _lastPlaybackFrameTime; // Optimize: For animation state updates. Not relevant when _finished is true.
        private bool _finished;
        private bool _lastFinished; // Optimize: For animation state updates.
        private bool _delaying;
        private bool _mirroring;
        private bool _playbackStateChanged; // Optimize: Do we need to call ComputePlaybackFrameTime?
        private bool _animationStateChanged; // Optimize: Force animation state updates.
        private bool _reverse;
        private AnimationLoopMode _loopMode = AnimationLoopMode.Loop;

        // Real time in seconds (including looping and delayed time).
        public double Time
        {
            get => _time;
            set
            {
                // todo: Should we cap time at 0, or handle negative time?
                value = Math.Max(0d, value);
                if (_time != value)
                {
                    _time = value;
                    _playbackStateChanged = true;
                    // Don't assume changes to time will invalidate the animation state.
                    // We'll figure this out after ComputePlaybackFrameTime().
                }
            }
        }

        // Play animation in reverse. AddTime subtracts seconds instead of adding them.
        public bool Reverse
        {
            get => _reverse;
            set
            {
                if (_reverse != value)
                {
                    _reverse = value;
                    Invalidate();
                }
            }
        }

        // How to handle restarting animation after reaching FrameCount.
        public AnimationLoopMode LoopMode
        {
            get => _loopMode;
            set
            {
                if (_loopMode != value)
                {
                    _loopMode = value;
                    Invalidate();
                }
            }
        }

        // Seconds to wait before restarting loop.
        public double LoopDelayTime
        {
            get => _loopDelayTime;
            set
            {
                value = Math.Max(0d, value);
                if (_loopDelayTime != value)
                {
                    _loopDelayTime = value;
                    Invalidate();
                }
            }
        }

        // LoopMode is a Once type, and the animation has finished.
        // It's only recommended to check this after SetupAnimationFrame, because this is calculated before then.
        public bool IsFinished
        {
            get
            {
                ComputePlaybackFrameTime();
                return _finished;
            }
        }

        // The playback time is currently in the middle of LoopDelayTime.
        public bool IsDelaying
        {
            get
            {
                ComputePlaybackFrameTime();
                return _delaying;
            }
        }

        // The playback time is past FrameCount and is reversing back to the beginning.
        public bool IsMirroring
        {
            get
            {
                ComputePlaybackFrameTime();
                return _mirroring;
            }
        }

        // Frame rate of animation for converting Time to FrameTime.
        public float FPS => _animation?.FPS ?? 1f;
        // The duration of the animation in seconds.
        public double Duration => (FPS == 0 ? 0 : FrameCount / FPS);
        // Number of frames in the animation.
        public double FrameCount => (FPS == 0 ? 0 : (_animation?.FrameCount ?? 0));
        // Frames to wait before restarting loop.
        public double LoopDelayFrameCount => LoopDelayTime * FPS;
        // Total number of frames in animation and frames to wait between restarting loops.
        public double TotalFrameCount => FrameCount + LoopDelayFrameCount;
        // Real time in frames (including looping and delayed time). (Time x FPS)
        public double FrameTime
        {
            get => Time * FPS;
            set => Time = (FPS == 0 ? 0 : value / FPS);
        }
        // Looped, capped, and mirrored playback time in frames.
        public double CurrentFrameTime => GeomMath.Clamp(LoopFrameTime(), 0, FrameCount);


        public AnimationBatch(Scene scene)
        {
            _scene = scene;
            _lastRootEntity = new WeakReference<RootEntity>(null);
            Invalidate();
        }


        // Invalidate animation state to force an update during the next call to SetupAnimationFrame.
        public void Invalidate()
        {
            _playbackStateChanged = true;
            _animationStateChanged = true;
        }

        // Calling this also calls Invalidate.
        public void Restart()
        {
            Time = 0d;
            Invalidate();
        }

        public void AddTime(double seconds)
        {
            Time += seconds;
        }

        public void SetTimeToFrame(AnimationFrame frame, bool endOfFrame = false)
        {
            var timeMultiplier = FPS * Math.Abs(frame.AnimationObject.Speed);
            if (timeMultiplier != 0)
            {
                //const double EPSILON = 0.0001d;
                Time = (!endOfFrame ? frame.FrameTime : (frame.FrameEnd - EPSILON)) / timeMultiplier;
            }
            else
            {
                Time = 0d;
            }
        }

        public void SetupAnimationBatch(Animation animation, bool simulate = false)
        {
            // Support using AnimationBatch even if we have no Scene.
            if (!simulate && _scene != null)
            {
                // todo: Is this correct for handling reseting the mesh batch?
                // What if the animation doesn't match the object?
                var objectCount = animation == null ? 0 : (animation.ObjectCount + 1);
                _scene.MeshBatch.Reset(objectCount);
            }

            if (_animation != animation)
            {
                _animation = animation;
                _playbackStateChanged = true;
                Invalidate();
            }
            Restart();
        }

        // Returns true if the animation has been processed (updated), or false if nothing needed to be updated.
        public bool SetupAnimationFrame(RootEntity[] checkedEntities, RootEntity selectedRootEntity, ModelEntity selectedModelEntity, bool updateMeshData = false, bool simulate = false)
        {
            var rootEntity = selectedRootEntity ?? selectedModelEntity?.GetRootEntity();

            // Support using AnimationBatch even if we have no Scene.
            if (!simulate && _scene != null)
            {
                updateMeshData |= _scene.AutoAttach;
                _scene.MeshBatch.SetupMultipleEntityBatch(checkedEntities, selectedModelEntity, selectedRootEntity, updateMeshData);
            }

            ComputePlaybackFrameTime();

            var needsUpdate = (_animationStateChanged) ||
                              (_lastFinished != _finished) ||
                              (!_finished && _lastPlaybackFrameTime != _playbackFrameTime) ||
                              (!_lastRootEntity.TryGetTarget(out var lastRootEntity) || lastRootEntity != rootEntity);

            _animationStateChanged = false;
            _lastPlaybackFrameTime = _playbackFrameTime;
            _lastFinished = _finished;
            _lastRootEntity.SetTarget(rootEntity);

            // The result has been changed to signal if anything has been processed or not.
            if (needsUpdate)
            {
                // todo: What does ProcessAnimationObject's return boolean signify?
                ProcessAnimationObject(_animation.RootAnimationObject, rootEntity, Matrix4.Identity);
                return true;
            }
            return false;
        }

        private void ResetAnimationCoords(AnimationObject animationObject, RootEntity selectedRootEntity)
        {
            if (selectedRootEntity?.Coords != null)
            {
                var coords = selectedRootEntity.Coords;
                foreach (var coord in selectedRootEntity.Coords)
                {
                    coord.ResetTransform();
                }
                /*foreach (var tmdid in animationObject.TMDID)
                {
                    if (tmdid <= 0 || tmdid > selectedRootEntity.Coords.Length)
                    {
                        continue;
                    }
                    var coord = selectedRootEntity.Coords[tmdid - 1];

                    coord.ResetTransform();
                }
                foreach (var childAnimationObject in animationObject.Children)
                {
                    ResetAnimationCoords(childAnimationObject, selectedRootEntity);
                }*/
            }
        }

        private bool ProcessAnimationObject(AnimationObject animationObject, RootEntity selectedRootEntity, Matrix4 worldMatrix)
        {
            // Reset coordinate matrices in-case a frame requires the last state of the matrix.
            // This is important because there's no guarantee the animation won't skip a frame due to lag.
            if (_animation.RootAnimationObject == animationObject && _animation.AnimationType == AnimationType.HMD)
            {
                ResetAnimationCoords(animationObject, selectedRootEntity);
            }

            var frameTime = LoopFrameTime(animationObject, out var frameIndex, out var frameDelta);

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
                                        if (frameIndex > animationObject.AnimationFrames.Count - 1)
                                        {
                                            return false;
                                        }
                                        var animationFrame = animationObject.AnimationFrames[(uint)frameIndex];
                                        if (frameIndex > 0)
                                        {
                                            var lastFrame = animationObject.AnimationFrames[(uint)frameIndex - 1];
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
                                        var interpolator = frameDelta;
                                        //var interpolator = GetInterpolator(animationFrame, frameTime);
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
                            var frameTransfer = Vector3.Zero;
                            var frameRotation = Vector3.Zero;
                            var frameScale = Vector3.One;
                            Matrix4? frameMatrix = null;
                            for (uint f = 0; f <= frameIndex && f < totalFrames; f++)
                            {
                                if (!animationFrames.TryGetValue(f, out var animationFrame))
                                {
                                    continue;
                                }
                                if (animationFrame.Matrix != null)
                                {
                                    frameMatrix = Matrix4.CreateFromQuaternion(animationFrame.Matrix.Value.ExtractRotation()) * Matrix4.CreateScale(animationFrame.Matrix.Value.ExtractScale()) * Matrix4.CreateTranslation(animationFrame.Transfer.GetValueOrDefault());
                                }
                                if (!animationFrame.AbsoluteMatrix)
                                {
                                    if (animationFrame.EulerRotation != null)
                                    {
                                        frameRotation += animationFrame.EulerRotation.Value;
                                    }
                                    if (animationFrame.Scale != null)
                                    {
                                        frameScale *= animationFrame.Scale.Value;
                                    }
                                    if (animationFrame.Transfer != null)
                                    {
                                        frameTransfer += animationFrame.Transfer.Value;
                                    }
                                }
                                else
                                {
                                    if (animationFrame.EulerRotation != null)
                                    {
                                        frameRotation = animationFrame.EulerRotation.Value;
                                    }
                                    if (animationFrame.Scale != null)
                                    {
                                        frameScale = animationFrame.Scale.Value;
                                    }
                                    if (animationFrame.Transfer != null)
                                    {
                                        frameTransfer = animationFrame.Transfer.Value;
                                    }
                                }
                            }
                            if (frameMatrix == null)
                            {
                                frameMatrix = GeomMath.CreateRotation(frameRotation, RotationOrder.XYZ) * Matrix4.CreateScale(frameScale) * Matrix4.CreateTranslation(frameTransfer);
                            }
                            worldMatrix = frameMatrix.Value * worldMatrix;
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
                                        List<ModelEntity> models;
                                        if (_animation.TMDBindings.TryGetValue(objectId, out var binding))
                                        {
                                            models = selectedRootEntity.GetModelsWithTMDID(binding);
                                        }
                                        else
                                        {
                                            models = selectedRootEntity.GetModelsWithTMDID(objectId - 1);
                                        }
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

                            for (uint f = 0; f <= frameIndex && f < totalFrames; f++)
                            {
                                if (!animationFrames.TryGetValue(f, out var srcFrame))
                                {
                                    continue;
                                }

                                // todo: How to properly handle certain None interpolation types is not fully understood.

                                foreach (var tmdid in animationObject.TMDID)
                                {
                                    if (tmdid <= 0 || tmdid > selectedRootEntity.Coords.Length)
                                    {
                                        continue;
                                    }
                                    var coord = selectedRootEntity.Coords[tmdid - 1];

                                    var delta = GetInterpolator(srcFrame, frameTime);

                                    Matrix4 frameMatrix;
                                    var needsTranslation = false;
                                    var typeR = srcFrame.RotationType;
                                    var typeS = srcFrame.ScaleType;
                                    if (typeR != InterpolationType.None || typeS != InterpolationType.None)
                                    {
                                        // Use new matrix.
                                        frameMatrix = Matrix4.Identity;
                                        needsTranslation = true;

                                        if (typeS != InterpolationType.None) // has supported scale
                                        {
                                            var srcS = srcFrame.Scale;
                                            var curveS = srcFrame.CurveScales;
                                            var dstS = srcFrame.FinalScale;
                                            var scale = Interpolate(typeS, srcS, curveS, dstS, delta);
                                            var s = Matrix4.CreateScale(scale);
                                            frameMatrix *= s;
                                        }

                                        Vector3 e;
                                        RotationOrder rotOrder;
                                        if (typeR != InterpolationType.None) // has supported rotation
                                        {
                                            var srcR = srcFrame.EulerRotation;
                                            var curveR = srcFrame.CurveEulerRotations;
                                            var dstR = srcFrame.FinalEulerRotation;
                                            e = Interpolate(typeR, srcR, curveR, dstR, delta);
                                            rotOrder = srcFrame.RotationOrder;
                                        }
                                        else
                                        {
                                            // todo: Are we supposed to use the original rotation here?
                                            e = coord.Rotation;
                                            rotOrder = coord.OriginalRotationOrder; // Observed in Gods98 (PSX) source as the default rotation order.
                                        }
                                        var r = GeomMath.CreateRotation(e, rotOrder);
                                        frameMatrix *= r;
                                    }
                                    else
                                    {
                                        // Use original coord matrix...?
                                        frameMatrix = coord.LocalMatrix;
                                    }

                                    var typeT = srcFrame.TranslationType;
                                    if (typeT != InterpolationType.None) // has supported translation
                                    {
                                        var origT = Matrix4.CreateTranslation(frameMatrix.ExtractTranslation());
                                        var srcT = srcFrame.Translation;
                                        var curveT = srcFrame.CurveTranslations;
                                        var dstT = srcFrame.FinalTranslation;
                                        var translation = Interpolate(typeT, srcT, curveT, dstT, delta);
                                        var t = Matrix4.CreateTranslation(translation);
                                        frameMatrix *= origT.Inverted(); // Overwrite old translation
                                        frameMatrix *= t;
                                    }
                                    else if (needsTranslation)
                                    {
                                        // Preserve old translation
                                        frameMatrix *= Matrix4.CreateTranslation(coord.LocalMatrix.ExtractTranslation());
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
                if (!ProcessAnimationObject(childAnimationObject, selectedRootEntity, worldMatrix))
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
                            var childWorldMatrix = childModel.WorldMatrix;
                            childModel.TempMatrix =
                                coord.WorldMatrix * // Transform by new coord matrix
                                childModel.OriginalWorldMatrix.Inverted() * childWorldMatrix * // Preserve gizmo translations
                                childWorldMatrix.Inverted(); // Overwrite original transforms of models

                            childModel.TempLocalMatrix = childWorldMatrix;
                        }
                    }
                }
            }

            return true;
        }


        private void ComputePlaybackFrameTime()
        {
            // Well... this got a lot more complicated than I was expecting it to...
            // The biggest thing to blame is LoopDelayTime and objects with different frame counts/speeds.

            if (!_playbackStateChanged)
            {
                // Everything is already calculated.
                return;
            }

            var frameTime = FrameTime;
            var animFrameCount = (double)FrameCount;

            // When mirrored, we play twice as many animation frames before finishing/delaying.
            var mirroredFrameCount = animFrameCount * (_loopMode.IsMirrored() ? 2 : 1);

            // Compute playback frame time and delaying state.
            if (FrameCount == 0)
            {
                // No frames, default to frame 0...maybe?
                _delaying = false;
                _playbackFrameTime = 0d;
            }
            else if (LoopDelayTime == 0 || frameTime == 0)
            {
                // No delay, loopFrameTime is just frameTime, without any extra work.
                _delaying = false;
                _playbackFrameTime = frameTime;
            }
            else
            {
                // If we have a delay between loops, then we need to do extra calculations to get the real frameTime.

                // New time:   [0|......animation......|count]             + [[repeat]]
                // Frame time: [0|......animation......|~~delayed~~|count] + [[repeat]]
                //               ^                     ^           ^
                //             begin(start)           end(stop)  resume
                // Reverse: ==========================================================
                // Frame time: [0|~~delayed~~|......animation......|count]
                //               ^           ^                     ^
                //             resume      stop(begin)           start(end)

                // This reverse diagram above isn't related anymore, but I've
                // kept it, since it's a good reference for reversed animation.
                // If reversed, then don't pause at the start of the animation playback.

                var delayFrameCount = (double)LoopDelayFrameCount;
                var totalFrameCount = mirroredFrameCount + delayFrameCount;

                // Count number of times repeated with delay, and add that while taking out the delay time.
                var repeatFrameTime = Math.Floor(frameTime / totalFrameCount) * mirroredFrameCount;

                var totalFrameTime = GeomMath.PositiveModulus(frameTime, totalFrameCount);
                _delaying = totalFrameTime >= mirroredFrameCount;
                _playbackFrameTime = repeatFrameTime + (_delaying ? mirroredFrameCount : totalFrameTime);

                //_delayed = reverseTotalFrameTime <= delayFrameCount;
                //_reversePlaybackFrameTime = (_delayed ? 0d : reverseTotalFrameTime - delayFrameCount);
                //// Count number of times repeated with delay, and add that while taking out the delay time.
                //_reversePlaybackFrameTime -= repeatFrameTime;
            }

            // Compute finished state. Only set to finished if delay has ended.
            _finished = false;
            if (FrameCount == 0)
            {
                // todo: Should a zero-frame animation always be finished or never be finished?
                _finished = true;
            }
            else if (!_loopMode.IsLooping() && !_delaying)
            {
                _finished = _playbackFrameTime >= mirroredFrameCount;
            }

            // Compute mirroring state. We're mirroring if we're in the second half of the animation.
            _mirroring = false;
            if (_loopMode.IsMirrored() && !_finished && FrameCount != 0)
            {
                if (_loopMode.IsLooping())
                {
                    var mirroredFrameTime = GeomMath.PositiveModulus(_playbackFrameTime, mirroredFrameCount);
                    _mirroring = mirroredFrameTime >= animFrameCount;
                }
                else
                {
                    _mirroring = (_playbackFrameTime >= animFrameCount && _playbackFrameTime < mirroredFrameCount);
                }
            }

            _playbackStateChanged = false;
        }


        // Note: Currently loopedFrameDelta is no different than the unlooped FrameDelta,
        // but keep it in-case we add support for non-integer frame counts (due to varying FPS of animation objects).
        private double LoopFrameTime(AnimationObject animationObject = null)
        {
            ComputePlaybackFrameTime();

            var speed = animationObject?.Speed ?? 1f;
            double frameCount = animationObject?.FrameCount ?? 0;
            if (frameCount == 0)
            {
                // Not all animation objects define individual frame counts.
                // todo: We can't use Speed if the object doesn't have its own frame count... or can we?
                speed = 1f;
                frameCount = FrameCount;
            }
            // We can't animate if animation frame count is zero, even if using object's frame count.
            if (FrameCount == 0 || frameCount == 0 || speed == 0)
            {
                return 0d;
            }

            // Check if timing is done in reverse. DON'T use the Reverse property from here on out!
            // If speed is negative and Reverse is true, then we're just going forwards.
            var reverse = Reverse != (speed < 0);
            // Convert from animation to object frame time.
            var frameTime = _playbackFrameTime * Math.Abs(speed);
            // Convert from animation to object frame counts.
            var animFrameCount = (double)FrameCount * Math.Abs(speed);

            var closeToStart = false;

            switch (_loopMode)
            {
                default:
                case AnimationLoopMode.Once:
                    frameTime = Math.Min(frameTime, animFrameCount);
                    // Same functionality as Loop after capping.
                    goto case AnimationLoopMode.Loop;

                case AnimationLoopMode.Loop:
                    if (reverse)
                    {
                        frameTime = -frameTime;
                    }
                    frameTime = GeomMath.PositiveModulus(frameTime, frameCount);
                    closeToStart = reverse != (Time == 0);
                    break;

                case AnimationLoopMode.UnsyncedLoop:
                    if (reverse)
                    {
                        frameTime = -frameTime;
                    }
                    // This loop method effectively resets all object times
                    // to 0 after looping, so % animFrameCount first.
                    frameTime = GeomMath.PositiveModulus(frameTime, animFrameCount);
                    frameTime = GeomMath.PositiveModulus(frameTime, frameCount);
                    closeToStart = reverse != (Time == 0);
                    break;

                case AnimationLoopMode.MirrorOnce:
                    frameTime = Math.Min(frameTime, animFrameCount * 2);
                    // Same functionality as MirrorLoop after capping.
                    goto case AnimationLoopMode.MirrorLoop;

                case AnimationLoopMode.MirrorLoop:
                    frameTime = GeomMath.PositiveModulus(frameTime, animFrameCount * 2);
                    if (_mirroring) //if (frameTime >= animFrameCount)
                    {
                        frameTime = animFrameCount * 2 - frameTime; // Mirror animation on way back.
                    }
                    if (reverse)
                    {
                        frameTime = -frameTime; // Animation starts from end then mirrors at beginning.
                    }
                    frameTime = GeomMath.PositiveModulus(frameTime, frameCount);
                    closeToStart = !reverse;
                    break;
            }

            if (_finished || _delaying || Time == 0)
            {
                var timeMultiplier = FPS * Math.Abs(speed);
                frameTime = CloseToFrameCount(closeToStart, frameTime, frameCount, timeMultiplier);
            }

            // Enforce >=0 so that objects will at least animate their first frame.
            return Math.Max(0d, frameTime);
        }

        private double LoopFrameTime(AnimationObject animationObject, out long loopedFrameIndex, out float loopedFrameDelta)
        {
            var loopedFrameTime = LoopFrameTime(animationObject);
            loopedFrameIndex = (long)Math.Floor(loopedFrameTime);
            loopedFrameDelta = (float)GeomMath.PositiveModulus(loopedFrameTime, 1d);
            return loopedFrameTime;
        }


        // Returns 0 or frameCount if frameTime is close to it.
        // Useful for when Modulus calculations prevent the animation from displaying in its end state.
        private static double CloseToFrameCount(bool start, double frameTime, double frameCount, double timeMultiplier)
        {
            if (timeMultiplier == 0)
            {
                return 0d;
            }

            //const double EPSILON = 0.0001d;
            var frameEpsilon = EPSILON / timeMultiplier;

            bool near;
            if (frameTime >= frameCount / 2)
            {
                near = (frameTime + frameEpsilon >= frameCount); // Check near frameCount
            }
            else
            {
                near = (frameTime - frameEpsilon <= 0d); // Check near zero
            }
            if (near)
            {
                frameTime = start ? 0d : frameCount; //(frameCount - frameEpsilon);
            }
            return frameTime;
        }

        private static float GetInterpolator(AnimationFrame frame, double frameTime)
        {
            if (frameTime < frame.FrameTime)
            {
                return 0f;
            }
            else if (frameTime >= frame.FrameEnd)
            {
                return 1f; // Also accounts for if range is zero.
            }
            else
            {
                return GeomMath.Clamp(((float)(frameTime - frame.FrameTime) / frame.FrameDuration), 0f, 1f);
            }
        }

        private static float GetInterpolator(AnimationFrame frame, long frameIndex, float frameDelta)
        {
            return GetInterpolator(frame, (double)frameIndex + frameDelta);
        }

        private static Vector3 Interpolate(InterpolationType interpType, Vector3? src, Vector3[] curve, Vector3? dst, float delta)
        {
            switch (interpType)
            {
                case InterpolationType.Linear:
                    if (!src.HasValue)// || !dst.HasValue)
                    {
                        break; // Missing source value //or missing destination value
                    }
                    return Vector3.Lerp(src.Value, dst ?? src.Value, delta);
                case InterpolationType.Bezier:
                    if (curve == null || curve.Length < 3 || !dst.HasValue)
                    {
                        break; // Invalid curve array or missing destination value
                    }
                    return GeomMath.InterpolateBezierCurve(curve[0], curve[1], curve[2], dst.Value, delta);
                case InterpolationType.BSpline:
                    if (curve == null || curve.Length < 3 || !dst.HasValue)
                    {
                        break; // Invalid curve array or missing destination value
                    }
                    return GeomMath.InterpolateBSpline(curve[0], curve[1], curve[2], dst.Value, delta);
                case InterpolationType.BetaSpline:
                    if (curve == null || curve.Length < 3 || !dst.HasValue)
                    {
                        break; // Invalid curve array or missing destination value
                    }
                    //return GeomMath.InterpolateBetaSpline(curve[0], curve[1], curve[2], dst.Value, delta);
                    break; // Not supported yet
            }
            return Vector3.Zero;
        }
    }
}

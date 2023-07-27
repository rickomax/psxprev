using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PSXPrev.Common.Animator
{
    public enum AnimationType
    {
        Common,
        VertexDiff,
        NormalDiff,
        RPYDiff,
        MatrixDiff,
        AxisDiff,
        HMD, // Multiple methods of interpolation that can change between frames.
    }

    public class Animation
    {
        private readonly WeakReference<RootEntity> _ownerEntity = new WeakReference<RootEntity>(null);

        [DisplayName("Animation Name")]
        public string AnimationName { get; set; }

        [DisplayName("Frame Count"), ReadOnly(true)]
        public uint FrameCount { get; set; }

        [DisplayName("Frames per Second")]
        public float FPS { get; set; } = 1f;

        // Total number of objects managed by this animation, not just number of children in RootAnimationObject.
        [DisplayName("Children"), ReadOnly(true)]
        public int ObjectCount { get; set; }

        [Browsable(false)]
        public AnimationObject RootAnimationObject { get; set; }

        [DisplayName("Animation Type")]
        public AnimationType AnimationType { get; set; }

        // The owner model that created this animation (if any).
        [Browsable(false)]
        public RootEntity OwnerEntity
        {
            get => _ownerEntity.TryGetTarget(out var owner) ? owner : null;
            set => _ownerEntity.SetTarget(value);
        }

        // TOD TMD bindings
        public Dictionary<uint, uint> TMDBindings = new Dictionary<uint, uint>();

        // When animation objects have their own frame counts/speeds, they will become unsynced as the animation loops.
        [DisplayName("Unsynced Objects")]
        public bool HasUnsyncedObjects
        {
            get
            {
                var queue = new Queue<AnimationObject>();
                queue.Enqueue(RootAnimationObject);
                while (queue.Count > 0)
                {
                    var animationObject = queue.Dequeue();
                    var objectFrameCount = animationObject.FrameCount;
                    // This lazily checks if frame count or speed doesn't match.
                    // It doesn't check if the different speed would actually result in the same frame count.
                    if (objectFrameCount != 0 && (objectFrameCount != FrameCount || animationObject.Speed != 1f))
                    {
                        return true;
                    }
                    foreach (var child in animationObject.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
                return false;
            }
        }

        public override string ToString()
        {
            var name = AnimationName ?? nameof(Animation);
            return $"{name} Frames={FrameCount} Objects={ObjectCount}";
        }


        // Assign a flat collection of objects and setup parenting and hierarchy.
        public void AssignObjects(Dictionary<uint, AnimationObject> animationObjects, bool calcObjectFrameCounts, bool calcFrameDurations)
        {
            RootAnimationObject = new AnimationObject();
            ObjectCount = animationObjects.Count;

            var maxAnimFrameCount = 0d;
            foreach (var animationObject in animationObjects.Values)
            {
                // Compute frame count for object and durations for frames.
                AnimationFrame lastAnimationFrame = null;
                uint maxObjectFrameCount = 0;
                foreach (var animationFrame in animationObject.AnimationFrames.Values.OrderBy(af => af.FrameTime))
                {
                    if (calcFrameDurations && lastAnimationFrame != null)
                    {
                        lastAnimationFrame.FrameDuration = animationFrame.FrameTime - lastAnimationFrame.FrameTime;
                    }
                    lastAnimationFrame = animationFrame;
                    maxObjectFrameCount = Math.Max(maxObjectFrameCount, animationFrame.FrameEnd);
                }

                if (calcObjectFrameCounts)
                {
                    animationObject.FrameCount = maxObjectFrameCount;
                }
                // Faster speeds means the actual animation has less frames.
                var speed = animationObject.Speed;
                maxAnimFrameCount = Math.Max(maxAnimFrameCount, (speed == 0 ? 0 : (double)maxObjectFrameCount / speed));

                // Assign parent and add to animation.
                if (animationObject.ParentID != 0 && animationObjects.TryGetValue(animationObject.ParentID, out var parent))
                {
                    animationObject.Parent = parent;
                    parent.Children.Add(animationObject);
                }
                else
                {
                    animationObject.Parent = RootAnimationObject;
                    RootAnimationObject.Children.Add(animationObject);
                }
            }

            FrameCount = (uint)Math.Ceiling(maxAnimFrameCount);
            if (calcObjectFrameCounts)
            {
                RootAnimationObject.FrameCount = FrameCount;
            }
        }
    }
}
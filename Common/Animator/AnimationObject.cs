using System.Collections.Generic;
using System.ComponentModel;

namespace PSXPrev.Common.Animator
{
    public class AnimationObject
    {
        [DisplayName("Frames")]
        public Dictionary<uint, AnimationFrame> AnimationFrames { get; set; }
        
        [Browsable(false)]
        public AnimationObject Parent { get; set; }

        [Browsable(false)]
        public List<AnimationObject> Children { get; set; }

        [Browsable(false)]
        public Animation Animation { get; set; }

        [Browsable(false)]
        public uint ParentID { get; set; }

        [ReadOnly(true)]
        public uint ID { get; set; }

        // Looping frame count for individual animation objects. Keep as 0 to use Animation.FrameCount.
        [DisplayName("Object Frame Count"), ReadOnly(true)]
        public uint FrameCount { get; set; }

        // Speed multiplier for individual animation objects.
        [DisplayName("Object Speed")]
        public float Speed { get; set; } = 1f;

        [DisplayName("TMD ID")]
        public List<uint> TMDID { get; set; } = new List<uint>();

        [DisplayName("Handles Root")]
        public bool HandlesRoot { get; set; }

        public AnimationObject()
        {
            Children = new List<AnimationObject>();
            AnimationFrames = new Dictionary<uint, AnimationFrame>();
        }
    }
}
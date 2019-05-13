using System.ComponentModel;

namespace PSXPrev
{
    public class Animation
    {
        [DisplayName("Animation Name")]
        public string AnimationName { get; set; }

        [DisplayName("Frame Count"), ReadOnly(true)]
        public uint FrameCount { get; set; }

        [DisplayName("Frames per Second")]
        public float FPS { get; set; }

        [DisplayName("Children"), ReadOnly(true)]
        public int ObjectCount { get; set; }

        [Browsable(false)]
        public AnimationObject RootAnimationObject { get; set; }
    }
}
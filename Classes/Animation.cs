using System.ComponentModel;

namespace PSXPrev.Classes
{
    public enum AnimationType
    {
        Common,
        VertexDiff,
        NormalDiff,
        RPYDiff,
        MatrixDiff,
        AxisDiff
    }

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

        public AnimationType AnimationType { get; set; }

    }
}
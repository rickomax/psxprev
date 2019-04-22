using System.ComponentModel;
using System.Drawing.Design;

namespace PSXPrev
{
    public class Animation
    {
        [DisplayName("Animation Name")]
        public string AnimationName { get; set; }

        [DisplayName("Frame Count"), ReadOnly(true)]
        public uint FrameCount { get; set; }

        [DisplayName("Frames per Second"), ReadOnly(true)]
        public float FPS { get; set; }

        [DisplayName("Children"), ReadOnly(true)]
        public int ObjectCount { get; set; }

        [DisplayName("Preview Model")]
        [Editor(typeof(RootEntitySelectorEditor), typeof(UITypeEditor))]
        public RootEntity RootEntity { get; set; }

        [Browsable(false)]
        public AnimationObject RootAnimationObject { get; set; }
    }
}
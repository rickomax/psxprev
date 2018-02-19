using System.Collections.Generic;
using System.ComponentModel;

namespace PSXPrev
{
    public class AnimationObject
    {
        [Browsable(false)]
        public Dictionary<int, AnimationFrame> AnimationFrames { get; set; }

        [Browsable(false)]
        public AnimationObject Parent { get; set; }

        [Browsable(false)]
        public List<AnimationObject> Children { get; set; }

        [Browsable(false)]
        public Animation Animation { get; set; }

        [DisplayName("Visible")]
        public bool Visible { get; set; }

        [Browsable(false)]
        public int ParentID { get; set; }

        [DisplayName("TMD ID")]
        public int TMDID { get; set; }

        [Browsable(false)]
        public bool IsSelected { get; set; }

        public AnimationObject()
        {
            Children = new List<AnimationObject>();
            AnimationFrames = new Dictionary<int, AnimationFrame>();
        }
    }
}
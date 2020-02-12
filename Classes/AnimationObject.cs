using System.Collections.Generic;
using System.ComponentModel;

namespace PSXPrev.Classes
{
    public class AnimationObject
    {
        [Browsable(false)]
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

        [DisplayName("TMD ID")]
        public uint? TMDID { get; set; }

        public AnimationObject()
        {
            Children = new List<AnimationObject>();
            AnimationFrames = new Dictionary<uint, AnimationFrame>();
        }
    }
}
using OpenTK;

namespace PSXPrev
{
    public class AnimationFrame
    {
        public bool? AbsoluteMatrix { get; set; }
        public Vector3? Rotation { get; set; }
        public Vector3? Scale { get; set; }
        public Vector3? Translation { get; set; }
        public Matrix4? Matrix { get; set; }
        public int FrameTime { get; set; }
    }
}
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Classes
{
    public class AnimationFrame
    {
        [Browsable(false)]
        public AnimationObject AnimationObject { get; set; }

        [Browsable(false)]
        public bool AbsoluteMatrix { get; set; }

        [Browsable(false)]
        public Quaternion? Rotation { get; set; }

        [Browsable(false)]
        public Vector3? EulerRotation { get; set; }

        [Browsable(false)]
        public Vector3? Scale { get; set; }

        [Browsable(false)]
        public Vector3? Translation { get; set; }
        
        [ReadOnly(true)]
        public uint FrameTime { get; set; }

        [ReadOnly(true)]
        public int VertexCount
        {
            get => Vertices == null ? 0 : Vertices.Length;
        }
        
        [Browsable(false)]
        public Vector3[] Vertices;

        [Browsable(false)]
        public Vector3[] TempVertices;

        public float RotationX => Rotation?.X ?? 0f;


        public float RotationY => Rotation?.Y ?? 0f;

        public float RotationZ => Rotation?.Z ?? 0f;

        public float ScaleX => Scale?.X ?? 0f;


        public float ScaleY => Scale?.Y ?? 0f;

        public float ScaleZ => Scale?.Z ?? 0f;

        public float PositionX => Translation?.X ?? 0f;


        public float PositionY => Translation?.Y ?? 0f;

        public float PositionZ => Translation?.Z ?? 0f;
    }
}
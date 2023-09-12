using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Common.Animator
{
    public class AnimationFrame
    {
        [Browsable(false)]
        public AnimationObject AnimationObject { get; set; }

        [Browsable(false)]
        public bool AbsoluteMatrix { get; set; }

        //Matrix coming from TOD files
        [Browsable(false)]
        public Matrix3? Matrix { get; set; }

        //Transfer coming from TOD files
        [Browsable(false)]
        public Vector3? Transfer { get; set; }

        [Browsable(false)]
        public Quaternion? Rotation { get; set; }

        [Browsable(false)]
        public Quaternion? FinalRotation { get; set; }

        [Browsable(false)]
        public Vector3? EulerRotation { get; set; }
        // HMD: 
        [Browsable(false)]
        public Vector3? FinalEulerRotation { get; set; }
        // HMD: Used for Bezier Curve and B-Spline.
        [Browsable(false)]
        public Vector3[] CurveEulerRotations { get; set; }

        [Browsable(false)]
        public InterpolationType RotationType { get; set; }

        [Browsable(false)]
        public RotationOrder RotationOrder { get; set; }

        [Browsable(false)]
        public Vector3? Scale { get; set; }
        // HMD: 
        [Browsable(false)]
        public Vector3? FinalScale { get; set; }
        // HMD: Used for Bezier Curve and B-Spline.
        [Browsable(false)]
        public Vector3[] CurveScales { get; set; }

        [Browsable(false)]
        public InterpolationType ScaleType { get; set; }

        [Browsable(false)]
        public Vector3? Translation { get; set; }
        // HMD: 
        [Browsable(false)]
        public Vector3? FinalTranslation { get; set; }
        // HMD: Used for Bezier Curve and B-Spline.
        [Browsable(false)]
        public Vector3[] CurveTranslations { get; set; }

        [Browsable(false)]
        public InterpolationType TranslationType { get; set; }
        
        [DisplayName("Frame Time"), ReadOnly(true)]
        public uint FrameTime { get; set; }

        // HMD: Not incorporated into any other animation types.
        [DisplayName("Frame Duration"), ReadOnly(true)]
        public uint FrameDuration { get; set; }

        [Browsable(false)]
        public uint FrameEnd => FrameTime + FrameDuration;

        [ReadOnly(true)]
        public int VertexCount => Vertices == null ? 0 : Vertices.Length;
        
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
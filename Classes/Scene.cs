using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class Scene
    {
        public const float CameraFOV = 60.0f;
        public const float CameraFOVRads = CameraFOV * ((float)Math.PI * 2f) / 360f;
        public const float CameraFarClip = 50000f;
        public const float CameraMinDistance = 1f;
        public const float MouseSensivity = 0.0035f;
        public const float MaxCameraPitch = 0.9f;

        public static int AttributeIndexPosition = 0;
        public static int AttributeIndexNormal = 1;
        public static int AttributeIndexColour = 2;
        public static int AttributeIndexUv = 3;
        public static int AttributeIndexIndex = 4;
        public static int AttributeIndexTexture = 5;
        public static int UniformIndexMVP;
        //public static int UniformIndexSelectedTriangle;
        public static int UniformIndexLightDirection;
        //public static int UniformIndexLine;

        public const string AttributeNamePosition = "in_Position";
        public const string AttributeNameNormal = "in_Normal";
        public const string AttributeNameColour = "in_Color";
        public const string AttributeNameUv = "in_Uv";
        //public const string AttributeNameIndex = "in_Index";
        public const string AttributeNameTexture = "mainTex";
        public const string UniformNameMVP = "mvpMatrix";
        //public const string UniformNameSelectedTriangle = "selectedIndex";
        public const string UniformNameLightDirection = "lightDirection";
        //public const string UniformNameLine = "line";

        public MeshBatch MeshBatch { get; set; }
        //public LineBatch LineBatch { get; set; }
        public AnimationBatch AnimationBatch { get; set; }
        public TextureBinder TextureBinder { get; set; }

        private Vector4 _transformedLight;
        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        private int _shaderProgram;

        private Color _clearColor;
        public Color ClearColor
        {
            get { return _clearColor; }
            set
            {
                GL.ClearColor(value.R, value.G, value.B, 0.0f);
                _clearColor = value;
            }
        }

        private bool _wireFrame;
        public bool WireFrame
        {
            get { return _wireFrame; }
            set
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, value ? PolygonMode.Line : PolygonMode.Fill);
                //GL.CullFace(CullFaceMode.Back);
                _wireFrame = value;
            }
        }

        private float _cameraDistanceIncrement = 1000f;
        public float CameraDistanceIncrement
        {
            get { return _cameraDistanceIncrement; }
        }

        private Vector3 _cameraPosition;
        public Vector3 CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                _cameraPosition = value;
                UpdateViewMatrix();
            }
        }


        private Vector3 _lightRotation;
        public Vector3 LightRotation
        {
            get { return _lightRotation; }
            set
            {
                _lightRotation = value;
                UpdateLightRotation();
            }
        }

        private Vector3 _cameraCenter;
        public Vector3 CameraCenter
        {
            get { return _cameraCenter; }
            set
            {
                _cameraCenter = value;
                UpdateViewMatrix();
            }
        }

        private float _cameraDistance;
        public float CameraDistance
        {
            get { return _cameraDistance; }
            set
            {
                _cameraDistance = Math.Max(CameraMinDistance, value);
                UpdateViewMatrix();
            }
        }

        private float _cameraYaw;
        public float CameraYaw
        {
            get { return _cameraYaw; }
            set
            {
                _cameraYaw = value;
                UpdateViewMatrix();
            }
        }

        private float _cameraPitch;
        public float CameraPitch
        {
            get { return _cameraPitch; }
            set
            {
                _cameraPitch = Math.Max(-MaxCameraPitch, Math.Min(MaxCameraPitch, value));
                UpdateViewMatrix();
            }
        }

        public void Initialise(float width, float height)
        {
            SetupGL();
            SetupShaders();
            SetupMatrices(width, height, CameraFarClip);
            SetupInternals();
            LightRotation = new Vector3(1f, -1f, -1f);
        }

        private void SetupInternals()
        {
            MeshBatch = new MeshBatch(this);
            //LineBatch = new LineBatch();
            AnimationBatch = new AnimationBatch(this);
            TextureBinder = new TextureBinder();
        }

        private void SetupGL()
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.ClearDepth(1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            WireFrame = false;
        }

        private void SetupShaders()
        {
            var vertexShaderSource = ManifestResourceLoader.LoadTextFile("Shaders\\Shader.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("Shaders\\Shader.frag");

            _shaderProgram = GL.CreateProgram();

            var vertexShaderAddress = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderAddress, vertexShaderSource);
            GL.CompileShader(vertexShaderAddress);
            GL.AttachShader(_shaderProgram, vertexShaderAddress);

            var fragmentShaderAddress = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderAddress, fragmentShaderSource);
            GL.CompileShader(fragmentShaderAddress);
            GL.AttachShader(_shaderProgram, fragmentShaderAddress);

            var attributes = new Dictionary<int, string>
            {
                {AttributeIndexPosition, AttributeNamePosition},
                {AttributeIndexNormal, AttributeNameNormal},
                {AttributeIndexColour, AttributeNameColour},
                {AttributeIndexUv, AttributeNameUv},
                //{AttributeIndexIndex, AttributeNameIndex},
                {AttributeIndexTexture, AttributeNameTexture}
            };
            foreach (var vertexAttributeLocation in attributes)
                GL.BindAttribLocation(_shaderProgram, vertexAttributeLocation.Key, vertexAttributeLocation.Value);

            GL.LinkProgram(_shaderProgram);

            if (!GetLinkStatus())
            {
                throw new Exception(GetInfoLog());
            }

            UniformIndexMVP = GL.GetUniformLocation(_shaderProgram, UniformNameMVP);
            //UniformIndexSelectedTriangle = GL.GetUniformLocation(_shaderProgram, UniformNameSelectedTriangle);
            UniformIndexLightDirection = GL.GetUniformLocation(_shaderProgram, UniformNameLightDirection);
            //UniformIndexLine = GL.GetUniformLocation(_shaderProgram, UniformNameLine);
        }

        public bool GetLinkStatus()
        {
            int[] parameters = { 0 };
            GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, parameters);
            return parameters[0] == 1;
        }

        public string GetInfoLog()
        {
            int[] infoLength = { 0 };
            GL.GetProgram(_shaderProgram, GetProgramParameterName.InfoLogLength  , infoLength);
            var bufSize = infoLength[0];

            //  Get the compile info.
            var il = new StringBuilder(bufSize);
            int bufferLength;
            GL.GetProgramInfoLog(_shaderProgram, bufSize, out bufferLength, il);

            return il.ToString();
        }

        private void SetupMatrices(float width, float height, float farClip)
        {
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(CameraFOVRads, width / height, 0.1f, farClip);
        }

        private void UpdateLightRotation()
        {
            _transformedLight = GeomUtils.CreateR(_lightRotation) * Vector4.One;
        }

        private void UpdateViewMatrix()
        {
            var mat = Matrix4.CreateRotationY(_cameraYaw) * Matrix4.CreateRotationX(_cameraPitch);
            var eye = mat * new Vector4(0f, 0f, -_cameraDistance, 1f);
            _viewMatrix = Matrix4.LookAt(new Vector3(eye.X, eye.Y, eye.Z), _cameraCenter, new Vector3(0f, -1f, 0f));
        }

        public void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);

            //GL.Uniform1(UniformIndexSelectedTriangle, selectedTriangle);
            GL.Uniform3(UniformIndexLightDirection, _transformedLight.X, _transformedLight.Y, _transformedLight.Z);

            //GL.Uniform1(UniformIndexLine, 0);
            MeshBatch.Draw(_viewMatrix, _projectionMatrix, TextureBinder);

            //GL.Uniform1(UniformIndexLine, 1);
            //LineBatch.SetupAndDraw(_viewMatrix, _projectionMatrix);

            GL.UseProgram(0);
        }
        
        public void FocusOnBounds(BoundingBox bounds)
        {
            _cameraYaw = 0f;
            _cameraPitch = 0f;
            DistanceToFitBounds(bounds);
        }

        public void FocusOnHeight(float height)
        {
            DistanceToFitHeight(height);
        }

        public void UpdateTexture(Bitmap textureBitmap, int texturePage)
        {
            TextureBinder.UpdateTexture(textureBitmap, texturePage);
        }

        public void DistanceToFitBounds(BoundingBox bounds)
        {
            var center = CameraCenter;
            var boundSphereRadius = bounds.Corners.Select(x => GeomUtils.VecDistance(x, center)).Max();
            var camDistance = boundSphereRadius / 2.0f / (float)Math.Tan(CameraFOVRads / 2.0f) * 2f;
            _cameraDistanceIncrement = camDistance * 0.25f;
            CameraDistance = camDistance;
        }

        public void DistanceToFitHeight(float height)
        {
            var camDistance = (height / 2.0f) / (float)Math.Tan(CameraFOVRads / 2.0f) * 2f;
            _cameraDistanceIncrement = camDistance * 0.25f;
            CameraDistance = camDistance;
        }
    }
}
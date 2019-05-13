using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev
{
    public class Scene
    {
        private const float CameraFOV = 60.0f;
        private const float CameraFOVRads = CameraFOV * ((float)Math.PI * 2f) / 360f;
        private const float CameraNearClip = 0.1f;
        private const float CameraFarClip = 500000f;
        private const float CameraMinDistance = 0.01f;
        private const float MaxCameraPitch = 0.9f;
        private const float GizmoHeight = 0.075f;
        private const float GizmoWidth = 0.005f;
        private const float CameraDistanceIncrementFactor = 0.25f;
        private const float CameraPanIncrementFactor = 0.5f;

        public static Vector3 XGizmoDimensions = new Vector3(GizmoHeight, GizmoWidth, GizmoWidth);
        public static Vector3 YGizmoDimensions = new Vector3(GizmoWidth, GizmoHeight, GizmoWidth);
        public static Vector3 ZGizmoDimensions = new Vector3(GizmoWidth, GizmoWidth, GizmoHeight);

        public static int AttributeIndexPosition = 0;
        public static int AttributeIndexNormal = 1;
        public static int AttributeIndexColour = 2;
        public static int AttributeIndexUv = 3;
        //public static int AttributeIndexIndex = 4;
        public static int AttributeIndexTexture = 5;
        public static int UniformIndexMVP;
        public static int UniformIndexLightDirection;

        public const string AttributeNamePosition = "in_Position";
        public const string AttributeNameNormal = "in_Normal";
        public const string AttributeNameColour = "in_Color";
        public const string AttributeNameUv = "in_Uv";
        public const string AttributeNameTexture = "mainTex";
        public const string UniformNameMVP = "mvpMatrix";
        public const string UniformNameLightDirection = "lightDirection";

        public enum GizmoId
        {
            None,
            XMover,
            YMover,
            ZMover
        }

        public MeshBatch MeshBatch { get; private set; }
        public MeshBatch GizmosMeshBatch { get; private set; }
        public LineBatch BoundsBatch { get; private set; }
        public LineBatch SkeletonBatch { get; private set; }
        public AnimationBatch AnimationBatch { get; private set; }
        public TextureBinder TextureBinder { get; private set; }

        private Vector4 _transformedLight;
        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        private bool _viewMatrixValid;
        private int _shaderProgram;
        private Vector3 _rayOrigin;
        private Vector3 _rayTarget;
        private Vector3 _rayDirection;
        private Vector3? _projected;

        private Color _clearColor;
        public Color ClearColor
        {
            get => _clearColor;
            set
            {
                GL.ClearColor(value.R, value.G, value.B, 0.0f);
                _clearColor = value;
            }
        }

        public bool Wireframe { get; set; }

        public bool ShowGizmos { get; set; } = true;

        public bool ShowBounds { get; set; } = true;

        public bool ShowSkeleton { get; set; }

        public float CameraDistanceIncrement => CameraDistanceToOrigin * CameraDistanceIncrementFactor;

        public float CameraPanIncrement => CameraDistanceToOrigin * CameraPanIncrementFactor;

        private float _cameraDistance;
        public float CameraDistance
        {
            get => _cameraDistance;
            set => _cameraDistance = Math.Max(CameraMinDistance, value);
        }

        private float _cameraYaw;
        public float CameraYaw
        {
            get => _cameraYaw;
            set => _cameraYaw = value;
        }

        private float _cameraPitch;
        public float CameraPitch
        {
            get => _cameraPitch;
            set => _cameraPitch = Math.Max(-MaxCameraPitch, Math.Min(MaxCameraPitch, value));
        }

        public float CameraX { get; set; }

        public float CameraY { get; set; }

        public Quaternion CameraRotation => _viewMatrix.Inverted().ExtractRotation();

        public Vector3 CameraDirection => CameraRotation * GeomUtils.ZVector;

        public float CameraDistanceToOrigin => -_viewMatrix.ExtractTranslation().Z;

        private Vector3 _lightRotation;
        public Vector3 LightRotation
        {
            get => _lightRotation;
            set
            {
                _lightRotation = value;
                UpdateLightRotation();
            }
        }

        public void Initialise(float width, float height)
        {
            SetupGL();
            SetupShaders();
            SetupMatrices(width, height, CameraNearClip, CameraFarClip);
            SetupInternals();
            LightRotation = new Vector3(1f, -1f, -1f);
        }

        private void SetupInternals()
        {
            MeshBatch = new MeshBatch(this);
            GizmosMeshBatch = new MeshBatch(this);
            BoundsBatch = new LineBatch();
            SkeletonBatch = new LineBatch();
            AnimationBatch = new AnimationBatch(this);
            TextureBinder = new TextureBinder();
        }

        private void SetupGL()
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            Wireframe = false;
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
                {AttributeIndexTexture, AttributeNameTexture}
            };
            foreach (var vertexAttributeLocation in attributes)
            {
                GL.BindAttribLocation(_shaderProgram, vertexAttributeLocation.Key, vertexAttributeLocation.Value);
            }
            GL.LinkProgram(_shaderProgram);
            if (!GetLinkStatus())
            {
                throw new Exception(GetInfoLog());
            }
            UniformIndexMVP = GL.GetUniformLocation(_shaderProgram, UniformNameMVP);
            UniformIndexLightDirection = GL.GetUniformLocation(_shaderProgram, UniformNameLightDirection);
        }

        private bool GetLinkStatus()
        {
            int[] parameters = { 0 };
            GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, parameters);
            return parameters[0] == 1;
        }

        private string GetInfoLog()
        {
            int[] infoLength = { 0 };
            GL.GetProgram(_shaderProgram, GetProgramParameterName.InfoLogLength, infoLength);
            var bufSize = infoLength[0];
            var il = new StringBuilder(bufSize);
            int bufferLength;
            GL.GetProgramInfoLog(_shaderProgram, bufSize, out bufferLength, il);
            return il.ToString();
        }

        private void SetupMatrices(float width, float height, float nearClip, float farClip)
        {
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(CameraFOVRads, width / height, nearClip, farClip);
        }

        private void UpdateLightRotation()
        {
            _transformedLight = GeomUtils.CreateR(_lightRotation) * Vector4.One;
        }

        public void UpdateViewMatrix()
        {
            var translation = Matrix4.CreateTranslation(CameraX, -CameraY, 0f);
            var rotation = Matrix4.CreateRotationY(_cameraYaw) * Matrix4.CreateRotationX(_cameraPitch);
            var eye = rotation * new Vector4(0f, 0f, -_cameraDistance, 1f);
            _viewMatrix = Matrix4.LookAt(new Vector3(eye.X, eye.Y, eye.Z), Vector3.Zero, new Vector3(0f, -1f, 0f));
            _viewMatrix *= translation;
            _viewMatrixValid = true;
        }

        public void Draw()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);
            GL.Uniform3(UniformIndexLightDirection, _transformedLight.X, _transformedLight.Y, _transformedLight.Z);
            MeshBatch.Draw(_viewMatrix, _projectionMatrix, TextureBinder, Wireframe);
            if (ShowBounds)
            {
                BoundsBatch.SetupAndDraw(_viewMatrix, _projectionMatrix);
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
            if (ShowGizmos)
            {
                GizmosMeshBatch.Draw(_viewMatrix, _projectionMatrix);
            }
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            if (ShowSkeleton)
            {
                SkeletonBatch.SetupAndDraw(_viewMatrix, _projectionMatrix, 2f);
            }
            GL.UseProgram(0);
        }

        public RootEntity GetRootEntityUnderMouse(RootEntity[] checkedEntities, RootEntity selectedEntity, int x, int y, float width, float height)
        {
            UpdatePicking(x, y, width, height);
            var pickedEntities = new List<Tuple<float, RootEntity>>();
            if (checkedEntities != null)
            {
                foreach (var entity in checkedEntities)
                {
                    if (entity == selectedEntity)
                    {
                        continue;
                    }
                    Vector3 boxMin;
                    Vector3 boxMax;
                    GeomUtils.GetBoxMinMax(entity.Bounds3D.Center, entity.Bounds3D.Extents, out boxMin, out boxMax);
                    var intersectionDistance = GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, boxMin, boxMax);
                    if (intersectionDistance > 0f)
                    {
                        pickedEntities.Add(new Tuple<float, RootEntity>(intersectionDistance, entity));
                    }
                }
            }
            if (selectedEntity != null)
            {
                Vector3 boxMin;
                Vector3 boxMax;
                GeomUtils.GetBoxMinMax(selectedEntity.Bounds3D.Center, selectedEntity.Bounds3D.Extents, out boxMin, out boxMax);
                var intersectionDistance = GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, boxMin, boxMax);
                if (intersectionDistance > 0f)
                {
                    pickedEntities.Add(new Tuple<float, RootEntity>(intersectionDistance, selectedEntity));
                }
            }
            pickedEntities.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return pickedEntities.Count > 0 ? pickedEntities[0].Item2 : null;
        }

        public Vector3 GetBestPlaneNormal(Vector3 a, Vector3 b)
        {
            return Math.Abs(Vector3.Dot(CameraDirection, a)) > 0.5f ? a : b;
        }

        public void FocusOnBounds(BoundingBox bounds)
        {
            _cameraYaw = 0f;
            _cameraPitch = 0f;
            CameraX = 0f;
            CameraY = 0f;
            DistanceToFitBounds(bounds);
        }

        public void UpdateTexture(Bitmap textureBitmap, int texturePage)
        {
            TextureBinder.UpdateTexture(textureBitmap, texturePage);
        }

        private void DistanceToFitBounds(BoundingBox bounds)
        {
            var radius = bounds.MagnitudeFromCenter;
            var distance = radius / (float)Math.Sin(CameraFOVRads * 0.5f) + 0.1f;
            CameraDistance = distance;
            UpdateViewMatrix();
        }

        public GizmoId GetGizmoUnderPosition(int x, int y, float width, float height, EntityBase selectedEntityBase)
        {
            if (!ShowGizmos)
            {
                return GizmoId.None;
            }
            _projected = null;
            if (selectedEntityBase != null)
            {
                UpdatePicking(x, y, width, height);
                var matrix = selectedEntityBase.WorldMatrix;
                var scaleMatrix = GetGizmoScaleMatrix(matrix.ExtractTranslation());
                var finalMatrix = scaleMatrix * matrix;
                Vector3 xMin;
                Vector3 xMax;
                GeomUtils.GetBoxMinMax(XGizmoDimensions, XGizmoDimensions, out xMin, out xMax, finalMatrix);
                if (GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, xMin, xMax) > 0f)
                {
                    return GizmoId.XMover;
                }
                Vector3 yMin;
                Vector3 yMax;
                GeomUtils.GetBoxMinMax(YGizmoDimensions, YGizmoDimensions, out yMin, out yMax, finalMatrix);
                if (GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, yMin, yMax) > 0f)
                {
                    return GizmoId.YMover;
                }
                Vector3 zMin;
                Vector3 zMax;
                GeomUtils.GetBoxMinMax(ZGizmoDimensions, ZGizmoDimensions, out zMin, out zMax, finalMatrix);
                if (GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, zMin, zMax) > 0f)
                {
                    return GizmoId.ZMover;
                }
            }
            return GizmoId.None;
        }

        public Vector3 GetGizmoProjectionOffset(int x, int y, float width, float height, EntityBase entityBase, Vector3 planeNormal, Vector3 projectionNormal)
        {
            var worldMatrix = entityBase.WorldMatrix;
            var planeOrigin = worldMatrix.ExtractTranslation();
            UpdatePicking(x, y, width, height);
            var projected = GeomUtils.PlaneIntersect(_rayOrigin, _rayDirection, planeOrigin, planeNormal).ProjectOnNormal(projectionNormal);
            Vector3 offset;
            if (_projected != null)
            {
                var previousProjected = _projected.Value;
                offset = new Vector3((int)(projected.X - previousProjected.X), (int)(projected.Y - previousProjected.Y), (int)(projected.Z - previousProjected.Z));
            }
            else
            {
                offset = Vector3.Zero;
            }
            _projected = projected;
            return Vector3.TransformVector(offset, worldMatrix.Inverted());
        }

        private void UpdatePicking(int x, int y, float width, float height)
        {
            if (!_viewMatrixValid)
            {
                return;
            }
            _rayOrigin = new Vector3(x, y, CameraNearClip).UnProject(_projectionMatrix, _viewMatrix, width, height);
            _rayTarget = new Vector3(x, y, 1f).UnProject(_projectionMatrix, _viewMatrix, width, height);
            _rayDirection = (_rayTarget - _rayOrigin).Normalized();
        }

        private float CameraDistanceFrom(Vector3 point)
        {
            return (_viewMatrix.Inverted().ExtractTranslation() - point).Length;
        }

        public Matrix4 GetGizmoScaleMatrix(Vector3 position)
        {
            return Matrix4.CreateScale(CameraDistanceFrom(position));
        }
    }
}
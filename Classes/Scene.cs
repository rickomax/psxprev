using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Classes
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
        public static int AttributeIndexColour = 1;
        public static int AttributeIndexNormal = 2;
        public static int AttributeIndexUv = 3;
        public static int AttributeIndexTiledArea = 4;
        public static int AttributeIndexTexture = 5;

        public static int UniformIndexMVP;
        public static int UniformIndexLightDirection;
        public static int UniformMaskColor;
        public static int UniformAmbientColor;
        public static int UniformRenderMode;
        public static int UniformSemiTransparentMode;
        public static int UniformLightIntensity;

        public const string AttributeNamePosition = "in_Position";
        public const string AttributeNameColour = "in_Color";
        public const string AttributeNameNormal = "in_Normal";
        public const string AttributeNameUv = "in_Uv";
        public const string AttributeNameTiledArea = "in_TiledArea";
        public const string AttributeNameTexture = "mainTex";

        public const string UniformNameMVP = "mvpMatrix";
        public const string UniformNameLightDirection = "lightDirection";
        public const string UniformMaskColorName = "maskColor";
        public const string UniformAmbientColorName = "ambientColor";
        public const string UniformRenderModeName = "renderMode";
        public const string UniformSemiTransparentModeName = "semiTransparentMode";
        public const string UniformLightIntensityName = "lightIntensity";

        public enum GizmoId
        {
            None,
            XMover,
            YMover,
            ZMover
        }

        public bool Initialized { get; private set; }

        public MeshBatch MeshBatch { get; private set; }
        public MeshBatch GizmosMeshBatch { get; private set; }
        public LineBatch BoundsBatch { get; private set; }
        public LineBatch TriangleOutlineBatch { get; private set; }
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
        private Vector3? _intersected;
        private List<EntityBase> _lastPickedEntities;
        private List<Tuple<ModelEntity, Triangle>> _lastPickedTriangles;
        private int _lastPickedEntityIndex;
        private int _lastPickedTriangleIndex;

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

        public bool AutoAttach { get; set; }

        public bool Wireframe { get; set; }

        public bool VibRibbonWireframe { get; set; }

        public bool ShowGizmos { get; set; } = true;

        public bool ShowBounds { get; set; } = true;

        public bool ShowSkeleton { get; set; }
        public bool VerticesOnly { get; set; }

        public bool SemiTransparencyEnabled { get; set; } = true;

        public bool ForceDoubleSided { get; set; }

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

        public System.Drawing.Color MaskColor { get; set; }

        public System.Drawing.Color DiffuseColor { get; set; }
        public System.Drawing.Color AmbientColor { get; set; }

        public bool LightEnabled { get; set; } = true;

        public float LightIntensity { get; set; } = 1f;

        public decimal VertexSize
        {
            get => (decimal)GL.GetFloat(GetPName.PointSize);
            set => GL.PointSize((float)value);
        }

        public void Initialize(float width, float height)
        {
            if (Initialized)
            {
                return;
            }
            SetupGL();
            SetupShaders();
            SetupMatrices(width, height, CameraNearClip, CameraFarClip);
            SetupInternals();
            Initialized = true;
        }

        public void Resize(float width, float height)
        {
            GL.Viewport(0, 0, (int)width, (int)height);
            SetupMatrices(width, height, CameraNearClip, CameraFarClip);
        }

        private void SetupInternals()
        {
            MeshBatch = new MeshBatch(this);
            GizmosMeshBatch = new MeshBatch(this);
            BoundsBatch = new LineBatch();
            TriangleOutlineBatch = new LineBatch();
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
                {AttributeIndexTiledArea, AttributeNameTiledArea},
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
            UniformMaskColor = GL.GetUniformLocation(_shaderProgram, UniformMaskColorName);
            UniformAmbientColor = GL.GetUniformLocation(_shaderProgram, UniformAmbientColorName);
            UniformRenderMode = GL.GetUniformLocation(_shaderProgram, UniformRenderModeName);
            UniformSemiTransparentMode = GL.GetUniformLocation(_shaderProgram, UniformSemiTransparentModeName);
            UniformLightIntensity = GL.GetUniformLocation(_shaderProgram, UniformLightIntensityName);
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
            GL.GetProgramInfoLog(_shaderProgram, bufSize, out var bufferLength, out var log);
            return log;
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
            GL.Uniform3(UniformMaskColor, MaskColor.R / 255f, MaskColor.G / 255f, MaskColor.B / 255f);
            GL.Uniform3(UniformAmbientColor, AmbientColor.R / 255f, AmbientColor.G / 255f, AmbientColor.B / 255f);
            GL.Uniform1(UniformLightIntensity, LightIntensity);
            GL.Uniform1(UniformRenderMode, LightEnabled ? 0 : 1);
            GL.Uniform1(UniformSemiTransparentMode, 0);
            MeshBatch.Draw(_viewMatrix, _projectionMatrix, TextureBinder, Wireframe, true, VerticesOnly);
            GL.Uniform1(UniformRenderMode, 2);
            if (ShowBounds)
            {
                BoundsBatch.SetupAndDraw(_viewMatrix, _projectionMatrix);
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
            if (ShowBounds)
            {
                TriangleOutlineBatch.SetupAndDraw(_viewMatrix, _projectionMatrix, 2f);
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);
            if (ShowGizmos)
            {
                GizmosMeshBatch.Draw(_viewMatrix, _projectionMatrix, standard: false);
            }
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            if (ShowSkeleton)
            {
                SkeletonBatch.SetupAndDraw(_viewMatrix, _projectionMatrix, 2f);
            }
            GL.UseProgram(0);
        }

        public EntityBase GetEntityUnderMouse(RootEntity[] checkedEntities, RootEntity selectedRootEntity, int x, int y, float width, float height, bool selectRoot = false)
        {
            UpdatePicking(x, y, width, height);
            var pickedEntities = new List<EntityBase>();
            if (!selectRoot)
            {
                if (checkedEntities != null)
                {
                    foreach (var entity in checkedEntities)
                    {
                        if (entity.ChildEntities != null)
                        {
                            foreach (var subEntity in entity.ChildEntities)
                            {
                                CheckEntity(subEntity, pickedEntities);
                            }
                        }
                    }
                }
                else if (selectedRootEntity != null)
                {
                    if (selectedRootEntity.ChildEntities != null)
                    {
                        foreach (var subEntity in selectedRootEntity.ChildEntities)
                        {
                            CheckEntity(subEntity, pickedEntities);
                        }
                    }
                }
            }
            pickedEntities.Sort((a, b) => a.IntersectionDistance.CompareTo(b.IntersectionDistance));
            if (!ListsMatches(pickedEntities, _lastPickedEntities))
            {
                _lastPickedEntityIndex = 0;
            }
            var pickedEntity = pickedEntities.Count > 0 ? pickedEntities[_lastPickedEntityIndex] : null;
            if (_lastPickedEntityIndex < pickedEntities.Count - 1)
            {
                _lastPickedEntityIndex++;
            }
            else
            {
                _lastPickedEntityIndex = 0;
            }
            _lastPickedEntities = pickedEntities;
            return pickedEntity;
        }

        public Tuple<ModelEntity, Triangle> GetTriangleUnderMouse(RootEntity[] checkedEntities, RootEntity selectedRootEntity, int x, int y, float width, float height, bool selectRoot = false)
        {
            UpdatePicking(x, y, width, height);
            var pickedTriangles = new List<Tuple<ModelEntity, Triangle>>();
            if (!selectRoot)
            {
                if (checkedEntities != null)
                {
                    foreach (var entity in checkedEntities)
                    {
                        if (entity.ChildEntities != null)
                        {
                            foreach (var subEntity in entity.ChildEntities)
                            {
                                CheckTriangles(subEntity, pickedTriangles);
                            }
                        }
                    }
                }
                else if (selectedRootEntity != null)
                {
                    if (selectedRootEntity.ChildEntities != null)
                    {
                        foreach (var subEntity in selectedRootEntity.ChildEntities)
                        {
                            CheckTriangles(subEntity, pickedTriangles);
                        }
                    }
                }
            }
            pickedTriangles.Sort((a, b) => a.Item2.IntersectionDistance.CompareTo(b.Item2.IntersectionDistance));
            if (!ListsMatches(pickedTriangles, _lastPickedTriangles))
            {
                _lastPickedTriangleIndex = 0;
            }
            var pickedTriangle = pickedTriangles.Count > 0 ? pickedTriangles[_lastPickedTriangleIndex] : null;
            if (_lastPickedTriangleIndex < pickedTriangles.Count - 1)
            {
                _lastPickedTriangleIndex++;
            }
            else
            {
                _lastPickedTriangleIndex = 0;
            }
            _lastPickedTriangles = pickedTriangles;
            return pickedTriangle;
        }

        private static bool ListsMatches<T>(List<T> a, List<T> b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Count != b.Count)
            {
                return false;
            }
            for (var i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckEntity(EntityBase entity, List<EntityBase> pickedEntities)
        {
            GeomUtils.GetBoxMinMax(entity.Bounds3D.Center, entity.Bounds3D.Extents, out var boxMin, out var boxMax);
            var intersectionDistance = GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, boxMin, boxMax);
            if (intersectionDistance > 0f)
            {
                entity.IntersectionDistance = intersectionDistance;
                pickedEntities.Add(entity);
            }
        }

        private void CheckTriangles(EntityBase entity, List<Tuple<ModelEntity, Triangle>> pickedTriangles)
        {
            if (entity is ModelEntity modelEntity && modelEntity.Triangles.Length > 0)
            {
                var worldMatrix = modelEntity.WorldMatrix;
                foreach (var triangle in modelEntity.Triangles)
                {
                    var vertex0 = Vector3.TransformPosition(triangle.Vertices[0], worldMatrix);
                    var vertex1 = Vector3.TransformPosition(triangle.Vertices[1], worldMatrix);
                    var vertex2 = Vector3.TransformPosition(triangle.Vertices[2], worldMatrix);

                    var intersectionDistance = GeomUtils.TriangleIntersect(_rayOrigin, _rayDirection, vertex0, vertex1, vertex2);
                    if (intersectionDistance > 0f)
                    {
                        triangle.IntersectionDistance = intersectionDistance;
                        pickedTriangles.Add(new Tuple<ModelEntity, Triangle>(modelEntity, triangle));
                    }
                }
            }
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

        public GizmoId GetGizmoUnderPosition(EntityBase selectedEntityBase)
        {
            if (!ShowGizmos)
            {
                return GizmoId.None;
            }
            if (selectedEntityBase != null)
            {
                var matrix = Matrix4.CreateTranslation(selectedEntityBase.Bounds3D.Center);
                var scaleMatrix = GetGizmoScaleMatrix(matrix.ExtractTranslation());
                var finalMatrix = scaleMatrix * matrix;
                GeomUtils.GetBoxMinMax(XGizmoDimensions, XGizmoDimensions, out var xMin, out var xMax, finalMatrix);
                if (GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, xMin, xMax) > 0f)
                {
                    return GizmoId.XMover;
                }
                GeomUtils.GetBoxMinMax(YGizmoDimensions, YGizmoDimensions, out var yMin, out var yMax, finalMatrix);
                if (GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, yMin, yMax) > 0f)
                {
                    return GizmoId.YMover;
                }
                GeomUtils.GetBoxMinMax(ZGizmoDimensions, ZGizmoDimensions, out var zMin, out var zMax, finalMatrix);
                if (GeomUtils.BoxIntersect(_rayOrigin, _rayDirection, zMin, zMax) > 0f)
                {
                    return GizmoId.ZMover;
                }
            }
            return GizmoId.None;
        }

        public Vector3 GetPickedPosition(Vector3 onNormal)
        {
            return GeomUtils.PlaneIntersect(_rayOrigin, _rayDirection, Vector3.Zero, onNormal);
        }

        public void UpdatePicking(int x, int y, float width, float height)
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

        public void ResetIntersection()
        {
            _intersected = null;
        }
    }
}
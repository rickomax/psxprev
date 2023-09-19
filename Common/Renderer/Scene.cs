using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common.Renderer
{
    public class Scene
    {
        public static bool JointsSupported { get; private set; } = true;
        public static string ShaderVersion { get; private set; }

        public const float CameraMinFOV = 1f;
        public const float CameraMaxFOV = 160f; // Anything above this just looks unintelligible
        private const float CameraDefaultNearClip = 0.1f;
        private const float CameraDefaultFarClip = 500000f;
        private const float CameraMinDistance = 0.01f;
        private const float CameraMaxPitch = 89f * GeomMath.Deg2Rad; // 90f is too high, the view flips once we hit that
        private const float CameraDistanceIncrementFactor = 0.25f;
        private const float CameraPanIncrementFactor = 0.5f;

        // All of our math is based off when FOV was 60 degrees. So use 60 degrees as the base scalar.
        private static readonly float CameraBaseDistanceScalar = (float)(Math.Tan(60d * GeomMath.Deg2Rad / 2d) * 2d);

        private const float TriangleOutlineThickness = 2f;

        private static readonly Color LightRotationRayColor = Color.Yellow;
        private const float LightRotationRayDelayTime = 2.5f;
        private const float LightRotationRayFadeTime = 0.5f;

        private const float DebugPickingRayLineThickness = 3f;
        private const float DebugPickingRayOriginSize = 0.03f;
        private static readonly Color DebugPickingRayColor = Color.Magenta;
        private static readonly Color DebugIntersectionsColor = DebugPickingRayColor;

        private const float DebugIntersectionLineThickness = 2f;

        private const float GizmoHeight = 0.075f;
        private const float GizmoWidth = 0.005f;
        private const float GizmoRadiusDiff = GizmoWidth * 1.5f;
        private const float GizmoOuterRadius = GizmoHeight * 2f * 0.75f;
        private const float GizmoInnerRadius = GizmoOuterRadius - GizmoWidth * 3f;
        private const float GizmoRingHeight = GizmoWidth * 1.5f;
        private const float GizmoEndBoxSize = GizmoWidth * 4f;
        private const float GizmoUniformBoxSize = GizmoWidth * 6f;
        private const float GizmoLengthBoxHeight = GizmoHeight - (GizmoUniformBoxSize + GizmoEndBoxSize) / 2f;
        private const float GizmoEndBoxCenter = GizmoUniformBoxSize + GizmoEndBoxSize + GizmoLengthBoxHeight * 2f;

        private static readonly Color SelectedGizmoColor = Color.White;

        private class GizmoInfo
        {
            // Translate boxes
            public Vector3 TranslateCenter { get; set; }
            public Vector3 TranslateSize { get; set; }
            // Unused at the moment
            public Vector3 TranslateConeBottom { get; set; }
            public float TranslateConeRadius { get; set; }
            public float TranslateConeHeight { get; set; }
            // Rotate rings
            public float RotateOuterRadius { get; set; }
            public float RotateInnerRadius { get; set; }
            public float RotateHeight { get; set; }
            // Scale boxes
            public Vector3 ScaleLengthCenter { get; set; }
            public Vector3 ScaleLengthSize { get; set; }
            public Vector3 ScaleEndCenter { get; set; }
            public Vector3 ScaleEndSize { get; set; }
            // Shared
            public Color Color { get; set; }
            public int AxisIndex { get; set; }
            public Vector3 AxisVector { get; set; }
        }

        private static readonly Dictionary<GizmoId, GizmoInfo> GizmoInfos = new Dictionary<GizmoId, GizmoInfo>
        {
            { GizmoId.AxisX, new GizmoInfo
            {
                TranslateCenter = new Vector3(GizmoHeight - GizmoWidth, 0f, 0f), // X gizmo occupies the center
                TranslateSize   = new Vector3(GizmoHeight, GizmoWidth, GizmoWidth),

                RotateOuterRadius = GizmoOuterRadius - GizmoRadiusDiff * 0, // Outermost ring
                RotateInnerRadius = GizmoInnerRadius - GizmoRadiusDiff * 0,
                RotateHeight      = GizmoRingHeight,

                ScaleLengthCenter = new Vector3((GizmoUniformBoxSize + GizmoLengthBoxHeight), 0f, 0f),
                ScaleLengthSize   = new Vector3(GizmoLengthBoxHeight, GizmoWidth, GizmoWidth),
                ScaleEndCenter    = new Vector3(GizmoEndBoxCenter, 0f, 0f),
                ScaleEndSize      = new Vector3(GizmoEndBoxSize),

                Color = Color.Red,
                AxisIndex = 0,
                AxisVector = Vector3.UnitX,
            } },
            { GizmoId.AxisY, new GizmoInfo
            {
                TranslateCenter = new Vector3(0f, -GizmoHeight, 0f), // Y center is inverted
                TranslateSize   = new Vector3(GizmoWidth, GizmoHeight - GizmoWidth, GizmoWidth),

                RotateOuterRadius = GizmoOuterRadius - GizmoRadiusDiff * 1, // Middle ring
                RotateInnerRadius = GizmoInnerRadius - GizmoRadiusDiff * 1,
                RotateHeight      = GizmoRingHeight,

                ScaleLengthCenter = new Vector3(0f, -(GizmoUniformBoxSize + GizmoLengthBoxHeight), 0f),
                ScaleLengthSize   = new Vector3(GizmoWidth, GizmoLengthBoxHeight, GizmoWidth),
                ScaleEndCenter    = new Vector3(0f, -GizmoEndBoxCenter, 0f),
                ScaleEndSize      = new Vector3(GizmoEndBoxSize),

                Color = Color.Green,
                AxisIndex = 1,
                AxisVector = Vector3.UnitY, // todo: Should we negate this?
            } },
            { GizmoId.AxisZ, new GizmoInfo
            {
                TranslateCenter = new Vector3(0f, 0f, GizmoHeight),
                TranslateSize   = new Vector3(GizmoWidth, GizmoWidth, GizmoHeight - GizmoWidth),

                RotateOuterRadius = GizmoOuterRadius - GizmoRadiusDiff * 2, // Innermost ring
                RotateInnerRadius = GizmoInnerRadius - GizmoRadiusDiff * 2,
                RotateHeight      = GizmoRingHeight,

                ScaleLengthCenter = new Vector3(0f, 0f, (GizmoUniformBoxSize + GizmoLengthBoxHeight)),
                ScaleLengthSize   = new Vector3(GizmoWidth, GizmoWidth, GizmoLengthBoxHeight),
                ScaleEndCenter    = new Vector3(0f, 0f, GizmoEndBoxCenter),
                ScaleEndSize      = new Vector3(GizmoEndBoxSize),

                Color = Color.Blue,
                AxisIndex = 2,
                AxisVector = Vector3.UnitZ,
            } },
            { GizmoId.Uniform, new GizmoInfo
            {
                ScaleEndCenter = Vector3.Zero,
                ScaleEndSize   = new Vector3(GizmoUniformBoxSize),

                Color = Color.Yellow,
                AxisIndex = 3,
                AxisVector = Vector3.One,
            } },
        };


        public static int AttributeIndexPosition = 0;
        public static int AttributeIndexColor = 1;
        public static int AttributeIndexNormal = 2;
        public static int AttributeIndexUv = 3;
        public static int AttributeIndexTiledArea = 4;
        public static int AttributeIndexTexture = 5;
        public static int AttributeIndexJoint = 6;

        public static int BufferIndexJoints = 0;

        public static int UniformNormalMatrix;
        public static int UniformModelMatrix;
        public static int UniformMVPMatrix;
        public static int UniformViewMatrix;
        public static int UniformProjectionMatrix;
        public static int UniformLightDirection;
        public static int UniformMaskColor;
        public static int UniformAmbientColor;
        public static int UniformSolidColor;
        public static int UniformUVOffset;
        public static int UniformJointMode;
        public static int UniformLightMode;
        public static int UniformColorMode;
        public static int UniformTextureMode;
        public static int UniformSemiTransparentPass;
        public static int UniformLightIntensity;

        public const string AttributeNamePosition = "in_Position";
        public const string AttributeNameColor = "in_Color";
        public const string AttributeNameNormal = "in_Normal";
        public const string AttributeNameUv = "in_Uv";
        public const string AttributeNameTiledArea = "in_TiledArea";
        public const string AttributeNameTexture = "mainTex";
        public const string AttributeNameJoint = "in_Joint";

        public const string UniformNormalMatrixName = "normalMatrix";
        public const string UniformModelMatrixName = "modelMatrix";
        public const string UniformMVPMatrixName = "mvpMatrix";
        public const string UniformViewMatrixName = "viewMatrix";
        public const string UniformProjectionMatrixName = "projectionMatrix";
        public const string UniformLightDirectionName = "lightDirection";
        public const string UniformMaskColorName = "maskColor";
        public const string UniformAmbientColorName = "ambientColor";
        public const string UniformSolidColorName = "solidColor";
        public const string UniformUVOffsetName = "uvOffset";
        public const string UniformJointModeName = "jointMode";
        public const string UniformLightModeName = "lightMode";
        public const string UniformColorModeName = "colorMode";
        public const string UniformTextureModeName = "textureMode";
        public const string UniformSemiTransparentPassName = "semiTransparentPass";
        public const string UniformLightIntensityName = "lightIntensity";

        public bool Initialized { get; private set; }

        public MeshBatch MeshBatch { get; private set; }
        public MeshBatch GizmosBatch { get; private set; }
        public MeshBatch BoundsBatch { get; private set; }
        public MeshBatch TriangleOutlineBatch { get; private set; }
        public MeshBatch LightRotationRayBatch { get; private set; }
        public MeshBatch DebugPickingRayBatch { get; private set; }
        public MeshBatch DebugIntersectionsBatch { get; private set; }
        public TextureBinder TextureBinder { get; private set; }

        public event EventHandler CameraChanged;
        public event EventHandler LightChanged;
        public event EventHandler TimeChanged;

        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix; // Final view matrix.
        private Matrix4 _viewOriginMatrix; // View matrix without target translation.
        private Vector3 _viewTarget; // Center of view that camera rotates around.
        private BoundingBox _viewTargetBounds = new BoundingBox();
        private bool _viewMatrixValid;
        private int _shaderProgram;
        private Vector3 _rayOrigin;
        private Vector3 _rayTarget;
        private Vector3 _rayDirection = -Vector3.UnitZ; // Some arbitrary default value
        private Vector3? _intersected;
        private List<EntityBase> _lastPickedEntities;
        private List<Tuple<ModelEntity, Triangle>> _lastPickedTriangles;
        private int _lastPickedEntityIndex;
        private int _lastPickedTriangleIndex;
        private double _timeDelta;
        private double _time;
        private double _lightRayTimer;

        // Last-stored picking ray for debug visuals
        private Vector3 _debugRayOrigin;
        private Vector3 _debugRayDirection;
        private Quaternion _debugRayRotation;

        private EntityBase _gizmoEntity;
        private GizmoType _currentGizmoType;
        private GizmoId _highlightGizmo;

        private System.Drawing.Color _clearColor;
        public System.Drawing.Color ClearColor
        {
            get => _clearColor;
            set
            {
                _clearColor = value;
                // Use 1.0 alpha so that clear color shows up when using GL.ReadPixels.
                GL.ClearColor(value.R / 255f, value.G / 255f, value.B / 255f, 1f);
            }
        }

        public System.Drawing.Color MaskColor { get; set; }
        public System.Drawing.Color DiffuseColor { get; set; }
        public System.Drawing.Color AmbientColor { get; set; }
        public System.Drawing.Color SolidWireframeVerticesColor { get; set; }

        public AttachJointsMode AttachJointsMode { get; set; }

        public bool DrawFaces { get; set; }
        public bool DrawWireframe { get; set; }
        public bool DrawVertices { get; set; }
        public bool DrawSolidWireframeVertices { get; set; } // Wireframe and vertices will use WireframeVerticesColor
        public float WireframeSize { get; set; }
        public float VertexSize { get; set; }

        public bool VibRibbonWireframe { get; set; }

        public bool ShowMissingTextures { get; set; }

        public bool ShowGizmos { get; set; }
        public bool ShowBounds { get; set; }
        public bool ShowLightRotationRay { get; set; }
        public bool ShowDebugIntersections { get; set; }
        public bool ShowDebugPickingRay { get; set; }
        public bool ShowDebugVisuals { get; set; } // 3D debug information like picking ray lines
        public bool ShowVisuals { get; set; } = true; // Enables the use of ShowGizmos, ShowBounds, ShowDebugVisuals, etc.

        public bool AmbientEnabled { get; set; }
        public bool LightEnabled { get; set; }
        public bool TexturesEnabled { get; set; }
        public bool VertexColorEnabled { get; set; }
        public bool SemiTransparencyEnabled { get; set; }
        public bool ForceDoubleSided { get; set; }

        public float LightIntensity { get; set; }

        public double Time => _time;

        public float ViewportWidth { get; private set; } = 1f;
        public float ViewportHeight { get; private set; } = 1f;

        public float CameraDistanceIncrement => CameraDistanceToTarget * CameraDistanceIncrementFactor;
        public float CameraPanIncrement => CameraDistanceToTarget * CameraPanIncrementFactor;

        // Applied when using _viewMatrix(Origin) to correct distance
        private float _cameraDistanceScalar = CalculateCameraDistanceScalar(CameraMinFOV);
        private float _cameraFOV = CameraMinFOV;
        public float CameraFOV
        {
            get => _cameraFOV;
            set
            {
                value = GeomMath.Clamp(value, CameraMinFOV, CameraMaxFOV);
                if (_cameraFOV != value)
                {
                    _cameraFOV = value;
                    UpdateCameraFOV();
                    SetupMatrices();
                    UpdateViewMatrix(); // Update view matrix because it relies on FOV to preserve distance
                    OnCameraChanged();
                }
            }
        }
        private float CameraFOVRads => CameraFOV * GeomMath.Deg2Rad;

        private float _cameraDistance;
        public float CameraDistance
        {
            get => _cameraDistance;
            set
            {
                value = Math.Max(CameraMinDistance, value);
                if (_cameraDistance != value)
                {
                    _cameraDistance = value;
                    UpdateViewMatrix();
                    OnCameraChanged();
                }
            }
        }

        private float _cameraYaw;
        public float CameraYaw
        {
            get => _cameraYaw;
            set => CameraPitchYaw = new Vector2(_cameraPitch, value);
        }
        private float _cameraPitch;
        public float CameraPitch
        {
            get => _cameraPitch;
            set => CameraPitchYaw = new Vector2(value, _cameraYaw);
        }
        public Vector2 CameraPitchYaw
        {
            get => new Vector2(_cameraPitch, _cameraYaw);
            set
            {
                value.X = GeomMath.Clamp(value.X, -CameraMaxPitch, CameraMaxPitch);
                value.Y = GeomMath.PositiveModulus(value.Y, (float)(Math.PI * 2));
                if (CameraPitchYaw != value)
                {
                    _cameraPitch = value.X;
                    _cameraYaw   = value.Y;
                    UpdateViewMatrix();
                    OnCameraChanged();
                }
            }
        }

        private float _cameraX;
        public float CameraX
        {
            get => _cameraX;
            set => CameraPosition = new Vector2(value, _cameraY);
        }
        private float _cameraY;
        public float CameraY
        {
            get => _cameraY;
            set => CameraPosition = new Vector2(_cameraX, value);
        }
        public Vector2 CameraPosition
        {
            get => new Vector2(_cameraX, _cameraY);
            set
            {
                if (CameraPosition != value)
                {
                    _cameraX = value.X;
                    _cameraY = value.Y;
                    UpdateViewMatrix();
                    OnCameraChanged();
                }
            }
        }

        public float CameraNearClip { get; private set; } = CameraDefaultNearClip;
        public float CameraFarClip { get; private set; } = CameraDefaultFarClip;

        // I'm not too sure about this, but very specific camera angles would cause some sort of broken
        // quaternion when extracting from the view matrix. It's possible this has to do with gimbal
        // lock and the Matrix4.LookAt up vector. But all I know is getting the rotation this way does
        // not produce the same issue... or at least, I can't reproduce it through brute force.
        // See FocusOnBounds for settings to reproduce the broken CameraRotationOld.
        public Quaternion CameraRotation
        {
            get
            {
                // +180deg to X since camera faces opposite direction and up-side down
                return (Quaternion.FromAxisAngle(Vector3.UnitX, (float)(_cameraPitch + Math.PI)) *
                        Quaternion.FromAxisAngle(Vector3.UnitY, _cameraYaw)).Inverted();
            }
        }
        //public Quaternion CameraRotationOld => _viewOriginMatrix.Inverted().ExtractRotation();

        public Quaternion CameraYawRotation => Quaternion.FromAxisAngle(Vector3.UnitY, _cameraYaw).Inverted();

        public Vector3 CameraDirection => CameraRotation * Vector3.UnitZ;

        public float CameraDistanceToTarget => -_viewOriginMatrix.ExtractTranslation().Z * _cameraDistanceScalar;

        public Vector3 LightDirection { get; private set; } = -Vector3.UnitZ;

        private float _lightYaw;
        public float LightYaw
        {
            get => _lightYaw;
            set => LightPitchYaw = new Vector2(_lightPitch, value);
        }
        private float _lightPitch;
        public float LightPitch
        {
            get => _lightPitch;
            set => LightPitchYaw = new Vector2(value, _lightYaw);
        }
        public Vector2 LightPitchYaw
        {
            get => new Vector2(_lightPitch, _lightYaw);
            set
            {
                value.X = GeomMath.PositiveModulus(value.X, (float)(Math.PI * 2));
                value.Y = GeomMath.PositiveModulus(value.Y, (float)(Math.PI * 2));
                if (LightPitchYaw != value)
                {
                    _lightPitch = value.X;
                    _lightYaw   = value.Y;
                    UpdateLightRotation();
                    OnLightChanged();
                }
            }
        }

        public void Initialize(float width, float height)
        {
            if (Initialized)
            {
                return;
            }
            SetupGL();
            SetupShaders();
            Resize(width, height, true); // Force-resize if width/height equals initial values
            SetupMatrices();
            SetupInternals();
            Initialized = true;
        }

        public void Resize(float width, float height, bool force = false)
        {
            if (force || ViewportWidth != width || ViewportHeight != height)
            {
                ViewportWidth  = width;
                ViewportHeight = height;
                GL.Viewport(0, 0, (int)width, (int)height);
                SetupMatrices();
                OnCameraChanged();
            }
        }

        private void SetupInternals()
        {
            TextureBinder = new TextureBinder();
            MeshBatch = new MeshBatch(this)
            {
                TextureBinder = TextureBinder,
            };
            GizmosBatch = new MeshBatch(this)
            {
                Visible = false,
                AmbientEnabled = true,
                AmbientColor = MeshRenderInfo.DefaultAmbientColor,
                LightEnabled = true,
                LightIntensity = 1f,
                LightDirection = new Vector3(1f, -1f, 1f).Normalized(),
            };
            BoundsBatch = new MeshBatch(this);
            TriangleOutlineBatch = new MeshBatch(this);
            LightRotationRayBatch = new MeshBatch(this)
            {
                Visible = false,
                AmbientEnabled = true,
                AmbientColor = MeshRenderInfo.DefaultAmbientColor,
                LightEnabled = true,
                LightIntensity = 1f,
            };
            DebugPickingRayBatch = new MeshBatch(this)
            {
                Visible = false,
                AmbientEnabled = true,
                AmbientColor = MeshRenderInfo.DefaultAmbientColor,
                LightEnabled = true,
                LightIntensity = 1f,
            };
            DebugIntersectionsBatch = new MeshBatch(this);

            TimeChanged += (sender, e) => {
                if (ShowVisuals && ShowLightRotationRay && LightRotationRayBatch.Visible)
                {
                    _lightRayTimer += _timeDelta;
                    UpdateLightRotationRay();
                }
            };
            LightChanged += (sender, e) => {
                if (ShowVisuals && ShowLightRotationRay)
                {
                    _lightRayTimer = 0d; // Show light rotation ray and reset timer
                    LightRotationRayBatch.Visible = true;
                    UpdateLightRotationRay();
                }
            };
            CameraChanged += (sender, e) => {
                UpdateLightRotationRay();
                UpdateDebugPickingRay();
                UpdateGizmoVisual(_gizmoEntity, _currentGizmoType, _highlightGizmo);
            };
        }

        private void SetupGL()
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
        }

        // Note: Use "(?=[\r\n]|\z)" instead, because "$" doesn't handle CRLF... Wow.
        private const string ShaderFallbackVersionPrefix = "//FALLBACK_VERSION";
        private const string ShaderJointsDefine = "#define JOINTS_SUPPORTED";
        private const string ShaderVersionPattern = @"^#version\s+([0-9]+[^\n\r]*)(?=[\r\n]|\z)";

        private static string GetShaderFallbackVersion(string shaderSource)
        {
            // Capture string after FallbackVersion const until comment or EOL.
            var match = Regex.Match(shaderSource, $@"^{Regex.Escape(ShaderFallbackVersionPrefix)}\s+([0-9]+[^\n\r]*)(?=[\r\n]|\z)", RegexOptions.Multiline);
            if (match.Success)
            {
                return $"#version {match.Groups[1].Value.Trim()}";
            }
            return null;
        }

        private void SetupShaders()
        {
            var vertexShaderSource = ManifestResourceLoader.LoadTextFile("Shaders\\Shader.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("Shaders\\Shader.frag");

#if DEBUG
            JointsSupported = true; // Set to false to test shader without joints support
            const bool DebugTestFallback = false; // Set to true to allow joints support to silently fail
#endif

            var vertexFallbackVersion = GetShaderFallbackVersion(vertexShaderSource);
            if (vertexFallbackVersion == null)
            {
                throw new Exception($"Missing \"{ShaderFallbackVersionPrefix}\" in vertex shader");
            }
            if (!vertexShaderSource.Contains(ShaderJointsDefine))
            {
                throw new Exception($"Missing \"{ShaderJointsDefine}\" in vertex shader");
            }
            if (!Regex.IsMatch(vertexShaderSource, ShaderVersionPattern, RegexOptions.Multiline))
            {
                throw new Exception($"Failed to find \"#version\" in vertex shader");
            }

            string jointsSupportError = null, noJointsSupportError = null;
            var failed = false;
            for (var i = 0; i < 2; i++)
            {
                _shaderProgram = GL.CreateProgram();

                if (!JointsSupported)
                {
                    // Remove the define that enables joints
                    vertexShaderSource = vertexShaderSource.Replace(ShaderJointsDefine, string.Empty);
                    // Overwrite the version header with the fallback version
                    vertexShaderSource = Regex.Replace(vertexShaderSource, ShaderVersionPattern, vertexFallbackVersion);
                }

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
                    { AttributeIndexPosition, AttributeNamePosition },
                    { AttributeIndexNormal, AttributeNameNormal },
                    { AttributeIndexColor, AttributeNameColor },
                    { AttributeIndexUv, AttributeNameUv },
                    { AttributeIndexTiledArea, AttributeNameTiledArea },
                    { AttributeIndexTexture, AttributeNameTexture },
                    //{ AttributeIndexJoint, AttributeNameJoint },
                };
                if (JointsSupported)
                {
                    attributes.Add(AttributeIndexJoint, AttributeNameJoint);
                }
                foreach (var vertexAttributeLocation in attributes)
                {
                    GL.BindAttribLocation(_shaderProgram, vertexAttributeLocation.Key, vertexAttributeLocation.Value);
                }

                GL.LinkProgram(_shaderProgram);
                if (!GetLinkStatus())
                {
                    if (JointsSupported)
                    {
                        // Try again but without joints support
                        jointsSupportError = GetInfoLog();
                        JointsSupported = false;
#if DEBUG
                        // Always throw an exception if we're debugging and not testing fallback
                        failed |= !DebugTestFallback;
#endif
                        GL.DeleteProgram(_shaderProgram);
                        _shaderProgram = 0;
                        continue;
                    }
                    else
                    {
                        // Both with and without joints failed
                        noJointsSupportError = GetInfoLog();
                        failed = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (failed)
            {
                throw new Exception($"joints: {jointsSupportError}\n\nno joints: {noJointsSupportError}");
            }
            ShaderVersion = Regex.Match(vertexShaderSource, ShaderVersionPattern).Groups[1].Value.Trim();

            UniformNormalMatrix = GL.GetUniformLocation(_shaderProgram, UniformNormalMatrixName);
            UniformModelMatrix = GL.GetUniformLocation(_shaderProgram, UniformModelMatrixName);
            UniformMVPMatrix = GL.GetUniformLocation(_shaderProgram, UniformMVPMatrixName);
            UniformViewMatrix = GL.GetUniformLocation(_shaderProgram, UniformViewMatrixName);
            UniformProjectionMatrix = GL.GetUniformLocation(_shaderProgram, UniformProjectionMatrixName);
            UniformLightDirection = GL.GetUniformLocation(_shaderProgram, UniformLightDirectionName);
            UniformMaskColor = GL.GetUniformLocation(_shaderProgram, UniformMaskColorName);
            UniformAmbientColor = GL.GetUniformLocation(_shaderProgram, UniformAmbientColorName);
            UniformSolidColor = GL.GetUniformLocation(_shaderProgram, UniformSolidColorName);
            UniformUVOffset = GL.GetUniformLocation(_shaderProgram, UniformUVOffsetName);
            UniformJointMode = GL.GetUniformLocation(_shaderProgram, UniformJointModeName);
            UniformLightMode = GL.GetUniformLocation(_shaderProgram, UniformLightModeName);
            UniformColorMode = GL.GetUniformLocation(_shaderProgram, UniformColorModeName);
            UniformTextureMode = GL.GetUniformLocation(_shaderProgram, UniformTextureModeName);
            UniformSemiTransparentPass = GL.GetUniformLocation(_shaderProgram, UniformSemiTransparentPassName);
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

        private void SetupMatrices()
        {
            // I'm not sure what the math is here, I just know multiplying the clips like this is what works.
            // 1 FOV has too much Z-fighting, without any division and when dividing by scalar.
            CameraNearClip = CameraDefaultNearClip / (_cameraDistanceScalar * _cameraDistanceScalar);
            // 1 FOV will disappear too soon if we don't divide by scalar.
            // 160 FOV will disappear too soon if we divide by scalar^2.
            CameraFarClip = CameraDefaultFarClip / _cameraDistanceScalar;

            var aspect = ViewportWidth / ViewportHeight;
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(CameraFOVRads, aspect, CameraNearClip, CameraFarClip);
        }

        private void UpdateCameraFOV()
        {
            _cameraDistanceScalar = CalculateCameraDistanceScalar(_cameraFOV);
        }

        private void UpdateLightRotation()
        {
            var rotation = Matrix4.CreateRotationX(_lightPitch) * Matrix4.CreateRotationY(_lightYaw);

            LightDirection = Vector3.TransformVector(-Vector3.UnitZ, rotation);
        }

        private void UpdateViewMatrix()
        {
            // The target (_viewTarget) is the origin of the view that the camera rotates around.
            // CameraY represents the vertical distance of the camera from the origin (rotated by the pitch).
            // CameraX represents the horizontal distance of the camera from the origin (rotated by the yaw).
            // The camera always rotates around the origin.
            // The camera cannot move forwards or back, it can only zoom in and out.
            var targetTranslation = Matrix4.CreateTranslation(-_viewTarget);
            var cameraTranslation = Matrix4.CreateTranslation(_cameraX, -_cameraY, 0f);
            var rotation = Matrix4.CreateRotationY(_cameraYaw) * Matrix4.CreateRotationX(_cameraPitch);
            var distance = -_cameraDistance / _cameraDistanceScalar;
            var eye = (rotation * new Vector4(0f, 0f, distance, 1f)).Xyz;

            // Target (0, 0, 0), then apply _viewTarget translation later, because we need _viewOriginMatrix.
            _viewOriginMatrix = Matrix4.LookAt(eye, Vector3.Zero, -Vector3.UnitY);
            // Apply camera translation (after rotation).
            _viewOriginMatrix *= cameraTranslation;
            // Apply target translation (before rotation).
            _viewMatrix = targetTranslation * _viewOriginMatrix;

            _viewMatrixValid = true;
        }

        private void OnCameraChanged()
        {
            if (Initialized)
            {
                CameraChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnLightChanged()
        {
            if (Initialized)
            {
                LightChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnTimeChanged()
        {
            if (Initialized)
            {
                TimeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DrawPassModelMeshBatch(RenderPass pass, MeshBatch meshBatch, ref int triangleCount, ref int meshCount, ref int skinCount)
        {
            // The main model mesh batch uses the scene's settings.
            meshBatch.DrawFaces = DrawFaces;
            meshBatch.DrawWireframe = !DrawSolidWireframeVertices && DrawWireframe;
            meshBatch.DrawVertices = !DrawSolidWireframeVertices && DrawVertices;
            meshBatch.WireframeSize = WireframeSize;
            meshBatch.VertexSize = VertexSize;
            meshBatch.AmbientEnabled = AmbientEnabled;
            meshBatch.LightEnabled = LightEnabled;
            meshBatch.TexturesEnabled = TexturesEnabled;
            meshBatch.SemiTransparencyEnabled = SemiTransparencyEnabled;
            meshBatch.ForceDoubleSided = ForceDoubleSided;
            meshBatch.LightDirection = null; //LightDirection;
            meshBatch.LightIntensity = null; //LightIntensity;
            meshBatch.AmbientColor = null; //(Color)AmbientColor;
            meshBatch.SolidColor = null;
            if (!VertexColorEnabled)
            {
                meshBatch.SolidColor = Color.Grey;
            }

            if (DrawFaces || (!DrawSolidWireframeVertices && (DrawWireframe || DrawVertices)))
            {
                meshBatch.DrawPass(pass, _viewMatrix, _projectionMatrix, ref triangleCount, ref meshCount, ref skinCount);
            }

            // Wireframe/Vertices are being drawn with a solid color, so
            // they need to be drawn separately with different settings.
            if (DrawSolidWireframeVertices && (DrawWireframe || DrawVertices))
            {
                meshBatch.DrawFaces = false;
                meshBatch.DrawWireframe = DrawWireframe;
                meshBatch.DrawVertices = DrawVertices;
                // todo: Should we disable ambient/light for solid wireframe/vertices?
                meshBatch.AmbientEnabled = false;
                meshBatch.LightEnabled = false;
                meshBatch.TexturesEnabled = false;
                meshBatch.SemiTransparencyEnabled = false;
                meshBatch.SolidColor = (Color)SolidWireframeVerticesColor;

                meshBatch.DrawPass(pass, _viewMatrix, _projectionMatrix, ref triangleCount, ref meshCount, ref skinCount);
            }
        }

        public void Draw(out int triangleCount, out int meshCount, out int skinCount)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);

            GL.Uniform3(UniformMaskColor, MaskColor.ToVector3());
            GL.Uniform2(UniformUVOffset, Vector2.Zero);

            triangleCount = 0; // Only count triangles/meshes from the models mesh batch.
            meshCount = 0;
            skinCount = 0;

            foreach (var pass in MeshBatch.GetPasses())
            {
                DrawPassModelMeshBatch(pass, MeshBatch, ref triangleCount, ref meshCount, ref skinCount);

                // Preserve depth buffer for these batches.

                if (ShowVisuals && ShowBounds)
                {
                    BoundsBatch.DrawPass(pass, _viewMatrix, _projectionMatrix);
                }

                if (ShowVisuals && ShowLightRotationRay)
                {
                    // Make light face towards the origin, to light up the rectangle where the ray starts from
                    LightRotationRayBatch.LightDirection = -LightDirection;
                    LightRotationRayBatch.DrawPass(pass, _viewMatrix, _projectionMatrix);
                }

                if (ShowVisuals && ShowDebugVisuals && ShowDebugPickingRay)
                {
                    // Make light face towards the origin, to light up the rectangle where the ray starts from
                    DebugPickingRayBatch.LightDirection = _debugRayDirection;
                    DebugPickingRayBatch.DrawPass(pass, _viewMatrix, _projectionMatrix);
                }
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);
            if (ShowVisuals && ShowDebugVisuals && ShowDebugIntersections)
            {
                DebugIntersectionsBatch.Draw(_viewMatrix, _projectionMatrix);
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);
            // todo: Should ShowBounds really determine if the selected triangle is highlighted?
            if (ShowVisuals && ShowBounds)
            {
                TriangleOutlineBatch.Draw(_viewMatrix, _projectionMatrix);
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);
            if (ShowVisuals)
            {
                GizmosBatch.Draw(_viewMatrix, _projectionMatrix);
            }

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            GL.UseProgram(0);
        }

        public void ResetModelBatches(int meshCount)
        {
            MeshBatch.Reset(meshCount);
        }

        public void AddTime(double seconds)
        {
            _time += seconds;
            _timeDelta = seconds;
            if (seconds != 0)
            {
                OnTimeChanged();
            }
        }

        public void UpdateGizmoVisual(EntityBase selectedEntityBase, GizmoType currentType, GizmoId highlightGizmo)
        {
            _gizmoEntity = selectedEntityBase;
            _currentGizmoType = currentType;
            _highlightGizmo = highlightGizmo;
            if (selectedEntityBase == null || currentType == GizmoType.None)
            {
                GizmosBatch.Visible = false;
                return;
            }
            GizmosBatch.Visible = true;

            // Force-prepare the batch the first time.
            var updateMeshData = !GizmosBatch.IsValid;

            GizmosBatch.ResetMeshIndex();
            if (updateMeshData)
            {
                GizmosBatch.Reset(3 + 3 + (3 + 1)); // Translate + Rotate + (Scale + Uniform)
            }

            var matrix = GetGizmoMatrix(selectedEntityBase, currentType);

            var triangleBuilder = new TriangleMeshBuilder();
            for (var type = GizmoType.Translate; type <= GizmoType.Scale; type++)
            {
                for (var gizmo = GizmoId.AxisX; gizmo <= GizmoId.Uniform; gizmo++)
                {
                    if (type != GizmoType.Scale && gizmo == GizmoId.Uniform)
                    {
                        continue;
                    }
                    var gizmoInfo = GizmoInfos[gizmo];

                    var selected = highlightGizmo == gizmo;
                    triangleBuilder.Visible = currentType == type;
                    triangleBuilder.SolidColor = selected ? SelectedGizmoColor : gizmoInfo.Color;

                    if (updateMeshData)
                    {
                        triangleBuilder.Clear();
                        switch (type)
                        {
                            case GizmoType.Translate when gizmo != GizmoId.Uniform:
                                triangleBuilder.AddCube(gizmoInfo.TranslateCenter, gizmoInfo.TranslateSize);
                                //triangleBuilder.AddCone(gizmoInfo.AxisIndex, gizmoInfo.TranslateConeBottom, gizmoInfo.TranslateConeHeight,
                                //                        gizmoInfo.TranslateConeRadius, 12, false, flip: gizmo == GizmoId.AxisY);
                                break;

                            case GizmoType.Rotate when gizmo != GizmoId.Uniform:
                                triangleBuilder.AddRing(gizmoInfo.AxisIndex, Vector3.Zero, gizmoInfo.RotateHeight,
                                                        gizmoInfo.RotateOuterRadius, gizmoInfo.RotateInnerRadius, 32);
                                break;

                            case GizmoType.Scale when gizmo != GizmoId.Uniform:
                                triangleBuilder.AddCube(gizmoInfo.ScaleLengthCenter, gizmoInfo.ScaleLengthSize);
                                goto case GizmoType.Scale;
                            case GizmoType.Scale:
                                triangleBuilder.AddCube(gizmoInfo.ScaleEndCenter, gizmoInfo.ScaleEndSize);
                                break;
                        }
                    }

                    GizmosBatch.BindTriangleMesh(triangleBuilder, matrix, updateMeshData);
                }
            }
        }


        public void SetDebugPickingRay(bool show = true)
        {
            if (show && !_rayDirection.IsZero())
            {
                DebugPickingRayBatch.Visible = true;
                _debugRayOrigin = _rayOrigin;
                _debugRayDirection = _rayDirection;
                _debugRayRotation = Matrix4.LookAt(_rayTarget, _rayOrigin, new Vector3(0f, -1f, 0f)).Inverted().ExtractRotation();

                UpdateDebugPickingRay();
            }
            else
            {
                DebugPickingRayBatch.Visible = false;
            }
        }

        private void UpdateDebugPickingRay()
        {
            if (DebugPickingRayBatch.Visible)
            {
                BindRay(DebugPickingRayBatch, _debugRayOrigin, _debugRayRotation, DebugPickingRayColor);
            }
        }

        private void UpdateLightRotationRay()
        {
            var blend = 1f;
            if (LightRotationRayBatch.Visible && _lightRayTimer >= LightRotationRayDelayTime)
            {
                // Make things look ~fancy~ by fading out after the delay.
                var fadeTime = _lightRayTimer - LightRotationRayDelayTime;
                if (fadeTime >= LightRotationRayFadeTime)
                {
                    LightRotationRayBatch.Visible = false;
                }
                else
                {
                    blend = 1f - (float)(fadeTime / LightRotationRayFadeTime);
                }
            }

            if (!LightRotationRayBatch.Visible)
            {
                return;
            }

            var rotationMatrix = Matrix4.CreateRotationX(_lightPitch) * Matrix4.CreateRotationY(_lightYaw);
            var distance = DistanceToFitBounds(_viewTargetBounds) / _cameraDistanceScalar;
            var origin = _viewTarget + LightDirection * distance * 0.5f;
            var rotation = rotationMatrix.ExtractRotation();

            BindRay(LightRotationRayBatch, origin, rotation, LightRotationRayColor, blend);
        }

        private void BindRay(MeshBatch rayBatch, Vector3 rayOrigin, Quaternion rayRotation, Color color, float blend = 1f)
        {
            // Force-prepare the batch the first time.
            var updateMeshData = !rayBatch.IsValid;

            rayBatch.ResetMeshIndex();
            if (updateMeshData)
            {
                rayBatch.Reset(3);
            }

            var lineBuilder = new LineMeshBuilder
            {
                RenderFlags = RenderFlags.Unlit | RenderFlags.NoAmbient | RenderFlags.SemiTransparent,
                MixtureRate = MixtureRate.Alpha,
                Alpha = 1f * blend,
                Thickness = 3f,
            };
            var originOutlineBuilder = new LineMeshBuilder
            {
                RenderFlags = RenderFlags.Unlit | RenderFlags.NoAmbient | RenderFlags.SemiTransparent,
                MixtureRate = MixtureRate.Alpha,
                Alpha = 1f * blend,
                Thickness = 1f,
            };
            var originBuilder = new TriangleMeshBuilder
            {
                RenderFlags = RenderFlags.SemiTransparent,
                MixtureRate = MixtureRate.Alpha,
                Alpha = 0.5f * blend,
            };

            if (updateMeshData)
            {
                var farClip = CameraDefaultFarClip;
                lineBuilder.AddLine(Vector3.Zero, new Vector3(0f, 0f, farClip), color);

                var size = new Vector3(DebugPickingRayOriginSize);
                originOutlineBuilder.AddCubeOutline(Vector3.Zero, size, color);
                originBuilder.AddCube(Vector3.Zero, size, color);
            }

            var matrix = Matrix4.CreateTranslation(rayOrigin);
            var rotationMatrix = Matrix4.CreateFromQuaternion(rayRotation);
            var lineScaleMatrix = Matrix4.CreateScale(1f, 1f, (1f / _cameraDistanceScalar));
            var originScaleMatrix = Matrix4.CreateScale(GetGizmoScale(rayOrigin));
            var lineMatrix = lineScaleMatrix * rotationMatrix * matrix;
            var originMatrix = originScaleMatrix * rotationMatrix * matrix;

            rayBatch.BindLineMesh(lineBuilder, lineMatrix, updateMeshData);
            rayBatch.BindLineMesh(originOutlineBuilder, originMatrix, updateMeshData);
            rayBatch.BindTriangleMesh(originBuilder, originMatrix, updateMeshData);
        }

        public void ClearUnderMouseCycleLists()
        {
            _lastPickedEntities = null;
            _lastPickedTriangles = null;
        }

        public void ClearEntityUnderMouseCycleList()
        {
            _lastPickedEntities = null;
        }

        public void ClearTriangleUnderMouseCycleList()
        {
            _lastPickedTriangles = null;
        }

        public EntityBase GetEntityUnderMouse(RootEntity[] checkedEntities, RootEntity selectedRootEntity, int x, int y, bool selectRoot = false, bool boundsPicking = true)
        {
            UpdatePicking(x, y);
            var intersectedEntities = new List<EntityBase>();
            if (!selectRoot)
            {
                if (checkedEntities != null)
                {
                    foreach (var entity in checkedEntities)
                    {
                        if (entity.ChildEntities != null)
                        {
                            var jointMatrices = !boundsPicking ? entity.JointMatrices : null;
                            foreach (var subEntity in entity.ChildEntities)
                            {
                                CheckEntity(subEntity, intersectedEntities, boundsPicking, jointMatrices);
                            }
                        }
                    }
                }
                else if (selectedRootEntity != null)
                {
                    if (selectedRootEntity.ChildEntities != null)
                    {
                        var jointMatrices = !boundsPicking ? selectedRootEntity.JointMatrices : null;
                        foreach (var subEntity in selectedRootEntity.ChildEntities)
                        {
                            CheckEntity(subEntity, intersectedEntities, boundsPicking, jointMatrices);
                        }
                    }
                }
            }
            intersectedEntities.Sort((a, b) => a.IntersectionDistance.CompareTo(b.IntersectionDistance));
            if (!ListsMatches(intersectedEntities, _lastPickedEntities))
            {
                _lastPickedEntityIndex = 0;
            }
            var pickedEntity = intersectedEntities.Count > 0 ? intersectedEntities[_lastPickedEntityIndex] : null;
            if (_lastPickedEntityIndex < intersectedEntities.Count - 1)
            {
                _lastPickedEntityIndex++;
            }
            else
            {
                _lastPickedEntityIndex = 0;
            }
            _lastPickedEntities = intersectedEntities;
            if (ShowDebugVisuals && ShowDebugIntersections)
            {
                DebugIntersectionsBatch.Reset(1);
                var lineBuilder = new LineMeshBuilder
                {
                    RenderFlags = RenderFlags.Unlit | RenderFlags.NoAmbient,
                    Thickness = DebugIntersectionLineThickness,
                    SolidColor = DebugIntersectionsColor,
                };
                foreach (var intersectedEntity in intersectedEntities)
                {
                    if (intersectedEntity != pickedEntity)
                    {
                        lineBuilder.AddEntityBounds(intersectedEntity);
                    }
                }
                DebugIntersectionsBatch.BindLineMesh(lineBuilder, null, true);
            }
            return pickedEntity;
        }

        public Tuple<ModelEntity, Triangle> GetTriangleUnderMouse(RootEntity[] checkedEntities, RootEntity selectedRootEntity, int x, int y, bool selectRoot = false)
        {
            UpdatePicking(x, y);
            var intersectedTriangles = new List<Tuple<ModelEntity, Triangle>>();
            if (!selectRoot)
            {
                if (checkedEntities != null)
                {
                    foreach (var entity in checkedEntities)
                    {
                        if (entity.ChildEntities != null)
                        {
                            var jointMatrices = entity.JointMatrices;
                            foreach (var subEntity in entity.ChildEntities)
                            {
                                CheckTriangles(subEntity, intersectedTriangles, jointMatrices);
                            }
                        }
                    }
                }
                else if (selectedRootEntity != null)
                {
                    if (selectedRootEntity.ChildEntities != null)
                    {
                        var jointMatrices = selectedRootEntity.JointMatrices;
                        foreach (var subEntity in selectedRootEntity.ChildEntities)
                        {
                            CheckTriangles(subEntity, intersectedTriangles, jointMatrices);
                        }
                    }
                }
            }
            intersectedTriangles.Sort((a, b) => a.Item2.IntersectionDistance.CompareTo(b.Item2.IntersectionDistance));
            if (!ListsMatches(intersectedTriangles, _lastPickedTriangles))
            {
                _lastPickedTriangleIndex = 0;
            }
            var pickedTriangle = intersectedTriangles.Count > 0 ? intersectedTriangles[_lastPickedTriangleIndex] : null;
            if (_lastPickedTriangleIndex < intersectedTriangles.Count - 1)
            {
                _lastPickedTriangleIndex++;
            }
            else
            {
                _lastPickedTriangleIndex = 0;
            }
            _lastPickedTriangles = intersectedTriangles;
            if (ShowDebugVisuals && ShowDebugIntersections)
            {
                DebugIntersectionsBatch.Reset(1);
                var lineBuilder = new LineMeshBuilder
                {
                    RenderFlags = RenderFlags.Unlit | RenderFlags.NoAmbient,
                    Thickness = DebugIntersectionLineThickness,
                    SolidColor = DebugIntersectionsColor,
                };
                foreach (var intersectedTriangle in intersectedTriangles)
                {
                    var model = intersectedTriangle.Item1;
                    var triangle = intersectedTriangle.Item2;
                    var worldMatrix = model.WorldMatrix;
                    Vector3 vertex0, vertex1, vertex2;
                    if (AttachJointsMode != AttachJointsMode.Attach || !model.NeedsJointTransform)
                    {
                        Vector3.TransformPosition(ref triangle.Vertices[0], ref worldMatrix, out vertex0);
                        Vector3.TransformPosition(ref triangle.Vertices[1], ref worldMatrix, out vertex1);
                        Vector3.TransformPosition(ref triangle.Vertices[2], ref worldMatrix, out vertex2);
                    }
                    else
                    {
                        // We know that JointMatrices has previously been calculated when picking triangles,
                        // so we can just use the cached version.
                        var jointMatrices = model.GetRootEntity().JointMatricesCache;
                        triangle.TransformPositions(ref worldMatrix, jointMatrices, out vertex0, out vertex1, out vertex2);
                    }
                    lineBuilder.AddTriangleOutline(vertex0, vertex1, vertex2);
                }
                DebugIntersectionsBatch.BindLineMesh(lineBuilder, null, true);
            }
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

        private void CheckEntity(EntityBase entity, List<EntityBase> pickedEntities, bool boundsPicking, Matrix4[] jointMatrices)
        {
            if (!boundsPicking && entity is ModelEntity model)
            {
                if (model.Triangles.Length == 0)
                {
                    return;
                }
                var worldMatrix = model.WorldMatrix;
                var rayOrigin = _rayOrigin;
                var rayDirection = _rayDirection;
                var needsJointTransform = AttachJointsMode == AttachJointsMode.Attach && model.NeedsJointTransform;
                if (!needsJointTransform)
                {
                    // It might be cheaper to just transform the ray, for models with a lot of triangles.
                    GeomMath.TransformRay(_rayOrigin, _rayDirection, worldMatrix, out rayOrigin, out rayDirection);
                }

                var minIntersectionDistance = -1f;
                foreach (var triangle in model.Triangles)
                {
                    if (AttachJointsMode == AttachJointsMode.Hide && triangle.HasAttached)
                    {
                        continue;
                    }

                    Vector3 vertex0, vertex1, vertex2;
                    if (!needsJointTransform)
                    {
                        vertex0 = triangle.Vertices[0];
                        vertex1 = triangle.Vertices[1];
                        vertex2 = triangle.Vertices[2];
                        // We would use this if we're not transforming the ray
                        //Vector3.TransformPosition(ref triangle.Vertices[0], ref worldMatrix, out vertex0);
                        //Vector3.TransformPosition(ref triangle.Vertices[1], ref worldMatrix, out vertex1);
                        //Vector3.TransformPosition(ref triangle.Vertices[2], ref worldMatrix, out vertex2);
                    }
                    else
                    {
                        triangle.TransformPositions(ref worldMatrix, jointMatrices, out vertex0, out vertex1, out vertex2);
                    }
                    var intersectionDistance = GeomMath.TriangleIntersect(rayOrigin, rayDirection, vertex0, vertex1, vertex2, out _);

                    if (intersectionDistance > 0f && (intersectionDistance < minIntersectionDistance || minIntersectionDistance <= 0f))
                    {
                        minIntersectionDistance = intersectionDistance;
                    }
                }
                if (minIntersectionDistance > 0f)
                {
                    model.IntersectionDistance = minIntersectionDistance;
                    pickedEntities.Add(model);
                }
            }
            else
            {
                var intersectionDistance = GeomMath.BoxIntersect2(_rayOrigin, _rayDirection, entity.Bounds3D.Center, entity.Bounds3D.Extents);
                if (intersectionDistance > 0f)
                {
                    entity.IntersectionDistance = intersectionDistance;
                    pickedEntities.Add(entity);
                }
            }
        }

        private void CheckTriangles(EntityBase entity, List<Tuple<ModelEntity, Triangle>> pickedTriangles, Matrix4[] jointMatrices)
        {
            if (entity is ModelEntity model && model.Triangles.Length > 0)
            {
                var worldMatrix = model.WorldMatrix;
                var rayOrigin = _rayOrigin;
                var rayDirection = _rayDirection;
                var needsJointTransform = AttachJointsMode == AttachJointsMode.Attach && model.NeedsJointTransform;
                if (!needsJointTransform)
                {
                    // It might be cheaper to just transform the ray, for models with a lot of triangles.
                    GeomMath.TransformRay(_rayOrigin, _rayDirection, worldMatrix, out rayOrigin, out rayDirection);
                }

                foreach (var triangle in model.Triangles)
                {
                    if (AttachJointsMode == AttachJointsMode.Hide && triangle.HasAttached)
                    {
                        continue;
                    }

                    Vector3 vertex0, vertex1, vertex2;
                    if (!needsJointTransform)
                    {
                        vertex0 = triangle.Vertices[0];
                        vertex1 = triangle.Vertices[1];
                        vertex2 = triangle.Vertices[2];
                        // We would use this if we're not transforming the ray
                        //Vector3.TransformPosition(ref triangle.Vertices[0], ref worldMatrix, out vertex0);
                        //Vector3.TransformPosition(ref triangle.Vertices[1], ref worldMatrix, out vertex1);
                        //Vector3.TransformPosition(ref triangle.Vertices[2], ref worldMatrix, out vertex2);
                    }
                    else
                    {
                        triangle.TransformPositions(ref worldMatrix, jointMatrices, out vertex0, out vertex1, out vertex2);
                    }
                    var intersectionDistance = GeomMath.TriangleIntersect(rayOrigin, rayDirection, vertex0, vertex1, vertex2, out _);

                    if (intersectionDistance > 0f)
                    {
                        triangle.IntersectionDistance = intersectionDistance;
                        pickedTriangles.Add(new Tuple<ModelEntity, Triangle>(model, triangle));
                    }
                }
            }
        }

        public void FocusOnBounds(BoundingBox bounds)
        {
            _cameraYaw = 0f;
            _cameraPitch = 0f;
            _cameraX = 0f;
            _cameraY = 0f;
            // Target the center of the bounding box, so that models that aren't close to the origin are easy to view.
            _viewTarget = bounds.Center;
            _viewTargetBounds = new BoundingBox(bounds);
            CameraDistance = DistanceToFitBounds(bounds);

            // Settings to reproduce broken CameraRotationOld quaternion.
            // Broken:  {V: (1.331799E-08, -0.1654844, -2.234731E-09), W: 0.9862124}
            // Working: {V: (0.6572002, 0, -0.1102769), W: 0.745605}
#if false
            CameraFOV      = 60f;
            CameraDistance = 378.244873f;
            _cameraX       = 0f;
            _cameraY       = 0f;
            _cameraPitch   = 2.70083547E-08f;
            _cameraYaw     = 5.950687f;
            LightIntensity = 1f;
            LightPitchYaw  = new Vector2(3.92699075f, 2.3561945f);
#endif

            UpdateViewMatrix();
            OnCameraChanged();
        }

        public void UpdateTexture(Bitmap textureBitmap, int texturePage)
        {
            TextureBinder.UpdateTexture(textureBitmap, texturePage);
        }

        private float DistanceToFitBounds(BoundingBox bounds)
        {
            var radius = bounds.MagnitudeFromPosition(_viewTarget);
            // Legacy FOV logic: camera distance is already divided by
            // FOV distance scalar, but we want 60 FOV as the baseline.
            return (radius / (float)Math.Sin(60f * GeomMath.Deg2Rad / 2f)) + 0.1f;
        }

        public GizmoId GetGizmoUnderPosition(EntityBase selectedEntityBase, GizmoType currentType)
        {
            if (!ShowVisuals || currentType == GizmoType.None)
            {
                return GizmoId.None;
            }
            if (selectedEntityBase != null)
            {
                var matrix = GetGizmoMatrix(selectedEntityBase, currentType);

                // Find the closest gizmo that's intersected
                var minIntersectionGizmo = GizmoId.None;
                var minIntersectionDistance = float.MaxValue;
                GeomMath.TransformRay(_rayOrigin, _rayDirection, matrix, out var rayOrigin, out var rayDirection);
                for (var gizmo = GizmoId.AxisX; gizmo <= GizmoId.Uniform; gizmo++)
                {
                    if (currentType != GizmoType.Scale && gizmo == GizmoId.Uniform)
                    {
                        continue;
                    }
                    var gizmoInfo = GizmoInfos[gizmo];

                    var intersectionDistance = -1f;
                    switch (currentType)
                    {
                        case GizmoType.Translate when gizmo != GizmoId.Uniform:
                            intersectionDistance = GeomMath.BoxIntersect2(rayOrigin, rayDirection, gizmoInfo.TranslateCenter, gizmoInfo.TranslateSize);
                            break;

                        case GizmoType.Rotate when gizmo != GizmoId.Uniform:
                            intersectionDistance = GeomMath.RingIntersect(rayOrigin, rayDirection, Vector3.Zero, gizmoInfo.AxisVector,
                                                        gizmoInfo.RotateHeight, gizmoInfo.RotateOuterRadius, gizmoInfo.RotateInnerRadius, out _);
                            break;

                        case GizmoType.Scale when gizmo != GizmoId.Uniform:
                            intersectionDistance = GeomMath.BoxIntersect2(rayOrigin, rayDirection, gizmoInfo.ScaleLengthCenter, gizmoInfo.ScaleLengthSize);
                            if (intersectionDistance > 0f)
                            {
                                break;
                            }
                            goto case GizmoType.Scale;

                        case GizmoType.Scale:
                            var intersectionDistance2 = GeomMath.BoxIntersect2(rayOrigin, rayDirection, gizmoInfo.ScaleEndCenter, gizmoInfo.ScaleEndSize);
                            if (intersectionDistance > 0f && intersectionDistance2 > 0f)
                            {
                                intersectionDistance = Math.Min(intersectionDistance, intersectionDistance2);
                            }
                            else if (intersectionDistance2 > 0f)
                            {
                                intersectionDistance = intersectionDistance2;
                            }
                            break;
                    }
                    if (intersectionDistance > 0f && intersectionDistance < minIntersectionDistance)
                    {
                        minIntersectionDistance = intersectionDistance;
                        minIntersectionGizmo = gizmo;
                    }
                }
                return minIntersectionGizmo;
            }
            return GizmoId.None;
        }

        public Vector3 GetPickedPosition()
        {
            GeomMath.PlaneIntersect(_rayOrigin, _rayDirection, Vector3.Zero, -CameraDirection, out var intersection);
            return intersection;
        }

        public Vector3? GetPickedPosition(Vector3 onNormal)
        {
            if (GeomMath.PlaneIntersect(_rayOrigin, _rayDirection, Vector3.Zero, onNormal, out var intersection) > 0f)
            {
                return intersection;
            }
            return null;
        }

        public void UpdatePicking(int x, int y)
        {
            if (!_viewMatrixValid)
            {
                return;
            }
            // See notes in SetupMatrices.
            //var nearClip = CameraNearClip;
            var nearClip = CameraDefaultNearClip;

            _rayOrigin = new Vector3(x, y, nearClip).UnProject(_projectionMatrix, _viewMatrix, ViewportWidth, ViewportHeight);
            _rayTarget = new Vector3(x, y, 1f).UnProject(_projectionMatrix, _viewMatrix, ViewportWidth, ViewportHeight);
            _rayDirection = (_rayTarget - _rayOrigin).Normalized();
        }

        public float CameraDistanceFrom(Vector3 point)
        {
            var distance = (_viewMatrix.Inverted().ExtractTranslation() - point).Length;
            return distance * _cameraDistanceScalar;
        }

        public float GetGizmoScale(Vector3 position)
        {
            return CameraDistanceFrom(position);
        }

        public Vector3 GetGizmoOrigin(EntityBase selectedEntityBase, GizmoType currentType)
        {
            switch (currentType)
            {
                case GizmoType.Translate:
                    return selectedEntityBase.Bounds3D.Center;
                case GizmoType.Rotate:
                case GizmoType.Scale:
                default:
                    return selectedEntityBase.WorldOrigin;
            }
        }

        private Matrix4 GetGizmoMatrix(EntityBase selectedEntityBase, GizmoType currentType)
        {
            var center = GetGizmoOrigin(selectedEntityBase, currentType);
            var translationMatrix = Matrix4.CreateTranslation(center);
            var scaleMatrix = Matrix4.CreateScale(GetGizmoScale(center));
            var rotationMatrix = Matrix4.Identity;
            switch (currentType)
            {
                case GizmoType.Translate:
                    if (selectedEntityBase.ParentEntity != null)
                    {
                        var parentWorldMatrix = selectedEntityBase.ParentEntity.WorldMatrix;
                        rotationMatrix = Matrix4.CreateFromQuaternion(parentWorldMatrix.ExtractRotationSafe());
                    }
                    break;
                case GizmoType.Rotate:
                case GizmoType.Scale:
                default:
                    {
                        var worldMatrix = selectedEntityBase.WorldMatrix;
                        rotationMatrix = Matrix4.CreateFromQuaternion(worldMatrix.ExtractRotationSafe());
                    }
                    break;
            }

            return scaleMatrix * rotationMatrix * translationMatrix;
        }

        public void ResetIntersection()
        {
            _intersected = null;
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            var screen = Vector3.Project(position, 0, 0, ViewportWidth, ViewportHeight, 0f, 1f, _viewMatrix * _projectionMatrix);
            screen.Y = ViewportHeight - 1f - screen.Y;
            return screen;
        }

        public Vector3 ScreenToWorldPoint(Vector3 screen)
        {
            return screen.UnProject(_projectionMatrix, _viewMatrix, ViewportWidth, ViewportHeight);
        }


        private static float CalculateCameraDistanceScalar(float fov)
        {
            return (float)(Math.Tan(fov * GeomMath.Deg2Rad / 2d) * 2d / CameraBaseDistanceScalar);
        }
    }
}
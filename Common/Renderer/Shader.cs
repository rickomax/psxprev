using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common.Renderer
{
    // OpenGL state machine wrapper to avoid unecessary expensive changes to state
    public class Shader : IDisposable
    {
        public static bool SSBOSupported { get; private set; } = true;
        public static bool JointsSupported { get; private set; } = true;
        public static int MaxJoints { get; private set; } // Maximum number of shader-time joints
        public static string GLSLVersion { get; private set; }

        // True to always force state changes, even if the shader's cached variable is the same
        private const bool ForceState = false;


        private readonly Scene _scene;
        private int _programId;
        public bool IsInitialized { get; private set; }
        public bool IsUsing { get; private set; }

        public Shader(Scene scene)
        {
            _scene = scene;
        }

        public void Dispose()
        {
            if (IsInitialized)
            {
                GL.DeleteProgram(_programId);
                _programId = 0;
                IsInitialized = false;
            }
        }

        public void Use()
        {
            if (ForceState || !IsUsing)
            {
                IsUsing = true;
                GL.UseProgram(_programId);
                GL.Enable(EnableCap.Texture2D);
                GL.DepthFunc(DepthFunction.Lequal);
                GL.CullFace(CullFaceMode.Front);
            }
            DepthTest = true;
            DepthMask = true; // Depth mask needs to be on when clearing depth buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Reset bindings, since they could have been modified outside of the draw call
            _activeTextureUnit = -1;
            _mainTextureId = 0;
            _vertexArrayObject = null;
            _skin = null;
        }

        public void Unuse()
        {
            if (ForceState || IsUsing)
            {
                IsUsing = false;
                DepthTest = false;
                GL.Disable(EnableCap.Texture2D);
                GL.UseProgram(0);
            }
        }

        public void ClearDepthBuffer()
        {
            DepthMask = true; // Depth mask needs to be on when clearing depth buffer
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        public void Reset()
        {
            IsUsing = false;
            _blend = false;
            _cullFace = false;
            _depthTest = false;
            _depthMask = false;
            _polygonMode = (PolygonMode)0;
            _pointSize = 0f;
            _lineWidth = 0f;
            _alpha = 0f;
            _mixtureRate = MixtureRate.None;
            _activeTextureUnit = 0;
            _viewportSize = Size.Empty;
            _clearColor = Vector4.Zero;
            _jointMode = 0;
            _lightMode = 0;
            _colorMode = 0;
            _textureMode = 0;
            _semiTransparentPass = 0;
            _lightIntensity = 0f;
            _lightDirection = Vector3.Zero;
            _maskColor = Vector3.Zero;
            _ambientColor = Vector3.Zero;
            _solidColor = Vector3.Zero;
            _uvOffset = Vector2.Zero;
            _normalMatrix = Matrix3.Zero;
            _normalSpriteMatrix = Matrix3.Zero;
            _modelMatrix = Matrix4.Zero;
            _modelSpriteMatrix = Matrix4.Zero;
            _viewMatrix = Matrix4.Zero;
            _projectionMatrix = Matrix4.Zero;
            _mainTextureId = 0;
            _mainTextureUnit = 0;
            _vertexArrayObject = null;
            _skin = null;
            _blendSourceFactor = (BlendingFactor)0;
            _blendDestFactor = (BlendingFactor)0;
            _blendColor = Vector4.Zero;
            _blendEquation = (BlendEquationMode)0;
        }

        #region Shader Locations

        public const int AttributeIndex_Position  = 0;
        public const int AttributeIndex_Normal    = 1;
        public const int AttributeIndex_Color     = 2;
        public const int AttributeIndex_Uv        = 3;
        public const int AttributeIndex_TiledArea = 4;
        public const int AttributeIndex_Joint     = 5;

        public const int BufferIndex_JointTransforms = 0;

        public const int TextureUnit_MainTexture = 0;

        public static int UniformIndex_MainTexture;
        private static int UniformIndex_NormalMatrix;
        private static int UniformIndex_NormalSpriteMatrix;
        private static int UniformIndex_ModelMatrix;
        private static int UniformIndex_ModelSpriteMatrix;
        private static int UniformIndex_ViewMatrix;
        private static int UniformIndex_ProjectionMatrix;
        private static int UniformIndex_LightDirection;
        private static int UniformIndex_MaskColor;
        private static int UniformIndex_AmbientColor;
        private static int UniformIndex_SolidColor;
        private static int UniformIndex_UVOffset;
        private static int UniformIndex_JointMode;
        private static int UniformIndex_LightMode;
        private static int UniformIndex_ColorMode;
        private static int UniformIndex_TextureMode;
        private static int UniformIndex_SemiTransparentPass;
        private static int UniformIndex_LightIntensity;

        private void BindAttributeLocations()
        {
            var attributes = new Dictionary<int, string>
            {
                { AttributeIndex_Position,  "in_Position" },
                { AttributeIndex_Normal,    "in_Normal" },
                { AttributeIndex_Color,     "in_Color" },
                { AttributeIndex_Uv,        "in_Uv" },
                { AttributeIndex_TiledArea, "in_TiledArea" },
                { AttributeIndex_Joint,     "in_Joint" },
            };
            foreach (var vertexAttributeLocation in attributes)
            {
                GL.BindAttribLocation(_programId, vertexAttributeLocation.Key, vertexAttributeLocation.Value);
            }
        }

        private void GetUniformLocations()
        {
            int Get(string name)
            {
                return GL.GetUniformLocation(_programId, name);
            }

            if (!SSBOSupported && JointsSupported)
            {
                var blockIndex_JointTransforms = GL.GetUniformBlockIndex(_programId, "buffer_JointTransforms");
                GL.UniformBlockBinding(_programId, blockIndex_JointTransforms, BufferIndex_JointTransforms);
            }

            UniformIndex_MainTexture         = Get("u_MainTexture");
            UniformIndex_NormalMatrix        = Get("u_NormalMatrix");
            UniformIndex_NormalSpriteMatrix  = Get("u_NormalSpriteMatrix");
            UniformIndex_ModelMatrix         = Get("u_ModelMatrix");
            UniformIndex_ModelSpriteMatrix   = Get("u_ModelSpriteMatrix");
            UniformIndex_ViewMatrix          = Get("u_ViewMatrix");
            UniformIndex_ProjectionMatrix    = Get("u_ProjectionMatrix");
            UniformIndex_LightDirection      = Get("u_LightDirection");
            UniformIndex_MaskColor           = Get("u_MaskColor");
            UniformIndex_AmbientColor        = Get("u_AmbientColor");
            UniformIndex_SolidColor          = Get("u_SolidColor");
            UniformIndex_UVOffset            = Get("u_UvOffset");
            UniformIndex_JointMode           = Get("u_JointMode");
            UniformIndex_LightMode           = Get("u_LightMode");
            UniformIndex_ColorMode           = Get("u_ColorMode");
            UniformIndex_TextureMode         = Get("u_TextureMode");
            UniformIndex_SemiTransparentPass = Get("u_SemiTransparentPass");
            UniformIndex_LightIntensity      = Get("u_LightIntensity");
        }

        #endregion

        #region Initialize

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            var glVersion = GL.GetString(StringName.Version);
            var shadingLanguageVersion = GL.GetString(StringName.ShadingLanguageVersion);
            var maxSSBOBlockSize = GL.GetInteger((GetPName)ArbShaderStorageBufferObject.MaxShaderStorageBlockSize);
            var maxUBOBlockSize  = GL.GetInteger(GetPName.MaxUniformBlockSize);
            var maxSSBOJoints = maxSSBOBlockSize / (sizeof(float) * 32); // sizeof(Matrix4[2])
            var maxUBOJoints  = maxUBOBlockSize  / (sizeof(float) * 32); // sizeof(Matrix4[2])

            // DON'T CHANGE THIS FOR TESTING. Change the property inside the DEBUG preprocessor.
            SSBOSupported = JointsSupported = true; // First try to support joints, then fallback to no joints on failure

#if DEBUG
            SSBOSupported = true; // Set to false to test shader with uniform buffer object joints support (limited joint count)
            JointsSupported = true; // Set to false to test shader without joints support
            const bool DebugTestFallbackNoSSBO = false; // Set to true to allow joints (SSBO) support to silently fail
            const bool DebugTestFallbackNoJoints = false; // Set to true to allow joints support to silently fail
            //Program.ConsoleLogger.WriteLine($"OpenGL   Version: {glVersion}");
            //Program.ConsoleLogger.WriteLine($"Max GLSL Version: {shadingLanguageVersion}");
            //Program.ConsoleLogger.WriteLine($"Max SSBO Joints: {maxSSBOJoints}");
            //Program.ConsoleLogger.WriteLine($"Max UBO  Joints: {maxUBOJoints}");
#endif

            var vertexShaderSource   = ManifestResourceLoader.LoadTextFile("Shaders/Shader.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("Shaders/Shader.frag");

            // Ensure that the shader sources have patterns that we're expecting
            CheckSource(VersionRegex, VersionPrefix, fragmentShaderSource, "fragment");
            CheckSource(VersionRegex, VersionPrefix, vertexShaderSource, "vertex");
            CheckSource(FallbackVersionRegex, FallbackPrefix, vertexShaderSource, "vertex");
            CheckSource(SSBODefineRegex, SSBODefine, vertexShaderSource, "vertex");
            CheckSource(JointsDefineRegex, JointsDefine, vertexShaderSource, "vertex");
            CheckSource(MaxJointsDefineRegex, MaxJointsDefine, vertexShaderSource, "vertex");

            if (maxSSBOJoints <= 0)
            {
                SSBOSupported = false;
            }

            string withSSBOJointsError = null, noSSBOJointsError = null, noJointsError = null;
            var failed = false;
            for (var i = 0; i < 2; i++)
            {
                _programId = GL.CreateProgram();

                if (!SSBOSupported)
                {
                    // Remove the define that enables SSBOs,
                    // and overwrite the version header with the fallback version.
                    var fallbackVertexVer = GetVersion(vertexShaderSource, FallbackVersionRegex);
                    vertexShaderSource = SSBODefineRegex.Replace(vertexShaderSource, string.Empty);
                    vertexShaderSource = ChangeVersion(vertexShaderSource, fallbackVertexVer);
                    vertexShaderSource = MaxJointsDefineRegex.Replace(vertexShaderSource, $"{MaxJointsDefine} {maxUBOJoints}");
                }
                if (!JointsSupported)
                {
                    // Remove the define that enables joints.
                    vertexShaderSource = JointsDefineRegex.Replace(vertexShaderSource, string.Empty);
                }

                var vertexShaderAddress = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShaderAddress, vertexShaderSource);
                GL.CompileShader(vertexShaderAddress);
                GL.AttachShader(_programId, vertexShaderAddress);

                var fragmentShaderAddress = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShaderAddress, fragmentShaderSource);
                GL.CompileShader(fragmentShaderAddress);
                GL.AttachShader(_programId, fragmentShaderAddress);

                BindAttributeLocations();

                GL.LinkProgram(_programId);
                if (GetLinkStatus())
                {
                    break; // Link successful
                }
                else if (!JointsSupported)
                {
                    // Both with and without joints support failed
                    noJointsError = GetInfoLog();
                    failed = true;
                    break;
                }
                else
                {
                    // With joints support failed, try again but without joints support
                    if (!SSBOSupported)
                    {
                        noSSBOJointsError = GetInfoLog();
                        JointsSupported = false;
#if DEBUG
                        // Always throw an exception if we're debugging and not testing fallback
                        failed |= !DebugTestFallbackNoJoints;
#endif
                    }
                    else
                    {
                        withSSBOJointsError = GetInfoLog();
                        SSBOSupported = false;
                        if (maxUBOJoints <= 0)
                        {
                            JointsSupported = false;
                        }
#if DEBUG
                        // Always throw an exception if we're debugging and not testing fallback
                        failed |= !DebugTestFallbackNoSSBO;
#endif
                    }

                    GL.DeleteProgram(_programId);
                    _programId = 0;
                }
            }

            if (failed)
            {
#if DEBUG
                //Program.ConsoleLogger.WriteLine(vertexShaderSource);
#endif
                throw new Exception($"joints: {withSSBOJointsError}\n\nno ssbo:{noSSBOJointsError}\n\nno joints: {noJointsError}");
            }

            GetUniformLocations();

            var vertexVer   = GetVersion(vertexShaderSource);
            var fragmentVer = GetVersion(fragmentShaderSource);
            // Find the higher version to display to the user.
            // Note: This won't work if one of the versions is compatibility.
            GLSLVersion = (vertexVer.CompareTo(fragmentVer) > 0 ? vertexVer : fragmentVer);
            if (JointsSupported)
            {
                MaxJoints = (SSBOSupported ? maxSSBOJoints : maxUBOJoints);
            }
            else
            {
                MaxJoints = 0;
            }

            IsInitialized = true;
        }

        // Note: Use "(?=[\r\n]|\z)" instead, because "$" doesn't handle CRLF... Wow.
        private const string FallbackPrefix = "//FALLBACK_VERSION ";
        private const string VersionPrefix = "#version ";
        private const string SSBODefine = "#define SSBO_SUPPORTED";
        private const string JointsDefine = "#define JOINTS_SUPPORTED";
        private const string MaxJointsDefine = "#define MAX_JOINTS";
        private const string PatternEnd = @"[ \t]*(?=//|[\r\n]|\z)";
        private const string PatternVersion = @"([0-9]+[^\n\r/]*?)" + PatternEnd;

        private static readonly Regex VersionRegex         = new Regex($@"^{RegexEscapeWS(VersionPrefix)}{PatternVersion}",
                                                                       RegexOptions.Multiline);
        private static readonly Regex FallbackVersionRegex = new Regex($@"^{RegexEscapeWS(FallbackPrefix)}{PatternVersion}",
                                                                       RegexOptions.Multiline);
        private static readonly Regex SSBODefineRegex      = new Regex($@"^({RegexEscapeWS(SSBODefine)}){PatternEnd}",
                                                                       RegexOptions.Multiline);
        private static readonly Regex JointsDefineRegex    = new Regex($@"^({RegexEscapeWS(JointsDefine)}){PatternEnd}",
                                                                       RegexOptions.Multiline);
        private static readonly Regex MaxJointsDefineRegex = new Regex($@"^{RegexEscapeWS(MaxJointsDefine)}[ \t]+([0-9]+){PatternEnd}",
                                                                       RegexOptions.Multiline);

        // Regex.Escape that also replaces spaces with any variable-length whitespace matching
        private static string RegexEscapeWS(string str)
        {
            // Note that Regex.Escape will escape spaces, so replace the backslash too
            return Regex.Replace(Regex.Escape(str), @"\\ ", @"[ \t]+");
        }

        private static string GetVersion(string source, Regex versionRegex = null)
        {
            var match = (versionRegex ?? VersionRegex).Match(source);
            if (match.Success)
            {
                var version = match.Groups[1].Value;
                version = Regex.Replace(version.Trim(), @"[ \t]+", " "); // Normalize whitespace
                return version;
            }
            return null;
        }

        private static string ChangeVersion(string source, string newVersion)
        {
            var match = VersionRegex.Match(source);
            return match.Success ? source.Replace(match.Groups[1], newVersion) : source;
        }

        // Ensures a regex pattern exists in the shader source. The last two arguments are for the error message.
        private static void CheckSource(Regex regex, string missingName, string source, string shaderName)
        {
            if (!regex.IsMatch(source))
            {
                throw new Exception($"Missing \"{missingName}\" in {shaderName} shader");
            }
        }

        private bool GetLinkStatus()
        {
            var parameters = new int[1];
            GL.GetProgram(_programId, GetProgramParameterName.LinkStatus, parameters);
            return parameters[0] == 1;
        }

        private string GetInfoLog()
        {
            var infoLength = new int[1];
            GL.GetProgram(_programId, GetProgramParameterName.InfoLogLength, infoLength);
            var bufSize = infoLength[0];
            GL.GetProgramInfoLog(_programId, bufSize, out _, out var log);
            return log;
        }

        #endregion

        #region OpenGL States

        private bool _blend;
        public bool Blend
        {
            get => _blend;
            set => Enable(ref _blend, value, EnableCap.Blend);
        }

        private bool _cullFace;
        public bool CullFace
        {
            get => _cullFace;
            set => Enable(ref _cullFace, value, EnableCap.CullFace);
        }

        private bool _depthTest;
        public bool DepthTest
        {
            get => _depthTest;
            set => Enable(ref _depthTest, value, EnableCap.DepthTest);
        }

        private bool _depthMask;
        public bool DepthMask
        {
            get => _depthMask;
            set
            {
                if (ForceState || _depthMask != value)
                {
                    _depthMask = value;
                    GL.DepthMask(value);
                }
            }
        }

        private PolygonMode _polygonMode;
        public PolygonMode PolygonMode
        {
            get => _polygonMode;
            set
            {
                if (ForceState || _polygonMode != value)
                {
                    _polygonMode = value;
                    GL.PolygonMode(MaterialFace.FrontAndBack, value);
                }
            }
        }

        private float _pointSize;
        public float PointSize
        {
            get => _pointSize;
            set
            {
                if (ForceState || _pointSize != value)
                {
                    _pointSize = value;
                    GL.PointSize(value);
                }
            }
        }

        private float _lineWidth;
        public float LineWidth
        {
            get => _lineWidth;
            set
            {
                if (ForceState || _lineWidth != value)
                {
                    _lineWidth = value;
                    GL.LineWidth(value);
                }
            }
        }

        private float _alpha;
        public float Alpha
        {
            get => _alpha;
            set
            {
                if (ForceState || _alpha != value)
                {
                    _alpha = value;
                    if (_mixtureRate == MixtureRate.Alpha)
                    {
                        // Update the blend color if the mixture rate is already alpha
                        BlendColor(1.0f, 1.0f, 1.0f, value); // C = A%
                    }
                }
            }
        }

        private MixtureRate _mixtureRate;
        public MixtureRate MixtureRate
        {
            get => _mixtureRate;
            set
            {
                if (ForceState || _mixtureRate != value)
                {
                    _mixtureRate = value;
                    switch (value)
                    {
                        case MixtureRate.Back50_Poly50:    //  50% back +  50% poly
                            BlendFunc(BlendingFactor.ConstantColor, BlendingFactor.ConstantColor); // C poly, C back
                            BlendColor(0.50f, 0.50f, 0.50f, 1.0f); // C = 50%
                            BlendEquation(BlendEquationMode.FuncAdd);
                            break;
                        case MixtureRate.Back100_Poly100:  // 100% back + 100% poly
                            BlendFunc(BlendingFactor.One, BlendingFactor.One); // 100% poly, 100% back
                            BlendEquation(BlendEquationMode.FuncAdd);
                            break;
                        case MixtureRate.Back100_PolyM100: // 100% back - 100% poly
                            BlendFunc(BlendingFactor.One, BlendingFactor.One);    // 100% poly, 100% back
                            BlendEquation(BlendEquationMode.FuncReverseSubtract); // back - poly
                            break;
                        case MixtureRate.Back100_Poly25:   // 100% back +  25% poly
                            BlendFunc(BlendingFactor.ConstantColor, BlendingFactor.One); // C poly, 100% back
                            BlendColor(0.25f, 0.25f, 0.25f, 1.0f); // C = 25%
                            BlendEquation(BlendEquationMode.FuncAdd);
                            break;
                        case MixtureRate.Alpha:            // 1-A% back +   A% poly
                            BlendFunc(BlendingFactor.ConstantAlpha, BlendingFactor.OneMinusConstantAlpha);
                            BlendColor(1.0f, 1.0f, 1.0f, _alpha); // C = A%
                            BlendEquation(BlendEquationMode.FuncAdd);
                            break;
                    }
                }
            }
        }

        private int _activeTextureUnit;
        public int ActiveTextureUnit
        {
            get => _activeTextureUnit;
            set
            {
                if (ForceState || _activeTextureUnit != value)
                {
                    _activeTextureUnit = value;
                    GL.ActiveTexture(TextureUnit.Texture0 + value);
                }
            }
        }

        private Size _viewportSize;
        public Size Viewport
        {
            get => _viewportSize;
            set
            {
                value.Width  = Math.Max(1, value.Width);
                value.Height = Math.Max(1, value.Height);
                if (ForceState || _viewportSize != value) // Size has no Equals(Size) overload
                {
                    _viewportSize = value;
                    GL.Viewport(value);
                }
            }
        }

        private Vector4 _clearColor;
        public Vector4 ClearColor
        {
            get => _clearColor;
            set
            {
                if (ForceState || !_clearColor.Equals(value))
                {
                    _clearColor = value;
                    GL.ClearColor(value.X, value.Y, value.Z, value.W);
                }
            }
        }

        #endregion

        #region Uniform States

        private int _jointMode;
        public int UniformJointMode
        {
            get => _jointMode;
            set
            {
                if (ForceState || _jointMode != value)
                {
                    _jointMode = value;
                    GL.Uniform1(UniformIndex_JointMode, value);
                }
            }
        }

        private int _lightMode;
        public int UniformLightMode
        {
            get => _lightMode;
            set
            {
                if (ForceState || _lightMode != value)
                {
                    _lightMode = value;
                    GL.Uniform1(UniformIndex_LightMode, value);
                }
            }
        }

        private int _colorMode;
        public int UniformColorMode
        {
            get => _colorMode;
            set
            {
                if (ForceState || _colorMode != value)
                {
                    _colorMode = value;
                    GL.Uniform1(UniformIndex_ColorMode, value);
                }
            }
        }

        private int _textureMode;
        public int UniformTextureMode
        {
            get => _textureMode;
            set
            {
                if (ForceState || _textureMode != value)
                {
                    _textureMode = value;
                    GL.Uniform1(UniformIndex_TextureMode, value);
                }
            }
        }

        private int _semiTransparentPass;
        public int UniformSemiTransparentPass
        {
            get => _semiTransparentPass;
            set
            {
                if (ForceState || _semiTransparentPass != value)
                {
                    _semiTransparentPass = value;
                    GL.Uniform1(UniformIndex_SemiTransparentPass, value);
                }
            }
        }

        private float _lightIntensity;
        public float UniformLightIntensity
        {
            get => _lightIntensity;
            set
            {
                if (ForceState || _lightIntensity != value)
                {
                    _lightIntensity = value;
                    GL.Uniform1(UniformIndex_LightIntensity, value);
                }
            }
        }

        private Vector3 _lightDirection;
        public Vector3 UniformLightDirection
        {
            get => _lightDirection;
            set
            {
                if (ForceState || !_lightDirection.Equals(value))
                {
                    _lightDirection = value;
                    GL.Uniform3(UniformIndex_LightDirection, ref value);
                }
            }
        }

        private Vector3 _maskColor;
        public Vector3 UniformMaskColor
        {
            get => _maskColor;
            set
            {
                if (ForceState || !_maskColor.Equals(value))
                {
                    _maskColor = value;
                    GL.Uniform3(UniformIndex_MaskColor, ref value);
                }
            }
        }

        private Vector3 _ambientColor;
        public Vector3 UniformAmbientColor
        {
            get => _ambientColor;
            set
            {
                if (ForceState || !_ambientColor.Equals(value))
                {
                    _ambientColor = value;
                    GL.Uniform3(UniformIndex_AmbientColor, ref value);
                }
            }
        }

        private Vector3 _solidColor;
        public Vector3 UniformSolidColor
        {
            get => _solidColor;
            set
            {
                if (ForceState || !_solidColor.Equals(value))
                {
                    _solidColor = value;
                    GL.Uniform3(UniformIndex_SolidColor, ref value);
                }
            }
        }

        private Vector2 _uvOffset;
        public Vector2 UniformUVOffset
        {
            get => _uvOffset;
            set
            {
                if (ForceState || !_uvOffset.Equals(value))
                {
                    _uvOffset = value;
                    GL.Uniform2(UniformIndex_UVOffset, ref value);
                }
            }
        }

        private Matrix3 _normalMatrix;
        public void UniformNormalMatrix(ref Matrix3 value)
        {
            if (ForceState || !_normalMatrix.Equals(value))
            {
                _normalMatrix = value;
                GL.UniformMatrix3(UniformIndex_NormalMatrix, true, ref value);
            }
        }

        private Matrix3 _normalSpriteMatrix;
        public void UniformNormalSpriteMatrix(ref Matrix3 value)
        {
            if (ForceState || !_normalSpriteMatrix.Equals(value))
            {
                _normalSpriteMatrix = value;
                GL.UniformMatrix3(UniformIndex_NormalSpriteMatrix, true, ref value);
            }
        }

        private Matrix4 _modelMatrix;
        public void UniformModelMatrix(ref Matrix4 value)
        {
            if (ForceState || !_modelMatrix.Equals(value))
            {
                _modelMatrix = value;
                GL.UniformMatrix4(UniformIndex_ModelMatrix, false, ref value);
            }
        }

        private Matrix4 _modelSpriteMatrix;
        public void UniformModelSpriteMatrix(ref Matrix4 value)
        {
            if (ForceState || !_modelSpriteMatrix.Equals(value))
            {
                _modelSpriteMatrix = value;
                GL.UniformMatrix4(UniformIndex_ModelSpriteMatrix, false, ref value);
            }
        }

        private Matrix4 _viewMatrix;
        public void UniformViewMatrix(ref Matrix4 value)
        {
            if (ForceState || !_viewMatrix.Equals(value))
            {
                _viewMatrix = value;
                GL.UniformMatrix4(UniformIndex_ViewMatrix, false, ref value);
            }
        }

        private Matrix4 _projectionMatrix;
        public void UniformProjectionMatrix(ref Matrix4 value)
        {
            if (ForceState || !_projectionMatrix.Equals(value))
            {
                _projectionMatrix = value;
                GL.UniformMatrix4(UniformIndex_ProjectionMatrix, false, ref value);
            }
        }

        #endregion

        #region Bindings

        private int _mainTextureId;
        private int _mainTextureUnit;
        public void BindTexture(int textureId)
        {
            if (ForceState || _mainTextureId != textureId)
            {
                if (textureId == 0)
                {
                    _mainTextureId = 0;
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
                else
                {
                    ActiveTextureUnit = TextureUnit_MainTexture;

                    _mainTextureId = textureId;
                    GL.BindTexture(TextureTarget.Texture2D, textureId);

                    // Sampler uniform NEEDS to be set with uint, not int!
                    //GL.Uniform1(UniformIndex_MainTexture, (uint)textureId); // Using uint overload binds to textureId
                    if (ForceState || _mainTextureUnit != TextureUnit_MainTexture)
                    {
                        _mainTextureUnit = TextureUnit_MainTexture;
                        GL.Uniform1(UniformIndex_MainTexture, TextureUnit_MainTexture); // Using int overload binds to TextureUnit (Texture0)
                    }
                }
            }
        }

        private VertexArrayObject _vertexArrayObject;
        public void BindMesh(Mesh mesh) => BindMesh(mesh?.VertexArrayObject);

        public void BindMesh(VertexArrayObject vertexArrayObject)
        {
            // Meshes may use the same mesh data as another mesh, so compare against the meshes with the data
            if (ForceState || _vertexArrayObject != vertexArrayObject)
            {
                if (vertexArrayObject == null)
                {
                    _vertexArrayObject?.Unbind();
                    _vertexArrayObject = null;
                }
                else
                {
                    _vertexArrayObject = vertexArrayObject;
                    vertexArrayObject.Bind();
                }
            }
        }

        private Skin _skin;
        public void BindSkin(Skin skin)
        {
            if (ForceState || _skin != skin)
            {
                if (skin == null)
                {
                    _skin?.Unbind();
                    _skin = null;
                }
                else
                {
                    _skin = skin;
                    skin.Bind();
                }
            }
        }

        #endregion

        #region Helpers

        // Helper function to call both GL.Enable and GL.Disable depending on the state.
        private static void Enable(ref bool field, bool value, EnableCap cap)
        {
            if (ForceState || field != value)
            {
                field = value;
                if (value)
                {
                    GL.Enable(cap);
                }
                else
                {
                    GL.Disable(cap);
                }
            }
        }

        // Internal functions called by MixtureRate property setter
        private BlendingFactor _blendSourceFactor;
        private BlendingFactor _blendDestFactor;
        private void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            if (ForceState || _blendSourceFactor != sfactor || _blendDestFactor != dfactor)
            {
                _blendSourceFactor = sfactor;
                _blendDestFactor   = dfactor;
                GL.BlendFunc(sfactor, dfactor);
            }
        }

        private Vector4 _blendColor;
        private void BlendColor(float red, float green, float blue, float alpha)
        {
            var value = new Vector4(red, green, blue, alpha);
            if (ForceState || !_blendColor.Equals(value))
            {
                _blendColor = value;
                GL.BlendColor(red, green, blue, alpha);
            }
        }

        private BlendEquationMode _blendEquation;
        private void BlendEquation(BlendEquationMode mode)
        {
            if (ForceState || _blendEquation != mode)
            {
                _blendEquation = mode;
                GL.BlendEquation(mode);
            }
        }

        #endregion
    }
}

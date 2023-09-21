using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common.Renderer
{
    // OpenGL state machine wrapper to avoid unecessary expensive changes to state
    public class Shader : IDisposable
    {
        public static bool JointsSupported { get; private set; } = true;
        public static string GLSLVersion { get; private set; }

        // True to always force state changes, even if the shader's cached variable is the same
        private const bool ForceState = false;


        private readonly Scene _scene;
        private int _programId;
        public bool Initialized { get; private set; }
        public bool Using { get; private set; }

        public Shader(Scene scene)
        {
            _scene = scene;
        }

        public void Dispose()
        {
            if (Initialized)
            {
                GL.DeleteProgram(_programId);
                _programId = 0;
                Initialized = false;
            }
        }

        public void Use()
        {
            if (ForceState || !Using)
            {
                Using = true;
                GL.UseProgram(_programId);
                GL.Enable(EnableCap.Texture2D);
                GL.DepthFunc(DepthFunction.Lequal);
                GL.CullFace(CullFaceMode.Front);
            }
            DepthTest = true;
            DepthMask = true; // Depth mask needs to be on when clearing depth buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Reset bindings, since they could have been modified outside of the draw call
            _activeTextureId = 0;
            _mesh = null;
            _skin = null;
        }

        public void Unuse()
        {
            if (ForceState || Using)
            {
                Using = false;
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

        #region Shader Locations

        public const int AttributeIndex_Position  = 0;
        public const int AttributeIndex_Color     = 1;
        public const int AttributeIndex_Normal    = 2;
        public const int AttributeIndex_Uv        = 3;
        public const int AttributeIndex_TiledArea = 4;
        public const int AttributeIndex_Texture   = 5;
        public const int AttributeIndex_Joint     = 6;

        public const int BufferIndex_Joints       = 0;

        private static int UniformIndex_NormalMatrix;
        private static int UniformIndex_ModelMatrix;
        private static int UniformIndex_MVPMatrix;
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
                { AttributeIndex_Position,   "in_Position" },
                { AttributeIndex_Color,      "in_Color" },
                { AttributeIndex_Normal,     "in_Normal" },
                { AttributeIndex_Uv,         "in_Uv" },
                { AttributeIndex_TiledArea,  "in_TiledArea" },
                { AttributeIndex_Texture,    "mainTex" },
                //{ AttributeIndex_Joint,      "in_Joint" },
            };
            if (JointsSupported)
            {
                attributes.Add(AttributeIndex_Joint, "in_Joint");
            }
            foreach (var vertexAttributeLocation in attributes)
            {
                GL.BindAttribLocation(_programId, vertexAttributeLocation.Key, vertexAttributeLocation.Value);
            }
        }

        private void GetUniformLocations()
        {
            int Loc(string name)
            {
                return GL.GetUniformLocation(_programId, name);
            }
            UniformIndex_NormalMatrix        = Loc("normalMatrix");
            UniformIndex_ModelMatrix         = Loc("modelMatrix");
            UniformIndex_MVPMatrix           = Loc("mvpMatrix");
            UniformIndex_ViewMatrix          = Loc("viewMatrix");
            UniformIndex_ProjectionMatrix    = Loc("projectionMatrix");
            UniformIndex_LightDirection      = Loc("lightDirection");
            UniformIndex_MaskColor           = Loc("maskColor");
            UniformIndex_AmbientColor        = Loc("ambientColor");
            UniformIndex_SolidColor          = Loc("solidColor");
            UniformIndex_UVOffset            = Loc("uvOffset");
            UniformIndex_JointMode           = Loc("jointMode");
            UniformIndex_LightMode           = Loc("lightMode");
            UniformIndex_ColorMode           = Loc("colorMode");
            UniformIndex_TextureMode         = Loc("textureMode");
            UniformIndex_SemiTransparentPass = Loc("semiTransparentPass");
            UniformIndex_LightIntensity      = Loc("lightIntensity");
        }

        #endregion

        #region Initialize

        public void Initialize()
        {
            if (Initialized)
            {
                return;
            }
            // DON'T CHANGE THIS FOR TESTING. Change the property inside the DEBUG preprocessor.
            JointsSupported = true; // First try to support joints, then fallback to no joints on failure

#if DEBUG
            JointsSupported = true; // Set to false to test shader without joints support
            const bool DebugTestFallback = false; // Set to true to allow joints support to silently fail
#endif

            var vertexShaderSource   = ManifestResourceLoader.LoadTextFile("Shaders/Shader.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("Shaders/Shader.frag");

            // Ensure that the shader sources have patterns that we're expecting
            CheckSource(VersionRegex, VersionPrefix, fragmentShaderSource, "fragment");
            CheckSource(VersionRegex, VersionPrefix, vertexShaderSource, "vertex");
            CheckSource(FallbackVersionRegex, FallbackPrefix, vertexShaderSource, "vertex");
            CheckSource(JointsDefineRegex, JointsDefine, vertexShaderSource, "vertex");

            string jointsError = null, noJointsError = null;
            var failed = false;
            for (var i = 0; i < 2; i++)
            {
                _programId = GL.CreateProgram();

                if (!JointsSupported)
                {
                    // Remove the define that enables joints,
                    // and overwrite the version header with the fallback version.
                    var fallbackVersion = GetVersion(vertexShaderSource, FallbackVersionRegex);
                    vertexShaderSource = JointsDefineRegex.Replace(vertexShaderSource, string.Empty);
                    vertexShaderSource = ChangeVersion(vertexShaderSource, fallbackVersion);
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
                    jointsError = GetInfoLog();
                    JointsSupported = false;
#if DEBUG
                    // Always throw an exception if we're debugging and not testing fallback
                    failed |= !DebugTestFallback;
#endif
                    GL.DeleteProgram(_programId);
                    _programId = 0;
                }
            }

            if (failed)
            {
                throw new Exception($"joints: {jointsError}\n\nno joints: {noJointsError}");
            }

            GetUniformLocations();

            var vertexVer   = GetVersion(vertexShaderSource);
            var fragmentVer = GetVersion(fragmentShaderSource);
            // Find the higher version to display to the user.
            // Note: This won't work if one of the versions is compatibility.
            GLSLVersion = (vertexVer.CompareTo(fragmentVer) > 0 ? vertexVer : fragmentVer);

            Initialized = true;
        }

        // Note: Use "(?=[\r\n]|\z)" instead, because "$" doesn't handle CRLF... Wow.
        private const string FallbackPrefix = "//FALLBACK_VERSION ";
        private const string VersionPrefix = "#version ";
        private const string JointsDefine = "#define JOINTS_SUPPORTED";
        private const string PatternEnd = @"[ \t]*(?=//|[\r\n]|\z)";
        private const string PatternVersion = @"([0-9]+[^\n\r/]*?)" + PatternEnd;

        private static readonly Regex VersionRegex         = new Regex($@"^{RegexEscapeWS(VersionPrefix)}{PatternVersion}",
                                                                       RegexOptions.Multiline);
        private static readonly Regex FallbackVersionRegex = new Regex($@"^{RegexEscapeWS(FallbackPrefix)}{PatternVersion}",
                                                                       RegexOptions.Multiline);
        private static readonly Regex JointsDefineRegex    = new Regex($@"^({RegexEscapeWS(JointsDefine)}){PatternEnd}",
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
                if (ForceState || _lightDirection != value)
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
                if (ForceState || _maskColor != value)
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
                if (ForceState || _ambientColor != value)
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
                if (ForceState || _solidColor != value)
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
                if (ForceState || _uvOffset != value)
                {
                    _uvOffset = value;
                    GL.Uniform2(UniformIndex_UVOffset, ref value);
                }
            }
        }

        private Matrix3 _normalMatrix;
        public void UniformNormalMatrix(ref Matrix3 value)
        {
            if (ForceState || _normalMatrix != value)
            {
                _normalMatrix = value;
                GL.UniformMatrix3(UniformIndex_NormalMatrix, true, ref value);
            }
        }

        private Matrix4 _modelMatrix;
        public void UniformModelMatrix(ref Matrix4 value)
        {
            if (ForceState || _modelMatrix != value)
            {
                _modelMatrix = value;
                GL.UniformMatrix4(UniformIndex_ModelMatrix, false, ref value);
            }
        }

        private Matrix4 _mvpMatrix;
        public void UniformMVPMatrix(ref Matrix4 value)
        {
            if (ForceState || _mvpMatrix != value)
            {
                _mvpMatrix = value;
                GL.UniformMatrix4(UniformIndex_MVPMatrix, false, ref value);
            }
        }

        private Matrix4 _viewMatrix;
        public void UniformViewMatrix(ref Matrix4 value)
        {
            if (ForceState || _viewMatrix != value)
            {
                _viewMatrix = value;
                GL.UniformMatrix4(UniformIndex_ViewMatrix, false, ref value);
            }
        }

        private Matrix4 _projectionMatrix;
        public void UniformProjectionMatrix(ref Matrix4 value)
        {
            if (ForceState || _projectionMatrix != value)
            {
                _projectionMatrix = value;
                GL.UniformMatrix4(UniformIndex_ProjectionMatrix, false, ref value);
            }
        }

        #endregion

        #region Bindings

        private int _activeTextureId;
        public void BindTexture(int textureId)
        {
            if (ForceState || _activeTextureId != textureId)
            {
                if (textureId == 0)
                {
                    //_scene.TextureBinder.Unbind();
                    _activeTextureId = 0;
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
                else
                {
                    _activeTextureId = textureId;
                    //_scene.TextureBinder.BindTexture(textureId);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, textureId);
                    // Sampler uniform NEEDS to be set with uint, not int!
                    GL.Uniform1(AttributeIndex_Texture, (uint)textureId);
                }
            }
        }

        private Mesh _mesh;
        public void BindMesh(Mesh mesh)
        {
            // Meshes may use the same mesh data as another mesh, so compare against the meshes with the data
            if (ForceState || _mesh?.OwnerMesh != mesh?.OwnerMesh)
            {
                if (mesh == null)
                {
                    _mesh?.Unbind();
                    _mesh = null;
                }
                else
                {
                    _mesh = mesh;
                    mesh.Bind();
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
            if (ForceState || _blendColor != value)
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

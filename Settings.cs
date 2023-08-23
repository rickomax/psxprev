using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using OpenTK;
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Exporters;
using PSXPrev.Common.Renderer;
using PSXPrev.Common.Utils;

namespace PSXPrev
{
    [JsonObject]
    public class Settings
    {
        // Returns "<path>\\<exename>.settings.json"
        public static string FilePath => Path.ChangeExtension(Application.ExecutablePath, ".settings.json");

        public static readonly Settings Defaults = new Settings();

        public static Settings Instance { get; set; } = new Settings();


        [JsonProperty("gridSnap")]
        public float GridSnap { get; set; } = 1f;

        [JsonProperty("angleSnap")]
        public float AngleSnap { get; set; } = 1f;

        [JsonProperty("scaleSnap")]
        public float ScaleSnap { get; set; } = 0.05f;

        [JsonProperty("cameraFOV")]
        public float CameraFOV { get; set; } = 60f;

        [JsonIgnore]
        public float CameraFOVRads => CameraFOV * GeomMath.Deg2Rad;

        [JsonProperty("lightIntensity")]
        public float LightIntensity { get; set; } = 100f;

        [JsonProperty("lightYaw")]
        public float LightYaw { get; set; } = 135f;

        [JsonProperty("lightPitch")]
        public float LightPitch { get; set; } = 225f;

        [JsonIgnore]
        public Vector2 LightPitchYawRads => new Vector2(LightPitch * GeomMath.Deg2Rad, LightYaw * GeomMath.Deg2Rad);

        [JsonProperty("lightEnabled")]
        public bool LightEnabled { get; set; } = true;

        [JsonProperty("ambientEnabled")]
        public bool AmbientEnabled { get; set; } = true;

        [JsonProperty("texturesEnabled")]
        public bool TexturesEnabled { get; set; } = true;

        [JsonProperty("semiTransparencyEnabled")]
        public bool SemiTransparencyEnabled { get; set; } = true;

        [JsonProperty("forceDoubleSided")]
        public bool ForceDoubleSided { get; set; } = false;

        [JsonProperty("autoAttachLimbs")]
        public bool AutoAttachLimbs { get; set; } = true;

        [JsonProperty("drawModeFaces")]
        public bool DrawFaces { get; set; } = true;

        [JsonProperty("drawModeWireframe")]
        public bool DrawWireframe { get; set; } = false;

        [JsonProperty("drawModeVertices")]
        public bool DrawVertices { get; set; } = false;

        [JsonProperty("drawModeSolidWireframeAndVertices")]
        public bool DrawSolidWireframeVertices { get; set; } = true;

        [JsonProperty("wireframeSize")]
        public float WireframeSize { get; set; } = 1f;

        [JsonProperty("vertexSize")]
        public float VertexSize { get; set; } = 2f;

        [JsonProperty("gizmoTool"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public GizmoType GizmoType { get; set; } = GizmoType.Translate;

        [JsonProperty("showBounds")]
        public bool ShowBounds { get; set; } = true;

        [JsonProperty("showLightRotationRay")]
        public bool ShowLightRotationRay { get; set; } = true;

        [JsonProperty("showDebugVisuals")]
        public bool ShowDebugVisuals { get; set; } = false;

        [JsonProperty("showDebugPickingRay")]
        public bool ShowDebugPickingRay { get; set; } = false;

        [JsonProperty("showDebugIntersections")]
        public bool ShowDebugIntersections { get; set; } = false;

        [JsonProperty("backgroundColor"), JsonConverter(typeof(JsonStringColorConverter))]
        public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.LightSkyBlue;

        [JsonProperty("ambientColor"), JsonConverter(typeof(JsonStringColorConverter))]
        public System.Drawing.Color AmbientColor { get; set; } = System.Drawing.Color.LightGray;

        [JsonProperty("maskColor"), JsonConverter(typeof(JsonStringColorConverter))]
        public System.Drawing.Color MaskColor { get; set; } = System.Drawing.Color.Black;

        [JsonProperty("solidWireframeAndVerticesColor"), JsonConverter(typeof(JsonStringColorConverter))]
        public System.Drawing.Color SolidWireframeVerticesColor { get; set; } = System.Drawing.Color.Gray;

        [JsonProperty("colorDialogCustomColors", ItemConverterType = typeof(JsonStringColorConverter))]
        private System.Drawing.Color[] ColorDialogCustomColors { get; set; } = new System.Drawing.Color[0];

        [JsonProperty("currentCLUTIndex")]
        public int CurrentCLUTIndex { get; set; } = 0;

        [JsonProperty("showUVsInVRAM")]
        public bool ShowUVsInVRAM { get; set; } = true;

        [JsonProperty("autoDrawModelTextures")]
        public bool AutoDrawModelTextures { get; set; } = false;

        [JsonProperty("autoPlayAnimation")]
        public bool AutoPlayAnimation { get; set; } = false;

        [JsonProperty("autoSelectAnimationModel")]
        public bool AutoSelectAnimationModel { get; set; } = true;

        [JsonProperty("animationLoopMode"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public AnimationLoopMode AnimationLoopMode { get; set; } = AnimationLoopMode.Loop;

        [JsonProperty("animationReverse")]
        public bool AnimationReverse { get; set; } = false;

        [JsonProperty("animationSpeed")]
        public float AnimationSpeed { get; set; } = 1f;

        [JsonProperty("fastWindowResize")]
        public bool FastWindowResize { get; set; } = false;

        [JsonProperty("logStandardColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public ConsoleColor LogStandardColor { get; set; } = ConsoleColor.White;

        [JsonProperty("logPositiveColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public ConsoleColor LogPositiveColor { get; set; } = ConsoleColor.Green;

        [JsonProperty("logWarningColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public ConsoleColor LogWarningColor { get; set; } = ConsoleColor.Yellow;

        [JsonProperty("logErrorColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public ConsoleColor LogErrorColor { get; set; } = ConsoleColor.Red;

        [JsonProperty("logExceptionPrefixColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public ConsoleColor LogExceptionPrefixColor { get; set; } = ConsoleColor.DarkGray;

        [JsonProperty("scanOptionsShowAdvanced")]
        public bool ShowAdvancedScanOptions { get; set; } = false;

        [JsonProperty("scanOptions")]
        public ScanOptions ScanOptions { get; set; } = new ScanOptions();

        [JsonProperty("exportOptions")]
        public ExportModelOptions ExportModelOptions { get; set; } = new ExportModelOptions();


        // Assigns default color to the final index (15) if specified.
        public int[] GetColorDialogCustomColors(System.Drawing.Color? defaultColor = null)
        {
            int ToBgr(System.Drawing.Color col)
            {
                return (int)((uint)col.R | ((uint)col.G << 8) | ((uint)col.B << 16));
            }

            var customBgrColors = new int[16];
            for (var i = 0; i < customBgrColors.Length; i++)
            {
                customBgrColors[i] = ToBgr(System.Drawing.Color.White);
            }
            if (ColorDialogCustomColors != null)
            {
                for (var i = 0; i < Math.Min(customBgrColors.Length, ColorDialogCustomColors.Length); i++)
                {
                    customBgrColors[i] = ToBgr(ColorDialogCustomColors[i]);
                }
            }
            // Reserve final slot for default color value
            if (defaultColor.HasValue)
            {
                customBgrColors[customBgrColors.Length - 1] = ToBgr(defaultColor.Value);
            }
            return customBgrColors;
        }

        // Only includes up to 15 colors, index 15 is reserved for default colors, and is ignored.
        public void SetColorDialogCustomColors(int[] customBgrColors)
        {
            System.Drawing.Color FromBgr(int bgr)
            {
                return System.Drawing.Color.FromArgb(bgr & 0xff, (bgr >> 8) & 0xff, (bgr >> 16) & 0xff);
            }

            if (customBgrColors == null)
            {
                ColorDialogCustomColors = new System.Drawing.Color[0];
            }
            else
            {
                // Don't fill the settings array with empty white colors.
                // Use as many custom colors as there are until all remaining colors are white.
                var white = System.Drawing.Color.White;
                var customCount = Math.Min(15, customBgrColors.Length);
                for (; customCount > 0; customCount--)
                {
                    // NEVER use System.Drawing.Color equality, because it also checks
                    // stupid things like name, and we also ignore the alpha value.
                    var color = FromBgr(customBgrColors[customCount - 1]);
                    if (color.R != white.R || color.G != white.G || color.B != white.B)
                    {
                        break;
                    }
                }
                ColorDialogCustomColors = new System.Drawing.Color[customCount];
                for (var i = 0; i < customCount; i++)
                {
                    ColorDialogCustomColors[i] = FromBgr(customBgrColors[i]);
                }
            }
        }


        public void Validate()
        {
            GridSnap          = ValidateMax(  GridSnap,          Defaults.GridSnap, 0f);
            CameraFOV         = ValidateClamp(CameraFOV,         Defaults.CameraFOV, Scene.CameraMinFOV, Scene.CameraMaxFOV);
            LightIntensity    = ValidateMax(  LightIntensity,    Defaults.LightIntensity, 0f);
            LightYaw          = ValidateAngle(LightYaw,          Defaults.LightYaw);
            LightPitch        = ValidateAngle(LightPitch,        Defaults.LightPitch);
            WireframeSize     = ValidateMax(  WireframeSize,     Defaults.WireframeSize, 1f);
            VertexSize        = ValidateMax(  VertexSize,        Defaults.VertexSize,    1f);
            BackgroundColor   = ValidateColor(BackgroundColor,   Defaults.BackgroundColor);
            AmbientColor      = ValidateColor(AmbientColor,      Defaults.AmbientColor);
            MaskColor         = ValidateColor(MaskColor,         Defaults.MaskColor);
            CurrentCLUTIndex  = ValidateClamp(CurrentCLUTIndex,  Defaults.CurrentCLUTIndex, 0, 255);
            AnimationLoopMode = ValidateEnum( AnimationLoopMode, Defaults.AnimationLoopMode);
            AnimationSpeed    = ValidateClamp(AnimationSpeed,    Defaults.AnimationSpeed, 0.01f, 100f);
            LogStandardColor        = ValidateEnum(LogStandardColor,        Defaults.LogStandardColor);
            LogPositiveColor        = ValidateEnum(LogPositiveColor,        Defaults.LogPositiveColor);
            LogWarningColor         = ValidateEnum(LogWarningColor,         Defaults.LogWarningColor);
            LogErrorColor           = ValidateEnum(LogErrorColor,           Defaults.LogErrorColor);
            LogExceptionPrefixColor = ValidateEnum(LogExceptionPrefixColor, Defaults.LogExceptionPrefixColor);

            if (ColorDialogCustomColors == null)
            {
                ColorDialogCustomColors = new System.Drawing.Color[0];
            }

            if (ScanOptions == null)
            {
                ScanOptions = new ScanOptions();
            }
            if (ExportModelOptions == null)
            {
                ExportModelOptions = new ExportModelOptions();
            }
        }

        public Settings Clone()
        {
            var settings = (Settings)MemberwiseClone();
            settings.ScanOptions = settings.ScanOptions?.Clone();
            settings.ExportModelOptions = settings.ExportModelOptions?.Clone();
            return settings;
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch
            {
                // Failure to write settings file should not be fatal.
            }
        }

        public static void Load()
        {
            try
            {
                Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath), new JsonSerializerSettings
                {
                    // Fix security vulnerability in-case the settings file somehow gets replaced by someone else
                    MaxDepth = 128,
                });
                Instance.Validate();

                // Create a settings file if one doesn't already exist.
                if (!File.Exists(FilePath))
                {
                    Instance.Save();
                }
            }
            catch
            {
                // Load default settings on failure.
                LoadDefaults();
            }
        }

        public static void LoadDefaults()
        {
            var oldInstance = Instance;
            Instance = new Settings();

            // There's no reason not to preserve color dialog custom colors when resetting settings.
            if (oldInstance?.ColorDialogCustomColors != null)
            {
                Instance.ColorDialogCustomColors = (System.Drawing.Color[])oldInstance.ColorDialogCustomColors.Clone();
            }

            // Create a settings file if one doesn't already exist.
            if (!File.Exists(FilePath))
            {
                Instance.Save();
            }
        }


        private static float ValidateMax(float value, float @default, float min)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? @default : Math.Max(value, min);
        }

        private static float ValidateMin(float value, float @default, float max)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? @default : Math.Min(value, max);
        }

        private static float ValidateClamp(float value, float @default, float min, float max)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? @default : GeomMath.Clamp(value, min, max);
        }

        private static int ValidateClamp(int value, int @default, int min, int max)
        {
            // Yeah, this ignores default...
            return GeomMath.Clamp(value, min, max);
        }

        private static float ValidateAngle(float value, float @default)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? @default : GeomMath.PositiveModulus(value, 360f);
        }

        private static System.Drawing.Color ValidateColor(System.Drawing.Color value, System.Drawing.Color @default)
        {
            return value.A != 255 ? @default : value;
        }

        private static TEnum ValidateEnum<TEnum>(TEnum value, TEnum @default)
        {
            return !Enum.IsDefined(typeof(TEnum), value) ? @default : value;
        }
    }
}
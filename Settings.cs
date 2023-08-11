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
        private static readonly Settings _defaults = new Settings();

        // Returns "<path>\\<exename>.settings.json"
        private static string SettingsFilePath => Path.ChangeExtension(Application.ExecutablePath, ".settings.json");

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

        [JsonProperty("scanOptions")]
        public ScanOptions ScanOptions { get; set; } = new ScanOptions();

        [JsonProperty("exportOptions")]
        public ExportModelOptions ExportModelOptions { get; set; } = new ExportModelOptions();


        public void Validate()
        {
            GridSnap          = ValidateMax(  GridSnap,          _defaults.GridSnap, 0f);
            CameraFOV         = ValidateClamp(CameraFOV,         _defaults.CameraFOV, Scene.CameraMinFOV, Scene.CameraMaxFOV);
            LightIntensity    = ValidateMax(  LightIntensity,    _defaults.LightIntensity, 0f);
            LightYaw          = ValidateAngle(LightYaw,          _defaults.LightYaw);
            LightPitch        = ValidateAngle(LightPitch,        _defaults.LightPitch);
            WireframeSize     = ValidateMax(  WireframeSize,     _defaults.WireframeSize, 1f);
            VertexSize        = ValidateMax(  VertexSize,        _defaults.VertexSize,    1f);
            BackgroundColor   = ValidateColor(BackgroundColor,   _defaults.BackgroundColor);
            AmbientColor      = ValidateColor(AmbientColor,      _defaults.AmbientColor);
            MaskColor         = ValidateColor(MaskColor,         _defaults.MaskColor);
            AnimationLoopMode = ValidateEnum( AnimationLoopMode, _defaults.AnimationLoopMode);
            AnimationSpeed    = ValidateClamp(AnimationSpeed,    _defaults.AnimationSpeed, 0.01f, 100f);
            LogStandardColor        = ValidateEnum(LogStandardColor,        _defaults.LogStandardColor);
            LogPositiveColor        = ValidateEnum(LogPositiveColor,        _defaults.LogPositiveColor);
            LogWarningColor         = ValidateEnum(LogWarningColor,         _defaults.LogWarningColor);
            LogErrorColor           = ValidateEnum(LogErrorColor,           _defaults.LogErrorColor);
            LogExceptionPrefixColor = ValidateEnum(LogExceptionPrefixColor, _defaults.LogExceptionPrefixColor);

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
                File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
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
                Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFilePath));
                Instance.Validate();

                // Create a settings file if one doesn't already exist.
                if (!File.Exists(SettingsFilePath))
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
            Instance = new Settings();

            // Create a settings file if one doesn't already exist.
            if (!File.Exists(SettingsFilePath))
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
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

        private static string SettingsFilePath => Path.Combine(Application.StartupPath, "PSXPrev.settings.json");

        public static Settings Instance { get; set; } = new Settings();


        [JsonProperty("gridSnap")]
        public float GridSnap { get; set; } = 1f;

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

        [JsonProperty("showWireframe")]
        public bool ShowWireframe { get; set; } = false;

        [JsonProperty("showVertices")]
        public bool ShowVertices { get; set; } = false;

        [JsonProperty("wireframeSize")]
        public float WireframeSize { get; set; } = 1f;

        [JsonProperty("vertexSize")]
        public float VertexSize { get; set; } = 2f;

        [JsonProperty("showGizmos")]
        public bool ShowGizmos { get; set; } = true;

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

        [JsonProperty("showUVsInVRAM")]
        public bool ShowUVsInVRAM { get; set; } = true;

        [JsonProperty("autoDrawModelTextures")]
        public bool AutoDrawModelTextures { get; set; } = false;

        [JsonProperty("autoPlayAnimation")]
        public bool AutoPlayAnimation { get; set; } = false;

        [JsonProperty("autoSelectAnimationModel")]
        public bool AutoSelectAnimationModel { get; set; } = false;

        [JsonProperty("animationLoopMode"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public AnimationLoopMode AnimationLoopMode { get; set; } = AnimationLoopMode.Loop;

        [JsonProperty("animationReverse")]
        public bool AnimationReverse { get; set; } = false;

        [JsonProperty("animationSpeed")]
        public float AnimationSpeed { get; set; } = 1f;

        [JsonProperty("exportOptions")]
        public ExportModelOptions ExportModelOptions { get; set; } = new ExportModelOptions();

        [JsonProperty("scanOptions")]
        public ScanOptions ScanOptions { get; set; } = new ScanOptions();


        public void Validate()
        {
            GridSnap        = ValidateMax(  GridSnap,        _defaults.GridSnap, 0f);
            CameraFOV       = ValidateClamp(CameraFOV,       _defaults.CameraFOV, Scene.CameraMinFOV, Scene.CameraMaxFOV);
            LightIntensity  = ValidateMax(  LightIntensity,  _defaults.LightIntensity, 0f);
            LightYaw        = ValidateAngle(LightYaw,        _defaults.LightYaw);
            LightPitch      = ValidateAngle(LightPitch,      _defaults.LightPitch);
            WireframeSize   = ValidateMax(  WireframeSize,   _defaults.WireframeSize, 1f);
            VertexSize      = ValidateMax(  VertexSize,      _defaults.VertexSize,    1f);
            BackgroundColor = ValidateColor(BackgroundColor, _defaults.BackgroundColor);
            AmbientColor    = ValidateColor(AmbientColor,    _defaults.AmbientColor);
            MaskColor       = ValidateColor(MaskColor,       _defaults.MaskColor);
            AnimationSpeed  = ValidateClamp(AnimationSpeed,  _defaults.AnimationSpeed, 0.01f, 100f);

            if (!Enum.IsDefined(typeof(AnimationLoopMode), AnimationLoopMode))
            {
                AnimationLoopMode = _defaults.AnimationLoopMode;
            }

            if (ExportModelOptions == null)
            {
                ExportModelOptions = new ExportModelOptions();
            }
            if (ScanOptions == null)
            {
                ScanOptions = new ScanOptions();
            }
        }

        public Settings Clone()
        {
            var settings = (Settings)MemberwiseClone();
            settings.ExportModelOptions = settings.ExportModelOptions?.Clone();
            settings.ScanOptions = settings.ScanOptions?.Clone();
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
            }
            catch
            {
                Instance = new Settings(); // Load default settings on failure.
            }
            Instance.Save();
        }

        public static void LoadDefaults()
        {
            Instance = new Settings();
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
    }
}
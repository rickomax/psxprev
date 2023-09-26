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

        public const uint CurrentVersion = 2;


        // Any settings that need to be changed beteen versions can be handled with this.
        [JsonProperty("version")]
        public uint Version { get; set; } = CurrentVersion;

        [JsonProperty("antialiasing")]
        public int Multisampling { get; set; } = 0;

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
        public float LightIntensity { get; set; } = 1f;

        [JsonProperty("lightYaw")]
        public float LightYaw { get; set; } = 135f;

        [JsonProperty("lightPitch")]
        public float LightPitch { get; set; } = 225f;

        [JsonIgnore]
        public Vector2 LightPitchYawRads => new Vector2(LightPitch * GeomMath.Deg2Rad, LightYaw * GeomMath.Deg2Rad);

        [JsonProperty("lightEnabled")]
        public bool LightEnabled { get; set; } = true;

        [JsonProperty("ambientEnabled")]
        public bool AmbientEnabled { get; set; } = true; // We can probably remove this, it's just the same as changing ambient color to black.

        [JsonProperty("texturesEnabled")]
        public bool TexturesEnabled { get; set; } = true;

        [JsonProperty("vertexColorEnabled")]
        public bool VertexColorEnabled { get; set; } = true;

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

        [JsonProperty("modelSelectionMode"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public EntitySelectionMode ModelSelectionMode { get; set; } = EntitySelectionMode.Bounds;

        [JsonProperty("subModelVisibility"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public SubModelVisibility SubModelVisibility { get; set; } = SubModelVisibility.All;

        [JsonProperty("autoFocusOnRootModel")]
        public bool AutoFocusOnRootModel { get; set; } = true;

        [JsonProperty("autoFocusOnSubModel")]
        public bool AutoFocusOnSubModel { get; set; } = false;

        [JsonProperty("autoFocusIncludeWholeModel")]
        public bool AutoFocusIncludeWholeModel { get; set; } = false;

        [JsonProperty("autoFocusIncludeCheckedModels")]
        public bool AutoFocusIncludeCheckedModels { get; set; } = true;

        [JsonProperty("autoFocusResetCameraRotation")]
        public bool AutoFocusResetCameraRotation { get; set; } = true;

        [JsonProperty("showBounds")]
        public bool ShowBounds { get; set; } = true;

        [JsonProperty("showSkeleton")]
        public bool ShowSkeleton { get; set; } = false;

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

        [JsonProperty("showTexturePalette")]
        public bool ShowTexturePalette { get; set; } = false;

        [JsonProperty("showTextureSemiTransparency")]
        public bool ShowTextureSemiTransparency { get; set; } = false;

        [JsonProperty("showMissingTextures")]
        public bool ShowMissingTextures { get; set; } = true;

        [JsonProperty("autoDrawModelTextures")]
        public bool AutoDrawModelTextures { get; set; } = false;

        [JsonProperty("autoPackModelTextures")]
        public bool AutoPackModelTextures { get; set; } = false;

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

        [JsonProperty("showFPS")]
        public bool ShowFPS { get; set; } = false;

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


        // 30f for Frogger 2 and Chicken Run, 60f for Action Man 2.
        [JsonProperty("advancedBFFFrameRate")]
        public float AdvancedBFFFrameRate { get; set; } = 30f;

        [JsonProperty("advancedBFFSpriteScale")]
        public float AdvancedBFFSpriteScale { get; set; } = 2.5f;

        // Unknown what the real divisor is, but the default setting creates very large models.
        [JsonProperty("advancedBFFScaleDivisor")]
        public float AdvancedBFFScaleDivisor { get; set; } = 1.0f;

        // The real divisor is supposedly 4096f, but that creates VERY SMALL models.
        [JsonProperty("advancedMODScaleDivisor")]
        public float AdvancedMODScaleDivisor { get; set; } = 16.0f;

        // The real divisor is supposedly 4096f, but that creates VERY SMALL models.
        [JsonProperty("advancedPSXScaleDivisor")]
        public float AdvancedPSXScaleDivisor { get; set; } = 2.25f;

        [JsonProperty("advancedPSXIncludeLODLevels")]
        public bool AdvancedPSXIncludeLODLevels { get; set; } = false;

        [JsonProperty("advancedPSXIncludeInvisible")]
        public bool AdvancedPSXIncludeInvisible { get; set; } = false;


        [JsonProperty("scanProgressFrequency")]
        public float ScanProgressFrequency { get; set; } = 1f / 60f; // 1 frame (60FPS)

        [JsonProperty("scanPopulateFrequency")]
        public float ScanPopulateFrequency { get; set; } = 4f; // 4 seconds

        [JsonProperty("scanOptionsShowAdvanced")]
        public bool ShowAdvancedScanOptions { get; set; } = false;

        [JsonProperty("scanOptions")]
        public ScanOptions ScanOptions { get; set; } = new ScanOptions();

        [JsonProperty("exportOptions")]
        public ExportModelOptions ExportModelOptions { get; set; } = new ExportModelOptions();

        [JsonProperty("recentScanOptionsMax")]
        public int ScanHistoryMax { get; set; } = 20;

        // If true when adding scan history, any previous history will be removed if it matches.
        // Otherwise only the first history will be checked.
        [JsonProperty("recentScanOptionsRemoveDuplicates")]
        public bool ScanHistoryRemoveDuplicates { get; set; } = true;

        [JsonProperty("recentScanOptions")]
        public List<ScanOptions> ScanHistory { get; set; } = new List<ScanOptions>();


        public void AddScanHistory(ScanOptions history)
        {
            history = history.Clone();
            history.IsReadOnly = true; // Mark as ReadOnly so that equality checks can be cached

            // Check for duplicate scan histories
            if (!history.IsBookmarked)
            {
                for (var i = 0; i < ScanHistory.Count; i++)
                {
                    var historyOther = ScanHistory[i];
                    if (!historyOther.IsBookmarked && historyOther.Equals(history))
                    {
                        ScanHistory.RemoveAt(i);
                        i--;
                    }
                    if (!ScanHistoryRemoveDuplicates)
                    {
                        break; // Only check the first history in the list
                    }
                }
            }

            // New history always goes at the top of the list
            ScanHistory.Insert(0, history);

            // Remove overflow
            TrimScanHistory();
        }

        private void TrimScanHistory()
        {
            // Count how many histories are bookmarked, and use that to determine how many others to remove.
            var bookmarkedCount = 0;
            foreach (var history in ScanHistory)
            {
                if (history.IsBookmarked)
                {
                    bookmarkedCount++;
                }
            }

            var removeStartIndex = Math.Max(0, ScanHistoryMax - bookmarkedCount);

            // Remove non-bookmarked overflow
            var nonBookmarkedIndex = 0;
            // Always preserve the most-recent scan history, even if we overflow.
            for (var i = 1; i < ScanHistory.Count; i++)
            {
                var history = ScanHistory[i];
                if (!history.IsBookmarked)
                {
                    if (nonBookmarkedIndex++ >= removeStartIndex)
                    {
                        ScanHistory.RemoveAt(i);
                        i--;
                    }
                }
            }
        }


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
            if (Version < CurrentVersion)
            {
                // Handle changes to how settings are stored here
                if (Version <= 1)
                {
                    LightIntensity /= 100f; // Light intensity is no longer stored as a percent.
                }
            }
            Version = CurrentVersion;


            GridSnap          = ValidateMax(  GridSnap,          Defaults.GridSnap,  0f);
            AngleSnap         = ValidateMax(  AngleSnap,         Defaults.AngleSnap, 0f);
            ScaleSnap         = ValidateMax(  ScaleSnap,         Defaults.ScaleSnap, 0f);
            CameraFOV         = ValidateClamp(CameraFOV,         Defaults.CameraFOV, Scene.CameraMinFOV, Scene.CameraMaxFOV);
            LightIntensity    = ValidateClamp(LightIntensity,    Defaults.LightIntensity, 0f, 10f);
            LightYaw          = ValidateAngle(LightYaw,          Defaults.LightYaw);
            LightPitch        = ValidateAngle(LightPitch,        Defaults.LightPitch);
            WireframeSize     = ValidateMax(  WireframeSize,     Defaults.WireframeSize, 1f);
            VertexSize        = ValidateMax(  VertexSize,        Defaults.VertexSize,    1f);
            GizmoType         = ValidateEnum( GizmoType,         Defaults.GizmoType);
            SubModelVisibility = ValidateEnum(SubModelVisibility, Defaults.SubModelVisibility);
            ModelSelectionMode     = ValidateEnum( ModelSelectionMode,     Defaults.ModelSelectionMode);
            BackgroundColor   = ValidateColor(BackgroundColor,   Defaults.BackgroundColor);
            AmbientColor      = ValidateColor(AmbientColor,      Defaults.AmbientColor);
            MaskColor         = ValidateColor(MaskColor,         Defaults.MaskColor);
            SolidWireframeVerticesColor = ValidateColor(SolidWireframeVerticesColor, Defaults.SolidWireframeVerticesColor);
            CurrentCLUTIndex  = ValidateClamp(CurrentCLUTIndex,  Defaults.CurrentCLUTIndex, 0, 255);
            AnimationLoopMode = ValidateEnum( AnimationLoopMode, Defaults.AnimationLoopMode);
            AnimationSpeed    = ValidateClamp(AnimationSpeed,    Defaults.AnimationSpeed, 0.01f, 100f);

            LogStandardColor        = ValidateEnum(LogStandardColor,        Defaults.LogStandardColor);
            LogPositiveColor        = ValidateEnum(LogPositiveColor,        Defaults.LogPositiveColor);
            LogWarningColor         = ValidateEnum(LogWarningColor,         Defaults.LogWarningColor);
            LogErrorColor           = ValidateEnum(LogErrorColor,           Defaults.LogErrorColor);
            LogExceptionPrefixColor = ValidateEnum(LogExceptionPrefixColor, Defaults.LogExceptionPrefixColor);

            AdvancedBFFFrameRate    = ValidateMax(AdvancedBFFFrameRate,    Defaults.AdvancedBFFFrameRate,    0.000001f);
            AdvancedBFFSpriteScale  = ValidateMax(AdvancedBFFSpriteScale,  Defaults.AdvancedBFFSpriteScale,  0.000001f);
            AdvancedBFFScaleDivisor = ValidateMax(AdvancedBFFScaleDivisor, Defaults.AdvancedBFFScaleDivisor, 0.000001f);
            AdvancedMODScaleDivisor = ValidateMax(AdvancedMODScaleDivisor, Defaults.AdvancedMODScaleDivisor, 0.000001f);
            AdvancedPSXScaleDivisor = ValidateMax(AdvancedPSXScaleDivisor, Defaults.AdvancedPSXScaleDivisor, 0.000001f);

            ScanProgressFrequency = ValidateMax(ScanProgressFrequency, Defaults.ScanProgressFrequency, 0f);
            ScanPopulateFrequency = ValidateMax(ScanPopulateFrequency, Defaults.ScanPopulateFrequency, 0f);
            ScanHistoryMax        = ValidateClamp(ScanHistoryMax,      Defaults.ScanHistoryMax, 0, 100);

            // Clamp multisampling to power of two
            if (Multisampling >= 16)
            {
                Multisampling = 16;
            }
            else if (Multisampling >= 8)
            {
                Multisampling = 8;
            }
            else if (Multisampling >= 4)
            {
                Multisampling = 4;
            }
            else if (Multisampling >= 2)
            {
                Multisampling = 2;
            }
            else if (Multisampling < 0)
            {
                Multisampling = 0;
            }

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

            if (ScanHistory == null)
            {
                ScanHistory = new List<ScanOptions>();
            }
            // Remove null values from history and set ScanOptions.IsReadOnly to true
            for (var i = 0; i < ScanHistory.Count; i++)
            {
                var history = ScanHistory[i];
                if (history == null)
                {
                    ScanHistory.RemoveAt(i);
                    i--;
                    continue;
                }
                history.IsReadOnly = true; // Mark as ReadOnly so that equality checks can be cached
            }

            // Remove all duplicate instances of the same history
            if (ScanHistoryRemoveDuplicates)
            {
                for (var i = 0; i < ScanHistory.Count; i++)
                {
                    var history = ScanHistory[i];
                    if (history.IsBookmarked)
                    {
                        continue;
                    }
                    for (var j = i + 1; j < ScanHistory.Count; j++)
                    {
                        var historyOther = ScanHistory[j];
                        if (historyOther.IsBookmarked)
                        {
                            continue;
                        }
                        if (historyOther.Equals(history))
                        {
                            ScanHistory.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }

            // Remove overflow
            TrimScanHistory();
        }

        public Settings Clone()
        {
            var settings = (Settings)MemberwiseClone();
            settings.ColorDialogCustomColors = (System.Drawing.Color[])settings.ColorDialogCustomColors?.Clone();
            settings.ScanOptions = settings.ScanOptions?.Clone();
            settings.ExportModelOptions = settings.ExportModelOptions?.Clone();
            return settings;
        }

        public bool Save()
        {
            try
            {
                Version = CurrentVersion; // Ensure we're saving as the current version

                // Use JsonTextWriter instead of JsonConvert so that we
                // can set the indentation to tabs to reduce file size.
                using (var streamWriter = File.CreateText(FilePath))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.Indentation = 1;
                    jsonWriter.IndentChar = '\t';

                    var jsonSerializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented,
                    };
                    jsonSerializer.Serialize(jsonWriter, this);
                }
                //File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
                return true;
            }
            catch
            {
                // Failure to write settings file should not be fatal.
                return false;
            }
        }

        public static bool Load(bool loadDefaults, bool preserveScanOptions = false, bool preserveExportModelOptions = false)
        {
            try
            {
                var errorProperties = new List<string>(); // Unused for now
                var jsonSettings = new JsonSerializerSettings
                {
                    // Fix security vulnerability in-case the settings file somehow gets replaced by someone else
                    MaxDepth = 128,
                    // Allow parsing the rest of the settings, even if a few properties can't be parsed
                    Error = (sender, e) => {
                        errorProperties.Add(e.ErrorContext.Path);
                        e.ErrorContext.Handled = true;
                    },
                };

                var newInstance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath), jsonSettings);
                newInstance.Validate();

                Preserve(newInstance, false, false, preserveScanOptions, preserveExportModelOptions);

                Instance = newInstance;

                // Correct any errors with properties by saving over settings.
                Instance.Save();
                return true;
            }
            catch (Exception exp)
            {
                // Load default settings on failure.
                if (loadDefaults)
                {
                    LoadDefaults(preserveScanOptions, preserveExportModelOptions);
                    // It's not an error if the file didn't exist
                    return (exp is FileNotFoundException);
                }
                return false;
            }
        }

        public static void LoadDefaults(bool preserveScanOptions = false, bool preserveExportModelOptions = false)
        {
            var newInstance = new Settings();

            // There's no reason not to preserve color dialog custom colors when resetting settings.
            Preserve(newInstance, true, true, preserveScanOptions, preserveExportModelOptions);

            Instance = newInstance;

            // Create a settings file if one doesn't already exist.
            if (!File.Exists(FilePath))
            {
                Instance.Save();
            }
        }


        private static void Preserve(Settings newInstance, bool customColors, bool paths, bool scanOptions, bool exportModelOptions)
        {
            if (Instance?.ColorDialogCustomColors != null && customColors)
            {
                newInstance.ColorDialogCustomColors = (System.Drawing.Color[])Instance.ColorDialogCustomColors.Clone();
            }

            if (Instance?.ScanOptions != null)
            {
                if (scanOptions)
                {
                    newInstance.ScanOptions = Instance.ScanOptions.Clone();
                }
                else if (paths)
                {
                    newInstance.ScanOptions.Path = Instance.ScanOptions.Path;
                }
            }

            if (Instance?.ExportModelOptions != null)
            {
                if (exportModelOptions)
                {
                    newInstance.ExportModelOptions = Instance.ExportModelOptions.Clone();
                }
                else if (paths)
                {
                    newInstance.ExportModelOptions.Path = Instance.ExportModelOptions.Path;
                }
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

        private static int ValidateMax(int value, int @default, int min)
        {
            return Math.Max(value, min); // Yeah, this ignores default...
        }

        private static int ValidateMin(int value, int @default, int max)
        {
            return Math.Min(value, max); // Yeah, this ignores default...
        }

        private static int ValidateClamp(int value, int @default, int min, int max)
        {
            return GeomMath.Clamp(value, min, max); // Yeah, this ignores default...
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
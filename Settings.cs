using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK;
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Exporters;
using PSXPrev.Common.Renderer;
using PSXPrev.Common.Utils;

namespace PSXPrev
{
    [JsonObject]
    public class Settings : ICloneable
    {
        // Returns "<path>\\<exename>.settings.json"
        public static string FilePath => Path.ChangeExtension(Application.ExecutablePath, ".settings.json");

        public static readonly Settings Defaults = new Settings();

        public static Settings Instance { get; set; } = new Settings();

        // Version of settings file, to check if we need to make changes to loaded settings.
        public const uint CurrentVersion = 3;

        // True if actions like leaving the scanner/export form, or accepting
        // the advanced settings form will save settings changes to file.
        public const bool ImplicitSave = true;

        // True if the max amount of saved scan histories includes bookmarks in the count.
        // One normal history will always be saved regardless (unless the max is 0).
        public const bool CountBookmarksTowardsScanHistoryMax = false;


        // Any settings that need to be changed beteen versions can be handled with this.
        [JsonProperty("version"), Browsable(false)]
        public uint Version { get; set; } = CurrentVersion;

        [JsonProperty("antialiasing")]
        [TypeConverter(typeof(MultisamplingTypeConverter))]
        [Category("Graphics"), DisplayName("Anti-aliasing")]
        [Description("Anti-aliasing level for the renderer.\n(Requires restart)")]
        [DefaultValue(0)]
        public int Multisampling { get; set; } = 0;

        /*[JsonProperty("oldUVAlignment")]
        [Category("Graphics"), DisplayName("Old UV Alignment")]
        [Description("Use old incorrect alignment for UVs.\n(Requires Clear Scan Results)")]
        [DefaultValue(false)]
        public bool OldUVAlignment { get; set; } = false;*/

        [JsonProperty("gridSnap"), Browsable(false)]
        public float GridSnap { get; set; } = 1f;

        [JsonProperty("angleSnap"), Browsable(false)]
        public float AngleSnap { get; set; } = 1f;

        [JsonProperty("scaleSnap"), Browsable(false)]
        public float ScaleSnap { get; set; } = 0.05f;

        [JsonProperty("cameraFOV"), Browsable(false)]
        public float CameraFOV { get; set; } = 60f;

        [JsonIgnore, Browsable(false)]
        public float CameraFOVRads => CameraFOV * GeomMath.Deg2Rad;

        [JsonProperty("lightIntensity"), Browsable(false)]
        public float LightIntensity { get; set; } = 1f;

        [JsonProperty("lightYaw"), Browsable(false)]
        public float LightYaw { get; set; } = 135f;

        [JsonProperty("lightPitch"), Browsable(false)]
        public float LightPitch { get; set; } = 225f;

        [JsonIgnore, Browsable(false)]
        public Vector2 LightPitchYawRads => new Vector2(LightPitch * GeomMath.Deg2Rad, LightYaw * GeomMath.Deg2Rad);

        [JsonProperty("lightEnabled"), Browsable(false)]
        public bool LightEnabled { get; set; } = true;

        [JsonProperty("ambientEnabled"), Browsable(false)]
        public bool AmbientEnabled { get; set; } = true; // We can probably remove this, it's just the same as changing ambient color to black.

        [JsonProperty("texturesEnabled"), Browsable(false)]
        public bool TexturesEnabled { get; set; } = true;

        [JsonProperty("vertexColorEnabled"), Browsable(false)]
        public bool VertexColorEnabled { get; set; } = true;

        [JsonProperty("semiTransparencyEnabled"), Browsable(false)]
        public bool SemiTransparencyEnabled { get; set; } = true;

        [JsonProperty("forceDoubleSided"), Browsable(false)]
        public bool ForceDoubleSided { get; set; } = false;

        [JsonProperty("autoAttachLimbs"), Browsable(false)]
        public bool AutoAttachLimbs { get; set; } = true;

        [JsonProperty("drawModeFaces"), Browsable(false)]
        public bool DrawFaces { get; set; } = true;

        [JsonProperty("drawModeWireframe"), Browsable(false)]
        public bool DrawWireframe { get; set; } = false;

        [JsonProperty("drawModeVertices"), Browsable(false)]
        public bool DrawVertices { get; set; } = false;

        [JsonProperty("drawModeSolidWireframeAndVertices"), Browsable(false)]
        public bool DrawSolidWireframeVertices { get; set; } = true;

        [JsonProperty("wireframeSize"), Browsable(false)]
        public float WireframeSize { get; set; } = 1f;

        [JsonProperty("vertexSize"), Browsable(false)]
        public float VertexSize { get; set; } = 2f;

        [JsonProperty("gizmoTool"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Browsable(false)]
        public GizmoType GizmoType { get; set; } = GizmoType.Translate;

        [JsonProperty("modelSelectionMode"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Browsable(false)]
        public EntitySelectionMode ModelSelectionMode { get; set; } = EntitySelectionMode.Bounds;

        [JsonProperty("subModelVisibility"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Browsable(false)]
        public SubModelVisibility SubModelVisibility { get; set; } = SubModelVisibility.All;

        [JsonProperty("autoFocusOnRootModel"), Browsable(false)]
        public bool AutoFocusOnRootModel { get; set; } = true;

        [JsonProperty("autoFocusOnSubModel"), Browsable(false)]
        public bool AutoFocusOnSubModel { get; set; } = false;

        [JsonProperty("autoFocusIncludeWholeModel"), Browsable(false)]
        public bool AutoFocusIncludeWholeModel { get; set; } = false;

        [JsonProperty("autoFocusIncludeCheckedModels"), Browsable(false)]
        public bool AutoFocusIncludeCheckedModels { get; set; } = true;

        [JsonProperty("autoFocusResetCameraRotation"), Browsable(false)]
        public bool AutoFocusResetCameraRotation { get; set; } = true;

        [JsonProperty("showBounds"), Browsable(false)]
        public bool ShowBounds { get; set; } = true;

        [JsonProperty("showSkeleton"), Browsable(false)]
        public bool ShowSkeleton { get; set; } = false;

        [JsonProperty("showLightRotationRay")]
        [Category("Graphics"), DisplayName("Show Light Rotation Ray")]
        [Description("Show a visual when changing light rotation to see where it's pointed.")]
        [DefaultValue(true)]
        public bool ShowLightRotationRay { get; set; } = true;

        [JsonProperty("durationLightRotationRay")]
        [Category("Graphics"), DisplayName("Light Rotation Ray Duration")]
        [Description("Change how long the light rotation visual shows before fading away (in seconds).")]
        [DefaultValue(2.5f)]
        public float LightRotationRayDelayTime { get; set; } = 2.5f;

        [JsonProperty("showDebugVisuals")]
        [Category("Graphics"), DisplayName("Show Debug Visuals")]
        [Description("Show debugging visuals in the renderer (requires enabling individual visual types).")]
        [DefaultValue(false)]
        public bool ShowDebugVisuals { get; set; } = false;

        [JsonProperty("showDebugPickingRay")]
        [Category("Graphics"), DisplayName("Show Debug Picking Ray")]
        [Description("Show a magenta ray when pressing P to see the line of intersection used for selecting models.\n(Requires Show Debug Visuals)")]
        [DefaultValue(false)]
        public bool ShowDebugPickingRay { get; set; } = false;

        [JsonProperty("showDebugIntersections")]
        [Category("Graphics"), DisplayName("Show Debug Intersections")]
        [Description("Show magenta outlines for all intersected models when selecting models.\n(Requires Show Debug Visuals)")]
        [DefaultValue(false)]
        public bool ShowDebugIntersections { get; set; } = false;

        [JsonProperty("backgroundColor"), JsonConverter(typeof(JsonStringColorConverter))]
        [Browsable(false)]
        public Color BackgroundColor { get; set; } = Color.LightSkyBlue;

        [JsonProperty("ambientColor"), JsonConverter(typeof(JsonStringColorConverter))]
        [Browsable(false)]
        public Color AmbientColor { get; set; } = Color.LightGray;

        [JsonProperty("maskColor"), JsonConverter(typeof(JsonStringColorConverter))]
        [Browsable(false)]
        public Color MaskColor { get; set; } = Color.Black;

        [JsonProperty("solidWireframeAndVerticesColor"), JsonConverter(typeof(JsonStringColorConverter))]
        [Browsable(false)]
        public Color SolidWireframeVerticesColor { get; set; } = Color.Gray;

        [JsonProperty("colorDialogCustomColors", ItemConverterType = typeof(JsonStringColorConverter))]
        [Browsable(false)]
        private Color[] ColorDialogCustomColors { get; set; } = new Color[0];

        [JsonProperty("currentCLUTIndex"), Browsable(false)]
        public int CurrentCLUTIndex { get; set; } = 0;

        [JsonProperty("showVRAMSemiTransparency"), Browsable(false)]
        public bool ShowVRAMSemiTransparency { get; set; } = false;

        [JsonProperty("showVRAMUVs"), Browsable(false)]
        public bool ShowVRAMUVs { get; set; } = true;

        [JsonProperty("showTexturePalette"), Browsable(false)]
        public bool ShowTexturePalette { get; set; } = false;

        [JsonProperty("showTextureSemiTransparency"), Browsable(false)]
        public bool ShowTextureSemiTransparency { get; set; } = false;

        [JsonProperty("showTextureUVs"), Browsable(false)]
        public bool ShowTextureUVs { get; set; } = false;

        [JsonProperty("showMissingTextures"), Browsable(false)]
        public bool ShowMissingTextures { get; set; } = true;

        [JsonProperty("autoDrawModelTextures"), Browsable(false)]
        public bool AutoDrawModelTextures { get; set; } = false;

        [JsonProperty("autoPackModelTextures"), Browsable(false)]
        public bool AutoPackModelTextures { get; set; } = false;

        [JsonProperty("autoPlayAnimation"), Browsable(false)]
        public bool AutoPlayAnimation { get; set; } = false;

        [JsonProperty("autoSelectAnimationModel"), Browsable(false)]
        public bool AutoSelectAnimationModel { get; set; } = true;

        [JsonProperty("animationLoopMode"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Browsable(false)]
        public AnimationLoopMode AnimationLoopMode { get; set; } = AnimationLoopMode.Loop;

        [JsonProperty("animationReverse"), Browsable(false)]
        public bool AnimationReverse { get; set; } = false;

        [JsonProperty("animationSpeed"), Browsable(false)]
        public float AnimationSpeed { get; set; } = 1f;

        [JsonProperty("showFPS"), Browsable(false)]
        public bool ShowFPS { get; set; } = false;

        [JsonProperty("fastWindowResize"), Browsable(false)]
        public bool FastWindowResize { get; set; } = false;

        [JsonProperty("logStandardColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Category("Log"), DisplayName("Standard Color")]
        [Description("Standard color used when logging to the console (requires new scan).")]
        [DefaultValue(ConsoleColor.White)]
        public ConsoleColor LogStandardColor { get; set; } = ConsoleColor.White;

        [JsonProperty("logPositiveColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Category("Log"), DisplayName("Positive Color")]
        [Description("Positive color used when logging to the console (requires new scan).")]
        [DefaultValue(ConsoleColor.Green)]
        public ConsoleColor LogPositiveColor { get; set; } = ConsoleColor.Green;

        [JsonProperty("logWarningColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Category("Log"), DisplayName("Warning Color")]
        [Description("Warning color used when logging to the console (requires new scan).")]
        [DefaultValue(ConsoleColor.Yellow)]
        public ConsoleColor LogWarningColor { get; set; } = ConsoleColor.Yellow;

        [JsonProperty("logErrorColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Category("Log"), DisplayName("Error Color")]
        [Description("Error color used when logging to the console (requires new scan).")]
        [DefaultValue(ConsoleColor.Red)]
        public ConsoleColor LogErrorColor { get; set; } = ConsoleColor.Red;

        [JsonProperty("logExceptionPrefixColor"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        [Category("Log"), DisplayName("Exception Prefix Color")]
        [Description("Error message color displayed before the stack trace when logging to the console (requires new scan).")]
        [DefaultValue(ConsoleColor.DarkGray)]
        public ConsoleColor LogExceptionPrefixColor { get; set; } = ConsoleColor.DarkGray;

        [JsonProperty("logUseConsoleColor")]
        [Category("Log"), DisplayName("Use Console Color")]
        [Description("Enable use of colored text when logging to the console (requires new scan).")]
        [DefaultValue(true)]
        public bool LogUseConsoleColor { get; set; } = true;


        // 30f for Frogger 2 and Chicken Run, 60f for Action Man 2.
        [JsonProperty("advancedBFFFrameRate")]
        [Category("Parsers"), DisplayName("BFF Framerate")]
        [Description("The default framerate for scanned BFF/PSI animations.")]
        [DefaultValue(30f)]
        public float AdvancedBFFFrameRate { get; set; } = 30f;

        [JsonProperty("advancedBFFSpriteScale")]
        [Category("Parsers"), DisplayName("BFF Sprite Scale")]
        [Description("The scaling for sprite size in scanned BFF/FMM models.")]
        [DefaultValue(2.5f)]
        public float AdvancedBFFSpriteScale { get; set; } = 2.5f;

        // Unknown what the real divisor is, but the default setting creates very large models.
        [JsonProperty("advancedBFFScaleDivisor")]
        [Category("Parsers"), DisplayName("BFF Scale Divisor")]
        [Description("The value vertex positions are divided by for scanned BFF/FMM and BFF/PSI models.")]
        [DefaultValue(1.0f)]
        public float AdvancedBFFScaleDivisor { get; set; } = 1.0f;

        [JsonProperty("advancedHMDExtraAnimations")]
        [Category("Parsers"), DisplayName("HMD Extra Animations")]
        [Description("Some HMD models will define a single animation with multiple possibilities. This will attempt to include all of those possibilities as separate animations. Note that this will break models where more than one animation is defined. (Experimental)")]
        [DefaultValue(false)]
        public bool AdvancedHMDExtraAnimations { get; set; } = false;

        // The real divisor is supposedly 4096f, but that creates VERY SMALL models.
        [JsonProperty("advancedMODScaleDivisor")]
        [Category("Parsers"), DisplayName("MOD Scale Divisor")]
        [Description("The value vertex positions are divided by for scanned MOD (Croc) models.")]
        [DefaultValue(16.0f)]
        public float AdvancedMODScaleDivisor { get; set; } = 16.0f;

        // The real divisor is supposedly 4096f, but that creates VERY SMALL models.
        [JsonProperty("advancedPSXScaleDivisor")]
        [Category("Parsers"), DisplayName("PSX Scale Divisor")]
        [Description("The value vertex positions are divided by for scanned PSX (Neversoft) models.")]
        [DefaultValue(2.25f)]
        public float AdvancedPSXScaleDivisor { get; set; } = 2.25f;

        [JsonProperty("advancedPSXIncludeLODLevels")]
        [Category("Parsers"), DisplayName("PSX Include All LOD Levels")]
        [Description("All level of detail versions of each model will be included when scanning PSX (Neversoft) models. Otherwise only the highest quality model is included.")]
        [DefaultValue(false)]
        public bool AdvancedPSXIncludeLODLevels { get; set; } = false;

        [JsonProperty("advancedPSXIncludeInvisible")]
        [Category("Parsers"), DisplayName("PSX Include Invisible Triangles")]
        [Description("Include triangle faces marked as invisible when scanning PSX (Neversoft) models. These are usually faces that show debug surfaces for map interactions.")]
        [DefaultValue(false)]
        public bool AdvancedPSXIncludeInvisible { get; set; } = false;

        [JsonProperty("clearConsoleAfterClearResults")]
        [Category("Scanning"), DisplayName("Clear Console after Clear Results")]
        [Description("Pressing the Clear Scan Results option will also clear the console log.")]
        [DefaultValue(false)]
        public bool ClearConsoleAfterClearResults { get; set; } = false;

        [JsonProperty("scanProgressFrequency")]
        [Category("Scanning"), DisplayName("Scan Progress Frequency")]
        [Description("Change how often the status bar updates the progress bar/text for the current scan (in seconds).")]
        [DefaultValue(1f / 60f)]
        public float ScanProgressFrequency { get; set; } = 1f / 60f; // 1 frame (60FPS)

        [JsonProperty("scanPopulateFrequency")]
        [Category("Scanning"), DisplayName("Scan Populate Frequency")]
        [Description("Change how often the results are populated into the tree and list views for the current scan (in seconds).")]
        [DefaultValue(4f)]
        public float ScanPopulateFrequency { get; set; } = 4f; // 4 seconds

        [JsonProperty("scanOptionsShowAdvanced"), Browsable(false)]
        public bool ShowAdvancedScanOptions { get; set; } = false;

        //[JsonProperty("exportOptionsShowAdvanced"), Browsable(false)]
        //public bool ShowAdvancedExportOptions { get; set; } = false;

        [JsonProperty("scanOptions"), Browsable(false)]
        public ScanOptions ScanOptions { get; set; } = new ScanOptions();

        [JsonProperty("exportOptions"), Browsable(false)]
        public ExportModelOptions ExportModelOptions { get; set; } = new ExportModelOptions();

        [JsonProperty("recentScanOptionsMax")]
        [Category("Scanning"), DisplayName("Max Scan History")]
        [Description("Change how many recent scan histories will be remembered at a single time.")]
        [DefaultValue(20)]
        public int ScanHistoryMax { get; set; } = 20;

        // If true when adding scan history, any previous history will be removed if it matches.
        // Otherwise only the first history will be checked.
        [JsonProperty("recentScanOptionsRemoveDuplicates")]
        [Category("Scanning"), DisplayName("Remove Duplicate Scan History")]
        [Description("Change if a scan will replace old recent scan histories that use the same options.")]
        [DefaultValue(true)]
        public bool ScanHistoryRemoveDuplicates { get; set; } = true;

        [JsonProperty("recentScanOptions"), Browsable(false)]
        public List<ScanOptions> ScanHistory { get; set; } = new List<ScanOptions>();


        // Used for version upgrades to read properties that are no longer present in the current class
        [JsonExtensionData(ReadData = true, WriteData = false), Browsable(false)]
        private Dictionary<string, JToken> _unknownData;

        //[OnDeserialized]
        //private void OnDeserializedMethod(StreamingContext context)
        //{
        //}


        public void AddScanHistory(ScanOptions history)
        {
            history = history.Clone();
            history.Validate();
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
            if (CountBookmarksTowardsScanHistoryMax)
            {
                foreach (var history in ScanHistory)
                {
                    if (history.IsBookmarked)
                    {
                        bookmarkedCount++;
                    }
                }
            }

            var removeStartIndex = Math.Max(0, ScanHistoryMax - bookmarkedCount);
            if (ScanHistoryMax > 0 && removeStartIndex == 0)
            {
                removeStartIndex = 1; // Always preserve the most-recent scan history, even if we overflow.
            }

            // Remove non-bookmarked overflow
            var nonBookmarkedIndex = 0;
            for (var i = 0; i < ScanHistory.Count; i++)
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
        public int[] GetColorDialogCustomColors(Color? defaultColor = null)
        {
            int ToBgr(Color col)
            {
                return (int)((uint)col.R | ((uint)col.G << 8) | ((uint)col.B << 16));
            }

            var customBgrColors = new int[16];
            for (var i = 0; i < customBgrColors.Length; i++)
            {
                customBgrColors[i] = ToBgr(Color.White);
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
            Color FromBgr(int bgr)
            {
                return Color.FromArgb(bgr & 0xff, (bgr >> 8) & 0xff, (bgr >> 16) & 0xff);
            }

            if (customBgrColors == null)
            {
                ColorDialogCustomColors = new Color[0];
            }
            else
            {
                // Don't fill the settings array with empty white colors.
                // Use as many custom colors as there are until all remaining colors are white.
                var customCount = Math.Min(15, customBgrColors.Length);
                for (; customCount > 0; customCount--)
                {
                    // NEVER use Color equality, because it also checks
                    // stupid things like name, and we also ignore the alpha value.
                    var color = FromBgr(customBgrColors[customCount - 1]);
                    if (!color.EqualsRgb(Color.White))//.R != white.R || color.G != white.G || color.B != white.B)
                    {
                        break;
                    }
                }
                ColorDialogCustomColors = new Color[customCount];
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

            // Perform validation and handle version upgrades for all nested objects
            ScanOptions?.ValidateDeserialization(Version);
            ExportModelOptions?.ValidateDeserialization(Version);
            if (ScanHistory != null)
            {
                foreach (var scanHistory in ScanHistory)
                {
                    scanHistory?.ValidateDeserialization(Version);
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
            ModelSelectionMode = ValidateEnum(ModelSelectionMode, Defaults.ModelSelectionMode);
            LightRotationRayDelayTime = ValidateMax(LightRotationRayDelayTime, Defaults.LightRotationRayDelayTime, 0.1f);
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
            else if (Multisampling >= 1) // samples value of 1 is treated as 2
            {
                Multisampling = 2;
            }
            else
            {
                Multisampling = 0;
            }

            if (ColorDialogCustomColors == null)
            {
                ColorDialogCustomColors = new Color[0];
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
            settings._unknownData = null;
            settings.ColorDialogCustomColors = (Color[])settings.ColorDialogCustomColors?.Clone() ?? new Color[0];
            settings.ScanOptions = settings.ScanOptions?.Clone() ?? new ScanOptions();
            settings.ExportModelOptions = settings.ExportModelOptions?.Clone() ?? new ExportModelOptions();
            // We don't need to clone these ScanOptions since we treat them as immutable
            settings.ScanHistory = new List<ScanOptions>((IEnumerable<ScanOptions>)settings.ScanHistory ?? new ScanOptions[0]);
            return settings;
        }

        object ICloneable.Clone() => Clone();

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

        public static bool Load(bool loadDefaults, bool preserveScanOptions = false, bool preserveExportModelOptions = false, bool preserveScanHistory = false)
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

                Preserve(newInstance, false, false, preserveScanOptions, preserveExportModelOptions, preserveScanHistory);

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

        public static void LoadDefaults(bool preserveScanOptions = false, bool preserveExportModelOptions = false, bool preserveScanHistory = true)
        {
            var newInstance = new Settings();

            // There's no reason not to preserve color dialog custom colors when resetting settings.
            Preserve(newInstance, true, true, preserveScanOptions, preserveExportModelOptions, preserveScanHistory);

            Instance = newInstance;

            // Create a settings file if one doesn't already exist.
            if (!File.Exists(FilePath))
            {
                Instance.Save();
            }
        }


        private static void Preserve(Settings newInstance, bool customColors, bool paths, bool scanOptions, bool exportModelOptions, bool scanHistory)
        {
            if (Instance?.ColorDialogCustomColors != null && customColors)
            {
                newInstance.ColorDialogCustomColors = (Color[])Instance.ColorDialogCustomColors.Clone();
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

            if (Instance.ScanHistory != null)
            {
                if (scanHistory)
                {
                    newInstance.ScanHistory = new List<ScanOptions>(Instance.ScanHistory);
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

        private static Color ValidateColor(Color value, Color @default)
        {
            return value.A != 255 ? @default : value;
        }

        internal static TEnum ValidateEnum<TEnum>(TEnum value, TEnum @default)
        {
            return !Enum.IsDefined(typeof(TEnum), value) ? @default : value;
        }


        // All this boilerplate just to make a clean dropdown setting...
        private class MultisamplingTypeConverter : Int32Converter
        {
            private static readonly int[] _standardValues = { 0, 2, 4, 8, 16 };
            private static readonly string[] _standardValueNames = { "Off", "2x FSAA", "4x FSAA", "8x FSAA", "16x FSAA" };

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(_standardValueNames);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(int) || sourceType == typeof(string);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
            {
                return t == typeof(int) || t == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string str)
                {
                    if (int.TryParse(str, out var intResult))
                    {
                        return intResult;
                    }
                    var index = Array.IndexOf(_standardValueNames, str);
                    if (index != -1)
                    {
                        return _standardValues[index];
                    }
                    throw new Exception("Invalid value");
                }
                else if (value is int intValue)
                {
                    return intValue;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (value is int intValue)
                {
                    var index = Array.IndexOf(_standardValues, intValue);
                    if (index != -1)
                    {
                        return _standardValueNames[index];
                    }
                    return intValue;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows.Forms;
using Manina.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Exporters;
using PSXPrev.Common.Renderer;
using PSXPrev.Forms;
using PSXPrev.Forms.Dialogs;
using PSXPrev.Forms.Utils;
using Timer = System.Timers.Timer;

namespace PSXPrev.Forms
{
    public partial class PreviewForm : Form
    {
        private const float MouseSensivity = 0.0035f;

        private const double AnimationProgressPrecision = 200d;

        private const int CheckAllNodesWarningCount = 100;

        private const double DefaultElapsedTime = 1d / 60d; // 1 frame (60FPS)
        private const double MaxElapsedTime = 10d / 60d; // 10 frames (60FPS)

        private const bool IncludeAnimationFrameTreeViewNodes = false;

        private const int ModelsTabIndex     = 0;
        private const int TexturesTabIndex   = 1;
        private const int VRAMTabIndex       = 2;
        private const int AnimationsTabIndex = 3;


        private readonly List<RootEntity> _rootEntities = new List<RootEntity>();
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly List<Texture> _packedTextures = new List<Texture>();
        private readonly List<Animation> _animations = new List<Animation>();
        private readonly Scene _scene;
        private readonly VRAM _vram;
        private readonly AnimationBatch _animationBatch;
        private GLControl _glControl;
        private int _currentMultisampling;

        // Form timers
        private Timer _mainTimer; // Main timer used by all timed events
        private Stopwatch _mainWatch; // Watch to track elapsed time between main timer events
        private Stopwatch _fpsWatch;
        private bool _fixedTimer = false; // If true, then timer always updates with the same time delta
        // Timers that are updated during main timer Elapsed event
        private RefreshDelayTimer _animationProgressBarRefreshDelayTimer;
        private RefreshDelayTimer _modelPropertyGridRefreshDelayTimer;
        private RefreshDelayTimer _fpsLabelRefreshDelayTimer;
        private RefreshDelayTimer _scanProgressRefreshDelayTimer;
        private RefreshDelayTimer _scanPopulateRefreshDelayTimer;
        private float _fps = (float)(1d / DefaultElapsedTime);
        private int _trianglesDrawn;
        private int _meshesDrawn;
        private int _skinsDrawn;
        private double _fpsCalcElapsedSeconds;
        private int _fpsCalcElapsedFrames;

        // Scanning
        private readonly object _scanProgressLock = new object();
        private Program.ScanProgressReport _scanProgressReport;
        private string _scanProgressMessage;
        private bool _drawAllToVRAMAfterScan;
        private List<RootEntity> _addedRootEntities = new List<RootEntity>();
        private List<Texture> _addedTextures = new List<Texture>();
        private List<Animation> _addedAnimations = new List<Animation>();
        // Swap lists are used so that we don't need to allocate new lists every time we call ScanPopulateItems.
        private List<RootEntity> _addedRootEntitiesSwap = new List<RootEntity>();
        private List<Texture> _addedTexturesSwap = new List<Texture>();
        private List<Animation> _addedAnimationsSwap = new List<Animation>();

        // Form state
        private bool _closing;
        private bool _inDialog; // Prevent timers from performing updates while true
        private bool _resizeLayoutSuspended; // True if we need to ResumeLayout during ResizeEnd event
        private object _cancelMenuCloseItemClickedSender; // Prevent closing menus on certain click events (like separators)
        private bool _busyChecking; // Checking or unchecking multiple tree view items, and events need to be delayed

        private string _baseWindowTitle;
        private Animation _curAnimation;
        private AnimationObject _curAnimationObject;
        private AnimationFrame _curAnimationFrame;
        private float _animationSpeed = 1f;
        private bool _inAnimationTab;
        private bool _playing;
        private int _lastMouseX;
        private int _lastMouseY;
        private Tuple<ModelEntity, Triangle> _selectedTriangle;
        private ModelEntity _selectedModelEntity;
        private RootEntity _selectedRootEntity;
        private EntitySelectionSource _selectionSource;
        private int _vramSelectedPage = -1; // Used because combo box SelectedIndex can be -1 while typing.
        private TexturesListViewItemAdaptor _texturesListViewAdaptor;
        private Bitmap _maskColorBitmap;
        private Bitmap _ambientColorBitmap;
        private Bitmap _backgroundColorBitmap;
        private Bitmap _wireframeVerticesColorBitmap;
        private int _clutIndex;
        private bool _autoDrawModelTextures;
        private bool _autoPackModelTextures;
        private bool _autoSelectAnimationModel;
        private bool _autoPlayAnimations;
        private SubModelVisibility _subModelVisibility;
        private bool _autoFocusOnRootModel;
        private bool _autoFocusOnSubModel;
        private bool _autoFocusIncludeWholeModel;
        private bool _autoFocusIncludeCheckedModels;
        private bool _autoFocusResetCameraRotation;
        private EntitySelectionMode _modelSelectionMode;
        private bool _showSkeleton;
        private uint? _fallbackTextureID;

        private GizmoType _gizmoType;
        private GizmoId _hoveredGizmo;
        private GizmoId _selectedGizmo;
        private Vector3 _gizmoAxis;
        private Vector3 _gizmoOrigin;
        private Vector3 _gizmoInitialTranslation;
        private Quaternion _gizmoInitialRotation;
        private Vector3 _gizmoInitialScale;
        private float _gizmoRotateAngle;
        private Vector2 _gizmoRotateStartDirection;
        private float _gizmoScaleStartDistance;


        public PreviewForm()
        {
            _scene = new Scene();
            _vram = new VRAM(_scene);
            _animationBatch = new AnimationBatch();
            _texturesListViewAdaptor = new TexturesListViewItemAdaptor(this);
            // It's observed in GLControl's source that this is called with PreferNative in the
            // control's constructor. Changing this from PreferDefault is needed to prevent SDL2
            // from creating the graphics context for certain users, which can throw an AccessViolationException.
            Toolkit.Init(new ToolkitOptions
            {
                Backend = PlatformBackend.PreferNative,
            });
            InitializeComponent();
            SetupControls();
        }

        private bool Playing
        {
            get => _playing;
            set
            {
                if (_playing != value)
                {
                    _playing = value;

                    // Make sure we restart the animation if it was finished.
                    if (!value && _animationBatch.IsFinished)
                    {
                        _animationBatch.Restart();
                    }
                    // Refresh to make sure the button text updates quickly.
                    animationPlayButton.Text = value ? "Pause Animation" : "Play Animation";
                    animationPlayButton.Refresh();
                }
            }
        }

        private bool IsSceneTab
        {
            get
            {
                switch (menusTabControl.SelectedIndex)
                {
                    case ModelsTabIndex:
                    case AnimationsTabIndex:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private bool IsImageTab
        {
            get
            {
                switch (menusTabControl.SelectedIndex)
                {
                    case TexturesTabIndex:
                    case VRAMTabIndex:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private Control PrimaryControl
        {
            get
            {
                switch (menusTabControl.SelectedIndex)
                {
                    case ModelsTabIndex:
                        return scenePreviewer;//.GLControl;

                    case TexturesTabIndex:
                        return texturePreviewer;

                    case VRAMTabIndex:
                        return vramPreviewer;

                    case AnimationsTabIndex:
                        return animationPreviewer;//.GLControl;

                    default:
                        return null;
                }
            }
        }

        private bool IsControlDown => ModifierKeys.HasFlag(Keys.Control);

        private bool IsShiftDown => ModifierKeys.HasFlag(Keys.Shift);

        #region Scanning

        private void ScanPopulateItems()
        {
            // A-B swap between two lists, one used to populate items, and one used to hold items to be populated.
            lock (_scanProgressLock)
            {
                var rootEntitiesSwap = _addedRootEntities;
                _addedRootEntities = _addedRootEntitiesSwap;
                _addedRootEntitiesSwap = rootEntitiesSwap;

                var texturesSwap = _addedTextures;
                _addedTextures = _addedTexturesSwap;
                _addedTexturesSwap = texturesSwap;

                var animationsSwap = _addedAnimations;
                _addedAnimations = _addedAnimationsSwap;
                _addedAnimationsSwap = animationsSwap;
            }

            AddRootEntities(_addedRootEntitiesSwap);
            AddTextures(_addedTexturesSwap);
            AddAnimations(_addedAnimationsSwap);
            _addedRootEntitiesSwap.Clear();
            _addedTexturesSwap.Clear();
            _addedAnimationsSwap.Clear();
        }

        private void ScanUpdated()
        {
            Program.ScanProgressReport p;
            string message;
            lock (_scanProgressLock)
            {
                p = _scanProgressReport;
                message = _scanProgressMessage;
                _scanProgressReport = null;
                _scanProgressMessage = null;
            }
            if (p == null)
            {
                return;
            }

            statusStrip1.SuspendLayout();
            const int max = 100;

            // Avoid divide-by-zero when scanning 0 files, or a file that's 0 bytes in size.
            // Update major progress bar, default to 100% when zero files
            var totalFilesPercent = (p.TotalFiles > 0) ? ((double)p.CurrentFile / p.TotalFiles) : 1d;
            statusTotalFilesProgressBar.Maximum = max;
            statusTotalFilesProgressBar.SetValueSafe((int)(totalFilesPercent * max));

            // Update minor progress bar, default to 0% when zero length
            var currentFilePercent = (p.CurrentLength > 0) ? ((double)p.CurrentPosition / p.CurrentLength) : 0d;
            statusCurrentFileProgressBar.Maximum = max;
            statusCurrentFileProgressBar.SetValueSafe((int)(currentFilePercent * max));

            if (message != null)
            {
                statusMessageLabel.Text = message;
            }
            statusStrip1.ResumeLayout();

            // Instantly update status bar at end of scan, since populating the rest of the views may be a little slow.
            if (p.State == Program.ScanProgressState.Finished)
            {
                // Vista introduced progress bar animation that slowly moves the bar.
                // We don't want that when finishing a scan, so force the value to instantly change.
                statusTotalFilesProgressBar.UpdateValueInstant();
                statusCurrentFileProgressBar.UpdateValueInstant();

                statusStrip1.Refresh();
            }
        }

        private void ScanStarted()
        {
            pauseScanningToolStripMenuItem.Checked = false;
            pauseScanningToolStripMenuItem.Enabled = true;
            stopScanningToolStripMenuItem.Enabled = true;
            startScanToolStripMenuItem.Enabled = false;
            clearScanResultsToolStripMenuItem.Enabled = false;

            statusStrip1.SuspendLayout();

            // Reset positions back to start, since they may not get updated right away.
            statusTotalFilesProgressBar.Maximum  = 1;
            statusTotalFilesProgressBar.Value    = 0;
            statusCurrentFileProgressBar.Maximum = 1;
            statusCurrentFileProgressBar.Value   = 0;
            statusTotalFilesProgressBar.Visible = true;
            statusCurrentFileProgressBar.Visible = true;

            statusMessageLabel.Text = $"Scan Started";

            statusStrip1.ResumeLayout();
            statusStrip1.Refresh();

            lock (_scanProgressLock)
            {
                _scanProgressReport = null;
                _scanProgressMessage = null;
            }
            _scanProgressRefreshDelayTimer.Restart();
            _scanPopulateRefreshDelayTimer.Restart();
        }

        private void ScanFinished()
        {
            _scanProgressRefreshDelayTimer.Reset();
            _scanPopulateRefreshDelayTimer.Reset();

            ScanUpdated();

            lock (_scanProgressLock)
            {
                _scanProgressReport = null;
                _scanProgressMessage = null;
            }

            ScanPopulateItems();

            SelectFirstEntity(); // Select something if the user hasn't already done so.
            if (_drawAllToVRAMAfterScan)
            {
                DrawTexturesToVRAM(_textures, _clutIndex);
            }

            pauseScanningToolStripMenuItem.Checked = false;
            pauseScanningToolStripMenuItem.Enabled = false;
            stopScanningToolStripMenuItem.Enabled = false;
            startScanToolStripMenuItem.Enabled = true;
            clearScanResultsToolStripMenuItem.Enabled = true;

            statusStrip1.SuspendLayout();

            statusTotalFilesProgressBar.Visible = false;
            statusCurrentFileProgressBar.Visible = false;

            statusMessageLabel.Text = $"Models: {_rootEntities.Count}, Textures: {_textures.Count}, Animations: {_animations.Count}";

            statusStrip1.ResumeLayout();
            statusStrip1.Refresh();


            if (_autoPackModelTextures)
            {
                // Ensure model is redrawn with packed textures applied.
                UpdateSelectedEntity();
            }

            // Debugging: Quickly export models and animations on startup.
            /*if (_rootEntities.Count >= 1 && _animations.Count >= 1)
            {
                var options = new ExportModelOptions
                {
                    Path = @"",
                    Format = ExportModelOptions.GLTF2,
                    SingleTexture = false,
                    AttachLimbs = true,
                    RedrawTextures = true,
                    ExportAnimations = true,
                };
                ExportModelsForm.Export(options, new[] { _rootEntities[0] }, new[] { _animations[0] }, _animationBatch);
                Close();
            }*/
        }

        private void OnScanProgressCallback(Program.ScanProgressReport p)
        {
            // WARNING: This function is not called on the UI thread!
            switch (p.State)
            {
                case Program.ScanProgressState.Started:
                    // Instantly handle starting the scan.
                    Invoke(new Action(ScanStarted));
                    break;

                case Program.ScanProgressState.Finished:
                    // Instantly refresh the status bar while the scan finishes up.
                    // Then instantly handle finishing the scan.
                    lock (_scanProgressLock)
                    {
                        _scanProgressMessage = "Scan Finishing";
                        _scanProgressReport = p;
                    }
                    Invoke(new Action(ScanFinished));
                    break;

                case Program.ScanProgressState.Updated:
                    // Prepare progress that will be updated in future main timer Elapsed events.
                    lock (_scanProgressLock)
                    {
                        _scanProgressMessage = null;

                        // Added items are reloaded infrequently.
                        if (p.Result is RootEntity rootEntity)
                        {
                            _scanProgressMessage = $"Found {rootEntity.FormatName} Model with {rootEntity.ChildEntities.Length} objects";
                            _addedRootEntities.Add(rootEntity);
                        }
                        else if (p.Result is Texture texture)
                        {
                            _scanProgressMessage = $"Found {texture.FormatName} Texture {texture.Width}x{texture.Height} {texture.Bpp}bpp";
                            _addedTextures.Add(texture);
                        }
                        else if (p.Result is Animation animation)
                        {
                            _scanProgressMessage = $"Found {animation.FormatName} Animation with {animation.ObjectCount} objects and {animation.FrameCount} frames";
                            _addedAnimations.Add(animation);
                        }

                        // Progress bars and status message updates are handled frequently.
                        _scanProgressReport = p;
                    }
                    break;
            }
        }

        #endregion

        #region Settings

        public void LoadDefaultSettings()
        {
            Settings.LoadDefaults();
            ReadSettings(Settings.Instance);
            UpdateSelectedEntity();
        }

        public void LoadSettings()
        {
            if (Settings.Load(false))
            {
                ReadSettings(Settings.Instance);
                UpdateSelectedEntity();
            }
        }

        public void SaveSettings()
        {
            WriteSettings(Settings.Instance);
            Settings.Instance.Save();
        }

        public void ReadSettings(Settings settings)
        {
            if (!Program.IsScanning)
            {
                Program.Logger.ReadSettings(settings);
            }
            Program.ConsoleLogger.ReadSettings(settings);

            gridSnapUpDown.SetValueSafe((decimal)settings.GridSnap);
            angleSnapUpDown.SetValueSafe((decimal)settings.AngleSnap);
            scaleSnapUpDown.SetValueSafe((decimal)settings.ScaleSnap);
            cameraFOVUpDown.SetValueSafe((decimal)settings.CameraFOV);
            {
                // Prevent light rotation ray from showing up while reading settings.
                // This is re-assigned later in the function.
                _scene.ShowLightRotationRay = false;
                lightIntensityNumericUpDown.SetValueSafe((decimal)settings.LightIntensity);
                lightYawNumericUpDown.SetValueSafe((decimal)settings.LightYaw);
                lightPitchNumericUpDown.SetValueSafe((decimal)settings.LightPitch);
                enableLightToolStripMenuItem.Checked = settings.LightEnabled;
            }
            _scene.AmbientEnabled = settings.AmbientEnabled;
            enableTexturesToolStripMenuItem.Checked = settings.TexturesEnabled;
            enableVertexColorToolStripMenuItem.Checked = settings.VertexColorEnabled;
            enableSemiTransparencyToolStripMenuItem.Checked = settings.SemiTransparencyEnabled;
            forceDoubleSidedToolStripMenuItem.Checked = settings.ForceDoubleSided;
            autoAttachLimbsToolStripMenuItem.Checked = settings.AutoAttachLimbs;
            drawModeFacesToolStripMenuItem.Checked = settings.DrawFaces;
            drawModeWireframeToolStripMenuItem.Checked = settings.DrawWireframe;
            drawModeVerticesToolStripMenuItem.Checked = settings.DrawVertices;
            drawModeSolidWireframeVerticesToolStripMenuItem.Checked = settings.DrawSolidWireframeVertices;
            wireframeSizeUpDown.SetValueSafe((decimal)settings.WireframeSize);
            vertexSizeUpDown.SetValueSafe((decimal)settings.VertexSize);
            SetGizmoType(settings.GizmoType, force: true);
            SetModelSelectionMode(settings.ModelSelectionMode, force: true);
            SetSubModelVisibility(settings.SubModelVisibility, force: true);
            autoFocusOnRootModelToolStripMenuItem.Checked = settings.AutoFocusOnRootModel;
            autoFocusOnSubModelToolStripMenuItem.Checked = settings.AutoFocusOnSubModel;
            autoFocusIncludeWholeModelToolStripMenuItem.Checked = settings.AutoFocusIncludeWholeModel;
            autoFocusIncludeCheckedModelsToolStripMenuItem.Checked = settings.AutoFocusIncludeCheckedModels;
            autoFocusResetCameraRotationToolStripMenuItem.Checked = settings.AutoFocusResetCameraRotation;
            showBoundsToolStripMenuItem.Checked = settings.ShowBounds;
            showSkeletonToolStripMenuItem.Checked = settings.ShowSkeleton;
            _scene.ShowLightRotationRay = settings.ShowLightRotationRay;
            _scene.LightRotationRayDelayTime = settings.LightRotationRayDelayTime;
            _scene.ShowDebugVisuals = settings.ShowDebugVisuals;
            _scene.ShowDebugPickingRay = settings.ShowDebugPickingRay;
            _scene.ShowDebugIntersections = settings.ShowDebugIntersections;
            SetBackgroundColor(settings.BackgroundColor);
            SetAmbientColor(settings.AmbientColor);
            SetMaskColor(settings.MaskColor);
            SetSolidWireframeVerticesColor(settings.SolidWireframeVerticesColor);
            SetCurrentCLUTIndex(settings.CurrentCLUTIndex);
            showVRAMSemiTransparencyToolStripMenuItem.Checked = settings.ShowVRAMSemiTransparency;
            showVRAMUVsToolStripMenuItem.Checked = settings.ShowVRAMUVs;
            showTexturePaletteToolStripMenuItem.Checked = settings.ShowTexturePalette;
            showTextureSemiTransparencyToolStripMenuItem.Checked = settings.ShowTextureSemiTransparency;
            showTextureUVsToolStripMenuItem.Checked = settings.ShowTextureUVs;
            showMissingTexturesToolStripMenuItem.Checked = settings.ShowMissingTextures;
            autoDrawModelTexturesToolStripMenuItem.Checked = settings.AutoDrawModelTextures;
            autoPackModelTexturesToolStripMenuItem.Checked = settings.AutoPackModelTextures;
            autoPlayAnimationsToolStripMenuItem.Checked = settings.AutoPlayAnimation;
            autoSelectAnimationModelToolStripMenuItem.Checked = settings.AutoSelectAnimationModel;
            animationLoopModeComboBox.SelectedIndex = (int)settings.AnimationLoopMode;
            animationReverseCheckBox.Checked = settings.AnimationReverse;
            animationSpeedNumericUpDown.SetValueSafe((decimal)settings.AnimationSpeed);
            showFPSToolStripMenuItem.Checked = settings.ShowFPS;
            fastWindowResizeToolStripMenuItem.Checked = settings.FastWindowResize;

            _scanProgressRefreshDelayTimer.Interval = settings.ScanProgressFrequency;
            _scanPopulateRefreshDelayTimer.Interval = settings.ScanPopulateFrequency;
        }

        public void WriteSettings(Settings settings)
        {
            Program.Logger.WriteSettings(settings);

            settings.GridSnap = (float)gridSnapUpDown.Value;
            settings.AngleSnap = (float)angleSnapUpDown.Value;
            settings.ScaleSnap = (float)scaleSnapUpDown.Value;
            settings.CameraFOV = (float)cameraFOVUpDown.Value;
            settings.LightIntensity = (float)lightIntensityNumericUpDown.Value;
            settings.LightYaw = (float)lightYawNumericUpDown.Value;
            settings.LightPitch = (float)lightPitchNumericUpDown.Value;
            settings.LightEnabled = enableLightToolStripMenuItem.Checked;
            settings.AmbientEnabled = _scene.AmbientEnabled;
            settings.TexturesEnabled = enableTexturesToolStripMenuItem.Checked;
            settings.VertexColorEnabled = enableVertexColorToolStripMenuItem.Checked;
            settings.SemiTransparencyEnabled = enableSemiTransparencyToolStripMenuItem.Checked;
            settings.ForceDoubleSided = forceDoubleSidedToolStripMenuItem.Checked;
            settings.AutoAttachLimbs = autoAttachLimbsToolStripMenuItem.Checked;
            settings.DrawFaces = drawModeFacesToolStripMenuItem.Checked;
            settings.DrawWireframe = drawModeWireframeToolStripMenuItem.Checked;
            settings.DrawVertices = drawModeVerticesToolStripMenuItem.Checked;
            settings.DrawSolidWireframeVertices = drawModeSolidWireframeVerticesToolStripMenuItem.Checked;
            settings.WireframeSize = (float)wireframeSizeUpDown.Value;
            settings.VertexSize = (float)vertexSizeUpDown.Value;
            settings.GizmoType = _gizmoType;
            settings.ModelSelectionMode = _modelSelectionMode;
            settings.SubModelVisibility = _subModelVisibility;
            settings.AutoFocusOnRootModel = autoFocusOnRootModelToolStripMenuItem.Checked;
            settings.AutoFocusOnSubModel = autoFocusOnSubModelToolStripMenuItem.Checked;
            settings.AutoFocusIncludeWholeModel = autoFocusIncludeWholeModelToolStripMenuItem.Checked;
            settings.AutoFocusIncludeCheckedModels = autoFocusIncludeCheckedModelsToolStripMenuItem.Checked;
            settings.AutoFocusResetCameraRotation = autoFocusResetCameraRotationToolStripMenuItem.Checked;
            settings.ShowBounds = showBoundsToolStripMenuItem.Checked;
            settings.ShowSkeleton = showSkeletonToolStripMenuItem.Checked;
            settings.ShowLightRotationRay = _scene.ShowLightRotationRay;
            settings.LightRotationRayDelayTime = _scene.LightRotationRayDelayTime;
            settings.ShowDebugVisuals = _scene.ShowDebugVisuals;
            settings.ShowDebugPickingRay = _scene.ShowDebugPickingRay;
            settings.ShowDebugIntersections = _scene.ShowDebugIntersections;
            settings.BackgroundColor = _scene.ClearColor;
            settings.AmbientColor = _scene.AmbientColor;
            settings.MaskColor = _scene.MaskColor;
            settings.SolidWireframeVerticesColor = _scene.SolidWireframeVerticesColor;
            settings.CurrentCLUTIndex = _clutIndex;
            settings.ShowVRAMSemiTransparency = showVRAMSemiTransparencyToolStripMenuItem.Checked;
            settings.ShowVRAMUVs = showVRAMUVsToolStripMenuItem.Checked;
            settings.ShowTexturePalette = showTexturePaletteToolStripMenuItem.Checked;
            settings.ShowTextureSemiTransparency = showTextureSemiTransparencyToolStripMenuItem.Checked;
            settings.ShowTextureUVs = showTextureUVsToolStripMenuItem.Checked;
            settings.ShowMissingTextures = showMissingTexturesToolStripMenuItem.Checked;
            settings.AutoDrawModelTextures = autoDrawModelTexturesToolStripMenuItem.Checked;
            settings.AutoPackModelTextures = autoPackModelTexturesToolStripMenuItem.Checked;
            settings.AutoPlayAnimation = autoPlayAnimationsToolStripMenuItem.Checked;
            settings.AutoSelectAnimationModel = autoSelectAnimationModelToolStripMenuItem.Checked;
            settings.AnimationLoopMode = (AnimationLoopMode)animationLoopModeComboBox.SelectedIndex;
            settings.AnimationReverse = animationReverseCheckBox.Checked;
            settings.AnimationSpeed = (float)animationSpeedNumericUpDown.Value;
            settings.ShowFPS = showFPSToolStripMenuItem.Checked;
            settings.FastWindowResize = fastWindowResizeToolStripMenuItem.Checked;
        }

        #endregion


        #region Main/GLControl

        private void SetupControls()
        {
            // Set window title to format: PSXPrev #.#.#.#
            _baseWindowTitle = $"{Text} {GetVersionString()}";
            Text = _baseWindowTitle;


            // Setup GLControl
            try
            {
                // 24-bit depth buffer fixes issues where lower-end laptops
                // with integrated graphics render larger models horribly.
                var samples = Settings.Instance.Multisampling;
                var graphicsMode = new GraphicsMode(color: 32, depth: 24, stencil: 0, samples: samples);
                _glControl = new GLControl(graphicsMode);
                _currentMultisampling = samples;
            }
            catch
            {
                // Don't know if an unsupported graphics mode can throw, but let's play it safe.
                _glControl = new GLControl();
                _currentMultisampling = 0;
            }
            _glControl.Name = "glControl";
            _glControl.TabIndex = 0;
            _glControl.BackColor = _scene.ClearColor;
            _glControl.Dock = DockStyle.Fill;
            _glControl.VSync = true;

            _glControl.MouseDown  += (sender, e) => glControl_MouseEvent(e, MouseEventType.Down);
            _glControl.MouseUp    += (sender, e) => glControl_MouseEvent(e, MouseEventType.Up);
            _glControl.MouseWheel += (sender, e) => glControl_MouseEvent(e, MouseEventType.Wheel);
            _glControl.MouseMove  += (sender, e) => glControl_MouseEvent(e, MouseEventType.Move);
            _glControl.Paint += glControl_Paint;


            // Assign user control properties that need something from the main form
            scenePreviewer.GLControl = _glControl;
            animationPreviewer.GLControl = _glControl;
            scenePreviewer.Scene = _scene;
            animationPreviewer.Scene = _scene;
            texturePreviewer.GetUVEntities = EnumerateUVEntities;
            vramPreviewer.GetUVEntities = EnumerateUVEntities;


            // Setup Timers
            // Don't start watch until first Elapsed event (and use a default time for that first event)
            // Don't start timer until the Form is loaded
            _fpsWatch  = new Stopwatch();
            _mainWatch = new Stopwatch();
            _mainTimer = new Timer(1d); // 1 millisecond, update as fast as possible (usually ~60FPS)
            _mainTimer.SynchronizingObject = this;
            _mainTimer.Elapsed += mainTimer_Elapsed;

            _animationProgressBarRefreshDelayTimer = new RefreshDelayTimer(1d / 60d); // 1 frame (60FPS)
            _animationProgressBarRefreshDelayTimer.Elapsed += () => UpdateAnimationProgressLabel(true);

            _modelPropertyGridRefreshDelayTimer = new RefreshDelayTimer(50d / 1000d); // 50 milliseconds
            _modelPropertyGridRefreshDelayTimer.Elapsed += () => UpdateModelPropertyGrid(true);

            _fpsLabelRefreshDelayTimer = new RefreshDelayTimer(1d); // 1 second
            _fpsLabelRefreshDelayTimer.Elapsed += () => UpdateFPSLabel();

            _scanProgressRefreshDelayTimer = new RefreshDelayTimer(); // Interval assigned by settings
            _scanProgressRefreshDelayTimer.AutoReset = true;
            _scanProgressRefreshDelayTimer.Elapsed += () => ScanUpdated();

            _scanPopulateRefreshDelayTimer = new RefreshDelayTimer(); // Interval assigned by settings
            _scanPopulateRefreshDelayTimer.AutoReset = true;
            _scanPopulateRefreshDelayTimer.Elapsed += () => ScanPopulateItems();


            // Setup Events
            // Allow changing multiple checkboxes while holding shift down.
            drawModeToolStripMenuItem.DropDown.Closing += OnCancelMenuCloseWhileHoldingShift;
            autoFocusToolStripMenuItem.DropDown.Closing += OnCancelMenuCloseWhileHoldingShift;

            // Ensure numeric up downs display the same value that they store internally.
            SetupNumericUpDownValidateEvents(this);

            // Normally clicking a menu separator will close the menu, which doesn't follow standard UI patterns.
            SetupCancelMenuCloseOnSeparatorClick(mainMenuStrip);

            // Make it so that checkboxes without text will still show a focus rectangle.
            // NOTE: Padding for these should be set to: 2, 2, 0, 1
            SetupFocusRectangleForCheckBoxesWithoutText();


            // Setup Enabled/Visible
            // Hide certain controls by default (rather than hiding them in the designer and making them hard to edit).
            // Default scanning state for controls
            pauseScanningToolStripMenuItem.Enabled = false;
            stopScanningToolStripMenuItem.Enabled = false;
            statusTotalFilesProgressBar.Visible = false;
            statusCurrentFileProgressBar.Visible = false;

            // Default to invisible until we have a gizmo type set
            gizmoSnapFlowLayoutPanel.Visible = false;

            // Default to invisible unless wireframe and/or vertices draw modes are enabled
            wireframeVertexSizeFlowLayoutPanel.Visible = false;


            // Setup ImageListView for textures
            // Set renderer to use a sharp-cornered square box, and nameplate below the box.
            texturesListView.SetRenderer(new Manina.Windows.Forms.ImageListViewRenderers.XPRenderer());

            // Use this cache mode so that thumbnails are already loaded when you scroll past them.
            texturesListView.CacheMode = CacheMode.Continuous;

            // Set handlers for "Found" and "Textures" groups.
            var grouper = new TexturesListViewGrouper();
            var comparer = Comparer<ImageListViewItem>.Create(CompareTexturesListViewItems);
            // Set grouper and comparer for "Found" group.
            texturesListView.Columns[0].Grouper  = grouper;
            texturesListView.Columns[0].Comparer = comparer;
            // Set grouper and comparer for "Textures" (everything else) group.
            texturesListView.Columns[1].Grouper  = grouper;
            texturesListView.Columns[1].Comparer = comparer;
        }

        private void SetupScene()
        {
            if (!_scene.IsInitialized)
            {
                // Make sure GLControl's handle is created, so that its graphics context is prepared.
                // GLControl attempts to do this automatically when calling MakeCurrent,
                // but it will throw an exception if not visible... so we can't rely on it.
                if (!_glControl.IsHandleCreated)
                {
                    // Calling the getter for Handle will force handle creation.
                    // CreateControl() will not do so if the control is not visible.
                    var dummyAssignToCreateHandle = _glControl.Handle;
                }

                // Setup classes that depend on OpenTK.
                // Make the GraphicsContext current before doing any OpenTK/GL stuff,
                // this only ever needs to be done once.
                _glControl.MakeCurrent();
                _scene.Initialize();
                _vram.Initialize();
            }
        }

        private void ClearScanResults()
        {
            if (!Program.IsScanning)
            {
                Program.ClearResults();

                // Clear selections
                //SelectEntity(null);
                UnselectTriangle();
                _selectedTriangle = null;
                _selectedModelEntity = null;
                _selectedRootEntity = null;
                UpdateSelectedEntity();
                _curAnimation = null;
                _curAnimationObject = null;
                _curAnimationFrame = null;
                UpdateSelectedAnimation(false);

                // Clear selected property grid objects
                modelPropertyGrid.SelectedObject = null;
                texturePropertyGrid.SelectedObject = null;
                animationPropertyGrid.SelectedObject = null;

                // Clear listed results
                entitiesTreeView.Nodes.Clear();
                texturesListView.Items.Clear();
                animationsTreeView.Nodes.Clear();

                // Clear picture boxes
                texturePreviewer.Texture = null;
                // We don't need to clear this for VRAM, since VRAM isn't a scan result

                // Clear actual results
                _rootEntities.Clear();
                foreach (var texture in _textures)
                {
                    texture.Dispose();
                }
                _textures.Clear();
                _packedTextures.Clear();
                _animations.Clear();
                _vram.ClearAllPages();
                vramPreviewer.InvalidateTexture(); // Invalidate to make sure we redraw.

                // Reset scene batches
                _scene.MeshBatch.Reset(0);
                _scene.BoundsBatch.Reset(1);
                _scene.TriangleOutlineBatch.Reset(2);
                _scene.DebugIntersectionsBatch.Reset(1);
                _scene.SetDebugPickingRay(false);

                // Update display information
                statusMessageLabel.Text = "Waiting";
                UpdateVRAMComboBoxPageItems();
                UpdateAnimationProgressLabel();

                _scene.ClearUnderMouseCycleLists();
                TMDBindingsForm.CloseTool();

                GC.Collect(); // It's my memory and I need it now!
            }
        }

        private void Redraw()
        {
            _glControl.Invalidate();
            /*if (menusTabControl.SelectedIndex == ModelsTabIndex)
            {
                scenePreviewer.InvalidateScene();
            }
            else if (menusTabControl.SelectedIndex == AnimationsTabIndex)
            {
                animationPreviewer.InvalidateScene();
            }*/
        }

        private static string GetVersionString()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.FileVersion;
        }

        private void UpdateFPSLabel()
        {
            _fpsLabelRefreshDelayTimer.Reset();
            if (showFPSToolStripMenuItem.Checked && IsSceneTab)
            {
                var skinsStr = string.Empty;// _skinsDrawn > 0 ? $", Skins: {_skinsDrawn}" : string.Empty;
                Text = $"{_baseWindowTitle} (FPS: {_fps:0.0}, Triangles: {_trianglesDrawn}, Meshes: {_meshesDrawn}{skinsStr})";
            }
            else
            {
                Text = _baseWindowTitle;
            }
        }

        private void UpdatePreviewerParents()
        {
            var hideUI = !showUIToolStripMenuItem.Checked;
            var tabIndex = menusTabControl.SelectedIndex;

            if (hideUI && tabIndex == ModelsTabIndex)
            {
                scenePreviewer.Parent = this;
            }
            else
            {
                // Change back to default parent while not in use
                scenePreviewer.Parent = modelsPreviewSplitContainer.Panel2;
            }


            if (hideUI && tabIndex == AnimationsTabIndex)
            {
                animationPreviewer.Parent = this;
            }
            else
            {
                // Change back to default parent while not in use
                animationPreviewer.Parent = animationPreviewPanel;
                animationPreviewer.BringToFront();
            }

            /*if (hideUI && (tabIndex == ModelsTabIndex || tabIndex == AnimationsTabIndex))
            {
                scenePreviewer.Parent = this;
            }
            else if (tabIndex == ModelsTabIndex)
            {
                scenePreviewer.Parent = modelsPreviewSplitContainer.Panel2;
            }
            else if (tabIndex == AnimationsTabIndex)
            {
                scenePreviewer.Parent = animationPreviewPanel;
            }
            else if (scenePreviewer.Parent == this)
            {
                // Assign back to default parent when not in use
                scenePreviewer.Parent = modelsPreviewSplitContainer.Panel2;
            }*/


            if (hideUI && tabIndex == TexturesTabIndex)
            {
                texturePreviewer.Parent = this;
            }
            else
            {
                texturePreviewer.Parent = texturesPreviewSplitContainer.Panel2;
            }


            if (hideUI && tabIndex == VRAMTabIndex)
            {
                vramPreviewer.Parent = this;
            }
            else
            {
                vramPreviewer.Parent = vramPreviewSplitContainer.Panel2;
            }


            // Update the previewer status bars while we're at it, since we only change visibilty when changing parents
            var showModelsStatusBar = showModelsStatusBarToolStripMenuItem.Checked;
            scenePreviewer.ShowStatusBar = !hideUI && showModelsStatusBar;
            texturePreviewer.ShowStatusBar = !hideUI;
            vramPreviewer.ShowStatusBar = !hideUI;
            //animationPreviewer.ShowStatusBar = false; // Always false
        }

        private void UpdateShowUIVisibility(bool changeShowUI)
        {
            PrimaryControl.SuspendLayout();
            SuspendLayout();

            var hideUI = !showUIToolStripMenuItem.Checked;
            mainMenuStrip.Visible = !hideUI;
            // WinForms tries to select the contents of a nested tree view when hiding the parent tab control.
            // So we want to hide the containers nested inside the tab control first to prevent this from happening.
            // WinForms moment...
            modelsPreviewSplitContainer.Visible = !hideUI;
            texturesPreviewSplitContainer.Visible = !hideUI;
            vramPreviewSplitContainer.Visible = !hideUI;
            animationsPreviewSplitContainer.Visible = !hideUI;
            menusTabControl.Visible = !hideUI;
            sceneControlsFlowLayoutPanel.Visible = !hideUI;
            statusStrip1.Visible = !hideUI;

            var showSideBar = showSideBarToolStripMenuItem.Checked;
            modelsPreviewSplitContainer.Panel1Collapsed = !showSideBar;
            texturesPreviewSplitContainer.Panel1Collapsed = !showSideBar;
            vramPreviewSplitContainer.Panel1Collapsed = !showSideBar;
            animationsPreviewSplitContainer.Panel1Collapsed = !showSideBar;

            var borderStyle = (hideUI ? BorderStyle.None : BorderStyle.FixedSingle);
            scenePreviewer.BorderStyle = borderStyle;
            texturePreviewer.BorderStyle = borderStyle;
            vramPreviewer.BorderStyle = borderStyle;
            animationPreviewer.BorderStyle = borderStyle;

            UpdatePreviewerParents();

            ResumeLayout();
            PrimaryControl.ResumeLayout();
            if (changeShowUI)
            {
                Refresh();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                // Take focus away from numeric up/downs,
                // and give it the primary control of this tab.
                var numericUpDown = this.GetFocusedControlOfType<NumericUpDown>();
                if (numericUpDown != null)
                {
                    switch (menusTabControl.SelectedIndex)
                    {
                        case ModelsTabIndex: // Models
                            scenePreviewer.Focus();
                            break;
                        case TexturesTabIndex: // Textures
                            texturePreviewer.Focus();
                            break;
                        case VRAMTabIndex: // VRAM
                            vramPreviewer.Focus();
                            break;
                        case AnimationsTabIndex: // Animations
                            animationPreviewer.Focus();
                            break;
                    }
                    return true; // Enter key handled
                }
            }
            else if (keyData == Keys.Escape)
            {
                if (!showUIToolStripMenuItem.Checked)
                {
                    showUIToolStripMenuItem.Checked = true;
                    return true; // Escape key handled
                }
            }
            
#if ENABLE_CLIPBOARD
            if (keyData == (Keys.Control | Keys.C))
            {
                var copied = false;
                var tabIndex = menusTabControl.SelectedIndex;
                if (scenePreviewer.Focused || animationPreviewer.Focused || entitiesTreeView.Focused || animationsTreeView.Focused)
                {
                    var currentScenePreviewer = tabIndex != AnimationsTabIndex ? scenePreviewer : animationPreviewer;
                    using (var bitmap = currentScenePreviewer.CreateBitmap())
                    {
                        if (bitmap != null)
                        {
                            // Supporting transparency in the copied image means that some programs
                            // like Paint won't use the fallback opaque image background color. :\
                            //ClipboardUtils.SetImageWithTransparency(bitmap, _scene.ClearColor);
                            Clipboard.SetImage(bitmap);
                            copied = true;
                        }
                    }
                }
                else if (texturePreviewer.Focused || vramPreviewer.Focused || texturesListView.Focused || vramListBox.Focused)
                {
                    var currentTexturePreviewer = tabIndex != VRAMTabIndex ? texturePreviewer : vramPreviewer;
                    using (var bitmap = currentTexturePreviewer.CreateBitmap())
                    {
                        if (bitmap != null)
                        {
                            ClipboardUtils.SetImageWithTransparency(bitmap);
                            //Clipboard.SetImage(bitmap);
                            copied = true;
                        }
                    }
                }
                if (copied)
                {
                    Program.ConsoleLogger.WriteLine("Copied to clipboard");
                    return true; // Ctrl+C hotkey handled
                }
            }
#endif
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void previewForm_Load(object sender, EventArgs e)
        {
            // SetupScene needs to be called before ReadSettings
            SetupScene();

            // Read and apply settings or default settings
            ReadSettings(Settings.Instance);

            // Start timers that should always be running
            _mainTimer.Start();

#if DEBUG
            SetupTestModels();
#endif
        }

        private void previewForm_Shown(object sender, EventArgs e)
        {
            if (Program.HasCommandLineArguments)
            {
                _drawAllToVRAMAfterScan = Program.CommandLineOptions.DrawAllToVRAM;
                Program.ScanCommandLineAsync(OnScanProgressCallback);
            }
            else
            {
                PromptScan();
            }
        }

        private void previewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true;

            // Automatically save settings when closing the program.
            // The user can still manually save settings with a menu item.
            SaveSettings();

            _mainTimer?.Stop();

            Program.CancelScan(); // Cancel the scan if one is active.
        }

        private void previewForm_ResizeBegin(object sender, EventArgs e)
        {
            if (fastWindowResizeToolStripMenuItem.Checked)
            {
                SuspendLayout();
                _resizeLayoutSuspended = true;
            }
        }

        private void previewForm_ResizeEnd(object sender, EventArgs e)
        {
            if (_resizeLayoutSuspended)
            {
                ResumeLayout();
                _resizeLayoutSuspended = false;
            }
        }

        private void menusTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabIndex = menusTabControl.SelectedIndex;

            // Handle leaving the animation tab.
            if (_inAnimationTab && tabIndex != AnimationsTabIndex)
            {
                _inAnimationTab = false;
                // Restart to force animation state update next time we're in the animation tab.
                // Reset animation when leaving the tab.
                _animationBatch.Restart();
                Playing = false;
                UpdateAnimationProgressLabel();
                // Update selected entity to invalidate the animation changes to the model.
                UpdateSelectedEntity(updateTextures: false);
            }


            // Force-hide all visuals when in animation tab.
            _scene.ShowVisuals = tabIndex != AnimationsTabIndex;

            UpdatePreviewerParents();

            switch (tabIndex)
            {
                case ModelsTabIndex: // Models
                    animationsTreeView.SelectedNode = null;
                    _fpsWatch.Reset();
                    break;

                case TexturesTabIndex: // Textures
                    break;

                case VRAMTabIndex: // VRAM
                    UpdateVRAMComboBoxPageItems();
                    break;

                case AnimationsTabIndex: // Animations
                    _inAnimationTab = true;
                    _fpsWatch.Reset();
                    UpdateSelectedAnimation();
                    break;
            }

            menusTabControl.Refresh(); // Refresh so that controls don't take an undetermined amount of time to render
            UpdateFPSLabel();
        }

        private void previewForm_KeyDown(object sender, KeyEventArgs e)
        {
            previewForm_KeyEvent(e, KeyEventType.Down);
        }

        private void previewForm_KeyUp(object sender, KeyEventArgs e)
        {
            previewForm_KeyEvent(e, KeyEventType.Up);
        }

        private void previewForm_KeyEvent(KeyEventArgs e, KeyEventType eventType)
        {
            if (eventType == KeyEventType.Down || eventType == KeyEventType.Up)
            {
                var state = eventType == KeyEventType.Down;
                var sceneFocused = (scenePreviewer.Focused || animationPreviewer.Focused);
                var textureFocused = (texturePreviewer.Focused || texturesListView.Focused);
                var vramFocused = (vramPreviewer.Focused || vramListBox.Focused);

                switch (e.KeyCode)
                {
                    // Press space to focus on the currently-selected model
                    case Keys.Space when state && sceneFocused:
                        if (_selectedRootEntity != null || _selectedModelEntity != null)
                        {
                            _scene.FocusOnBounds(GetFocusBounds(GetCheckedEntities()), resetRotation: _autoFocusResetCameraRotation);
                            e.Handled = true;
                        }
                        break;

                    // Gizmo tools
                    // We can't set these shortcut keys in the designer because it
                    // considers them "invalid" for not having modifier keys.
                    case Keys.W when state && sceneFocused:
                        if (!IsControlDown)
                        {
                            if (_gizmoType != GizmoType.Translate)
                            {
                                SetGizmoType(GizmoType.Translate);
                                //Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"GizmoType: {_gizmoType}");
                            }
                            e.Handled = true;
                        }
                        break;
                    case Keys.E when state && sceneFocused:
                        if (!IsControlDown)
                        {
                            if (_gizmoType != GizmoType.Rotate)
                            {
                                SetGizmoType(GizmoType.Rotate);
                                //Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"GizmoType: {_gizmoType}");
                            }
                            e.Handled = true;
                        }
                        break;
                    case Keys.R when state && sceneFocused:
                        if (!IsControlDown)
                        {
                            if (_gizmoType != GizmoType.Scale)
                            {
                                SetGizmoType(GizmoType.Scale);
                                //Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"GizmoType: {_gizmoType}");
                            }
                            e.Handled = true;
                        }
                        break;

                    case Keys.Oemplus when state && textureFocused:
                        if (_clutIndex < 255)
                        {
                            SetCurrentCLUTIndex(_clutIndex + 1);
                            e.Handled = true;
                        }
                        break;
                    case Keys.OemMinus when state && textureFocused:
                        if (_clutIndex > 0)
                        {
                            SetCurrentCLUTIndex(_clutIndex - 1);
                            e.Handled = true;
                        }
                        break;

                    case Keys.P when state && textureFocused:
                        if (!IsControlDown)
                        {
                            showTexturePaletteToolStripMenuItem.Checked = !showTexturePaletteToolStripMenuItem.Checked;
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"ShowTexturePalette: {showTexturePaletteToolStripMenuItem.Checked}");
                            e.Handled = true;
                        }
                        break;

                    case Keys.T when state && (textureFocused || vramFocused):
                        if (!IsControlDown)
                        {
                            if (textureFocused)
                            {
                                showTextureSemiTransparencyToolStripMenuItem.Checked = !showTextureSemiTransparencyToolStripMenuItem.Checked;
                                Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"ShowTextureSemiTransparency: {showTextureSemiTransparencyToolStripMenuItem.Checked}");
                            }
                            else
                            {
                                showVRAMSemiTransparencyToolStripMenuItem.Checked = !showVRAMSemiTransparencyToolStripMenuItem.Checked;
                                Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"ShowVRAMSemiTransparency: {showVRAMSemiTransparencyToolStripMenuItem.Checked}");
                            }
                            e.Handled = true;
                        }
                        break;

                    // Debugging keys for testing picking rays.
#if true
                    case Keys.D when state && sceneFocused:
                        if (IsControlDown) // Ctrl+D: Toggle debug visuals)
                        {
                            _scene.ShowDebugVisuals = !_scene.ShowDebugVisuals;
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"ShowDebugVisuals: {_scene.ShowDebugVisuals}");
                            e.Handled = true;
                        }
                        break;
                    case Keys.P when state && sceneFocused:
                        if (_scene.ShowDebugVisuals)
                        {
                            if (IsControlDown) // Ctrl+P (Toggle debug picking ray)
                            {
                                _scene.ShowDebugPickingRay = !_scene.ShowDebugPickingRay;
                                Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"ShowDebugPickingRay: {_scene.ShowDebugPickingRay}");
                                e.Handled = true;
                            }
                            else if (_scene.ShowDebugPickingRay) // P (Set debug picking ray)
                            {
                                _scene.SetDebugPickingRay();
                                e.Handled = true;
                            }
                        }
                        break;
                    case Keys.I when state && sceneFocused:
                        if (_scene.ShowDebugVisuals)
                        {
                            if (IsControlDown) // Ctrl+I (Toggle debug intersections)
                            {
                                _scene.ShowDebugIntersections = !_scene.ShowDebugIntersections;
                                Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"ShowDebugIntersections: {_scene.ShowDebugIntersections}");
                                e.Handled = true;
                            }
                            else if (_scene.ShowDebugIntersections) // Hold I/Hold Shift+I (Update debug intersections)
                            {
                                var checkedEntities = GetCheckedEntities();
                                var rootEntity = GetSelectedRootEntity();
                                if (IsTriangleSelectMode())
                                {
                                    _scene.GetTriangleUnderMouse(checkedEntities, rootEntity, _lastMouseX, _lastMouseY);
                                }
                                else
                                {
                                    _scene.GetEntityUnderMouse(checkedEntities, rootEntity, _lastMouseX, _lastMouseY, selectionMode: _modelSelectionMode);
                                }
                                e.Handled = true;
                            }
                        }
                        break;
#endif

                    // Debugging: Print information to help hardcode scene setup when taking screenshots of models.
#if DEBUG
                    case Keys.C when state && sceneFocused:
                        if (_scene.ShowDebugVisuals && !IsControlDown)
                        {
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"CameraFOV      = {_scene.CameraFOV:R};");
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"CameraDistance = {_scene.CameraDistance:R};");
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"_cameraX       = {_scene.CameraX:R};");
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"_cameraY       = {_scene.CameraY:R};");
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"_cameraPitch   = {_scene.CameraPitch:R};");
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"_cameraYaw     = {_scene.CameraYaw:R};");
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"LightIntensity = {_scene.LightIntensity:R};");
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"LightPitchYaw  = new Vector2({_scene.LightPitch:R}, {_scene.LightYaw:R});");
                            e.Handled = true;
                        }
                        break;
#endif

                        // Debugging keys for testing AnimationBatch settings while they still don't have UI controls.
#if false
                    case Keys.F when state:
                        _animationBatch.LoopDelayTime += 0.5;
                        Program.ConsoleLogger.WriteLine($"LoopDelayTime={_animationBatch.LoopDelayTime}");
                        break;
                    case Keys.G when state:
                        _animationBatch.LoopDelayTime -= 0.5;
                        Program.ConsoleLogger.WriteLine($"LoopDelayTime={_animationBatch.LoopDelayTime}");
                        break;
#endif
                }
            }
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            // Get elapsed time
            var deltaSeconds = _fpsWatch.IsRunning ? _fpsWatch.Elapsed.TotalSeconds : DefaultElapsedTime;
            _fpsWatch.Restart(); // Start or restart timer, use default time if timer wasn't running.


            // Update FPS tracker
            // Source: <http://www.david-amador.com/2009/11/how-to-do-a-xna-fps-counter/>
            _fpsCalcElapsedSeconds += deltaSeconds;
            if (_fpsCalcElapsedSeconds >= 1d) // Update FPS every one second
            {
                _fps = (float)(_fpsCalcElapsedFrames / _fpsCalcElapsedSeconds);
                _fpsCalcElapsedSeconds = 0d;
                _fpsCalcElapsedFrames = 0;
                if (showFPSToolStripMenuItem.Checked)
                {
                    // We don't want to be updating other controls in a paint event
                    _fpsLabelRefreshDelayTimer.Start();
                }
            }
            _fpsCalcElapsedFrames++;


            if (_inAnimationTab && _curAnimation != null)
            {
                var rootEntity = GetSelectedRootEntity();
                if (_animationBatch.SetupAnimationFrame(rootEntity) && rootEntity != null)
                {
                    var updateMeshData = _curAnimation.AnimationType.IsVertexBased();
                    //_scene.MeshBatch.UpdateMultipleEntityBatch(_selectedRootEntity, _selectedModelEntity, updateMeshData, fastSetup: true);
                    _scene.MeshBatch.SetupMultipleEntityBatch(GetCheckedEntities(), _selectedRootEntity, _selectedModelEntity, updateMeshData, fastSetup: true);

                    // Animation has been processed. Update attached limbs while animating.
                    if (_scene.AttachJointsMode == AttachJointsMode.Attach)
                    {
                        rootEntity.FixConnections();
                    }
                    else
                    {
                        rootEntity.UnfixConnections();
                    }

                    if (_showSkeleton)
                    {
                        _scene.SkeletonBatch.SetupEntitySkeleton(rootEntity, updateMeshData: false);
                    }
                }
            }
            _scene.Draw(out _trianglesDrawn, out _meshesDrawn, out _skinsDrawn);
            _glControl.SwapBuffers();
        }

        private void glControl_MouseEvent(MouseEventArgs e, MouseEventType eventType)
        {
            if (_inAnimationTab)
            {
                _selectedGizmo = GizmoId.None;
            }
            if (eventType == MouseEventType.Wheel)
            {
                _scene.CameraDistance -= e.Delta * MouseSensivity * _scene.CameraDistanceIncrement;
                return;
            }
            var deltaX = e.X - _lastMouseX;
            var deltaY = e.Y - _lastMouseY;
            var mouseLeft = e.Button == MouseButtons.Left;
            var mouseMiddle = e.Button == MouseButtons.Middle;
            var mouseRight = e.Button == MouseButtons.Right;
            var selectedEntityBase = GetSelectedEntityBase();
            _scene.UpdatePicking(e.X, e.Y);
            var hoveredGizmo = _scene.GetGizmoUnderPosition(selectedEntityBase, _gizmoType);
            switch (_selectedGizmo)
            {
                case GizmoId.None:
                    if (!_inAnimationTab && mouseLeft && eventType == MouseEventType.Down)
                    {
                        if (hoveredGizmo == GizmoId.None)
                        {
                            var checkedEntities = GetCheckedEntities();
                            var rootEntity = GetSelectedRootEntity();
                            if (IsTriangleSelectMode())
                            {
                                var newSelectedTriangle = _scene.GetTriangleUnderMouse(checkedEntities, rootEntity, e.X, e.Y);
                                if (newSelectedTriangle != null)
                                {
                                    SelectTriangle(newSelectedTriangle);
                                }
                                else
                                {
                                    UnselectTriangle();
                                }
                            }
                            else
                            {
                                var newSelectedEntity = _scene.GetEntityUnderMouse(checkedEntities, rootEntity, e.X, e.Y, selectionMode: _modelSelectionMode);
                                if (newSelectedEntity != null)
                                {
                                    SelectEntity(newSelectedEntity, false);
                                }
                                else
                                {
                                    UnselectTriangle();
                                }
                            }
                        }
                        else
                        {
                            StartGizmoAction(hoveredGizmo, e.X, e.Y);
                            _scene.ResetIntersection();
                        }
                    }
                    else
                    {
                        if (mouseRight && eventType == MouseEventType.Move)
                        {
                            _scene.CameraPitchYaw += new Vector2(deltaY * MouseSensivity,
                                                                 -deltaX * MouseSensivity);
                        }
                        if (mouseMiddle && eventType == MouseEventType.Move)
                        {
                            _scene.CameraPosition += new Vector2(deltaX * MouseSensivity * _scene.CameraPanIncrement,
                                                                 deltaY * MouseSensivity * _scene.CameraPanIncrement);
                        }
                    }
                    // Gizmos are already updated by Action methods when selected.
                    if (_selectedGizmo == GizmoId.None && hoveredGizmo != _hoveredGizmo)
                    {
                        UpdateGizmoVisualAndState(_selectedGizmo, hoveredGizmo);
                    }
                    break;
                case GizmoId.AxisX when !_inAnimationTab:
                case GizmoId.AxisY when !_inAnimationTab:
                case GizmoId.AxisZ when !_inAnimationTab:
                case GizmoId.Uniform when !_inAnimationTab:
                    if (mouseLeft && eventType == MouseEventType.Move && selectedEntityBase != null)
                    {
                        UpdateGizmoAction(e.X, e.Y);
                    }
                    else if (mouseRight && eventType == MouseEventType.Down)
                    {
                        CancelGizmoAction();
                    }
                    else
                    {
                        FinishGizmoAction();
                    }
                    break;
            }
            _lastMouseX = e.X;
            _lastMouseY = e.Y;
        }

        private void SetupNumericUpDownValidateEvents(Control parent)
        {
            // When a user manually enters in a value in a NumericUpDown,
            // the value will be shown with the specified number of decimal places,
            // BUT VALUE WILL NOT BE ROUNDED TO THE SPECIFIED NUMBER OF DECIMAL PLACES!

            // Anyways, this fixes that by rounding the value during the validating event.
            // We need to find all NumericUpDowns in the form, and register the event for those.
            var queue = new Queue<Control>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var container = queue.Dequeue();

                foreach (Control control in container.Controls)
                {
                    if (control is NumericUpDown numericUpDown)
                    {
                        // Use the ValueChanged event (instead of Validating)
                        // so that values assigned from settings are also enforced.
                        numericUpDown.ValueChanged += OnNumericUpDownValidateValueChanged;
                    }

                    if (control.Controls.Count > 0)
                    {
                        queue.Enqueue(control);
                    }
                }
            }
        }

        private void OnNumericUpDownValidateValueChanged(object sender, EventArgs e)
        {
            if (sender is NumericUpDown numericUpDown)
            {
                numericUpDown.Value = Math.Round(numericUpDown.Value, numericUpDown.DecimalPlaces,
                                                 MidpointRounding.AwayFromZero);
            }
        }

        private void SetupCancelMenuCloseOnSeparatorClick(object parent, bool setupChildren = true)
        {
            var queue = new Queue<object>();
            if (parent is ToolStrip toolStrip) // Parent is ToolStrip
            {
                // Ignore setupChildren here and treat toolStrip's items as the parents.
                foreach (var child in toolStrip.Items)
                {
                    queue.Enqueue(child);
                }
            }
            else
            {
                queue.Enqueue(parent);
            }

            while (queue.Count > 0)
            {
                var container = queue.Dequeue();

                if (container is ToolStripDropDownItem dropDownItem)
                {
                    dropDownItem.DropDown.Closing += OnCancelMenuCloseOnSeparatorClickClosing;
                    dropDownItem.DropDown.ItemClicked += OnCancelMenuCloseOnSeparatorClickItemClicked;

                    if (setupChildren)
                    {
                        foreach (var child in dropDownItem.DropDownItems)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        private void OnCancelMenuCloseOnSeparatorClickClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (_cancelMenuCloseItemClickedSender == sender)
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    // We're closing because a separator item was clicked, so cancel it.
                    e.Cancel = true;
                }
                // Always clear the clicked separator sender for the sender's Closing event.
                _cancelMenuCloseItemClickedSender = null;
            }
        }

        private void OnCancelMenuCloseOnSeparatorClickItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripSeparator)
            {
                // Store the sender of the clicked separator, we'll need it
                // to make sure we cancel during the correct Closing event.
                _cancelMenuCloseItemClickedSender = sender;
            }
            else
            {
                // Always reset this if a new item is clicked that isn't a separator.
                // In the scenario that Closing never happened after a separator was clicked.
                _cancelMenuCloseItemClickedSender = null;
            }
        }

        private void OnCancelMenuCloseWhileHoldingShift(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (IsShiftDown && e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                // Allow changing multiple draw modes without re-opening the menu.
                // A hacky solution until we have draw modes somewhere else like a toolbar.
                e.Cancel = true;
            }
        }

        private void SetupFocusRectangleForCheckBoxesWithoutText()
        {
            foreach (var checkBox in this.EnumerateAllControlsOfType<CheckBox>())
            {
                if (checkBox.Text.Length == 0)
                {
                    checkBox.Paint += OnPaintCheckBoxWithoutTextFocusRectangle;
                }
            }
        }

        private void OnPaintCheckBoxWithoutTextFocusRectangle(object sender, PaintEventArgs e)
        {
            // source: <https://stackoverflow.com/a/6025759/7517185>
            if (sender is CheckBox checkBox && checkBox.Focused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, e.ClipRectangle, checkBox.ForeColor, checkBox.BackColor);
            }
        }

        private void mainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsDisposed || _closing)
            {
                return;
            }

            if (_inDialog)
            {
                // Instantly finish delayed control refreshes
                _animationProgressBarRefreshDelayTimer.Finish();
                _modelPropertyGridRefreshDelayTimer.Finish();
                _fpsLabelRefreshDelayTimer.Finish();
            }
            else
            {
                // Get elapsed time
                var deltaSeconds = _mainWatch.IsRunning ? _mainWatch.Elapsed.TotalSeconds : DefaultElapsedTime;
                _mainWatch.Restart(); // Start or restart timer, use default time if timer wasn't running.

                // Don't skip too much time if we've fallen too far behind.
                var renderSeconds = _fixedTimer ? DefaultElapsedTime : Math.Min(deltaSeconds, MaxElapsedTime);


                // Update delayed control refreshes
                _animationProgressBarRefreshDelayTimer.AddTime(deltaSeconds);
                _modelPropertyGridRefreshDelayTimer.AddTime(deltaSeconds);
                _fpsLabelRefreshDelayTimer.AddTime(deltaSeconds);
                _scanProgressRefreshDelayTimer.AddTime(deltaSeconds);
                _scanPopulateRefreshDelayTimer.AddTime(deltaSeconds);


                // Update animation
                // Don't animate if we're not in the animation tab, or not currently playing.
                // todo: Or allow animations to play in other tabs, in-which case other checks for _inAnimationTab need to be removed.
                if (_inAnimationTab && Playing)
                {
                    if (Playing && _animationBatch.IsFinished)
                    {
                        Playing = false; // LoopMode is a Once type, and the animation has finished. Stop playing.
                    }
                    else
                    {
                        _animationBatch.AddTime(renderSeconds * _animationSpeed);
                    }
                    UpdateAnimationProgressLabel(false); // Delay update to avoid excessive control refresh
                }


                // Update scene timer and then mark for redraw (but only if visible)
                if (IsSceneTab)
                {
                    _scene.AddTime(renderSeconds);
                    Redraw();
                }
            }
        }

        #endregion

        #region GetChecked/AddResults

        private RootEntity GetSelectedRootEntity()
        {
            return _selectedRootEntity ?? _selectedModelEntity?.GetRootEntity();
        }

        private EntityBase GetSelectedEntityBase()
        {
            return (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
        }

        private RootEntity[] GetCheckedEntities(bool defaultToSelected = false)
        {
            var selectedEntities = new List<RootEntity>();
            for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
            {
                var node = entitiesTreeView.Nodes[i];
                if (node.Checked)
                {
                    var tagInfo = (EntitiesTreeViewTagInfo)node.Tag;
                    if (tagInfo.Entity is RootEntity rootEntity)
                    {
                        selectedEntities.Add(rootEntity);
                    }
                }
            }
            if (selectedEntities.Count == 0 && defaultToSelected)
            {
                var selectedRootEntity = GetSelectedRootEntity();
                if (selectedRootEntity != null)
                {
                    selectedEntities.Add(selectedRootEntity);
                }
            }
            return selectedEntities.Count == 0 ? null : selectedEntities.ToArray();
        }

        private IEnumerable<EntityBase> EnumerateUVEntities()
        {
            var selectedEntityBase = GetSelectedEntityBase();
            var selectedRootEntity = GetSelectedRootEntity();
            for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
            {
                var node = entitiesTreeView.Nodes[i];
                if (node.Checked)
                {
                    var tagInfo = (EntitiesTreeViewTagInfo)node.Tag;
                    if (tagInfo.Entity is RootEntity rootEntity && rootEntity != selectedRootEntity)
                    {
                        yield return rootEntity;
                    }
                }
            }
            if (selectedEntityBase != null)
            {
                yield return selectedEntityBase;
            }
        }

        private Animation[] GetCheckedAnimations(bool defaultToSelected = false)
        {
            var selectedAnimations = new List<Animation>();
            for (var i = 0; i < animationsTreeView.Nodes.Count; i++)
            {
                var node = animationsTreeView.Nodes[i];
                if (node.Checked)
                {
                    var tagInfo = (AnimationsTreeViewTagInfo)node.Tag;
                    if (tagInfo.Animation != null)
                    {
                        selectedAnimations.Add(tagInfo.Animation);
                    }
                }
            }
            if (selectedAnimations.Count == 0 && defaultToSelected)
            {
                if (_curAnimation != null)
                {
                    selectedAnimations.Add(_curAnimation);
                }
            }
            return selectedAnimations.Count == 0 ? null : selectedAnimations.ToArray();
        }

        private Texture[] GetSelectedTextures()
        {
            // Use enumerator instead of SelectedItems.Count and array, since:
            // * Count enumerates over all items for each call
            // * Indexer enumerates over items up until index for each call
            var textures = new List<Texture>();
            foreach (var selectedItem in texturesListView.SelectedItems)
            {
                var tagInfo = (TexturesListViewTagInfo)selectedItem.Tag;
                textures.Add(_textures[tagInfo.Index]);
            }
            return textures.ToArray();
        }

        private Texture GetSelectedTexture()
        {
            // Don't use SelectedItems.Count since it enumerates over all items. But...
            // We can't use most Linq functions with SelectedItems because key IList interface methods are not supported.
            var selectedItems = texturesListView.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return null;
            }
            var tagInfo = (TexturesListViewTagInfo)selectedItems[0].Tag;
            return _textures[tagInfo.Index];
        }

        private TreeNode CreateRootEntityNode(RootEntity rootEntity, int rootIndex)
        {
            var loaded = rootEntity.ChildEntities.Length == 0;
            var rootEntityNode = new TreeNode(rootEntity.Name)
            {
                Tag = new EntitiesTreeViewTagInfo
                {
                    Entity = rootEntity,
                    RootIndex = rootIndex,
                    LazyLoaded = loaded,
                },
            };
            if (!loaded)
            {
                rootEntityNode.Nodes.Add(string.Empty); // Show plus sign before we've lazy-loaded
            }
            return rootEntityNode;
        }

        private TreeNode CreateModelEntityNode(EntityBase modelEntity, int rootIndex, int childIndex)
        {
            var modelNode = new TreeNode(modelEntity.Name)
            {
                Tag = new EntitiesTreeViewTagInfo
                {
                    Entity = modelEntity,
                    RootIndex = rootIndex,
                    ChildIndex = childIndex,
                    LazyLoaded = true,
                },
            };
            return modelNode;
        }

        private void LoadEntityChildNodes(TreeNode entityNode)
        {
            //entitiesTreeView.BeginUpdate();
            var tagInfo = (EntitiesTreeViewTagInfo)entityNode.Tag;
            var entity = tagInfo.Entity;

            var modelNodes = new TreeNode[entity.ChildEntities.Length];
            for (var i = 0; i < entity.ChildEntities.Length; i++)
            {
                modelNodes[i] = CreateModelEntityNode(entity.ChildEntities[i], tagInfo.RootIndex, i);
            }
            entityNode.Nodes.Clear(); // Clear dummy node used to show plus sign
            entityNode.Nodes.AddRange(modelNodes);

            // We can't hide checkboxes until we've been added to the tree view structure.
            foreach (var modelNode in modelNodes)
            {
                modelNode.HideCheckBox();
            }
            tagInfo.LazyLoaded = true;
            //entitiesTreeView.EndUpdate();
        }

        private void EntitiesAdded(IReadOnlyList<RootEntity> rootEntities, int startIndex)
        {
            //entitiesTreeView.BeginUpdate();
            var rootEntityNodes = new TreeNode[rootEntities.Count];
            for (var i = 0; i < rootEntities.Count; i++)
            {
                rootEntityNodes[i] = CreateRootEntityNode(rootEntities[i], startIndex + i);
            }
            entitiesTreeView.Nodes.AddRange(rootEntityNodes);
            //entitiesTreeView.EndUpdate();

            // Assign textures to models
            foreach (var rootEntity in rootEntities)
            {
                _vram.AssignModelTextures(rootEntity);
            }
        }

        private void EntityAdded(RootEntity rootEntity, int index)
        {
            entitiesTreeView.Nodes.Add(CreateRootEntityNode(rootEntity, index));

            // Assign texture to model
            _vram.AssignModelTextures(rootEntity);
        }

        private TreeNode FindEntityNode(EntityBase entityBase, bool lazyLoad = true)
        {
            // First check if the selected node is the node we're looking for
            var selectedNode = entitiesTreeView.SelectedNode;

            var tagInfo = (EntitiesTreeViewTagInfo)selectedNode?.Tag;
            if (selectedNode == null || tagInfo.Entity != entityBase)
            {
                var modelEntity = entityBase as ModelEntity;
                var rootEntity = entityBase.GetRootEntity();
                // If the selected node is the root node of our child node, then skip enumerating through all other root nodes
                var startIndex = selectedNode != null && tagInfo.Entity == rootEntity ? selectedNode.Index : 0;

                // The selected node differs, find the associated node
                selectedNode = null;
                for (var i = startIndex; i < entitiesTreeView.Nodes.Count; i++)
                {
                    var rootNode = entitiesTreeView.Nodes[i];
                    tagInfo = (EntitiesTreeViewTagInfo)rootNode.Tag;
                    if (tagInfo.Entity == rootEntity)
                    {
                        if (modelEntity == null)
                        {
                            // Root entity is selected, use this node
                            selectedNode = rootNode;
                        }
                        else if (tagInfo.LazyLoaded || lazyLoad)
                        {
                            // Child of root entity is selected, find that node
                            if (!tagInfo.LazyLoaded)
                            {
                                // We can't search for this node if we haven't lazy-loaded yet
                                LoadEntityChildNodes(rootNode);
                            }
                            for (var j = 0; j < rootNode.Nodes.Count; j++)
                            {
                                var modelNode = rootNode.Nodes[j];
                                tagInfo = (EntitiesTreeViewTagInfo)rootNode.Tag;
                                if (tagInfo.Entity == modelEntity)
                                {
                                    selectedNode = modelNode;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            return selectedNode;
        }

        private ImageListViewItem CreateTextureItem(Texture texture, int index)
        {
            object key = index;
            var textureItem = new ImageListViewItem(key)
            {
                //Text = index.ToString(), //debug
                Text = texture.Name,
                Tag = new TexturesListViewTagInfo
                {
                    //Texture = texture,
                    Index = index,
                    Found = false,
                },
            };
            return textureItem;
        }

        private void TexturesAdded(IReadOnlyList<Texture> textures, int startIndex)
        {
            var textureItems = new ImageListViewItem[textures.Count];
            for (var i = 0; i < textures.Count; i++)
            {
                textureItems[i] = CreateTextureItem(textures[i], startIndex + i);
            }
            texturesListView.Items.AddRange(textureItems, _texturesListViewAdaptor);

            // Change CLUT index to current index
            foreach (var texture in textures)
            {
                texture.SetCLUTIndex(_clutIndex);
            }
        }

        private void TextureAdded(Texture texture, int index)
        {
            texturesListView.Items.Add(CreateTextureItem(texture, index), _texturesListViewAdaptor);

            // Change CLUT index to current index
            texture.SetCLUTIndex(_clutIndex);
        }

        private TreeNode CreateAnimationNode(Animation animation, int rootIndex)
        {
            var loaded = animation.RootAnimationObject.Children.Count == 0;
            var animationNode = new TreeNode(animation.Name)
            {
                Tag = new AnimationsTreeViewTagInfo
                {
                    Animation = animation,
                    LazyLoaded = loaded,
                    RootIndex = rootIndex,
                },
            };
            if (!loaded)
            {
                animationNode.Nodes.Add(string.Empty); // Show plus sign before we've lazy-loaded
            }
            return animationNode;
        }

        private TreeNode CreateAnimationObjectNode(AnimationObject animationObject, int rootIndex, int childIndex)
        {
            var loaded = animationObject.Children.Count == 0;
            if (IncludeAnimationFrameTreeViewNodes)
            {
                loaded &= animationObject.AnimationFrames.Count == 0;
            }
            var namePostfix = string.Empty;
            if (!string.IsNullOrEmpty(animationObject.ObjectName))
            {
                namePostfix = $" {animationObject.ObjectName}";
            }
            var animationObjectNode = new TreeNode($"Animation-Object {childIndex}{namePostfix}") // 0-indexed like Sub-Models
            {
                Tag = new AnimationsTreeViewTagInfo
                {
                    AnimationObject = animationObject,
                    LazyLoaded = loaded,
                    RootIndex = rootIndex,
                    ChildIndex = childIndex,
                },
            };
            if (!loaded)
            {
                animationObjectNode.Nodes.Add(string.Empty); // Show plus sign before we've lazy-loaded
            }
            return animationObjectNode;
        }

        private TreeNode CreateAnimationFrameNode(AnimationFrame animationFrame, int rootIndex, int childIndex)
        {
            var frameNumber = animationFrame.FrameTime; //childIndex;
            var animationFrameNode = new TreeNode("Animation-Frame " + frameNumber)
            {
                Tag = new AnimationsTreeViewTagInfo
                {
                    AnimationFrame = animationFrame,
                    LazyLoaded = true,
                    RootIndex = rootIndex,
                    ChildIndex = childIndex,
                },
            };
            return animationFrameNode;
        }

        private void LoadAnimationChildNodes(TreeNode animationNode)
        {
            //animationsTreeView.BeginUpdate();
            var tagInfo = (AnimationsTreeViewTagInfo)animationNode.Tag;
            var animationObject = tagInfo.AnimationObject ?? tagInfo.Animation?.RootAnimationObject;

            var frameCount = IncludeAnimationFrameTreeViewNodes ? animationObject.AnimationFrames.Count : 0;

            var animationChildNodes = new TreeNode[animationObject.Children.Count + frameCount];
            for (var i = 0; i < animationObject.Children.Count; i++)
            {
                animationChildNodes[i] = CreateAnimationObjectNode(animationObject.Children[i], tagInfo.RootIndex, i);
            }
            if (frameCount > 0)
            {
                var frameStart = animationObject.Children.Count;
                // Sort frames by order of appearance
                var animationFrames = new List<AnimationFrame>(animationObject.AnimationFrames.Values);
                animationFrames.Sort((a, b) => a.FrameTime.CompareTo(b.FrameTime));
                for (var i = 0; i < animationFrames.Count; i++)
                {
                    animationChildNodes[frameStart + i] = CreateAnimationFrameNode(animationFrames[i], tagInfo.RootIndex, i);
                }
            }
            animationNode.Nodes.Clear(); // Clear dummy node used to show plus sign
            animationNode.Nodes.AddRange(animationChildNodes);

            // We can't hide checkboxes until we've been added to the tree view structure.
            foreach (var animationChildNode in animationChildNodes)
            {
                animationChildNode.HideCheckBox();
            }
            tagInfo.LazyLoaded = true;
            //animationsTreeView.EndUpdate();
        }

        private void AnimationsAdded(IReadOnlyList<Animation> animations, int startIndex)
        {
            //animationsTreeView.BeginUpdate();
            var animationNodes = new TreeNode[animations.Count];
            for (var i = 0; i < animations.Count; i++)
            {
                animationNodes[i] = CreateAnimationNode(animations[i], startIndex + i);
            }
            animationsTreeView.Nodes.AddRange(animationNodes);
            //animationsTreeView.EndUpdate();
        }

        private void AnimationAdded(Animation animation, int index)
        {
            animationsTreeView.Nodes.Add(CreateAnimationNode(animation, index));
        }

        private TreeNode FindAnimationNode(Animation animation, bool lazyLoad = true)
        {
            // First check if the selected node is the node we're looking for
            var selectedNode = animationsTreeView.SelectedNode;

            var tagInfo = (AnimationsTreeViewTagInfo)selectedNode?.Tag;
            if (selectedNode == null || tagInfo.Animation != animation)
            {
                // The selected node differs, find the associated node
                selectedNode = null;
                for (var i = 0; i < animationsTreeView.Nodes.Count; i++)
                {
                    var animationNode = animationsTreeView.Nodes[i];
                    tagInfo = (AnimationsTreeViewTagInfo)animationNode.Tag;
                    if (tagInfo.Animation == animation)
                    {
                        selectedNode = animationNode;
                        break;
                    }
                }
            }
            return selectedNode;
        }

        private TreeNode FindAnimationNode(AnimationObject animationObject, AnimationFrame animationFrame, bool lazyLoad = true)
        {
            // First check if the selected node is the node we're looking for
            var selectedNode = animationsTreeView.SelectedNode;

            var tagInfo = (AnimationsTreeViewTagInfo)selectedNode?.Tag;
            var objectMatches = (animationObject != null && tagInfo?.AnimationObject == animationObject);
            var frameMatches  = (animationFrame  != null && tagInfo?.AnimationFrame  == animationFrame);
            if (selectedNode == null || (!objectMatches && !frameMatches))
            {
                var animation = animationObject.Animation;
                if (animation.RootAnimationObject == animationObject)
                {
                    return null; // We don't show nodes for root animation objects, unless we want to return the animation node...
                }
                // If the selected node is the root node of our child node, then skip enumerating through all other root nodes
                var startIndex = selectedNode != null && tagInfo.Animation == animation ? selectedNode.Index : 0;

                // The selected node differs, find the associated node
                selectedNode = null;
                for (var i = startIndex; i < animationsTreeView.Nodes.Count; i++)
                {
                    var animationNode = animationsTreeView.Nodes[i];
                    tagInfo = (AnimationsTreeViewTagInfo)animationNode.Tag;
                    if (tagInfo.Animation == animation)
                    {
                        var parentAnimationObject = animationObject?.Parent ?? animationFrame.AnimationObject;
                        var parentNode = FindAnimationParentNode(animationNode, parentAnimationObject, lazyLoad);
                        if (parentNode == null)
                        {
                            break; // Parent not found, or not loaded
                        }

                        for (var j = 0; j < parentNode.Nodes.Count; j++)
                        {
                            var childNode = parentNode.Nodes[j];
                            tagInfo = (AnimationsTreeViewTagInfo)childNode.Tag;
                            if (animationObject != null && tagInfo.AnimationObject == animationObject)
                            {
                                selectedNode = childNode;
                                break;
                            }
                            else if (animationFrame != null && tagInfo.AnimationFrame == animationFrame)
                            {
                                selectedNode = childNode;
                                break;
                            }
                        }

                        break;
                    }
                }
            }
            return selectedNode;
        }

        private TreeNode FindAnimationParentNode(TreeNode animationNode, AnimationObject parentObject, bool lazyLoad)
        {
            var tagInfo = (AnimationsTreeViewTagInfo)animationNode.Tag;
            if (!tagInfo.LazyLoaded)
            {
                if (!lazyLoad)
                {
                    return null; // We can't continue
                }
                // We can't search for this node if we haven't lazy-loaded yet
                LoadAnimationChildNodes(animationNode);
            }

            if (parentObject.Parent == null)
            {
                return animationNode; // This is the parent node
            }

            var stack = new Stack<AnimationObject>();
            while (parentObject != null && parentObject.Parent != null) // Don't include RootAnimationObject
            {
                stack.Push(parentObject);
                parentObject = parentObject.Parent;
            }

            var nextParentNode = animationNode;
            while (stack.Count > 0 && nextParentNode != null)
            {
                parentObject = stack.Pop();
                var parentNode = nextParentNode;

                nextParentNode = null;
                for (var j = 0; j < parentNode.Nodes.Count; j++)
                {
                    var childNode = parentNode.Nodes[j];
                    tagInfo = (AnimationsTreeViewTagInfo)childNode.Tag;
                    if (tagInfo.AnimationObject == parentObject)
                    {
                        if (!tagInfo.LazyLoaded)
                        {
                            if (!lazyLoad)
                            {
                                break; // We can't continue
                            }
                            // We can't search for this node if we haven't lazy-loaded yet
                            LoadAnimationChildNodes(parentNode);
                        }
                        nextParentNode = childNode;
                        break;
                    }
                }
            }

            return nextParentNode;
        }

        // Helper functions primarily intended for debugging models made by MeshBuilders.
        private void AddRootEntities(params RootEntity[] rootEntities)
        {
            AddRootEntities((IReadOnlyList<RootEntity>)rootEntities); // Cast required to avoid calling this same function
        }

        private void AddRootEntities(IReadOnlyList<RootEntity> rootEntities)
        {
            var startIndex = _rootEntities.Count;
            _rootEntities.AddRange(rootEntities);
            EntitiesAdded(rootEntities, startIndex);
        }

        private void AddRootEntity(RootEntity rootEntity)
        {
            _rootEntities.Add(rootEntity);
            EntityAdded(rootEntity, _rootEntities.Count - 1);
        }

        private void AddTextures(params Texture[] textures)
        {
            AddTextures((IReadOnlyList<Texture>)textures); // Cast required to avoid calling this same function
        }

        private void AddTextures(IReadOnlyList<Texture> textures)
        {
            var startIndex = _textures.Count;
            _textures.AddRange(textures);
            TexturesAdded(textures, startIndex);
        }

        private void AddTexture(Texture texture)
        {
            _textures.Add(texture);
            TextureAdded(texture, _textures.Count - 1);
        }

        private void AddAnimations(params Animation[] animations)
        {
            AddAnimations((IReadOnlyList<Animation>)animations); // Cast required to avoid calling this same function
        }

        private void AddAnimations(IReadOnlyList<Animation> animations)
        {
            var startIndex = _animations.Count;
            _animations.AddRange(animations);
            AnimationsAdded(animations, startIndex);
        }

        private void AddAnimation(Animation animation)
        {
            _animations.Add(animation);
            AnimationAdded(animation, _animations.Count - 1);
        }

        #endregion

        #region Gizmos

        private void SetGizmoType(GizmoType gizmoType, bool force = false)
        {
            if (_gizmoType != gizmoType || force)
            {
                FinishGizmoAction(); // Make sure to finish action before changing _gizmoType
                _gizmoType = gizmoType;

                gizmoToolNoneToolStripMenuItem.Checked = _gizmoType == GizmoType.None;
                gizmoToolTranslateToolStripMenuItem.Checked = _gizmoType == GizmoType.Translate;
                gizmoToolRotateToolStripMenuItem.Checked = _gizmoType == GizmoType.Rotate;
                gizmoToolScaleToolStripMenuItem.Checked = _gizmoType == GizmoType.Scale;

                // Suspend layout while changing visibility and text to avoid jittery movement of controls.
                sceneControlsFlowLayoutPanel.SuspendLayout();

                gizmoSnapFlowLayoutPanel.Visible = _gizmoType != GizmoType.None;
                gridSnapUpDown.Visible = _gizmoType == GizmoType.Translate;
                angleSnapUpDown.Visible = _gizmoType == GizmoType.Rotate;
                scaleSnapUpDown.Visible = _gizmoType == GizmoType.Scale;

                switch (_gizmoType)
                {
                    case GizmoType.Translate:
                        gizmoSnapLabel.Text = "Grid Snap:";
                        break;
                    case GizmoType.Rotate:
                        gizmoSnapLabel.Text = "Angle Snap:";
                        break;
                    case GizmoType.Scale:
                        gizmoSnapLabel.Text = "Scale Snap:";
                        break;
                }

                sceneControlsFlowLayoutPanel.ResumeLayout();

                // Gizmo shape has changed, recalculate hovered.
                var selectedEntityBase = GetSelectedEntityBase();

                _scene.UpdatePicking(_lastMouseX, _lastMouseY);
                var hoveredGizmo = _scene.GetGizmoUnderPosition(selectedEntityBase, _gizmoType);

                UpdateGizmoVisualAndState(_selectedGizmo, hoveredGizmo);
            }
        }

        private void UpdateGizmoVisualAndState(GizmoId selectedGizmo, GizmoId hoveredGizmo)
        {
            var selectedEntityBase = GetSelectedEntityBase();

            // Don't highlight hovered gizmo while selecting
            var highlightGizmo = selectedGizmo != GizmoId.None ? selectedGizmo : hoveredGizmo;
            _scene.UpdateGizmoVisual(selectedEntityBase, _gizmoType, highlightGizmo);

            if (selectedEntityBase != null)
            {
                _selectedGizmo = selectedGizmo;
                _hoveredGizmo = hoveredGizmo;
            }
        }

        private void StartGizmoAction(GizmoId hoveredGizmo, int x, int y)
        {
            if (_selectedGizmo != GizmoId.None || _gizmoType == GizmoType.None)
            {
                return;
            }
            var selectedEntityBase = GetSelectedEntityBase();

            switch (hoveredGizmo)
            {
                case GizmoId.AxisX: _gizmoAxis = Vector3.UnitX; break;
                case GizmoId.AxisY: _gizmoAxis = Vector3.UnitY; break;
                case GizmoId.AxisZ: _gizmoAxis = Vector3.UnitZ; break;
                case GizmoId.Uniform: _gizmoAxis = Vector3.One; break;
            }
            switch (_gizmoType)
            {
                case GizmoType.Translate:
                    _gizmoOrigin = _scene.GetPickedPosition();
                    _gizmoInitialTranslation = selectedEntityBase.Translation;
                    break;
                case GizmoType.Rotate:
                    _gizmoOrigin = selectedEntityBase.WorldOrigin;
                    // Must assign _gizmoOrigin before calling CalculateGizmoRotationDirection.
                    _gizmoRotateStartDirection = CalculateGizmoRotateDirection(x, y);
                    _gizmoInitialRotation = selectedEntityBase.Rotation;
                    break;
                case GizmoType.Scale:
                    _gizmoOrigin = selectedEntityBase.WorldOrigin;
                    // Must assign _gizmoOrigin before calling CalculateGizmoScaleDistance.
                    _gizmoScaleStartDistance = Math.Max(CalculateGizmoScaleDistance(x, y), 1f);
                    _gizmoInitialScale = selectedEntityBase.Scale;
                    break;
            }
            UpdateGizmoVisualAndState(hoveredGizmo, hoveredGizmo);
        }

        private void UpdateGizmoAction(int x, int y)
        {
            if (_selectedGizmo == GizmoId.None)
            {
                return;
            }
            var selectedEntityBase = GetSelectedEntityBase();

            switch (_gizmoType)
            {
                case GizmoType.Translate:
                    UpdateGizmoTranslate(selectedEntityBase);
                    break;
                case GizmoType.Rotate:
                    UpdateGizmoRotate(selectedEntityBase, x, y);
                    break;
                case GizmoType.Scale:
                    UpdateGizmoScale(selectedEntityBase, x, y);
                    break;
            }

            UpdateSelectedEntity(false, noDelayUpdatePropertyGrid: false, fastSetup: true, selectedOnly: true, updateTextures: false); // Delay updating property grid to reduce lag
        }

        private void FinishGizmoAction()
        {
            if (_selectedGizmo == GizmoId.None)
            {
                return;
            }
            var selectedEntityBase = GetSelectedEntityBase();

            switch (_gizmoType)
            {
                case GizmoType.Translate:
                    AlignSelectedEntityToGrid(selectedEntityBase);
                    break;
                case GizmoType.Rotate:
                    AlignSelectedEntityGizmoRotation(selectedEntityBase);
                    break;
                case GizmoType.Scale:
                    AlignSelectedEntityScale(selectedEntityBase);
                    break;
            }
            // UpdateSelectedEntity is already called by align functions.
            // Recalculate hovered gizmo, since the gizmo position/rotation may have changed after alignment.
            var hoveredGizmo = _scene.GetGizmoUnderPosition(selectedEntityBase, _gizmoType);
            UpdateGizmoVisualAndState(GizmoId.None, hoveredGizmo);
        }

        private void CancelGizmoAction()
        {
            if (_selectedGizmo == GizmoId.None)
            {
                return;
            }
            var selectedEntityBase = GetSelectedEntityBase();

            switch (_gizmoType)
            {
                case GizmoType.Translate:
                    selectedEntityBase.Translation = _gizmoInitialTranslation;
                    break;
                case GizmoType.Rotate:
                    selectedEntityBase.Rotation = _gizmoInitialRotation;
                    break;
                case GizmoType.Scale:
                    selectedEntityBase.Scale = _gizmoInitialScale;
                    break;
            }
            UpdateSelectedEntity(false, selectedOnly: true, updateTextures: false);
            // Recalculate hovered gizmo, since the gizmo position/rotation may have changed after alignment.
            var hoveredGizmo = _scene.GetGizmoUnderPosition(selectedEntityBase, _gizmoType);
            UpdateGizmoVisualAndState(GizmoId.None, hoveredGizmo);
        }

        private void UpdateGizmoTranslate(EntityBase selectedEntityBase)
        {
            // Make sure to scale translation by that of parent entity.
            var scale = Vector3.One;
            if (selectedEntityBase.ParentEntity != null)
            {
                var parentWorldMatrix = selectedEntityBase.ParentEntity.WorldMatrix;

                scale = parentWorldMatrix.ExtractScale();

                // The larger the scale, the less we want to move to preserve the same world translation.
                // No division operator with RHS Vector3, so we need to inverse here.
                scale.X = scale.X != 0f ? (1f / scale.X) : 0f;
                scale.Y = scale.Y != 0f ? (1f / scale.Y) : 0f;
                scale.Z = scale.Z != 0f ? (1f / scale.Z) : 0f;
            }

            var pickedPosition = _scene.GetPickedPosition();
            var projectedOffset = (pickedPosition - _gizmoOrigin).ProjectOnNormal(_gizmoAxis);

            if (selectedEntityBase.ParentEntity != null)
            {
                var parentWorldRotation = selectedEntityBase.ParentEntity.WorldMatrix.ExtractRotationSafe();//.Inverted();
                if (Vector3.Dot(parentWorldRotation * _gizmoAxis, _gizmoAxis) < 0f)
                {
                    // Axis is not "forward", reverse offset so that we move in the same direction as the mouse.
                    projectedOffset = -projectedOffset;
                }
            }
            selectedEntityBase.Translation = _gizmoInitialTranslation + projectedOffset * scale;
        }

        private void UpdateGizmoRotate(EntityBase selectedEntityBase, int x, int y)
        {
            var direction = CalculateGizmoRotateDirection(x, y);

            //var z = Vector3.Cross(new Vector3(direction), new Vector3(_gizmoRotateStartDirection)).Z;
            var z = direction.X * _gizmoRotateStartDirection.Y - direction.Y * _gizmoRotateStartDirection.X;
            var angle = (float)Math.Asin(GeomMath.Clamp(z, -1f, 1f));
            // Math.Asin only returns angles in the range 90deg to -90deg. For angles beyond
            // that, we need to check if direction is opposite of _gizmoRotateStartDirection.
            if (Vector2.Dot(direction, _gizmoRotateStartDirection) < 0f)
            {
                angle = (float)Math.PI - angle;
            }

            var worldRotation = selectedEntityBase.WorldMatrix.ExtractRotationSafe();
            if (Vector3.Dot(worldRotation * _gizmoAxis, _scene.CameraDirection) < 0f)
            {
                angle = -angle; // Camera view of axis is not "forward", reverse angle
            }

            // This isn't necessary, since angle isn't going to be compounded and end up as multiples of 360deg.
            //angle = GeomMath.PositiveModulus(angle, (float)(Math.PI * 2d));

            // Add axis rotation based on the current axis rotations.
            // Basically, if you rotate one axis, then that will change
            // the angle of rotation applied when using other axes.
            _gizmoRotateAngle = angle;
            var newRotation = _gizmoInitialRotation * Quaternion.FromAxisAngle(_gizmoAxis, _gizmoRotateAngle);

            selectedEntityBase.Rotation = newRotation;
        }

        private void UpdateGizmoScale(EntityBase selectedEntityBase, int x, int y)
        {
            var distance = CalculateGizmoScaleDistance(x, y);
            var amount = distance / _gizmoScaleStartDistance;

            if (_selectedGizmo == GizmoId.Uniform)
            {
                selectedEntityBase.Scale = _gizmoInitialScale * amount;
            }
            else
            {
                var newScaleMult = Vector3.One + _gizmoAxis * (amount - 1f);
                selectedEntityBase.Scale = _gizmoInitialScale * newScaleMult;
            }
        }

        private Vector2 CalculateGizmoRotateDirection(int x, int y)
        {
            // Get the center of the gizmo as seen on the screen.
            var screen = _scene.WorldToScreenPoint(_gizmoOrigin).Xy;
            var diff = new Vector2(x - screen.X, y - screen.Y);
            if (diff.IsZero())
            {
                // Choose some arbitrary default 2D unit vector in-case the user selected the
                // rotation gizmo right on the same world coordinates as the model's origin.
                return Vector2.UnitX;
            }
            return diff.Normalized();
        }

        private float CalculateGizmoScaleDistance(int x, int y)
        {
            var screen = _scene.WorldToScreenPoint(_gizmoOrigin).Xy;
            var diff = new Vector2(x - screen.X, y - screen.Y);
            return diff.Length;
        }

        private void AlignSelectedEntityToGrid(EntityBase selectedEntityBase)
        {
            if (selectedEntityBase != null)
            {
                selectedEntityBase.Translation = SnapToGrid(selectedEntityBase.Translation);
                UpdateSelectedEntity(false, selectedOnly: true, updateTextures: false);
            }
        }

        private void AlignSelectedEntityGizmoRotation(EntityBase selectedEntityBase)
        {
            if (selectedEntityBase != null)
            {
                var angle = SnapAngle(_gizmoRotateAngle); // Grid size is in units of 1 degree.
                var newRotation = _gizmoInitialRotation * Quaternion.FromAxisAngle(_gizmoAxis, angle);
                selectedEntityBase.Rotation = newRotation;
                UpdateSelectedEntity(false, selectedOnly: true, updateTextures: false);
            }
        }

        private void AlignSelectedEntityScale(EntityBase selectedEntityBase)
        {
            if (selectedEntityBase != null)
            {
                selectedEntityBase.Scale = SnapScale(selectedEntityBase.Scale);
                UpdateSelectedEntity(false, selectedOnly: true, updateTextures: false);
            }
        }

        private float SnapToGrid(float value)
        {
            var step = (double)gridSnapUpDown.Value;
            // Grid size of zero should not align at all. Also we want to avoid divide-by-zero.
            return step == 0 ? value : GeomMath.Snap(value, step);
        }

        private Vector3 SnapToGrid(Vector3 vector)
        {
            var step = (double)gridSnapUpDown.Value;
            // Grid size of zero should not align at all. Also we want to avoid divide-by-zero.
            return step == 0 ? vector : GeomMath.Snap(vector, step);
        }

        private float SnapAngle(float value)
        {
            var step = (double)angleSnapUpDown.Value * ((Math.PI * 2d) / 360d); // In units of 1 degree.
            // Snap of zero should not align at all. Also we want to avoid divide-by-zero.
            return step == 0 ? value : GeomMath.Snap(value, step);
        }

        private Vector3 SnapScale(Vector3 vector)
        {
            var step = (double)scaleSnapUpDown.Value;
            // Scale snap of zero should not align at all. Also we want to avoid divide-by-zero.
            return step == 0 ? vector : GeomMath.Snap(vector, step);
        }

        #endregion

        #region Prompts/SetColor

        private void PromptScan()
        {
            if (!Program.IsScanning)
            {
                EnterDialog();
                try
                {
                    var options = ScannerForm.Show(this);
                    if (options != null)
                    {
                        _drawAllToVRAMAfterScan = options.DrawAllToVRAM;
                        if (!Program.ScanAsync(options, OnScanProgressCallback))
                        {
                            ShowMessageBox($"Directory/File not found: {options.Path}", "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                finally
                {
                    LeaveDialog();
                }
            }
        }

        private void PromptAdvancedSettings()
        {
            EnterDialog();
            try
            {
                bool Validate(object obj, PropertyValueChangedEventArgs args)
                {
                    var settings = (Settings)obj;
                    settings.Validate();
                    return true;
                }

                // Write unsaved changes to the settings so that we don't lose them when reading them back.
                WriteSettings(Settings.Instance);
                // Use a clone of the settings so that changes can be cancelled.
                var clonedSettings = Settings.Instance.Clone();
                if (AdvancedSettingsForm.Show(this, "Advanced Program Settings", clonedSettings, out var modified, validate: Validate))
                {
                    // User pressed Accept, and at least one property was modified
                    if (modified)
                    {
                        Settings.Instance = clonedSettings;
                        ReadSettings(Settings.Instance);
                        UpdateSelectedEntity();
                    }
                    // Always save changes to settings if user presses accept (similarly to how we do so with the scanner/export forms)
                    if (Settings.ImplicitSave)
                    {
                        Settings.Instance.Save();
                    }
                }
            }
            finally
            {
                LeaveDialog();
            }
        }

        private void PromptOutputFolder(Action<string> pathCallback)
        {
            // Use BeginInvoke so that dialog doesn't show up behind menu items...
            BeginInvoke((Action)(() => {
                EnterDialog();
                try
                {
                    // Don't use FolderBrowserDialog because it has the usability of a brick.
                    using (var folderBrowserDialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
                    {
                        folderBrowserDialog.IsFolderPicker = true;
                        folderBrowserDialog.Title = "Select the output folder";
                        // Parameter name used to avoid overload resolution with WPF Window, which we don't have a reference to.
                        if (folderBrowserDialog.ShowDialog(ownerWindowHandle: Handle) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                        {
                            pathCallback(folderBrowserDialog.FileName);
                            return;
                        }
                    }
                    pathCallback(null);

                    //var fbd = new FolderBrowserDialog { Description = "Select the output folder" };
                    //var result = fbd.ShowDialog(this) == DialogResult.OK;
                    //path = fbd.SelectedPath;
                    //return result;
                }
                finally
                {
                    LeaveDialog();
                }
            }));
        }

        private bool PromptVRAMPage(string title, int? initialPage, out int pageIndex)
        {
            EnterDialog();
            try
            {
                pageIndex = 0;
                var defaultText = (initialPage.HasValue && initialPage.Value != -1) ? initialPage.ToString() : null;
                var pageStr = InputDialog.Show(this, $"Please type in a VRAM Page index (0-{VRAM.PageCount-1})", title, defaultText);
                if (pageStr != null)
                {
                    if (int.TryParse(pageStr, out pageIndex) && pageIndex >= 0 && pageIndex < VRAM.PageCount)
                    {
                        return true; // OK (valid page)
                    }
                    ShowMessageBox($"Please type in a valid VRAM Page index between 0 and {VRAM.PageCount-1}", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false; // Canceled / OK (invalid page)
            }
            finally
            {
                LeaveDialog();
            }
        }

        private bool PromptCLUTIndex(string title, int? initialIndex, out int clutIndex)
        {
            EnterDialog();
            try
            {
                clutIndex = 0;
                var defaultText = (initialIndex.HasValue && initialIndex.Value != -1) ? initialIndex.ToString() : null;
                var pageStr = InputDialog.Show(this, $"Please type in a Palette index (0-255)", title, defaultText);
                if (pageStr != null)
                {
                    if (int.TryParse(pageStr, out clutIndex) && clutIndex >= 0 && clutIndex < VRAM.PageCount)
                    {
                        return true; // OK (valid page)
                    }
                    ShowMessageBox($"Please type in a valid Palette index between 0 and 255", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false; // Canceled / OK (invalid page)
            }
            finally
            {
                LeaveDialog();
            }
        }

        private bool PromptColor(Color? initialColor, Color? defaultColor, out Color color)
        {
            EnterDialog();
            try
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.FullOpen = true;
                    // Use dialog custom colors chosen by the user
                    colorDialog.CustomColors = Settings.Instance.GetColorDialogCustomColors(defaultColor);
                    if (initialColor.HasValue)
                    {
                        colorDialog.Color = initialColor.Value;
                    }

                    if (colorDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Remember dialog custom colors chosen by the user
                        Settings.Instance.SetColorDialogCustomColors(colorDialog.CustomColors);

                        color = colorDialog.Color;
                        return true;
                    }
                    // todo: Should we still update custom colors when cancelled?
                    Settings.Instance.SetColorDialogCustomColors(colorDialog.CustomColors);
                }
                color = Color.Black;
                return false;
            }
            finally
            {
                LeaveDialog();
            }
        }

        private void PromptExportModels(bool all)
        {
            // Get all models, checked models, or selected model if nothing is checked.
            var entities = all ? _rootEntities.ToArray() : GetCheckedEntities(true);

#if DEBUG
            // Allow testing of export models form without needing to scan
            if (entities == null)
            {
                entities = new RootEntity[0];
            }
#else
            if (entities == null || entities.Length == 0)
            {
                var message = all ? "No models to export" : "No models checked or selected to export";
                ShowMessageBox(message, "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
#endif

            var animations = GetCheckedAnimations(true);

            EnterDialog();
            try
            {
                var options = ExportModelsForm.Show(this, entities, animations);
                if (options != null)
                {
                    var exportedCount = ExportModelsForm.Export(options, entities, animations);
                    ShowMessageBox($"{exportedCount} models exported", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                LeaveDialog();
            }
        }

        private void PromptExportTextures(bool all, bool vram, bool vramDrawnTo = false)
        {
            Texture[] textures;
            if (vram)
            {
                if (all && vramDrawnTo)
                {
                    var drawnToTextures = new List<Texture>();
                    for (var i = 0; i < _vram.Count; i++)
                    {
                        if (_vram.IsPageUsed(i))
                        {
                            drawnToTextures.Add(_vram[i]);
                        }
                    }
                    textures = drawnToTextures.ToArray();
                }
                else if (all)
                {
                    textures = _vram.ToArray();
                }
                else
                {
                    textures = _vramSelectedPage != -1 ? new[] { _vram[_vramSelectedPage] } : null;
                }
            }
            else
            {
                if (all)
                {
                    textures = _textures.ToArray();
                }
                else
                {
                    textures = GetSelectedTextures();
                }
            }

            if (textures == null || textures.Length == 0)
            {
                string message;
                if (vram)
                {
                    message = all ? "No drawn-to VRAM pages to export" : "No VRAM page selected to export";
                }
                else
                {
                    message = all ? "No textures to export" : "No textures selected to export";
                }
                ShowMessageBox(message, "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PromptOutputFolder(path => {
                if (path != null)
                {
                    var pngExporter = new PNGExporter();
                    if (vram)
                    {
                        // Always number exported VRAM pages by their page number, not export index.
                        foreach (var texture in textures)
                        {
                            pngExporter.Export(texture, $"vram{texture.TexturePage}", path);
                        }
                        ShowMessageBox($"{textures.Length} VRAM pages exported", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        pngExporter.Export(textures, path);
                        ShowMessageBox($"{textures.Length} textures exported", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            });
        }

        private DialogResult ShowMessageBox(string text)
        {
            return ShowMessageBox(text, "PSXPrev");
        }

        private DialogResult ShowMessageBox(string text, string title)
        {
            EnterDialog();
            try
            {
                return MessageBox.Show(this, text, title);
            }
            finally
            {
                LeaveDialog();
            }
        }

        private DialogResult ShowMessageBox(string text, string title, MessageBoxButtons buttons)
        {
            EnterDialog();
            try
            {
                return MessageBox.Show(this, text, title, buttons);
            }
            finally
            {
                LeaveDialog();
            }
        }

        private DialogResult ShowMessageBox(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            EnterDialog();
            try
            {
                return MessageBox.Show(this, text, title, buttons, icon);
            }
            finally
            {
                LeaveDialog();
            }
        }

        private DialogResult ShowMessageBox(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            EnterDialog();
            try
            {
                return MessageBox.Show(this, text, title, buttons, icon, defaultButton);
            }
            finally
            {
                LeaveDialog();
            }
        }

        private DialogResult ShowMessageBox(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            EnterDialog();
            try
            {
                return MessageBox.Show(this, text, title, buttons, icon, defaultButton, options);
            }
            finally
            {
                LeaveDialog();
            }
        }

        private void EnterDialog()
        {
            if (!_inDialog)
            {
                _inDialog = true;
                _mainWatch.Reset(); // Stop watch and use default time during the next Elapsed event
            }
        }

        private void LeaveDialog()
        {
            if (_inDialog)
            {
                _inDialog = false;
            }
        }

        private Bitmap DrawColorIcon(ref Bitmap bitmap, Color color)
        {
            if (bitmap == null)
            {
                bitmap = new Bitmap(16, 16);
            }
            using (var graphics = Graphics.FromImage(bitmap))
            {
#if false
                // Simple method just draws a solid 16x16 rectangle with color.
                graphics.Clear(color);
#else
                // Fancy method uses a bitmap with a shadow, and draws a 14x14 rectangle
                // with a darkened outline, and the solid color within a 12x12 rectangle.
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.SmoothingMode = SmoothingMode.None;

                graphics.Clear(Color.Transparent);

                // Draw the background image that casts a shadow
                graphics.DrawImage(Properties.Resources.ColorBackground, 0, 0);

                // Draw the solid color box
                using (var brush = new SolidBrush(color))
                {
                    graphics.FillRectangle(brush, 2, 2, 12, 12);
                }

                // Draw the darkened color border (color is brightened if maximum channel is less than threshold)
                const float inc = 80f;
                const int threshold = 40;
                var brighten = Math.Max(color.R, Math.Max(color.G, color.B)) <= threshold;
                int BorderChannel(byte channel)
                {
                    var c = channel + (brighten ? (inc * 1.25f) : -inc);
                    return GeomMath.Clamp((int)c, 0, 255);
                }
                var borderColor = Color.FromArgb(BorderChannel(color.R), BorderChannel(color.G), BorderChannel(color.B));
                using (var borderPen = new Pen(borderColor, 1f))
                {
                    graphics.DrawRectangle(borderPen, 1, 1, 14 - 1, 14 - 1); // -1 because size is inclusive for outlines
                }
#endif
            }
            return bitmap;
        }

        private void SetAmbientColor(Color color)
        {
            _scene.AmbientColor = color;
            setAmbientColorToolStripMenuItem.Image = DrawColorIcon(ref _ambientColorBitmap, color);
        }

        private void SetBackgroundColor(Color color)
        {
            // Match background color for panels containing the GLControl to reduce flicker when switching tabs
            // Also helps for when hiding/showing side bar
            scenePreviewer.BackColor = color;
            animationPreviewer.BackColor = color;
            modelsPreviewSplitContainer.Panel2.BackColor = color;
            animationsPreviewSplitContainer.Panel2.BackColor = color;
            animationPreviewPanel.BackColor = color;
            //_glControl.BackColor = color; // Handled by ScenePreviewer controls
            _scene.ClearColor = color;
            setBackgroundColorToolStripMenuItem.Image = DrawColorIcon(ref _backgroundColorBitmap, color);
        }

        private void SetSolidWireframeVerticesColor(Color color)
        {
            _scene.SolidWireframeVerticesColor = color;
            setWireframeVerticesColorToolStripMenuItem.Image = DrawColorIcon(ref _wireframeVerticesColorBitmap, color);
        }

        private void SetMaskColor(Color color)
        {
            _scene.MaskColor = color;
            setMaskColorToolStripMenuItem.Image = DrawColorIcon(ref _maskColorBitmap, color);
        }

        private void setAmbientColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PromptColor(_scene.AmbientColor, Settings.Defaults.AmbientColor, out var color))
            {
                SetAmbientColor(color);
            }
        }

        private void setBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PromptColor(_scene.ClearColor, Settings.Defaults.BackgroundColor, out var color))
            {
                SetBackgroundColor(color);
            }
        }

        private void setSolidWireframeVerticesColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PromptColor(_scene.SolidWireframeVerticesColor, Settings.Defaults.SolidWireframeVerticesColor, out var color))
            {
                SetSolidWireframeVerticesColor(color);
            }
        }

        private void setMaskColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PromptColor(_scene.MaskColor, Settings.Defaults.MaskColor, out var color))
            {
                SetMaskColor(color);
            }
        }

        private void exportSelectedModels_Click(object sender, EventArgs e)
        {
            PromptExportModels(false);
        }

        private void exportAllModels_Click(object sender, EventArgs e)
        {
            PromptExportModels(true);
        }

        private void exportSelectedTextures_Click(object sender, EventArgs e)
        {
            PromptExportTextures(false, false);
        }

        private void exportAllTextures_Click(object sender, EventArgs e)
        {
            PromptExportTextures(true, false);
        }

        private void exportSelectedVRAMPage_Click(object sender, EventArgs e)
        {
            PromptExportTextures(false, true);
        }

        private void exportDrawnToVRAMPages_Click(object sender, EventArgs e)
        {
            PromptExportTextures(true, true, vramDrawnTo: true);
        }

        private void exportAllVRAMPages_Click(object sender, EventArgs e)
        {
            PromptExportTextures(true, true);
        }

        #endregion

        #region Models

        private void SelectFirstEntity()
        {
            // Don't select the first model if we already have a selection.
            // Doing that would interrupt the user.
            if (entitiesTreeView.SelectedNode == null && _rootEntities.Count > 0)
            {
                SelectEntity(_rootEntities[0], true); // Select and focus
            }
        }

        private void SelectEntity(EntityBase entity, bool focus = false)
        {
            if (!focus)
            {
                _selectionSource = EntitySelectionSource.Click;
            }
            TreeNode newNode = null;
            if (entity is RootEntity rootEntity)
            {
                var rootIndex = _rootEntities.IndexOf(rootEntity);
                newNode = entitiesTreeView.Nodes[rootIndex];
            }
            else if (entity != null)
            {
                if (entity.ParentEntity is RootEntity rootEntityFromSub)
                {
                    var rootIndex = _rootEntities.IndexOf(rootEntityFromSub);
                    var rootNode = entitiesTreeView.Nodes[rootIndex];
                    var subIndex = Array.IndexOf(rootEntityFromSub.ChildEntities, entity);
                    var tagInfo = (EntitiesTreeViewTagInfo)rootNode.Tag;
                    if (!tagInfo.LazyLoaded)
                    {
                        LoadEntityChildNodes(rootNode);
                    }
                    newNode = rootNode.Nodes[subIndex];
                }
            }
            if (newNode != null && newNode == entitiesTreeView.SelectedNode)
            {
                // entitiesTreeView_AfterSelect won't be called. Reset the selection source.
                _selectionSource = EntitySelectionSource.None;
                if (entity != null)
                {
                    UnselectTriangle();
                }
            }
            else
            {
                entitiesTreeView.SelectedNode = newNode;
            }
        }

        private void SelectTriangle(Tuple<ModelEntity, Triangle> triangle)
        {
            if (_selectedTriangle?.Item2 != triangle?.Item2)
            {
                _selectedTriangle = triangle;
                UpdateSelectedTriangle();
                UpdateModelPropertyGrid();
            }
        }

        private void UnselectTriangle()
        {
            SelectTriangle(null);
        }

        private bool IsTriangleSelectMode()
        {
            return IsShiftDown;
        }

        private void UpdateSelectedEntity(bool updateMeshData = true, bool noDelayUpdatePropertyGrid = true, bool focus = false, bool fastSetup = false, bool selectedOnly = false, bool updateTextures = true)
        {
            _scene.BoundsBatch.Reset(1);
            var selectedEntityBase = GetSelectedEntityBase();
            var rootEntity = GetSelectedRootEntity();
            if (rootEntity != null)
            {
                rootEntity.ResetAnimationData();
                if (_scene.AttachJointsMode == AttachJointsMode.Attach)
                {
                    rootEntity.FixConnections();
                }
                else
                {
                    rootEntity.UnfixConnections();
                }
                rootEntity.ComputeBounds(_scene.AttachJointsMode);
            }

            if (_showSkeleton)
            {
                _scene.SkeletonBatch.SetupEntitySkeleton(rootEntity, updateMeshData: updateMeshData);
            }

            if (selectedEntityBase != null)
            {
                _scene.BoundsBatch.BindEntityBounds(selectedEntityBase);

                var needsCheckedEntities = true; // (updateMeshData && updateTextures) || !selectedOnly || focus;
                var checkedEntities = needsCheckedEntities ? GetCheckedEntities() : null;

                if (updateMeshData && updateTextures)
                {
                    if (_autoDrawModelTextures || _autoPackModelTextures)
                    {
                        if (_autoPackModelTextures)
                        {
                            // Check if the models being drawn have packed textures. If they do, 
                            // then we'll removed all current packed textures from VRAM to ensure we
                            // have room for new ones. This is an ugly way to do it, but it's simple.
                            var hasPacking = false;
                            if (checkedEntities != null)
                            {
                                foreach (var checkedEntity in checkedEntities)
                                {
                                    foreach (ModelEntity model in checkedEntity.ChildEntities)
                                    {
                                        if (model.NeedsTextureLookup)
                                        {
                                            hasPacking = true;
                                            break;
                                        }
                                    }
                                    if (hasPacking)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (rootEntity != null && !hasPacking)
                            {
                                foreach (ModelEntity model in rootEntity.ChildEntities)
                                {
                                    if (model.NeedsTextureLookup)
                                    {
                                        hasPacking = true;
                                        break;
                                    }
                                }
                            }
                            if (hasPacking)
                            {
                                // Packed textures found, clear existing ones from VRAM.
                                RemoveAllPackedTexturesFromVRAM(updateSelectedEntity: false);
                            }
                        }

                        if (_autoDrawModelTextures)
                        {
                            if (checkedEntities != null)
                            {
                                foreach (var checkedEntity in checkedEntities)
                                {
                                    DrawModelTexturesToVRAM(rootEntity, _clutIndex, updateSelectedEntity: false);
                                }
                            }
                            if (rootEntity != null)
                            {
                                // Selected entity gets drawing priority over checked entities.
                                DrawModelTexturesToVRAM(rootEntity, _clutIndex, updateSelectedEntity: false);
                            }
                        }
                        if (_autoPackModelTextures)
                        {
                            if (rootEntity != null)
                            {
                                // Selected entity gets packing priority over checked entities.
                                AssignModelLookupTexturesAndPackInVRAM(rootEntity, true);
                            }
                            if (checkedEntities != null)
                            {
                                foreach (var checkedEntity in checkedEntities)
                                {
                                    AssignModelLookupTexturesAndPackInVRAM(checkedEntity, true);
                                }
                            }
                            _vram.UpdateAllPages();
                        }
                    }
                    if (checkedEntities != null)
                    {
                        foreach (var checkedEntity in checkedEntities)
                        {
                            AssignModelLookupTextures(checkedEntity);
                        }
                    }
                    if (rootEntity != null)
                    {
                        AssignModelLookupTextures(rootEntity);
                    }
                }

                // SubModelVisibility setting is not supported in the Animations tab.
                var subModelVisibility = _inAnimationTab ? SubModelVisibility.All : _subModelVisibility;
                /*if (selectedOnly)
                {
                    _scene.MeshBatch.UpdateMultipleEntityBatch(_selectedRootEntity, _selectedModelEntity, updateMeshData,
                                                               subModelVisibility: subModelVisibility, fastSetup: fastSetup);
                }
                else*/
                {
                    _scene.MeshBatch.SetupMultipleEntityBatch(checkedEntities, _selectedRootEntity, _selectedModelEntity, updateMeshData,
                                                              subModelVisibility: subModelVisibility, fastSetup: fastSetup);
                }

                // todo: Ensure we focus when switching to a different root entity, even if the selected entity is a sub-model.
                if (focus)
                {
                    if (_selectedModelEntity == null)
                    {
                        focus = _autoFocusOnRootModel;
                    }
                    else if (_selectedModelEntity != null)
                    {
                        focus = _autoFocusOnSubModel;
                    }

                    if (focus)
                    {
                        _scene.FocusOnBounds(GetFocusBounds(checkedEntities), resetRotation: _autoFocusResetCameraRotation);
                    }
                }
            }
            else
            {
                _scene.ClearUnderMouseCycleLists();
                _scene.MeshBatch.Reset(0);
                _selectedGizmo = GizmoId.None;
                _hoveredGizmo = GizmoId.None;
            }
            if (_selectionSource != EntitySelectionSource.Click)
            {
                _scene.DebugIntersectionsBatch.Reset(1);
            }
            UpdateSelectedTriangle();
            UpdateModelPropertyGrid(noDelayUpdatePropertyGrid);
            UpdateGizmoVisualAndState(_selectedGizmo, _hoveredGizmo);
            _selectionSource = EntitySelectionSource.None;
        }

        private void UpdateSelectedTriangle(bool updateMeshData = true)
        {
            _scene.TriangleOutlineBatch.Reset(2);
            if (_selectedTriangle != null)
            {
                _scene.TriangleOutlineBatch.BindTriangleOutline(_selectedTriangle.Item1, _selectedTriangle.Item2);
            }
            else
            {
                // Fix it so that when a triangle is unselected, the cached list
                // (for selecting each triangle under the mouse) is reset.
                // Otherwise we end up picking the next triangle when the user isn't expecting it.
                _scene.ClearTriangleUnderMouseCycleList();
            }
        }

        private void UpdateModelPropertyGrid(bool noDelay = true)
        {
            var propertyObject = _selectedTriangle?.Item2 ?? _selectedRootEntity ?? (object)_selectedModelEntity;

            if (noDelay || modelPropertyGrid.SelectedObject != propertyObject)
            {
                _modelPropertyGridRefreshDelayTimer.Reset();

                modelPropertyGrid.SelectedObject = propertyObject;
            }
            else
            {
                // Delay updating the property grid to reduce lag.
                _modelPropertyGridRefreshDelayTimer.Start();
            }
        }

        private void SetModelSelectionMode(EntitySelectionMode selectionMode, bool force = false)
        {
            if (force || _modelSelectionMode != selectionMode)
            {
                _modelSelectionMode = selectionMode;

                selectionModeNoneToolStripMenuItem.Checked = _modelSelectionMode == EntitySelectionMode.None;
                selectionModeBoundsToolStripMenuItem.Checked = _modelSelectionMode == EntitySelectionMode.Bounds;
                selectionModeTriangleToolStripMenuItem.Checked = _modelSelectionMode == EntitySelectionMode.Triangle;
            }
        }

        private void SetSubModelVisibility(SubModelVisibility visibility, bool force = false)
        {
            if (force || _subModelVisibility != visibility)
            {
                _subModelVisibility = visibility;

                subModelVisibilityAllToolStripMenuItem.Checked = _subModelVisibility == SubModelVisibility.All;
                subModelVisibilitySelectedToolStripMenuItem.Checked = _subModelVisibility == SubModelVisibility.Selected;
                subModelVisibilityWithSameTMDIDToolStripMenuItem.Checked = _subModelVisibility == SubModelVisibility.WithSameTMDID;

                UpdateSelectedEntity(selectedOnly: true, updateTextures: false);
            }
        }

        private BoundingBox GetFocusBounds(RootEntity[] checkedEntities)
        {
            var bounds = new BoundingBox();
            var selectedRootEntity = GetSelectedRootEntity();

            if (_autoFocusIncludeCheckedModels && checkedEntities != null)
            {
                foreach (var checkedEntity in checkedEntities)
                {
                    if (checkedEntity == selectedRootEntity)
                    {
                        continue; // We'll add bounds for selectedRootEntity later on in the function.
                    }
                    bounds.AddBounds(checkedEntity.Bounds3D);
                }
            }

            if (selectedRootEntity != null)
            {
                if (_selectedModelEntity != null && (!_autoFocusIncludeWholeModel || _subModelVisibility != SubModelVisibility.All))
                {
                    IReadOnlyList<ModelEntity> models;
                    if (!_autoFocusIncludeWholeModel || _subModelVisibility == SubModelVisibility.Selected)
                    {
                        models = new ModelEntity[] { _selectedModelEntity };
                    }
                    else if (_subModelVisibility == SubModelVisibility.WithSameTMDID)
                    {
                        models = selectedRootEntity.GetModelsWithTMDID(_selectedModelEntity.TMDID);
                    }
                    else
                    {
                        models = new ModelEntity[0];
                    }
                    foreach (var model in models)
                    {
                        if (model.Triangles.Length > 0 && (!model.AttachedOnly || _scene.AttachJointsMode != AttachJointsMode.Hide))
                        {
                            bounds.AddBounds(model.Bounds3D);
                        }
                    }
                }
                else
                {
                    bounds.AddBounds(selectedRootEntity.Bounds3D);
                }
            }

            if (!bounds.IsSet)
            {
                bounds.AddPoint(Vector3.Zero);
            }
            return bounds;
        }

        private void FixConnectionsForCheckedNode(TreeNode checkedNode)
        {
            // Fix connections for entities when checked
            // (but not if it's the selected entity, since that's handled by UpdateSelectedEntity)
            var tagInfo = (EntitiesTreeViewTagInfo)checkedNode.Tag;
            // todo: If we support checking things other than root entities, then we need to handle it here
            var rootEntity = tagInfo.Entity as RootEntity;
            var selectedRootEntity = GetSelectedRootEntity();
            if (rootEntity != null && rootEntity != selectedRootEntity)
            {
                if (_scene.AttachJointsMode == AttachJointsMode.Attach)
                {
                    rootEntity.FixConnections();
                }
                else
                {
                    rootEntity.UnfixConnections();
                }
            }
        }

        private void entitiesTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var tagInfo = (EntitiesTreeViewTagInfo)e.Node.Tag;
            if (!tagInfo.LazyLoaded)
            {
                LoadEntityChildNodes(e.Node);
            }
        }

        private void entitiesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_selectionSource == EntitySelectionSource.None)
            {
                _selectionSource = EntitySelectionSource.TreeView;
            }
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode != null)
            {
                var tagInfo = (EntitiesTreeViewTagInfo)selectedNode.Tag;
                _selectedRootEntity = tagInfo.Entity as RootEntity;
                _selectedModelEntity = tagInfo.Entity as ModelEntity;
                UnselectTriangle();
            }
            var rootEntity = GetSelectedRootEntity();
            UpdateSelectedEntity(focus: _selectionSource == EntitySelectionSource.TreeView);
        }

        private void entitiesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            var checkedNode = e.Node;
            if (checkedNode != null)
            {
                FixConnectionsForCheckedNode(checkedNode);
            }

            if (!_busyChecking)
            {
                UpdateSelectedEntity(focus: _autoFocusIncludeCheckedModels);
            }
        }

        private void modelPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var updateEntity = true;
            var updateMeshData = false;
            var updateTextures = false;
            var selectedOnly = true;

            var propertyName = e.ChangedItem.PropertyDescriptor.Name;
            var parentPropertyName = e.ChangedItem.Parent?.PropertyDescriptor?.Name;
            var parentParentPropertyName = e.ChangedItem.Parent?.Parent?.PropertyDescriptor?.Name;

            if (modelPropertyGrid.SelectedObject is EntityBase entityBase)
            {
                var modelEntity = entityBase as ModelEntity;
                var rootEntity = entityBase.GetRootEntity();


                if (propertyName == nameof(ModelEntity.RenderFlags))
                {
                    var oldRenderFlags = (RenderFlags)e.OldValue;
                    var newRenderFlags = (RenderFlags)e.ChangedItem.Value;
                    // Check if any render flags have changed that require changes to mesh data
                    var changedRenderFlags = (oldRenderFlags ^ newRenderFlags);
                    if ((changedRenderFlags & (RenderFlags.Line | RenderFlags.Sprite | RenderFlags.SpriteNoPitch)) != 0)
                    {
                        updateMeshData = true;
                    }
                }
                else if (propertyName == nameof(ModelEntity.PositionX))
                {
                    // We shouldn't be snapping when the change is made by the user
                    //entityBase.PositionX = SnapToGrid(entityBase.PositionX);
                }
                else if (propertyName == nameof(ModelEntity.PositionY))
                {
                    // We shouldn't be snapping when the change is made by the user
                    //entityBase.PositionY = SnapToGrid(entityBase.PositionY);
                }
                else if (propertyName == nameof(ModelEntity.PositionZ))
                {
                    // We shouldn't be snapping when the change is made by the user
                    //entityBase.PositionZ = SnapToGrid(entityBase.PositionZ);
                }
                else if (propertyName == nameof(ModelEntity.TexturePage))
                {
                    // Update the texture associated with this model
                    if (modelEntity != null)
                    {
                        _vram.AssignModelTextures(modelEntity);
                    }
                    updateMeshData = true;
                    updateTextures = true;
                }
                else if (parentPropertyName == nameof(ModelEntity.TextureLookup) || parentParentPropertyName == nameof(ModelEntity.TextureLookup))
                {
                    updateMeshData = true;
                    updateTextures = true;
                }
                else if (parentPropertyName == nameof(ModelEntity.Texture) || parentPropertyName == nameof(TextureLookup.Texture))
                {
                    updateMeshData = true;
                    updateTextures = true;
                    selectedOnly = false; // We modified a texture, and may need to modify the UVs in the mesh data
                }
                else if (propertyName == nameof(EntityBase.Name))
                {
                    updateEntity = false;
                    // Updating TreeNodes can be extremely slow (before we fixed that),
                    // and the property setter doesn't check if the value is the same.
                    // So do the work ourselves and reduce some lag... WinForms moment.
                    var entityNode = FindEntityNode(entityBase);
                    if (entityNode != null && entityNode.Text != entityBase.Name)
                    {
                        entityNode.Text = entityBase.Name;
                    }
                }
                else if (parentPropertyName == nameof(EntityBase.DebugData))
                {
                    updateEntity = false;
                }
            }
            else if (modelPropertyGrid.SelectedObject is Triangle triangle)
            {
                if (parentPropertyName == nameof(Triangle.DebugData))
                {
                    updateEntity = false;
                }
            }

            if (updateEntity)
            {
                // If updating textures, then we may need to update the UV coordinates of meshes
                UpdateSelectedEntity(updateMeshData, selectedOnly: selectedOnly, updateTextures: updateTextures, fastSetup: true);
            }
        }

        private void checkAllModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (entitiesTreeView.Nodes.Count > CheckAllNodesWarningCount)
            {
                var result = ShowMessageBox($"You are about to check over {CheckAllNodesWarningCount} models. Displaying too many models may cause problems. Do you want to continue?",
                    "Check All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }
            entitiesTreeView.BeginUpdate();
            _busyChecking = true;
            try
            {
                for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
                {
                    var node = entitiesTreeView.Nodes[i];
                    node.Checked = true;
                }
            }
            finally
            {
                _busyChecking = false;
                entitiesTreeView.EndUpdate();
            }
            UpdateSelectedEntity(focus: _autoFocusIncludeCheckedModels);
            // Checked entities have changed, which are included in UV drawing
            texturePreviewer.InvalidateUVs();
            vramPreviewer.InvalidateUVs();
        }

        private void uncheckAllModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            entitiesTreeView.BeginUpdate();
            _busyChecking = true;
            try
            {
                for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
                {
                    var node = entitiesTreeView.Nodes[i];
                    node.Checked = false;
                }
            }
            finally
            {
                _busyChecking = false;
                entitiesTreeView.EndUpdate();
            }
            UpdateSelectedEntity(focus: _autoFocusIncludeCheckedModels);
            // Checked entities have changed, which are included in UV drawing
            texturePreviewer.InvalidateUVs();
            vramPreviewer.InvalidateUVs();
        }

        private void resetWholeModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRootEntity = GetSelectedRootEntity();
            if (selectedRootEntity != null)
            {
                // This could be changed to only reset the selected model and its children.
                // But that's only necessary if sub-sub-model support is ever added.
                selectedRootEntity.ResetTransform(true);
                UpdateSelectedEntity(false, selectedOnly: true, updateTextures: false);
            }
        }

        private void resetSelectedModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedEntityBase = GetSelectedEntityBase();
            if (selectedEntityBase != null)
            {
                selectedEntityBase.ResetTransform(false);
                UpdateSelectedEntity(false, selectedOnly: true, updateTextures: false);
            }
        }

        private void gizmoToolNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetGizmoType(GizmoType.None);
        }

        private void gizmoToolTranslateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetGizmoType(GizmoType.Translate);
        }

        private void gizmoToolRotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetGizmoType(GizmoType.Rotate);
        }

        private void gizmoToolScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetGizmoType(GizmoType.Scale);
        }

        private void selectionModeNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetModelSelectionMode(EntitySelectionMode.None);
        }

        private void selectionModeBoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetModelSelectionMode(EntitySelectionMode.Bounds);
        }

        private void selectionModeTriangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetModelSelectionMode(EntitySelectionMode.Triangle);
        }

        private void drawModeFacesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.DrawFaces = drawModeFacesToolStripMenuItem.Checked;
        }

        private void drawModeWireframeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.DrawWireframe = drawModeWireframeToolStripMenuItem.Checked;
            UpdateDrawModeWireframeVertices();
        }

        private void drawModeVerticesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.DrawVertices = drawModeVerticesToolStripMenuItem.Checked;
            UpdateDrawModeWireframeVertices();
        }

        private void drawModeSolidWireframeVerticesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.DrawSolidWireframeVertices = drawModeSolidWireframeVerticesToolStripMenuItem.Checked;
        }

        private void UpdateDrawModeWireframeVertices()
        {
            // Suspend layout while changing visibility and text to avoid jittery movement of controls.
            sceneControlsFlowLayoutPanel.SuspendLayout();

            wireframeVertexSizeFlowLayoutPanel.Visible = _scene.DrawWireframe || _scene.DrawVertices;
            wireframeSizeUpDown.Visible = _scene.DrawWireframe;
            vertexSizeUpDown.Visible = _scene.DrawVertices;
            if (_scene.DrawWireframe && _scene.DrawVertices)
            {
                wireframeVertexSizeLabel.Text = "Wireframe/Vertex Size:";
            }
            else if (_scene.DrawWireframe)
            {
                wireframeVertexSizeLabel.Text = "Wireframe Size:";
            }
            else if (_scene.DrawVertices)
            {
                wireframeVertexSizeLabel.Text = "Vertex Size:";
            }

            sceneControlsFlowLayoutPanel.ResumeLayout();
            sceneControlsFlowLayoutPanel.Refresh(); // Force controls to show/hide if the renderer is taking too long to paint
        }

        private void showBoundsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.ShowBounds = showBoundsToolStripMenuItem.Checked;
            Redraw();
        }

        private void showSkeletonToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _showSkeleton = showSkeletonToolStripMenuItem.Checked;
            if (!_showSkeleton)
            {
                _scene.SkeletonBatch.Reset(0);
            }
            else
            {
                var rootEntity = GetSelectedRootEntity();
                _scene.SkeletonBatch.SetupEntitySkeleton(rootEntity, updateMeshData: true);
            }
        }

        private void enableLightToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.LightEnabled = enableLightToolStripMenuItem.Checked;
            Redraw();
        }

        private void enableTexturesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.TexturesEnabled = enableTexturesToolStripMenuItem.Checked;
            Redraw();
        }

        private void enableVertexColorToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.VertexColorEnabled = enableVertexColorToolStripMenuItem.Checked;
            Redraw();
        }

        private void enableSemiTransparencyToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.SemiTransparencyEnabled = enableSemiTransparencyToolStripMenuItem.Checked;
            Redraw();
        }

        private void forceDoubleSidedToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.ForceDoubleSided = forceDoubleSidedToolStripMenuItem.Checked;
            Redraw();
        }

        private void autoAttachLimbsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var oldHide = _scene.AttachJointsMode == AttachJointsMode.Hide;
            if (autoAttachLimbsToolStripMenuItem.Checked)
            {
                _scene.AttachJointsMode = AttachJointsMode.Attach;
            }
            else
            {
                _scene.AttachJointsMode = AttachJointsMode.Hide;
            }
            var newHide = _scene.AttachJointsMode == AttachJointsMode.Hide;
            var updateMeshData = (oldHide != newHide);
            // Update mesh data, since limb vertices may have changed
            UpdateSelectedEntity(updateMeshData: updateMeshData, updateTextures: false);
        }

        // These two events are for testing AttachJointsMode.DontAttach. We don't have a proper UI selection for it yet.
        private void autoAttachLimbsToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            /*var oldHide = _scene.AttachJointsMode == AttachJointsMode.Hide;
            switch (autoAttachLimbsToolStripMenuItem.CheckState)
            {
                case CheckState.Unchecked:
                    _scene.AttachJointsMode = AttachJointsMode.Hide;
                    break;
                case CheckState.Indeterminate:
                    _scene.AttachJointsMode = AttachJointsMode.DontAttach;
                    break;
                case CheckState.Checked:
                    _scene.AttachJointsMode = AttachJointsMode.Attach;
                    break;
            }
            var newHide = _scene.AttachJointsMode == AttachJointsMode.Hide;
            var updateMeshData = (oldHide != newHide);
            // Update mesh data, since limb vertices may have changed
            UpdateSelectedEntity(updateMeshData: updateMeshData, updateTextures: false);*/
        }

        private void autoAttachLimbsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*// Lazy solution to turn off CheckOnClick since this is just for debugging
            autoAttachLimbsToolStripMenuItem.CheckOnClick = false;
            switch (autoAttachLimbsToolStripMenuItem.CheckState)
            {
                case CheckState.Unchecked:
                    autoAttachLimbsToolStripMenuItem.CheckState = CheckState.Indeterminate;
                    break;
                case CheckState.Indeterminate:
                    autoAttachLimbsToolStripMenuItem.CheckState = CheckState.Checked;
                    break;
                case CheckState.Checked:
                    autoAttachLimbsToolStripMenuItem.CheckState = CheckState.Unchecked;
                    break;
            }*/
        }

        private void subModelVisibilityAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSubModelVisibility(SubModelVisibility.All);
        }

        private void subModelVisibilitySelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSubModelVisibility(SubModelVisibility.Selected);
        }

        private void subModelVisibilityWithSameTMDIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSubModelVisibility(SubModelVisibility.WithSameTMDID);
        }

        private void autoFocusOnRootModelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoFocusOnRootModel = autoFocusOnRootModelToolStripMenuItem.Checked;
        }

        private void autoFocusOnSubModelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoFocusOnSubModel = autoFocusOnSubModelToolStripMenuItem.Checked;
        }

        private void autoFocusIncludeWholeModelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoFocusIncludeWholeModel = autoFocusIncludeWholeModelToolStripMenuItem.Checked;
        }

        private void autoFocusIncludeCheckedModelsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoFocusIncludeCheckedModels = autoFocusIncludeCheckedModelsToolStripMenuItem.Checked;
        }

        private void autoFocusResetCameraRotationToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoFocusResetCameraRotation = autoFocusResetCameraRotationToolStripMenuItem.Checked;
        }

        private void lineRendererToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.VibRibbonWireframe = lineRendererToolStripMenuItem.Checked;
            UpdateSelectedEntity(updateTextures: false); // Update mesh data, since vib ribbon redefines how mesh data is built.
        }

        private void UpdateLightDirection()
        {
            _scene.LightPitchYaw = new Vector2((float)lightPitchNumericUpDown.Value * GeomMath.Deg2Rad,
                                               (float)lightYawNumericUpDown.Value   * GeomMath.Deg2Rad);
        }

        private void lightPitchNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            lightPitchNumericUpDown.Value = GeomMath.PositiveModulus(lightPitchNumericUpDown.Value, 360m);
            UpdateLightDirection();
            lightPitchNumericUpDown.Refresh(); // Too slow to refresh number normally if using the arrow keys
        }

        private void lightYawNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            lightYawNumericUpDown.Value = GeomMath.PositiveModulus(lightYawNumericUpDown.Value, 360m);
            UpdateLightDirection();
            lightYawNumericUpDown.Refresh(); // Too slow to refresh number normally if using the arrow keys
        }

        private void lightIntensityNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _scene.LightIntensity = (float)lightIntensityNumericUpDown.Value;
            lightIntensityNumericUpDown.Refresh(); // Too slow to refresh number normally if using the arrow keys
        }

        private void gridSnapUpDown_ValueChanged(object sender, EventArgs e)
        {
            gridSnapUpDown.Refresh();
        }

        private void angleSnapUpDown_ValueChanged(object sender, EventArgs e)
        {
            angleSnapUpDown.Refresh();
        }

        private void scaleSnapUpDown_ValueChanged(object sender, EventArgs e)
        {
            scaleSnapUpDown.Refresh();
        }

        private void vertexSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            _scene.VertexSize = (float)vertexSizeUpDown.Value;
            vertexSizeUpDown.Refresh(); // Too slow to refresh number normally if using the arrow keys
        }

        private void wireframeSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            _scene.WireframeSize = (float)wireframeSizeUpDown.Value;
            wireframeSizeUpDown.Refresh(); // Too slow to refresh number normally if using the arrow keys
        }

        private void cameraFOVUpDown_ValueChanged(object sender, EventArgs e)
        {
            _scene.CameraFOV = (float)cameraFOVUpDown.Value;// * GeomMath.Deg2Rad;
            cameraFOVUpDown.Refresh(); // Too slow to refresh number normally if using the arrow keys
        }

        #endregion

        #region Textures/VRAM

        private int AssignModelLookupTextures(RootEntity rootEntity)
        {
            var locateFailedCount = 0;
            foreach (ModelEntity model in rootEntity.ChildEntities)
            {
                if (!AssignModelLookupTextures(model))
                {
                    locateFailedCount++;
                }
            }
            return locateFailedCount;
        }

        private bool AssignModelLookupTextures(ModelEntity model)
        {
            if (!model.NeedsTextureLookup)
            {
                return true;
            }
            model.TexturePage = 0;
            model.TextureLookup.Texture = null;
            _vram.AssignModelTextures(model);

            var expectedFormat = model.TextureLookup.ExpectedFormat;
            var id = model.TextureLookup.ID;
            if (!id.HasValue)
            {
                return false;
            }
            Texture found = null;
            foreach (var texture in _packedTextures)
            {
                if (!texture.LookupID.HasValue)
                {
                    continue; // This could be changed by the user in the property grid
                }
                if (!string.IsNullOrEmpty(expectedFormat))
                {
                    if (texture.FormatName == null || texture.FormatName != expectedFormat)
                    {
                        continue;
                    }
                }

                if (texture.LookupID.Value == id)
                {
                    found = texture;
                    break;
                }
                else if (_fallbackTextureID.HasValue && texture.LookupID.Value == _fallbackTextureID.Value)
                {
                    found = texture;
                    // Keep looking for the real texture
                }
            }
            if (found != null)
            {
                var texture = found;
                model.TexturePage = (uint)texture.TexturePage;
                model.TextureLookup.Texture = texture;
                _vram.AssignModelTextures(model);
                return true;
            }
            return false;
        }

        private int AssignModelLookupTexturesAndPackInVRAM(RootEntity rootEntity, bool suppressUpdate = false)
        {
            var locateOrPackFailedCount = 0;
            foreach (ModelEntity model in rootEntity.ChildEntities)
            {
                if (!AssignModelLookupTexturesAndPackInVRAM(model, suppressUpdate))
                {
                    locateOrPackFailedCount++;
                }
            }
            return locateOrPackFailedCount;
        }

        private bool AssignModelLookupTexturesAndPackInVRAM(ModelEntity model, bool suppressUpdate = false)
        {
            if (!model.NeedsTextureLookup)
            {
                return true;
            }
            model.TexturePage = 0;
            model.TextureLookup.Texture = null;
            _vram.AssignModelTextures(model);

            var expectedFormat = model.TextureLookup.ExpectedFormat;
            var id = model.TextureLookup.ID;
            if (!id.HasValue)
            {
                return false;
            }
            Texture found = null;
            foreach (var texture in _textures)
            {
                if (!texture.NeedsPacking)
                {
                    continue; // This isn't a locatable texture
                }
                if (!string.IsNullOrEmpty(expectedFormat))
                {
                    if (texture.FormatName == null || texture.FormatName != expectedFormat)
                    {
                        continue;
                    }
                }

                if (texture.LookupID.Value == id)
                {
                    found = texture;
                    break;
                }
                else if (_fallbackTextureID.HasValue && texture.LookupID.Value == _fallbackTextureID.Value)
                {
                    found = texture;
                    // Keep looking for the real texture
                }
            }
            if (found != null)
            {
                var texture = found;
                if (texture.IsPacked)
                {
                    model.TexturePage = (uint)texture.TexturePage;
                    model.TextureLookup.Texture = texture;
                    _vram.AssignModelTextures(model);
                    return true;
                }
                else if (PackTextureInVRAM(texture))
                {
                    _vram.DrawTexture(texture, suppressUpdate);
                    model.TexturePage = (uint)texture.TexturePage;
                    model.TextureLookup.Texture = texture;
                    _vram.AssignModelTextures(model);
                    return true;
                }
                // Failed to pack texture
            }
            return false;
        }

        // Returns number of textures that could not be packed
        private int DrawTexturesToVRAM(IEnumerable<Texture> textures, int? clutIndex, bool updateSelectedEntity = true)
        {
            var packedChanged = false;
            var packFailedCount = 0;
            foreach (var texture in textures)
            {
                // Textures that need packing must find a location to pack first
                if (texture.NeedsPacking)
                {
                    if (texture.IsPacked)
                    {
                        continue; // Texture is already drawn
                    }
                    if (!PackTextureInVRAM(texture))
                    {
                        packFailedCount++;
                        continue; // No place to pack this texture, can't draw
                    }
                    packedChanged = true;
                }

                var oldClutIndex = texture.CLUTIndex;
                if (clutIndex.HasValue && texture.CLUTCount > 1)
                {
                    texture.SetCLUTIndex(clutIndex.Value);
                }

                _vram.DrawTexture(texture, true); // Suppress updates to scene until all textures are drawn.

                if (clutIndex.HasValue && texture.CLUTCount > 1)
                {
                    texture.SetCLUTIndex(oldClutIndex);
                }
            }
            if (_vram.UpdateAllPages()) // True if any pages needed to be updated (aka textures wasn't empty)
            {
                vramPreviewer.InvalidateTexture(); // Invalidate to make sure we redraw.
                UpdateVRAMComboBoxPageItems();
            }

            if (packedChanged && updateSelectedEntity)
            {
                UpdateSelectedEntity();
            }
            return packFailedCount;
        }

        private int DrawModelTexturesToVRAM(RootEntity rootEntity, int? clutIndex, bool updateSelectedEntity = true)
        {
            // Note: We can't just use ModelEntity.Texture, since that just points to the VRAM page.
            return DrawTexturesToVRAM(rootEntity.OwnedTextures, clutIndex, updateSelectedEntity);
        }

        private bool PackTextureInVRAM(Texture texture)
        {
            if (!texture.NeedsPacking || texture.IsPacked)
            {
                return true;
            }
            if (_vram.FindPackLocation(texture, out var packPage, out var packX, out var packY))
            {
                texture.IsPacked = true;
                texture.TexturePage = packPage;
                texture.X = packX;
                texture.Y = packY;
                _packedTextures.Add(texture);
                return true;
            }
            return false;
        }

        private void ClearVRAMPage(int index, bool updateSelectedEntity = true)
        {
            var packedChanged = false;
            _vram.ClearPage(index);
            for (var i = 0; i < _packedTextures.Count; i++)
            {
                var texture = _packedTextures[i];
                if (texture.TexturePage == index)
                {
                    packedChanged = true;
                    texture.IsPacked = false;
                    texture.TexturePage = 0;
                    texture.X = 0;
                    texture.Y = 0;
                    _packedTextures.RemoveAt(i);
                    i--;
                }
            }
            vramPreviewer.InvalidateTexture(); // Invalidate to make sure we redraw.
            UpdateVRAMComboBoxPageItems();

            if (packedChanged && updateSelectedEntity)
            {
                UpdateSelectedEntity();
            }
        }

        private void ClearAllVRAMPages(bool updateSelectedEntity = true)
        {
            var packedChanged = _packedTextures.Count > 0;
            _vram.ClearAllPages();
            foreach (var texture in _packedTextures)
            {
                texture.IsPacked = false;
                texture.TexturePage = 0;
                texture.X = 0;
                texture.Y = 0;
            }
            _packedTextures.Clear();
            vramPreviewer.InvalidateTexture(); // Invalidate to make sure we redraw.
            UpdateVRAMComboBoxPageItems();

            if (packedChanged && updateSelectedEntity)
            {
                UpdateSelectedEntity();
            }
        }

        private void RemoveAllPackedTexturesFromVRAM(bool updateSelectedEntity = true)
        {
            var packedChanged = _packedTextures.Count > 0;
            foreach (var texture in _packedTextures)
            {
                _vram.RemoveTexturePacking(texture);
                texture.IsPacked = false;
                texture.TexturePage = 0;
                texture.X = 0;
                texture.Y = 0;
            }
            _packedTextures.Clear();
            vramPreviewer.InvalidateTexture(); // Invalidate to make sure we redraw.
            UpdateVRAMComboBoxPageItems();

            if (packedChanged && updateSelectedEntity)
            {
                UpdateSelectedEntity();
            }
        }

        private void SetCurrentCLUTIndex(int clutIndex)
        {
            _clutIndex = GeomMath.Clamp(clutIndex, 0, 255);
            foreach (var texture in _textures)
            {
                texture.SetCLUTIndex(_clutIndex);
            }

            setPaletteIndexToolStripMenuItem.Text = $"Set CLUT Index: {_clutIndex}";

            // Refresh texture thumbnails, texture preview, and property grid
            texturesListView.Invalidate();
            texturePreviewer.InvalidateTexture(); // Invalidate to make sure we redraw.
            texturePropertyGrid.SelectedObject = texturePropertyGrid.SelectedObject;
        }

        private static int CompareTexturesListViewItems(ImageListViewItem a, ImageListViewItem b)
        {
            var tagInfoA = (TexturesListViewTagInfo)a.Tag;
            var tagInfoB = (TexturesListViewTagInfo)b.Tag;
            return tagInfoA.Index.CompareTo(tagInfoB.Index);
        }

        private void UpdateVRAMComboBoxPageItems()
        {
            // Mark page numbers that have had textures drawn to them.
            // note: This assumes the combo box items are strings, which they currently are.
            for (var i = 0; i < _vram.Count; i++)
            {
                var used = _vram.IsPageUsed(i);
                vramListBox.Items[i] = $"{i}" + (used ? " (drawn to)" : string.Empty);
            }
        }

        private void texturesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Don't use SelectedItems.Count since it enumerates over all items. But...
            // We can't use most Linq functions with SelectedItems because key IList interface methods are not supported.
            if (texturesListView.SelectedItems.Count != 1)
            {
                // Only show a texture in the property grid if exactly one item is selected.
                texturePropertyGrid.SelectedObject = null;
                return;
            }
            var texture = GetSelectedTexture();
            if (texture == null)
            {
                return;
            }
            texturePreviewer.Texture = texture;
            texturePropertyGrid.SelectedObject = texture;
        }

        private void texturePropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var updateEntities = false;
            var updateMeshData = false;
            var updateTextures = false;

            var propertyName = e.ChangedItem.PropertyDescriptor.Name;
            var parentPropertyName = e.ChangedItem.Parent?.PropertyDescriptor?.Name;
            //var parentParentPropertyName = e.ChangedItem.Parent?.Parent?.PropertyDescriptor?.Name;

            // Validate changes to texture properties.
            if (texturePropertyGrid.SelectedObject is Texture texture)
            {
                if (propertyName == nameof(Texture.X))
                {
                    if (!texture.IsPacked)
                    {
                        texture.X = VRAM.ClampTextureX(texture.X);
                        texturePreviewer.InvalidateUVs(); // Offset of UV lines has changed
                    }
                    else
                    {
                        texture.X = (int)e.OldValue; // Can't changed value used to determine packing
                    }
                }
                else if (propertyName == nameof(Texture.Y))
                {
                    if (!texture.IsPacked)
                    {
                        texture.Y = VRAM.ClampTextureY(texture.Y);
                        texturePreviewer.InvalidateUVs(); // Offset of UV lines has changed
                    }
                    else
                    {
                        texture.Y = (int)e.OldValue; // Can't changed value used to determine packing
                    }
                }
                else if (propertyName == nameof(Texture.TexturePage))
                {
                    if (!texture.IsPacked)
                    {
                        texture.TexturePage = VRAM.ClampTexturePage(texture.TexturePage);
                        texturePreviewer.InvalidateUVs(); // Set of UV lines has changed
                    }
                    else
                    {
                        texture.TexturePage = (int)e.OldValue; // Can't changed value used to determine packing
                    }
                }
                else if (propertyName == nameof(Texture.CLUTIndex))
                {
                    texture.CLUTIndex = GeomMath.Clamp(texture.CLUTIndex, 0, Math.Max(0, texture.CLUTCount - 1));
                    texture.SetCLUTIndex(texture.CLUTIndex, force: true);
                    texturePreviewer.InvalidateTexture(); // Invalidate to make sure we redraw.
                }
                else if (propertyName == nameof(Texture.Name))
                {
                    // Update changes to Name property in ListViewItem.
                    // Don't use SelectedItems.Count since it enumerates over all items. But...
                    // We can't use most Linq functions with SelectedItems because key IList interface methods are not supported.
                    var selectedItems = texturesListView.SelectedItems;
                    var selectedItem = selectedItems.Count > 0 ? selectedItems[0] : null;

                    var tagInfo = (TexturesListViewTagInfo)selectedItem?.Tag;
                    if (selectedItem == null || _textures[tagInfo.Index] != texture)
                    {
                        // The selected item differs from the property grid's selected object, find the associated item
                        selectedItem = null;
                        foreach (var item in texturesListView.Items)
                        {
                            tagInfo = (TexturesListViewTagInfo)item.Tag;
                            if (_textures[tagInfo.Index] == texture)
                            {
                                selectedItem = item;
                                break;
                            }
                        }
                    }
                    if (selectedItem != null && selectedItem.Text != texture.Name)
                    {
                        selectedItem.Text = texture.Name;
                    }
                }
            }
        }

        private void drawSelectedToVRAM_Click(object sender, EventArgs e)
        {
            // Don't use SelectedItems.Count since it enumerates over all items.
            var selectedTextures = GetSelectedTextures();
            if (selectedTextures == null || selectedTextures.Length == 0)
            {
                ShowMessageBox("Select textures to draw to VRAM first", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var packFailedCount = DrawTexturesToVRAM(selectedTextures, null); // Null to draw selected textures of any clut index
            WarnPackFailedCount(packFailedCount);
        }

        private void drawAllToVRAM_Click(object sender, EventArgs e)
        {
            var packFailedCount = DrawTexturesToVRAM(_textures, _clutIndex);
            WarnPackFailedCount(packFailedCount);
        }

        private void WarnPackFailedCount(int packFailedCount)
        {
            if (packFailedCount > 0)
            {
                ShowMessageBox($"Not enough room to pack remaining {packFailedCount} textures.\nTry clearing VRAM pages first.", "Packing Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void findTextureByVRAMPage_Click(object sender, EventArgs e)
        {
            if (PromptVRAMPage("Find by Page", null, out var pageIndex))
            {
                var found = 0;
                foreach (var item in texturesListView.Items)
                {
                    var tagInfo = (TexturesListViewTagInfo)item.Tag;
                    tagInfo.Found = false;
                    var texture = _textures[tagInfo.Index];
                    if (texture.TexturePage == pageIndex)
                    {
                        tagInfo.Found = true; // Mark as found so the grouper will add it to the "Found" group
                        found++;
                    }
                }
                texturesListView.GroupColumn = found > 0 ? 0 : 1; // Set primary group to "Found" (only if any items were found)
                texturesListView.Sort(); // Sort items added to "Found" group (is this necessary?)
                ShowMessageBox(found > 0 ? $"Found {found} items" : "Nothing found", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void clearTextureFindResults_Click(object sender, EventArgs e)
        {
            foreach (var item in texturesListView.Items)
            {
                var tagInfo = (TexturesListViewTagInfo)item.Tag;
                tagInfo.Found = false; // Unmark found so the grouper will return it to the "Textures" group
            }
            texturesListView.GroupColumn = 1; // Set primary group back to "Textures"
            texturesListView.Sort(); // Re-sort now that "Found" group has been merged back with "Textures" group
            ShowMessageBox("Results cleared", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void setPaletteIndexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PromptCLUTIndex("Change CLUT Index", _clutIndex, out var newCLUTIndex))
            {
                if (_clutIndex != newCLUTIndex)
                {
                    SetCurrentCLUTIndex(newCLUTIndex);
                }
            }
        }

        private void vramComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = vramListBox.SelectedIndex;
            if (index >= 0 && index != _vramSelectedPage)
            {
                _vramSelectedPage = index;
                vramPreviewer.Texture = _vram[_vramSelectedPage];
            }
        }

        private void gotoVRAMPageButton_Click(object sender, EventArgs e)
        {
            if (PromptVRAMPage("Go to VRAM Page", _vramSelectedPage, out var pageIndex))
            {
                vramListBox.SelectedIndex = pageIndex;
            }
        }

        private void clearVRAMPage_Click(object sender, EventArgs e)
        {
            if (_vramSelectedPage < 0)
            {
                ShowMessageBox("Select a page first", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ClearVRAMPage(_vramSelectedPage);
        }

        private void clearAllVRAMPages_Click(object sender, EventArgs e)
        {
            ClearAllVRAMPages();
        }

        private void showTexturePaletteToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            texturePreviewer.ShowPalette = showTexturePaletteToolStripMenuItem.Checked;
        }

        private void showTextureSemiTransparencyToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            texturePreviewer.ShowSemiTransparency = showTextureSemiTransparencyToolStripMenuItem.Checked;
        }

        private void showTextureUVsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            texturePreviewer.ShowUVs = showTextureUVsToolStripMenuItem.Checked;
        }

        private void showMissingTexturesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.ShowMissingTextures = showMissingTexturesToolStripMenuItem.Checked;
        }

        private void autoDrawModelTexturesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoDrawModelTextures = autoDrawModelTexturesToolStripMenuItem.Checked;
        }

        private void autoPackModelTexturesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoPackModelTextures = autoPackModelTexturesToolStripMenuItem.Checked;
        }

        private void showVRAMSemiTransparencyToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            vramPreviewer.ShowSemiTransparency = showVRAMSemiTransparencyToolStripMenuItem.Checked;
        }

        private void showVRAMUVsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            vramPreviewer.ShowUVs = showVRAMUVsToolStripMenuItem.Checked;
        }


        #endregion

        #region Animations

        private void UpdateSelectedAnimation(bool play = false)
        {
            var propertyObject = _curAnimationFrame ?? _curAnimationObject ?? (object)_curAnimation;

            // Change Playing after Enabled, so that the call to Refresh in Playing will affect the enabled visual style too.
            animationPlayButton.Enabled = (_curAnimation != null);
            Playing = play;

            var rootEntity = GetSelectedRootEntity();

            animationPropertyGrid.SelectedObject = propertyObject;
            _animationBatch.SetupAnimationBatch(_curAnimation);
            if (_curAnimation != null)
            {
                _animationBatch.SetupAnimationFrame(rootEntity, force: true);
            }
            //_scene.MeshBatch.UpdateMultipleEntityBatch(_selectedRootEntity, _selectedModelEntity, true, fastSetup: false);
            _scene.MeshBatch.SetupMultipleEntityBatch(GetCheckedEntities(), _selectedRootEntity, _selectedModelEntity, true, fastSetup: false);

            if (_showSkeleton)
            {
                if (rootEntity != null)
                {
                    _scene.SkeletonBatch.SetupEntitySkeleton(rootEntity, updateMeshData: true);
                }
            }

            UpdateAnimationProgressLabel();
        }

        private void UpdateAnimationProgressLabel(bool noDelay = true)
        {
            if (noDelay)
            {
                _animationProgressBarRefreshDelayTimer.Reset();

                // Display max can be shown as 0, even if we require a minimum of 1 internally.
                var displayMax = (int)GeomMath.Clamp(_animationBatch.FrameCount, 0, int.MaxValue);
                var displayValue = (int)GeomMath.Clamp(_animationBatch.CurrentFrameTime, 0, int.MaxValue);

                var newMax = (int)GeomMath.Clamp(_animationBatch.FrameCount * AnimationProgressPrecision, 1, int.MaxValue);
                var newValue = (int)GeomMath.Clamp(_animationBatch.CurrentFrameTime * AnimationProgressPrecision, 0, int.MaxValue);

                animationProgressLabel.Text = $"{displayValue}/{displayMax}";
                if (newMax != animationFrameTrackBar.Maximum || newValue != animationFrameTrackBar.Value)
                {
                    animationFrameTrackBar.Maximum = newMax;
                    animationFrameTrackBar.SetValueSafe(newValue);
                    animationFrameTrackBar.Refresh();
                    animationProgressLabel.Refresh();
                }
            }
            else
            {
                _animationProgressBarRefreshDelayTimer.Start();
            }
        }

        private void animationsTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var tagInfo = (AnimationsTreeViewTagInfo)e.Node.Tag;
            if (!tagInfo.LazyLoaded)
            {
                LoadAnimationChildNodes(e.Node);
            }
        }

        private void animationsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNode = animationsTreeView.SelectedNode;
            if (selectedNode == null)
            {
                UpdateSelectedEntity();
                return;
            }

            var tagInfo = (AnimationsTreeViewTagInfo)selectedNode.Tag;

            if (tagInfo.Animation != null)
            {
                _curAnimation = tagInfo.Animation;
                _curAnimationObject = null;
                _curAnimationFrame = null;
            }
            else if (tagInfo.AnimationObject != null)
            {
                _curAnimation = tagInfo.AnimationObject.Animation;
                _curAnimationObject = tagInfo.AnimationObject;
                _curAnimationFrame = null;
            }
            else if (tagInfo.AnimationFrame != null)
            {
                _curAnimation = tagInfo.AnimationFrame.AnimationObject.Animation;
                _curAnimationObject = tagInfo.AnimationFrame.AnimationObject;
                _curAnimationFrame = tagInfo.AnimationFrame;
            }

            if (_autoSelectAnimationModel && _curAnimation.OwnerEntity != null)
            {
                SelectEntity(_curAnimation.OwnerEntity, true);
            }
            else
            {
                UpdateSelectedEntity();
            }
            UpdateSelectedAnimation(_autoPlayAnimations);

            if (_curAnimationFrame != null)
            {
                _animationBatch.SetTimeToFrame(_curAnimationFrame);
                UpdateAnimationProgressLabel();
            }

            if (TMDBindingsForm.IsVisible && _curAnimation != null)
            {
                TMDBindingsForm.ShowTool(this, _curAnimation);
            }
        }

        private void animationPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var propertyName = e.ChangedItem.PropertyDescriptor.Name;
            var parentPropertyName = e.ChangedItem.Parent?.PropertyDescriptor?.Name;
            var parentParentPropertyName = e.ChangedItem.Parent?.Parent?.PropertyDescriptor?.Name;

            if (animationPropertyGrid.SelectedObject is Animation animation)
            {
                if (propertyName == nameof(Animation.Name))
                {
                    var animationNode = FindAnimationNode(animation);
                    if (animationNode != null && animationNode.Text != animation.Name)
                    {
                        animationNode.Text = animation.Name;
                    }
                }
            }
            else if (animationPropertyGrid.SelectedObject is AnimationObject animationObject)
            {

            }
            else if (animationPropertyGrid.SelectedObject is AnimationFrame animationFrame)
            {

            }

            // Restart animation to force invalidate.
            _animationBatch.Restart();
            // todo: Can we remove this?
            UpdateSelectedEntity(updateTextures: false);
            UpdateSelectedAnimation();
        }

        private void animationPlayButton_Click(object sender, EventArgs e)
        {
            Playing = !Playing;
        }

        private void animationFrameTrackBar_Scroll(object sender, EventArgs e)
        {
            if (_inAnimationTab && !Playing)
            {
                var value = (double)animationFrameTrackBar.Value;
                // Sanity check that max is less than intmax, if it isn't, then we aren't using progress precision.
                if (IsShiftDown && animationFrameTrackBar.Maximum < int.MaxValue)
                {
                    // Hold shift down to reduce precision to individual frames
                    value = GeomMath.Snap(value, AnimationProgressPrecision);
                }
                var delta = value / animationFrameTrackBar.Maximum;
                _animationBatch.FrameTime = delta * _animationBatch.FrameCount;
                UpdateAnimationProgressLabel();
            }
        }

        private void animationSpeedNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _animationSpeed = (float)animationSpeedNumericUpDown.Value;
        }

        private void animationLoopModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = animationLoopModeComboBox.SelectedIndex;
            if (index >= 0)
            {
                _animationBatch.LoopMode = (AnimationLoopMode)index;
            }
        }

        private void animationReverseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _animationBatch.Reverse = animationReverseCheckBox.Checked;
        }

        private void checkAllAnimationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            animationsTreeView.BeginUpdate();
            _busyChecking = true;
            try
            {
                for (var i = 0; i < animationsTreeView.Nodes.Count; i++)
                {
                    var node = animationsTreeView.Nodes[i];
                    node.Checked = true;
                }
            }
            finally
            {
                _busyChecking = false;
                animationsTreeView.EndUpdate();
            }
        }

        private void uncheckAllAnimationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            animationsTreeView.BeginUpdate();
            _busyChecking = true;
            try
            {
                for (var i = 0; i < animationsTreeView.Nodes.Count; i++)
                {
                    var node = animationsTreeView.Nodes[i];
                    node.Checked = false;
                }
            }
            finally
            {
                _busyChecking = false;
                animationsTreeView.EndUpdate();
            }
        }

        private void autoPlayAnimationsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoPlayAnimations = autoPlayAnimationsToolStripMenuItem.Checked;
        }

        private void autoSelectAnimationModelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _autoSelectAnimationModel = autoSelectAnimationModelToolStripMenuItem.Checked;
        }

        private void showTMDBindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_curAnimation == null)
            {
                ShowMessageBox("Please select an Animation first", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                TMDBindingsForm.ShowTool(this, _curAnimation);
            }
        }

        #endregion
        
        #region File/Help

        private void startScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PromptScan();
        }

        private void clearScanResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Program.IsScanning && (_rootEntities.Count > 0 || _textures.Count > 0 || _animations.Count > 0))
            {
                var result = ShowMessageBox("Are you sure you want to clear scan results?", "Clear Scan Results", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    ClearScanResults();
                }
            }
        }

        private void pauseScanningToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (Program.IsScanning && !Program.IsScanCanceling)
            {
                var paused = Program.PauseScan(pauseScanningToolStripMenuItem.Checked);
                statusMessageLabel.Text = "Scan " + (paused ? "Paused" : "Resumed");
            }
        }

        private void stopScanningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Program.IsScanning && !Program.IsScanCanceling)
            {
                var result = ShowMessageBox("Are you sure you want to cancel the current scan?", "Stop Scanning", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    var canceled = Program.CancelScan();
                    if (canceled)
                    {
                        pauseScanningToolStripMenuItem.Checked = false;
                        pauseScanningToolStripMenuItem.Enabled = false;
                        stopScanningToolStripMenuItem.Enabled = false;
                    }
                }
            }
        }

        private void showFPSToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFPSLabel();
        }

        private void showModelsStatusBarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreviewerParents(); // This method also updates status bar visibility
        }

        private void showSideBarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            // No need to update side-bar if UI is hidden, since it won't be shown regardless
            if (showUIToolStripMenuItem.Checked)
            {
                UpdateShowUIVisibility(false);
            }
        }

        private void showUIToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateShowUIVisibility(true);
        }

        private void defaultSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = ShowMessageBox("Are you sure you want to reset settings to their default values?", "Reset Settings", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                LoadDefaultSettings();
            }
        }

        private void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = ShowMessageBox("Are you sure you want to reload settings from file?", "Reload Settings", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                LoadSettings();
            }
        }

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void advancedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PromptAdvancedSettings();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = ShowMessageBox("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void videoTutorialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/watch?v=hPDa8l3ZE6U");
        }

        private void compatibilityListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/spreadsheets/d/155pUzwl7CC14ssT0PJkaEA53CS1ijpOV04VitQCVBC4/edit?pli=1#gid=22642205");
        }

        private void viewOnGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/rickomax/psxprev");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Don't bother showing joints limit if it's high enough to never be a problem
            var maxJointsStr = Shader.MaxJoints <= 2048 ? $", max: {Shader.MaxJoints}" : string.Empty;
            var jointsSupportStr = Shader.JointsSupported ? $"Shader-time joints{maxJointsStr}" : "Pre-computed joints";
            var message = "PSXPrev - PlayStation (PSX) Files Previewer/Extractor\n" +
                          "\u00a9 PSXPrev Contributors - 2020-2023\n" +
                          $"Program Version {GetVersionString()}\n" +
                          $"GLSL Version {Shader.GLSLVersion} ({jointsSupportStr})\n" +
#if ENABLE_CLIPBOARD
                          "Clipboard Support Enabled"
#else
                          "Clipboard Support Disabled"
#endif
                          ;
            ShowMessageBox(message, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Testing

        // Create models that are used to test renderer/builder functionality.
        // Uncomment return; before committing.
        private void SetupTestModels()
        {
            return;

            SetupTestSpriteTransformModels();

            SelectFirstEntity();
        }

        // Testing for sprites that have model transforms applied, along with confirming normal transforms are correct.
        private void SetupTestSpriteTransformModels()
        {
            var size = 40;
            var center = new Vector3(size * 3, 0f, 0f);
            var triBuilder = new TriangleMeshBuilder
            {
                RenderFlags = /*RenderFlags.Unlit |*/ RenderFlags.Sprite,// | RenderFlags.DoubleSided,
                SpriteCenter = center,
            };
            /*var vertex0 = center + new Vector3(-size,  size, 0f);
            var vertex1 = center + new Vector3( size,  size, 0f);
            var vertex2 = center + new Vector3(-size, -size, 0f);
            var vertex3 = center + new Vector3( size, -size, 0f);*/
            //triBuilder.AddQuad(vertex0, vertex1, vertex2, vertex3,
            //    Color3.Red, Color3.Green, Color3.Blue, Color3.Yellow);
            triBuilder.AddSprite(center, new Vector2(size),
                Color3.Red, Color3.Green, Color3.Blue, Color3.Yellow);
            var model0 = triBuilder.CreateModelEntity(Matrix4.CreateTranslation(center * -2));
            triBuilder.Clear();
            triBuilder.RenderFlags &= ~(RenderFlags.Sprite | RenderFlags.SpriteNoPitch);
            triBuilder.AddOctaSphere(Vector3.Zero, size / 5, 4, true, Color3.Purple);
            //triBuilder.AddSphere(Vector3.Zero, size / 5, 16, 8, true, Color3.Purple);
            var model1 = triBuilder.CreateModelEntity();
            AddRootEntity(new RootEntity
            {
                Name = "Sprite",
                ChildEntities = new[] { model0, model1 },
                OriginalLocalMatrix = Matrix4.CreateRotationY(45f * GeomMath.Deg2Rad) * Matrix4.CreateTranslation(0f, 70f, -30f),
            });
        }

        #endregion


        #region Helper Types

        private enum MouseEventType
        {
            Down,
            Up,
            Move,
            Wheel,
        }

        private enum KeyEventType
        {
            Down,
            Up,
        }

        private enum EntitySelectionSource
        {
            None,
            TreeView,
            Click,
        }

        private class EntitiesTreeViewTagInfo
        {
            public EntityBase Entity { get; set; }
            public int RootIndex { get; set; }
            public int ChildIndex { get; set; } = -1;
            public bool IsRoot => ChildIndex == -1;
            public bool LazyLoaded { get; set; } // True if child nodes have been added
        }

        private class AnimationsTreeViewTagInfo
        {
            public Animation Animation { get; set; }
            public AnimationObject AnimationObject { get; set; }
            public AnimationFrame AnimationFrame { get; set; }
            public int RootIndex { get; set; }
            public int ChildIndex { get; set; } = -1;
            public bool IsRoot => ChildIndex == -1;
            public bool LazyLoaded { get; set; } // True if child nodes have been added
        }

        private class TexturesListViewTagInfo
        {
            //public Texture Texture { get; set; }
            // We need to store index because ImageListViewItem.Index does not represent the original index.
            public int Index { get; set; }
            public bool Found { get; set; } // Search results flag
        }

        private class TexturesListViewGrouper : ImageListView.IGrouper
        {
            public ImageListView.GroupInfo GetGroupInfo(ImageListViewItem item)
            {
                var tagInfo = (TexturesListViewTagInfo)item.Tag;
                var name = tagInfo.Found ? "Found" : "Textures"; // Name ID of group
                var order = tagInfo.Found ? 0 : 1;               // Index of group
                return new ImageListView.GroupInfo(name, order);
            }
        }

        // We need a custom adaptor to assign the image associated with the ImageListViewItem.
        // The default adaptor assumes the image is in the filesystem (which it's not),
        // and there's no way to refresh the image thumbnail if we don't use this class.
        private class TexturesListViewItemAdaptor : ImageListView.ImageListViewItemAdaptor
        {
            private static readonly Utility.Tuple<ColumnType, string, object>[] EmptyDetails = new Utility.Tuple<ColumnType, string, object>[0];

            private PreviewForm _previewForm;

            public TexturesListViewItemAdaptor(PreviewForm previewForm)
            {
                _previewForm = previewForm;
            }

            public override void Dispose()
            {
                _previewForm = null;
            }

            public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
            {
                var index = (int)key;
                return _previewForm._textures[index].Bitmap;
            }

            public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
            {
                var index = (int)key;
                return index.ToString();
            }

            public override string GetSourceImage(object key)
            {
                // This is just asking for the source filename, we don't have anything like that, but we can safely return null.
                return null;
            }

            public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key)
            {
                /*var index = (int)key;
                var texture = _previewForm._textures[index];
                var details = new Utility.Tuple<ColumnType, string, object>[]
                {
                    new Utility.Tuple<ColumnType, string, object>(ColumnType.Dimensions, string.Empty, new Size(texture.Width, texture.Height)),
                    new Utility.Tuple<ColumnType, string, object>(ColumnType.Custom, "TexturePage", texture.TexturePage),
                };
                return details;*/
                return EmptyDetails; // We're not displaying details columns
            }
        }

        #endregion
    }
}
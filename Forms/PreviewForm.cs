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
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Exporters;
using PSXPrev.Common.Renderer;
using PSXPrev.Forms;
using PSXPrev.Forms.Dialogs;
using PSXPrev.Forms.Utils;
using Color = System.Drawing.Color;
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

        private static readonly Pen Black3Px = new Pen(Color.Black, 3f);
        private static readonly Pen White1Px = new Pen(Color.White, 1f);
        private static readonly Pen Cyan1Px = new Pen(Color.Cyan, 1f);


        private readonly List<RootEntity> _rootEntities = new List<RootEntity>();
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly List<Texture> _packedTextures = new List<Texture>();
        private readonly List<Animation> _animations = new List<Animation>();
        private readonly Scene _scene;
        private readonly VRAM _vram;
        private readonly AnimationBatch _animationBatch;
        private GLControl _openTkControl;

        // Form timers
        private Timer _mainTimer; // Main timer used by all timed events
        private Stopwatch _mainWatch; // Watch to track elapsed time between main timer events
        private bool _fixedTimer = false; // If true, then timer always updates with the same time delta
        // Timers that are updated during main timer Elapsed event
        private RefreshDelayTimer _animationProgressBarRefreshDelayTimer;
        private RefreshDelayTimer _modelPropertyGridRefreshDelayTimer;
        private RefreshDelayTimer _scanProgressRefreshDelayTimer;
        private RefreshDelayTimer _scanPopulateRefreshDelayTimer;
        private float _fps = (float)(1d / DefaultElapsedTime);
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
        private bool _showUv;
        private int _vramSelectedPage = -1; // Used because combo box SelectedIndex can be -1 while typing.
        private Texture _texturePreviewImage;
        private TexturesListViewItemAdaptor _texturesListViewAdaptor;
        private Bitmap _maskColorBitmap;
        private Bitmap _ambientColorBitmap;
        private Bitmap _backgroundColorBitmap;
        private Bitmap _wireframeVerticesColorBitmap;
        private int _clutIndex;
        private float _texturePreviewScale = 1f;
        private float _vramPageScale = 1f;
        private bool _autoDrawModelTextures;
        private bool _autoPackModelTextures;
        private bool _autoSelectAnimationModel;
        private bool _autoPlayAnimations;
        private SubModelVisibility _subModelVisibility;
        private bool _autoFocusOnRootModel;
        private bool _autoFocusOnSubModel;
        private bool _autoFocusIncludeWholeModel;
        private bool _autoFocusIncludeCheckedModels;
        private bool _showTexturePalette;
        private bool _showTextureSemiTransparency;
        private bool _boundsEntityPicking;
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
            _animationBatch = new AnimationBatch(_scene);
            _texturesListViewAdaptor = new TexturesListViewItemAdaptor(this);
            Toolkit.Init();
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
                    animationPlayButtonx.Text = value ? "Pause Animation" : "Play Animation";
                    animationPlayButtonx.Refresh();
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
                Program.Logger.ReadSettings(Settings.Instance);
            }
            Program.ConsoleLogger.ReadSettings(Settings.Instance);

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
            SetSubModelVisibility(settings.SubModelVisibility, force: true);
            autoFocusOnRootModelToolStripMenuItem.Checked = settings.AutoFocusOnRootModel;
            autoFocusOnSubModelToolStripMenuItem.Checked = settings.AutoFocusOnSubModel;
            autoFocusIncludeWholeModelToolStripMenuItem.Checked = settings.AutoFocusIncludeWholeModel;
            autoFocusIncludeCheckedModelsToolStripMenuItem.Checked = settings.AutoFocusIncludeCheckedModels;
            showBoundsToolStripMenuItem.Checked = settings.ShowBounds;
            _scene.ShowLightRotationRay = settings.ShowLightRotationRay;
            _scene.ShowDebugVisuals = settings.ShowDebugVisuals;
            _scene.ShowDebugPickingRay = settings.ShowDebugPickingRay;
            _scene.ShowDebugIntersections = settings.ShowDebugIntersections;
            SetBackgroundColor(settings.BackgroundColor);
            SetAmbientColor(settings.AmbientColor);
            SetMaskColor(settings.MaskColor);
            SetSolidWireframeVerticesColor(settings.SolidWireframeVerticesColor);
            SetCurrentCLUTIndex(settings.CurrentCLUTIndex);
            showUVToolStripMenuItem.Checked = settings.ShowUVsInVRAM;
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
            Program.Logger.WriteSettings(Settings.Instance);

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
            settings.SubModelVisibility = _subModelVisibility;
            settings.AutoFocusOnRootModel = autoFocusOnRootModelToolStripMenuItem.Checked;
            settings.AutoFocusOnSubModel = autoFocusOnSubModelToolStripMenuItem.Checked;
            settings.AutoFocusIncludeWholeModel = autoFocusIncludeWholeModelToolStripMenuItem.Checked;
            settings.AutoFocusIncludeCheckedModels = autoFocusIncludeCheckedModelsToolStripMenuItem.Checked;
            settings.ShowBounds = showBoundsToolStripMenuItem.Checked;
            settings.ShowLightRotationRay = _scene.ShowLightRotationRay;
            settings.ShowDebugVisuals = _scene.ShowDebugVisuals;
            settings.ShowDebugPickingRay = _scene.ShowDebugPickingRay;
            settings.ShowDebugIntersections = _scene.ShowDebugIntersections;
            settings.BackgroundColor =  _scene.ClearColor;
            settings.AmbientColor = _scene.AmbientColor;
            settings.MaskColor = _scene.MaskColor;
            settings.SolidWireframeVerticesColor = _scene.SolidWireframeVerticesColor;
            settings.CurrentCLUTIndex = _clutIndex;
            settings.ShowUVsInVRAM = showUVToolStripMenuItem.Checked;
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
                var graphicsMode = new OpenTK.Graphics.GraphicsMode(32, 24, 0, samples);
                _openTkControl = new GLControl(graphicsMode);
            }
            catch
            {
                // Don't know if an unsupported graphics mode can throw, but let's play it safe.
                _openTkControl = new GLControl();
            }
            _openTkControl.Name = "openTKControl";
            _openTkControl.TabIndex = 15;
            _openTkControl.BackColor = Color.Black;
            _openTkControl.BorderStyle = BorderStyle.FixedSingle;
            _openTkControl.Dock = DockStyle.Fill;
            _openTkControl.Margin = Padding.Empty;
            _openTkControl.Parent = modelsSplitContainer.Panel2;
            _openTkControl.VSync = true;

            _openTkControl.Load += openTKControl_Load;
            _openTkControl.MouseDown += (sender, e) => openTkControl_MouseEvent(e, MouseEventType.Down);
            _openTkControl.MouseUp += (sender, e) => openTkControl_MouseEvent(e, MouseEventType.Up);
            _openTkControl.MouseWheel += (sender, e) => openTkControl_MouseEvent(e, MouseEventType.Wheel);
            _openTkControl.MouseMove += (sender, e) => openTkControl_MouseEvent(e, MouseEventType.Move);
            _openTkControl.Paint += _openTkControl_Paint;
            _openTkControl.Resize += _openTkControl_Resize;


            // Setup Timers
            // Don't start watch until first Elapsed event (and use a default time for that first event)
            // Don't start timer until the Form is loaded
            _mainWatch = new Stopwatch();
            _mainTimer = new Timer(1d); // 1 millisecond, update as fast as possible (usually ~60FPS)
            _mainTimer.SynchronizingObject = this;
            _mainTimer.Elapsed += _mainTimer_Elapsed;

            _animationProgressBarRefreshDelayTimer = new RefreshDelayTimer(1d / 60d); // 1 frame (60FPS)
            _animationProgressBarRefreshDelayTimer.Elapsed += () => UpdateAnimationProgressLabel(true);

            _modelPropertyGridRefreshDelayTimer = new RefreshDelayTimer(50d / 1000d); // 50 milliseconds
            _modelPropertyGridRefreshDelayTimer.Elapsed += () => UpdateModelPropertyGrid(true);

            _scanProgressRefreshDelayTimer = new RefreshDelayTimer(); // Interval assigned by settings
            _scanProgressRefreshDelayTimer.AutoReset = true;
            _scanProgressRefreshDelayTimer.Elapsed += () => ScanUpdated();

            _scanPopulateRefreshDelayTimer = new RefreshDelayTimer(); // Interval assigned by settings
            _scanPopulateRefreshDelayTimer.AutoReset = true;
            _scanPopulateRefreshDelayTimer.Elapsed += () => ScanPopulateItems();


            // Setup Events
            // Add events that are marked as unbrowsable, and don't show up in the designer.
            texturePanel.MouseWheel += TexturePanelOnMouseWheel;
            vramPanel.MouseWheel += VramPanelOnMouseWheel;

            // Allow changing multiple checkboxes while holding shift down.
            drawModeToolStripMenuItem.DropDown.Closing += OnCancelMenuCloseWhileHoldingShift;
            autoFocusToolStripMenuItem.DropDown.Closing += OnCancelMenuCloseWhileHoldingShift;

            // Debug information helpers
#if DEBUG
            texturePreviewPictureBox.MouseMove += OnTexturePreviewPictureBoxMouseMove;
            vramPagePictureBox.MouseMove += OnVramPagePictureBoxMouseMove;
#endif

            // Ensure numeric up downs display the same value that they store internally.
            SetupNumericUpDownValidateEvents(this);

            // Normally clicking a menu separator will close the menu, which doesn't follow standard UI patterns.
            SetupCancelMenuCloseOnSeparatorClick(mainMenuStrip);


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


            // Setup MainMenuStrip
            // Use this renderer because ToolStripSystemRenderer (the default) is garbage.
            // This one also handles checkmarks with images correctly.
            mainMenuStrip.Renderer = new ToolStripProfessionalRenderer();


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

        private void ClearScanResults()
        {
            if (!Program.IsScanning)
            {
                Program.ClearResults();

                // Clear selections
                SelectEntity(null);
                UnselectTriangle();
                _selectedTriangle = null;
                _selectedModelEntity = null;
                _selectedRootEntity = null;
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
                _texturePreviewImage = null;
                texturePreviewPictureBox.Image = null;
                vramPagePictureBox.Image = null;
                vramPagePictureBox.Invalidate();

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

                TMDBindingsForm.CloseTool();

                GC.Collect(); // It's my memory and I need it now!
            }
        }

        private void Redraw()
        {
            _openTkControl.Invalidate();
        }

        private static string GetVersionString()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.FileVersion;
        }

        private void UpdateFPSLabel()
        {
            if (showFPSToolStripMenuItem.Checked)
            {
                Text = $"{_baseWindowTitle} (FPS: {_fps:0.0})";
            }
            else
            {
                Text = _baseWindowTitle;
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
                    switch (menusTabControl.SelectedTab.TabIndex)
                    {
                        case ModelsTabIndex: // Models
                        case AnimationsTabIndex: // Animations
                            _openTkControl.Focus();
                            break;
                        case TexturesTabIndex: // Textures
                            texturePreviewPictureBox.Focus();
                            break;
                        case VRAMTabIndex: // VRAM
                            vramPagePictureBox.Focus();
                            break;
                    }
                    return true; // Enter key handled
                }
            }
            
#if ENABLE_CLIPBOARD
            if (keyData == (Keys.Control | Keys.C))
            {
                var copied = false;
                if (_openTkControl.Focused)
                {
                    var width  = _openTkControl.ClientSize.Width;
                    var height = _openTkControl.ClientSize.Height;
                    var rect = new Rectangle(0, 0, width, height);
                    using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        var bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        try
                        {
                            OpenTK.Graphics.OpenGL.GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, bmpData.Scan0);
                        }
                        finally
                        {
                            bitmap.UnlockBits(bmpData);
                        }
                        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        Clipboard.SetImage(bitmap);
                    }
                }
                else if (menusTabControl.SelectedIndex == TexturesTabIndex && !texturePropertyGrid.Focused)
                {
                    var width  = texturePreviewPictureBox.Width;
                    var height = texturePreviewPictureBox.Height;
                    var rect = new Rectangle(0, 0, width, height);
                    using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        texturePreviewPictureBox.DrawToBitmap(bitmap, rect);
                        Clipboard.SetImage(bitmap);
                    }
                }
                else if (menusTabControl.SelectedIndex == VRAMTabIndex)
                {
                    var width  = vramPagePictureBox.Width;
                    var height = vramPagePictureBox.Height;
                    var rect = new Rectangle(0, 0, width, height);
                    using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        vramPagePictureBox.DrawToBitmap(bitmap, rect);
                        Clipboard.SetImage(bitmap);
                    }
                }
                Program.ConsoleLogger.WriteLine("Copied to clipboard");
            }
#endif
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void previewForm_Load(object sender, EventArgs e)
        {
            // Read and apply settings or default settings
            ReadSettings(Settings.Instance);

            // Start timers that should always be running
            _mainTimer.Start();
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
            // Handle leaving the animation tab.
            if (_inAnimationTab && menusTabControl.SelectedTab.TabIndex != AnimationsTabIndex)
            {
                _inAnimationTab = false;
                // Restart to force animation state update next time we're in the animation tab.
                // Reset animation when leaving the tab.
                _animationBatch.Restart();
                Playing = false;
                UpdateAnimationProgressLabel();
                // Update selected entity to invalidate the animation changes to the model.
                UpdateSelectedEntity();
            }
            // Force-hide all visuals when in animation tab.
            _scene.ShowVisuals = menusTabControl.SelectedTab.TabIndex != AnimationsTabIndex;

            switch (menusTabControl.SelectedTab.TabIndex)
            {
                case ModelsTabIndex: // Models
                    {
                        animationsTreeView.SelectedNode = null;
                        _openTkControl.Parent = modelsSplitContainer.Panel2;
                        _openTkControl.Show();
                        break;
                    }
                case VRAMTabIndex: // VRAM
                    {
                        UpdateVRAMComboBoxPageItems();
                        goto default;
                    }
                case AnimationsTabIndex: // Animations
                    {
                        _inAnimationTab = true;
                        animationsTableLayoutPanel.Controls.Add(_openTkControl, 0, 0);
                        _openTkControl.Show();
                        UpdateSelectedAnimation();
                        break;
                    }
                default:
                    {
                        _openTkControl.Parent = null;
                        _openTkControl.Hide();
                        break;
                    }
            }

            menusTabControl.Refresh(); // Refresh so that controls don't take an undetermined amount of time to render
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
                var sceneFocused = _openTkControl.Focused;
                var vramTab = menusTabControl.SelectedIndex == VRAMTabIndex;
                var textureTab = menusTabControl.SelectedIndex == TexturesTabIndex;
                var vramFocused = vramTab && (vramPagePictureBox.Focused || vramListBox.Focused);
                var textureFocused = textureTab && (texturePreviewPictureBox.Focused || texturesListView.Focused);

                switch (e.KeyCode)
                {
                    // Press space to focus on the currently-selected model
                    case Keys.Space when state && sceneFocused:
                        if (_selectedRootEntity != null || _selectedModelEntity != null)
                        {
                            _scene.FocusOnBounds(GetFocusBounds(GetCheckedEntities()));
                            e.Handled = true;
                        }
                        break;

                    case Keys.B when state && sceneFocused:
                        if (!IsControlDown)
                        {
                            _boundsEntityPicking = !_boundsEntityPicking;
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"_boundsEntityPicking: {_boundsEntityPicking}");
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
                            _showTexturePalette = !_showTexturePalette;
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"_showTexturePalette: {_showTexturePalette}");
                            UpdateTexturePreviewSize();
                            texturePreviewPictureBox.Invalidate();
                            e.Handled = true;
                        }
                        break;

                    case Keys.T when state && (textureFocused || vramFocused):
                        if (!IsControlDown)
                        {
                            _showTextureSemiTransparency = !_showTextureSemiTransparency;
                            Program.ConsoleLogger.WriteColorLine(ConsoleColor.Magenta, $"_showTextureSemiTransparency: {_showTextureSemiTransparency}");
                            texturePreviewPictureBox.Invalidate();
                            vramPagePictureBox.Invalidate();
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
                                var rootEntity = _selectedRootEntity ?? _selectedModelEntity?.GetRootEntity();
                                if (IsTriangleSelectMode())
                                {
                                    _scene.GetTriangleUnderMouse(checkedEntities, rootEntity, _lastMouseX, _lastMouseY);
                                }
                                else
                                {
                                    _scene.GetEntityUnderMouse(checkedEntities, rootEntity, _lastMouseX, _lastMouseY);
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

        private void openTKControl_Load(object sender, EventArgs e)
        {
            // Setup classes that depend on OpenTK.
            var width  = Math.Max(1, _openTkControl.ClientSize.Width);
            var height = Math.Max(1, _openTkControl.ClientSize.Height);
            _openTkControl.MakeCurrent();
            _scene.Initialize(width, height);
            _vram.Initialize();
        }

        private void _openTkControl_Resize(object sender, EventArgs e)
        {
            // Make sure to use ClientSize to exclude size added by the borders.
            var width  = Math.Max(1, _openTkControl.ClientSize.Width);
            var height = Math.Max(1, _openTkControl.ClientSize.Height);
            if (_scene.Initialized)
            {
                _openTkControl.MakeCurrent();
                _scene.Resize(width, height);
            }
        }

        private void _openTkControl_Paint(object sender, PaintEventArgs e)
        {
            _openTkControl.MakeCurrent();
            if (_inAnimationTab && _curAnimation != null)
            {
                var checkedEntities = GetCheckedEntities();
                if (_animationBatch.SetupAnimationFrame(checkedEntities, _selectedRootEntity, _selectedModelEntity, true))
                {
                    // Animation has been processed. Update attached limbs while animating.
                    var rootEntity = _selectedRootEntity ?? _selectedModelEntity?.GetRootEntity();
                    if (rootEntity != null)
                    {
                        if (_scene.AutoAttach)
                        {
                            rootEntity.FixConnections();
                        }
                        else
                        {
                            rootEntity.UnfixConnections();
                        }
                    }
                }
            }
            _scene.Draw();
            _openTkControl.SwapBuffers();
        }

        private void openTkControl_MouseEvent(MouseEventArgs e, MouseEventType eventType)
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
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
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
                            var rootEntity = _selectedRootEntity ?? _selectedModelEntity?.GetRootEntity();
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
                                var newSelectedEntity = _scene.GetEntityUnderMouse(checkedEntities, rootEntity, e.X, e.Y);
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

        private void _mainTimer_Elapsed(object sender, ElapsedEventArgs e)
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
            }
            else
            {
                // Get elapsed time
                var deltaSeconds = _mainWatch.IsRunning ? _mainWatch.Elapsed.TotalSeconds : DefaultElapsedTime;
                _mainWatch.Restart(); // Start or restart timer, use default time if timer wasn't running.

                // Don't skip too much time if we've fallen too far behind.
                var renderSeconds = _fixedTimer ? DefaultElapsedTime : Math.Min(deltaSeconds, MaxElapsedTime);


                // Update FPS tracker
                // Source: <http://www.david-amador.com/2009/11/how-to-do-a-xna-fps-counter/>
                _fpsCalcElapsedSeconds += deltaSeconds;
                if (_fpsCalcElapsedSeconds >= 1d) // Update FPS every one second
                {
                    _fps = (float)(_fpsCalcElapsedFrames / _fpsCalcElapsedSeconds);
                    _fpsCalcElapsedSeconds = 0d;
                    _fpsCalcElapsedFrames = 0;
                    //Console.WriteLine($"FPS: {_fps:0.00}");
                    // todo: Update a label or something here
                    if (showFPSToolStripMenuItem.Checked)
                    {
                        UpdateFPSLabel();
                    }
                }
                _fpsCalcElapsedFrames++;


                // Update delayed control refreshes
                _animationProgressBarRefreshDelayTimer.AddTime(deltaSeconds);
                _modelPropertyGridRefreshDelayTimer.AddTime(deltaSeconds);
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
                if (_openTkControl.Parent != null)
                {
                    _scene.AddTime(renderSeconds);
                    Redraw();
                }
            }
        }

        #endregion

        #region GetChecked/AddResults

        private RootEntity[] GetCheckedEntities(bool defaultToSelected = false)
        {
            var selectedEntities = new List<RootEntity>();
            for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
            {
                var node = entitiesTreeView.Nodes[i];
                if (node.Checked)
                {
                    var tagInfo = (EntitiesTreeViewTagInfo)node.Tag;
                    if (tagInfo.Entity is RootEntity rootEnity)
                    {
                        selectedEntities.Add(rootEnity);
                    }
                }
            }
            if (selectedEntities.Count == 0 && defaultToSelected)
            {
                var selectedRootEntity = _selectedRootEntity ?? _selectedModelEntity?.GetRootEntity();
                if (selectedRootEntity != null)
                {
                    selectedEntities.Add(selectedRootEntity);
                }
            }
            return selectedEntities.Count == 0 ? null : selectedEntities.ToArray();
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
            var selectedItems = texturesListView.SelectedItems;
            var textures = new Texture[selectedItems.Count];
            for (var i = 0; i < selectedItems.Count; i++)
            {
                var tagInfo = (TexturesListViewTagInfo)selectedItems[i].Tag;
                textures[i] = _textures[tagInfo.Index];
            }
            return textures;
        }

        private Texture GetSelectedTexture()
        {
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
            var rootEntityNode = new TreeNode(rootEntity.EntityName)
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
            var modelNode = new TreeNode(modelEntity.EntityName)
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
        }

        private void EntitiesAdded(IReadOnlyList<RootEntity> rootEntities, int startIndex)
        {
            var rootEntityNodes = new TreeNode[rootEntities.Count];
            for (var i = 0; i < rootEntities.Count; i++)
            {
                rootEntityNodes[i] = CreateRootEntityNode(rootEntities[i], startIndex + i);
            }
            entitiesTreeView.Nodes.AddRange(rootEntityNodes);

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

        private ImageListViewItem CreateTextureItem(Texture texture, int index)
        {
            object key = index;
            var textureItem = new ImageListViewItem(key)
            {
                //Text = index.ToString(), //debug
                Text = texture.TextureName,
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
            var animationNode = new TreeNode(animation.AnimationName)
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
            var animationObjectNode = new TreeNode("Animation-Object " + childIndex) // 0-indexed like Sub-Models
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
        }

        private void AnimationsAdded(IReadOnlyList<Animation> animations, int startIndex)
        {
            var animationNodes = new TreeNode[animations.Count];
            for (var i = 0; i < animations.Count; i++)
            {
                animationNodes[i] = CreateAnimationNode(animations[i], startIndex + i);
            }
            animationsTreeView.Nodes.AddRange(animationNodes);
        }

        private void AnimationAdded(Animation animation, int index)
        {
            animationsTreeView.Nodes.Add(CreateAnimationNode(animation, index));
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
                var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;

                _scene.UpdatePicking(_lastMouseX, _lastMouseY);
                var hoveredGizmo = _scene.GetGizmoUnderPosition(selectedEntityBase, _gizmoType);

                UpdateGizmoVisualAndState(_selectedGizmo, hoveredGizmo);
            }
        }

        private void UpdateGizmoVisualAndState(GizmoId selectedGizmo, GizmoId hoveredGizmo)
        {
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;

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
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;

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
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;

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

            UpdateSelectedEntity(false, noDelayUpdatePropertyGrid: false); // Delay updating property grid to reduce lag
        }

        private void FinishGizmoAction()
        {
            if (_selectedGizmo == GizmoId.None)
            {
                return;
            }
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;

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
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;

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
            UpdateSelectedEntity(false);
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
                UpdateSelectedEntity(false);
            }
        }

        private void AlignSelectedEntityGizmoRotation(EntityBase selectedEntityBase)
        {
            if (selectedEntityBase != null)
            {
                var angle = SnapAngle(_gizmoRotateAngle); // Grid size is in units of 1 degree.
                var newRotation = _gizmoInitialRotation * Quaternion.FromAxisAngle(_gizmoAxis, angle);
                selectedEntityBase.Rotation = newRotation;
                UpdateSelectedEntity(false);
            }
        }

        private void AlignSelectedEntityScale(EntityBase selectedEntityBase)
        {
            if (selectedEntityBase != null)
            {
                selectedEntityBase.Scale = SnapScale(selectedEntityBase.Scale);
                UpdateSelectedEntity(false);
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

            if (entities == null || entities.Length == 0)
            {
                var message = all ? "No models to export" : "No models checked or selected to export";
                ShowMessageBox(message, "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var animations = GetCheckedAnimations(true);

            EnterDialog();
            try
            {
                var options = ExportModelsForm.Show(this, entities, animations, _animationBatch);
                if (options != null)
                {
                    ExportModelsForm.Export(options, entities, animations, _animationBatch);
                    ShowMessageBox($"{entities.Length} models exported", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                // Fancy method uses a bitmap with a shadow, and draws a 14x14 rectangle,
                // with a darkened outline, and the solid color within a 12x12 rectangle.
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

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
                using (var borderPen = new Pen(borderColor))
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
            _openTkControl.BackColor = _scene.ClearColor;
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
                // Fix it so that when a triangle is unselected, the cached list
                // (for selecting each triangle under the mouse) is reset.
                // Otherwise we end up picking the next triangle when the user isn't expecting it.
                if (_selectedTriangle == null)
                {
                    _scene.ClearTriangleUnderMouseList();
                }
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

        private void UpdateSelectedEntity(bool updateMeshData = true, bool noDelayUpdatePropertyGrid = true, bool focus = false)
        {
            _scene.BoundsBatch.Reset(1);
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            var rootEntity = selectedEntityBase?.GetRootEntity();
            if (rootEntity != null)
            {
                rootEntity.ResetAnimationData();
                if (_scene.AutoAttach)
                {
                    rootEntity.FixConnections();
                }
                else
                {
                    rootEntity.UnfixConnections();
                }
            }
            if (selectedEntityBase != null)
            {
                selectedEntityBase.ComputeBoundsRecursively();
                _scene.BoundsBatch.BindEntityBounds(selectedEntityBase);

                var checkedEntities = GetCheckedEntities();

                if (updateMeshData)
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

                updateMeshData |= _scene.AutoAttach;
                _scene.MeshBatch.SetupMultipleEntityBatch(checkedEntities, _selectedModelEntity, _selectedRootEntity, updateMeshData, subModelVisibility: _subModelVisibility);

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
                        _scene.FocusOnBounds(GetFocusBounds(checkedEntities));
                    }
                }
            }
            else
            {
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
                _scene.TriangleOutlineBatch.BindTriangleOutline(_selectedTriangle.Item1.WorldMatrix, _selectedTriangle.Item2);
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

        private void SetSubModelVisibility(SubModelVisibility visibility, bool force = false)
        {
            if (force || _subModelVisibility != visibility)
            {
                _subModelVisibility = visibility;

                subModelVisibilityAllToolStripMenuItem.Checked = _subModelVisibility == SubModelVisibility.All;
                subModelVisibilitySelectedToolStripMenuItem.Checked = _subModelVisibility == SubModelVisibility.Selected;
                subModelVisibilityWithSameTMDIDToolStripMenuItem.Checked = _subModelVisibility == SubModelVisibility.WithSameTMDID;

                UpdateSelectedEntity();
            }
        }

        private BoundingBox GetFocusBounds(RootEntity[] checkedEntities)
        {
            var bounds = new BoundingBox();
            var selectedRootEntity = _selectedRootEntity ?? _selectedModelEntity.GetRootEntity();

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
                        if (model.Triangles.Length > 0 && (!model.AttachedOnly || model.IsAttached))
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
            var rootEntity = _selectedRootEntity ?? _selectedModelEntity?.GetRootEntity();
            UpdateSelectedEntity(focus: _selectionSource == EntitySelectionSource.TreeView);
        }

        private void entitiesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (!_busyChecking)
            {
                UpdateSelectedEntity(focus: _autoFocusIncludeCheckedModels);
            }
        }

        private void entitiesTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // handle unselecting triangle when clicking on a node in the tree view if that node is already selected.
            if (e.Node != null)
            {
                // Removed for now, because this also triggers when pressing
                // the expand button (which doesn't perform selection).
                //UnselectTriangle();
            }
        }

        private void modelPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }
            if (_selectedModelEntity != null)
            {
                _vram.AssignModelTextures(_selectedModelEntity);
            }
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            if (selectedEntityBase != null)
            {
                selectedNode.Text = selectedEntityBase.EntityName;
                selectedEntityBase.Translation = SnapToGrid(selectedEntityBase.Translation);
            }
            UpdateSelectedEntity(false);
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
        }

        private void resetWholeModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            if (selectedEntityBase != null)
            {
                // This could be changed to only reset the selected model and its children.
                // But that's only necessary if sub-sub-model support is ever added.
                selectedEntityBase.GetRootEntity()?.ResetTransform(true);
                UpdateSelectedEntity();
            }
        }

        private void resetSelectedModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            if (selectedEntityBase != null)
            {
                selectedEntityBase.ResetTransform(false);
                UpdateSelectedEntity();
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
        }

        private void showBoundsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.ShowBounds = showBoundsToolStripMenuItem.Checked;
            Redraw();
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
            _scene.AutoAttach = autoAttachLimbsToolStripMenuItem.Checked;
            UpdateSelectedEntity(); // Update mesh data, since limb vertices may have changed
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

        private void lineRendererToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.VibRibbonWireframe = lineRendererToolStripMenuItem.Checked;
            UpdateSelectedEntity(); // Update mesh data, since vib ribbon redefines how mesh data is built.
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
                    texture.SetCLUTIndex(clutIndex.Value);
                }
            }
            if (_vram.UpdateAllPages()) // True if any pages needed to be updated (aka textures wasn't empty)
            {
                vramPagePictureBox.Invalidate(); // Invalidate to make sure we redraw.
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
            vramPagePictureBox.Invalidate(); // Invalidate to make sure we redraw.
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
            vramPagePictureBox.Invalidate(); // Invalidate to make sure we redraw.
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
            vramPagePictureBox.Invalidate(); // Invalidate to make sure we redraw.
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
            texturePreviewPictureBox.Invalidate();
            texturePropertyGrid.SelectedObject = texturePropertyGrid.SelectedObject;
        }

        private void UpdateTexturePreviewSize()
        {
            var texture = _texturePreviewImage;
            if (texture == null)
            {
                texturePreviewPictureBox.Width  = 1;
                texturePreviewPictureBox.Height = 1;
                return;
            }
            var width  = texture.Width;
            var height = texture.Height;
            if (_showTexturePalette && texture.Palettes != null)
            {
                var units = texture.Bpp == 4 ? 4 : 16;
                width = height = units * 8;
            }
            texturePreviewPictureBox.Width  = (int)(width  * _texturePreviewScale);
            texturePreviewPictureBox.Height = (int)(height * _texturePreviewScale);
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

        private void DrawUVLines(Graphics graphics, Pen pen, Vector2[] uvs, IUVConverter uvConverter)
        {
            var scalar = GeomMath.UVScalar * _vramPageScale;
            var uvLast = uvs[uvs.Length - 1];
            if (uvConverter != null)
            {
                uvLast = uvConverter.ConvertUV(uvLast, false);
            }
            for (var i = 0; i < uvs.Length; i++)
            {
                var uv = uvs[i];
                if (uvConverter != null)
                {
                    uv = uvConverter.ConvertUV(uv, false);
                }
                graphics.DrawLine(pen, uvLast.X * scalar, uvLast.Y * scalar, uv.X * scalar, uv.Y * scalar);
                uvLast = uv;
            }
        }

        private void DrawTiledUVRectangle(Graphics graphics, Pen pen, TiledUV tiledUv, IUVConverter uvConverter)
        {
            var scalar = GeomMath.UVScalar * _vramPageScale;
            var tiledArea = tiledUv.Area;
            if (uvConverter != null)
            {
                tiledArea = uvConverter.ConvertTiledArea(tiledArea);
            }
            graphics.DrawRectangle(pen, tiledArea.X * scalar, tiledArea.Y * scalar, tiledArea.Z * scalar, tiledArea.W * scalar);
        }

        private void DrawUV(EntityBase entity, Graphics graphics)
        {
            if (entity == null)
            {
                return;
            }
            // Don't draw UVs for this model unless it uses the same texture page that we're on.
            if (entity is ModelEntity model && model.IsTextured && !model.MissingTexture && model.TexturePage == _vramSelectedPage)
            {
                var uvConverter = model.TextureLookup;

                // Draw all black outlines before inner fill lines, so that black outline edges don't overlap fill lines.
                foreach (var triangle in model.Triangles)
                {
                    if (triangle.IsTiled)
                    {
                        // Triangle.Uv is useless when tiled, so draw the TiledUv area instead.
                        DrawTiledUVRectangle(graphics, Black3Px, triangle.TiledUv, uvConverter);
                    }
                    else
                    {
                        DrawUVLines(graphics, Black3Px, triangle.Uv, uvConverter);
                    }
                }

                foreach (var triangle in model.Triangles)
                {
                    if (triangle.IsTiled)
                    {
                        // Different color for tiled area.
                        DrawTiledUVRectangle(graphics, Cyan1Px, triangle.TiledUv, uvConverter);
                    }
                    else
                    {
                        DrawUVLines(graphics, White1Px, triangle.Uv, uvConverter);
                    }
                }
            }
            if (entity.ChildEntities == null)
            {
                return;
            }
            foreach (var subEntity in entity.ChildEntities)
            {
                DrawUV(subEntity, graphics);
            }
        }

        private void DrawSemiTransparencyLine(Graphics graphics, float x, float y, float width, float height, Color backColor, bool transparent)
        {
            if (width < 2 || height < 2)
            {
                return; // Too small to draw
            }
            var penColor = Color.FromArgb((backColor.R + 128) % 255, (backColor.G + 128) % 255, (backColor.B + 128) % 255);
            using (var pen = new Pen(penColor, 1f))
            {
                if (transparent)
                {
                    graphics.DrawLine(pen, x + width, y, x, y + height);
                }
                else
                {
                    graphics.DrawLine(pen, x, y, x + width, y + height);
                }
            }
        }

        private void DrawAllSemiTransparencyLines(Graphics graphics, Texture texture, float scale, Color backColor)
        {
            if (scale <= 2.0f)
            {
                return; // Too small to draw
            }
            texture.Lock();
            try
            {
                var width  = texture.RenderWidth;
                var height = texture.RenderHeight;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var xx = (x * scale);
                        var yy = (y * scale);
                        var solidColor = texture.GetPixel(x, y, out var stp, out var paletteIndex);
                        var transparent = (!stp && solidColor.R == 0 && solidColor.G == 0 && solidColor.B == 0);
                        if (transparent)
                        {
                            solidColor = backColor;
                        }
                        if (transparent || stp)
                        {
                            DrawSemiTransparencyLine(graphics, xx, yy, scale, scale, solidColor, transparent);
                        }
                    }
                }
            }
            finally
            {
                texture.Unlock();
            }
        }

        private void texturePreviewPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_texturePreviewImage == null)
            {
                return;
            }
            var dstRect = new Rectangle(0, 0, texturePreviewPictureBox.Width, texturePreviewPictureBox.Height);
            var srcRect = new Rectangle(0, 0, _texturePreviewImage.Width, _texturePreviewImage.Height);

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            if (_showTexturePalette)
            {
                var texture = _texturePreviewImage;
                if (texture?.Palettes != null)
                {
                    var palette = texture.Palettes[texture.CLUTIndex];
                    var origPalette = texture.OriginalPalettes?[texture.CLUTIndex] ?? palette;
                    var units = texture.Bpp == 4 ? 4 : 16;
                    var index = 0;
                    var width  = Math.Max(1, texturePreviewPictureBox.Width  / units);
                    var height = Math.Max(1, texturePreviewPictureBox.Height / units);
                    for (var y = 0; y < units; y++)
                    {
                        for (var x = 0; x < units; x++, index++)
                        {
                            var color = palette[index];
                            var solidColor = TexturePalette.ToColor(origPalette[index], noTransparent: true);// color);
                            var xx = x * width;
                            var yy = y * height;
                            using (var brush = new SolidBrush(solidColor))
                            {
                                e.Graphics.FillRectangle(brush, new Rectangle(xx, yy, width, height));
                            }
                            var transparent = color == TexturePalette.Transparent;
                            if (transparent || TexturePalette.GetStp(color))
                            {
                                if (transparent && texture.OriginalPalettes == null)
                                {
                                    // We don't have the original unmasked color, default to showing behind the palette
                                    solidColor = texturePreviewPictureBox.BackColor;
                                }
                                DrawSemiTransparencyLine(e.Graphics, xx, yy, width, height, solidColor, transparent);
                            }
                        }
                    }
                }
            }
            else
            {
                // Despite what it sounds like, we want Half. Otherwise we end up drawing half a pixel back.
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(
                    _texturePreviewImage.Bitmap,
                    dstRect,
                    srcRect,
                    GraphicsUnit.Pixel);

                if (_showTextureSemiTransparency)
                {
                    DrawAllSemiTransparencyLines(e.Graphics, _texturePreviewImage, _texturePreviewScale,
                                                 texturePreviewPictureBox.BackColor);
                }
            }

            // Reset drawing mode back to default.
            e.Graphics.InterpolationMode = InterpolationMode.Default;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
        }

        private void vramPagePictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_vramSelectedPage == -1)
            {
                return;
            }
            var dstRect = new Rectangle(0, 0, vramPagePictureBox.Width, vramPagePictureBox.Height);
            var srcRect = new Rectangle(0, 0, VRAM.PageSize, VRAM.PageSize);

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            // Despite what it sounds like, we want Half. Otherwise we end up drawing half a pixel back.
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.DrawImage(
                _vram[_vramSelectedPage].Bitmap, //vramPagePictureBox.Image,
                dstRect,
                srcRect,
                GraphicsUnit.Pixel);

            if (_showTextureSemiTransparency)
            {
                DrawAllSemiTransparencyLines(e.Graphics, _vram[_vramSelectedPage], _vramPageScale,
                                             vramPagePictureBox.BackColor);
            }

            if (_showUv)
            {
                // todo: If we want smoother lines, then we can turn on Anti-aliasing.
                // Note that PixelOffsetMode needs to be changed back to None first.
                // Also note that diagonal lines will be a bit thicker than normal.
                e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var checkedEntities = GetCheckedEntities();
                if (checkedEntities != null)
                {
                    foreach (var checkedEntity in checkedEntities)
                    {
                        if (checkedEntity == _selectedRootEntity)
                        {
                            continue;
                        }
                        DrawUV(checkedEntity, e.Graphics);
                    }
                }
                DrawUV((EntityBase)_selectedRootEntity ?? _selectedModelEntity, e.Graphics);
            }

            // Reset drawing mode back to default.
            e.Graphics.InterpolationMode = InterpolationMode.Default;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
            e.Graphics.SmoothingMode = SmoothingMode.Default;
        }

        private void TexturePanelOnMouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
            if (e.Delta > 0)
            {
                _texturePreviewScale *= 2f;
            }
            else if (e.Delta < 0)
            {
                _texturePreviewScale /= 2f;
            }
            _texturePreviewScale = GeomMath.Clamp(_texturePreviewScale, 0.25f, 16.0f);
            texturesZoomLabel.Text = $"{_texturePreviewScale:P0}"; // Percent format
            UpdateTexturePreviewSize();
        }

        private void VramPanelOnMouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
            if (e.Delta > 0)
            {
                _vramPageScale *= 2f;
            }
            else if (e.Delta < 0)
            {
                _vramPageScale /= 2f;
            }
            _vramPageScale = GeomMath.Clamp(_vramPageScale, 0.25f, 16.0f);
            vramPagePictureBox.Width  = (int)(VRAM.PageSize * _vramPageScale);
            vramPagePictureBox.Height = (int)(VRAM.PageSize * _vramPageScale);
            vramZoomLabel.Text = $"{_vramPageScale:P0}"; // Percent format
        }

        private void OnTexturePreviewPictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (IsControlDown && _texturePreviewImage != null)
            {
                PrintPixelInTexture(_texturePreviewImage, _texturePreviewScale, e.X, e.Y);
            }
        }

        private void OnVramPagePictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (IsControlDown && _vramSelectedPage != -1)
            {
                PrintPixelInTexture(_vram[_vramSelectedPage], _vramPageScale, e.X, e.Y);
            }
        }

        private void PrintPixelInTexture(Texture texture, float scale, int mouseX, int mouseY)
        {
            var x = (int)(mouseX / scale);
            var y = (int)(mouseY / scale);
            if (x >= 0 && y >= 0 && x < texture.Width && y < texture.Height)
            {
                var pixel = texture.GetPixel(x, y, out var stp, out var paletteIndex);
                Program.ConsoleLogger.Write($"({x,3},{y,3}): r={pixel.R,3} g={pixel.G,3} b={pixel.B,3} a={pixel.A,3}");
                if (paletteIndex.HasValue)
                {
                    var paletteColor = texture.Palettes[texture.CLUTIndex][paletteIndex.Value];
                    Program.ConsoleLogger.Write($"  idx={paletteIndex,3} value=0x{paletteColor:x04}");
                }
                Program.ConsoleLogger.WriteLine();
            }
        }

        private void texturesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            //_texturePreviewScale = 1f;
            _texturePreviewImage = texture;
            // Uncomment this line if you want to restore the blur shadow that draws around textures when zoomed in.
            //texturePreviewPictureBox.Image = texture.Bitmap;
            UpdateTexturePreviewSize();
            texturePreviewPictureBox.Refresh();
            texturePropertyGrid.SelectedObject = texture;
        }

        private void texturePropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var selectedItems = texturesListView.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }
            var selectedItem = texturesListView.SelectedItems[0];
            if (selectedItem == null)
            {
                return;
            }
            var texture = GetSelectedTexture();
            if (texture == null)
            {
                return;
            }
            texture.CLUTIndex = GeomMath.Clamp(texture.CLUTIndex, 0, Math.Max(0, texture.CLUTCount - 1));
            texture.SetCLUTIndex(texture.CLUTIndex);
            // Validate changes to texture properties.
            texture.X = VRAM.ClampTextureX(texture.X);
            texture.Y = VRAM.ClampTextureY(texture.Y);
            texture.TexturePage = VRAM.ClampTexturePage(texture.TexturePage);
            // Update changes to TextureName property in ListViewItem.
            selectedItem.Text = texture.TextureName;
        }

        private void drawSelectedToVRAM_Click(object sender, EventArgs e)
        {
            var selectedItems = texturesListView.SelectedItems;
            if (selectedItems.Count == 0)
            {
                ShowMessageBox("Select textures to draw to VRAM first", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var packFailedCount = DrawTexturesToVRAM(GetSelectedTextures(), null); // Null to draw selected textures of any clut index
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
            if (index > -1 && index != _vramSelectedPage)
            {
                _vramSelectedPage = index;
                // We can't assign VRAM image because it would include the semi-transparency zone.
                // The image is drawn manually instead.
                //vramPagePictureBox.Image = _vram[_vramSelectedPage].Bitmap;
                vramPagePictureBox.Invalidate(); // Invalidate to make sure we redraw.
            }
        }

        private void gotoVRAMPageButton_Click(object sender, EventArgs e)
        {
            if (PromptVRAMPage("Go to VRAM Page", _vramSelectedPage, out var pageIndex))
            {
                _vramSelectedPage = pageIndex;
                // We can't assign VRAM image because it would include the semi-transparency zone.
                // The image is drawn manually instead.
                //vramPagePictureBox.Image = _vram[_vramSelectedPage].Bitmap;
                vramListBox.SelectedIndex = pageIndex;
                vramPagePictureBox.Invalidate(); // Invalidate to make sure we redraw.
            }
        }

        private void clearVRAMPage_Click(object sender, EventArgs e)
        {
            var index = _vramSelectedPage;
            if (index <= -1)
            {
                ShowMessageBox("Select a page first", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ClearVRAMPage(index);
            //ShowMessageBox("Page cleared", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void clearAllVRAMPages_Click(object sender, EventArgs e)
        {
            ClearAllVRAMPages();
            //ShowMessageBox("Pages cleared", "PSXPrev", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void showUVToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _showUv = showUVToolStripMenuItem.Checked;
            vramPagePictureBox.Refresh(); // Repaint to change shown UVs
        }


        #endregion

        #region Animations

        private void UpdateSelectedAnimation(bool play = false)
        {
            var propertyObject = _curAnimationFrame ?? _curAnimationObject ?? (object)_curAnimation;

            // Change Playing after Enabled, so that the call to Refresh in Playing will affect the enabled visual style too.
            animationPlayButtonx.Enabled = (_curAnimation != null);
            Playing = play;

            animationPropertyGrid.SelectedObject = propertyObject;
            _animationBatch.SetupAnimationBatch(_curAnimation);

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
            // Restart animation to force invalidate.
            _animationBatch.Restart();
            UpdateSelectedEntity();
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
            if (index > -1)
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

        private void showSideBarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var visible = showSideBarToolStripMenuItem.Checked;
            modelsSplitContainer.Panel1Collapsed = !visible;
            texturesSplitContainer.Panel1Collapsed = !visible;
            vramSplitContainer.Panel1Collapsed = !visible;
            animationsSplitContainer.Panel1Collapsed = !visible;
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
            var message = "PSXPrev - PlayStation (PSX) Files Previewer/Extractor\n" +
                          "\u00a9 PSXPrev Contributors - 2020-2023\n" +
                          $"Version {GetVersionString()}";
            ShowMessageBox(message, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // Helper class to delay refreshing controls to reduce lag
        private class RefreshDelayTimer
        {
            // Time is in seconds
            public bool NeedsRefresh { get; private set; }
            public double ElapsedSeconds { get; private set; }
            public double Interval { get; set; }
            public bool AutoReset { get; set; }

            public event Action Elapsed;

            public RefreshDelayTimer(double interval = 1d / 1000d)
            {
                Interval = interval;
            }

            // Start the timer but keep the current elapsed time
            public void Start()
            {
                NeedsRefresh = true;
            }

            // Stop the timer and reset the elapsed time
            public void Reset()
            {
                NeedsRefresh = false;
                ElapsedSeconds = 0d;
            }

            // Start the timer and reset the elapsed time
            public void Restart()
            {
                NeedsRefresh = true;
                ElapsedSeconds = 0d;
            }

            // Finish the timer and raise the event if NeedsRefresh is true
            public bool Finish() => AddTime(Interval);

            // Update the timer if NeedsRefresh is true, and raise the event if finished
            public bool AddTime(double seconds)
            {
                if (NeedsRefresh)
                {
                    ElapsedSeconds += seconds;
                    if (ElapsedSeconds >= Interval)
                    {
                        NeedsRefresh = AutoReset;
                        ElapsedSeconds = 0d;

                        Elapsed?.Invoke();
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion
    }
}
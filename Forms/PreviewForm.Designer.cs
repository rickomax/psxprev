using PSXPrev.Common;

namespace PSXPrev.Forms
{
    partial class PreviewForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Manina.Windows.Forms.ImageListView.ImageListViewColumnHeader imageListViewColumnHeader1 = new Manina.Windows.Forms.ImageListView.ImageListViewColumnHeader(Manina.Windows.Forms.ColumnType.Custom, "Found", "Found", 100, 0, true);
            Manina.Windows.Forms.ImageListView.ImageListViewColumnHeader imageListViewColumnHeader2 = new Manina.Windows.Forms.ImageListView.ImageListViewColumnHeader(Manina.Windows.Forms.ColumnType.Custom, "Textures", "Textures", 100, 1, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewForm));
            this.entitiesTabPage = new System.Windows.Forms.TabPage();
            this.modelsPreviewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.menusTabControl = new System.Windows.Forms.TabControl();
            this.texturesTabPage = new System.Windows.Forms.TabPage();
            this.texturesPreviewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.texturesSideSplitContainer = new System.Windows.Forms.SplitContainer();
            this.texturePropertyGridTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.drawToVRAMButton = new System.Windows.Forms.Button();
            this.texturePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.exportBitmapButton = new System.Windows.Forms.Button();
            this.vramTabPage = new System.Windows.Forms.TabPage();
            this.vramPreviewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.vramSideSplitContainer = new System.Windows.Forms.SplitContainer();
            this.vramListBox = new System.Windows.Forms.ListBox();
            this.vramButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.vramGotoPageButton = new System.Windows.Forms.Button();
            this.vramClearPageButton = new System.Windows.Forms.Button();
            this.animationsTabPage = new System.Windows.Forms.TabPage();
            this.animationsPreviewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.animationGroupBoxMarginPanel = new System.Windows.Forms.Panel();
            this.animationGroupBox = new System.Windows.Forms.GroupBox();
            this.animationControlsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.animationProgressNameLabel = new System.Windows.Forms.Label();
            this.animationSpeedLabel = new System.Windows.Forms.Label();
            this.animationControlsProgressTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.animationFrameTrackBar = new System.Windows.Forms.TrackBar();
            this.animationProgressLabel = new System.Windows.Forms.Label();
            this.animationSpeedFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.animationSpeedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.animationPlayModeFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.animationPlayModeLabel = new System.Windows.Forms.Label();
            this.animationLoopModeComboBox = new System.Windows.Forms.ComboBox();
            this.animationReverseFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.animationReverseLabel = new System.Windows.Forms.Label();
            this.animationReverseCheckBox = new System.Windows.Forms.CheckBox();
            this.animationPlayButton = new System.Windows.Forms.Button();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startScanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearScanResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseScanningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopScanningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.showFPSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fastWindowResizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showModelsStatusBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSideBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkedModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkAllModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.resetTransformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetWholeModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gizmoToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gizmoToolNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gizmoToolTranslateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gizmoToolRotateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gizmoToolScaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectionModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectionModeNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectionModeBoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectionModeTriangleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.drawModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawModeFacesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawModeWireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawModeVerticesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawModeSolidWireframeVerticesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSkeletonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableVertexColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableSemiTransparencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forceDoubleSidedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAttachLimbsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.subModelVisibilityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subModelVisibilityAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subModelVisibilitySelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subModelVisibilityWithSameTMDIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFocusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFocusOnRootModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFocusOnSubModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFocusIncludeWholeModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFocusIncludeCheckedModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFocusResetCameraRotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.setAmbientColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setWireframeVerticesColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.lineRendererToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.drawToVRAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawAllToVRAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findByPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPaletteIndexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showTexturePaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTextureSemiTransparencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTextureUVsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.showMissingTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoDrawModelTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoPackModelTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.setMaskColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportVRAMPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedVRAMPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportDrawnToVRAMPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllVRAMPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.clearPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.showVRAMSemiTransparencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showVRAMUVsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkedAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkAllAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.autoPlayAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoSelectAnimationModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.showTMDBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoTutorialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compatibilityListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewOnGitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusTotalFilesProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusCurrentFileProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusMessageLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.sceneControlsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lightRotationFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lightRotationLabel = new System.Windows.Forms.Label();
            this.lightYawNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.lightPitchNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.lightIntensityFlowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.lightIntensityLabel = new System.Windows.Forms.Label();
            this.lightIntensityNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.cameraFOVFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.cameraFOVLabel = new System.Windows.Forms.Label();
            this.cameraFOVUpDown = new System.Windows.Forms.NumericUpDown();
            this.gizmoSnapFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.gizmoSnapLabel = new System.Windows.Forms.Label();
            this.gridSnapUpDown = new System.Windows.Forms.NumericUpDown();
            this.angleSnapUpDown = new System.Windows.Forms.NumericUpDown();
            this.scaleSnapUpDown = new System.Windows.Forms.NumericUpDown();
            this.wireframeVertexSizeFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.wireframeVertexSizeLabel = new System.Windows.Forms.Label();
            this.wireframeSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.vertexSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.animationsSideSplitContainer = new System.Windows.Forms.SplitContainer();
            this.animationPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.animationPreviewPanel = new System.Windows.Forms.Panel();
            this.modelsSideSplitContainer = new System.Windows.Forms.SplitContainer();
            this.entitiesTreeView = new PSXPrev.Forms.Controls.ExtendedTreeView();
            this.modelPropertyGridTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.exportSelectedModelsButton = new System.Windows.Forms.Button();
            this.modelPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.scenePreviewer = new PSXPrev.Forms.Controls.ScenePreviewer();
            this.texturesListView = new PSXPrev.Forms.Controls.ExtendedImageListView();
            this.texturePreviewer = new PSXPrev.Forms.Controls.TexturePreviewer();
            this.vramPreviewer = new PSXPrev.Forms.Controls.TexturePreviewer();
            this.animationsTreeView = new PSXPrev.Forms.Controls.ExtendedTreeView();
            this.animationPreviewer = new PSXPrev.Forms.Controls.ScenePreviewer();
            this.entitiesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modelsPreviewSplitContainer)).BeginInit();
            this.modelsPreviewSplitContainer.Panel1.SuspendLayout();
            this.modelsPreviewSplitContainer.Panel2.SuspendLayout();
            this.modelsPreviewSplitContainer.SuspendLayout();
            this.menusTabControl.SuspendLayout();
            this.texturesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texturesPreviewSplitContainer)).BeginInit();
            this.texturesPreviewSplitContainer.Panel1.SuspendLayout();
            this.texturesPreviewSplitContainer.Panel2.SuspendLayout();
            this.texturesPreviewSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texturesSideSplitContainer)).BeginInit();
            this.texturesSideSplitContainer.Panel1.SuspendLayout();
            this.texturesSideSplitContainer.Panel2.SuspendLayout();
            this.texturesSideSplitContainer.SuspendLayout();
            this.texturePropertyGridTableLayoutPanel.SuspendLayout();
            this.vramTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vramPreviewSplitContainer)).BeginInit();
            this.vramPreviewSplitContainer.Panel1.SuspendLayout();
            this.vramPreviewSplitContainer.Panel2.SuspendLayout();
            this.vramPreviewSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vramSideSplitContainer)).BeginInit();
            this.vramSideSplitContainer.Panel1.SuspendLayout();
            this.vramSideSplitContainer.Panel2.SuspendLayout();
            this.vramSideSplitContainer.SuspendLayout();
            this.vramButtonsTableLayoutPanel.SuspendLayout();
            this.animationsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationsPreviewSplitContainer)).BeginInit();
            this.animationsPreviewSplitContainer.Panel1.SuspendLayout();
            this.animationsPreviewSplitContainer.Panel2.SuspendLayout();
            this.animationsPreviewSplitContainer.SuspendLayout();
            this.animationGroupBoxMarginPanel.SuspendLayout();
            this.animationGroupBox.SuspendLayout();
            this.animationControlsTableLayoutPanel.SuspendLayout();
            this.animationControlsProgressTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationFrameTrackBar)).BeginInit();
            this.animationSpeedFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationSpeedNumericUpDown)).BeginInit();
            this.animationPlayModeFlowLayoutPanel.SuspendLayout();
            this.animationReverseFlowLayoutPanel.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.sceneControlsFlowLayoutPanel.SuspendLayout();
            this.lightRotationFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightYawNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightPitchNumericUpDown)).BeginInit();
            this.lightIntensityFlowLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightIntensityNumericUpDown)).BeginInit();
            this.cameraFOVFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraFOVUpDown)).BeginInit();
            this.gizmoSnapFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSnapUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.angleSnapUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scaleSnapUpDown)).BeginInit();
            this.wireframeVertexSizeFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wireframeSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vertexSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.animationsSideSplitContainer)).BeginInit();
            this.animationsSideSplitContainer.Panel1.SuspendLayout();
            this.animationsSideSplitContainer.Panel2.SuspendLayout();
            this.animationsSideSplitContainer.SuspendLayout();
            this.animationPreviewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modelsSideSplitContainer)).BeginInit();
            this.modelsSideSplitContainer.Panel1.SuspendLayout();
            this.modelsSideSplitContainer.Panel2.SuspendLayout();
            this.modelsSideSplitContainer.SuspendLayout();
            this.modelPropertyGridTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // entitiesTabPage
            // 
            this.entitiesTabPage.BackColor = System.Drawing.SystemColors.Window;
            this.entitiesTabPage.Controls.Add(this.modelsPreviewSplitContainer);
            this.entitiesTabPage.Location = new System.Drawing.Point(4, 22);
            this.entitiesTabPage.Name = "entitiesTabPage";
            this.entitiesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.entitiesTabPage.Size = new System.Drawing.Size(1000, 573);
            this.entitiesTabPage.TabIndex = 0;
            this.entitiesTabPage.Text = "Models";
            // 
            // modelsPreviewSplitContainer
            // 
            this.modelsPreviewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelsPreviewSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.modelsPreviewSplitContainer.Name = "modelsPreviewSplitContainer";
            // 
            // modelsPreviewSplitContainer.Panel1
            // 
            this.modelsPreviewSplitContainer.Panel1.Controls.Add(this.modelsSideSplitContainer);
            // 
            // modelsPreviewSplitContainer.Panel2
            // 
            this.modelsPreviewSplitContainer.Panel2.BackColor = System.Drawing.Color.LightSkyBlue;
            this.modelsPreviewSplitContainer.Panel2.Controls.Add(this.scenePreviewer);
            this.modelsPreviewSplitContainer.Size = new System.Drawing.Size(994, 567);
            this.modelsPreviewSplitContainer.SplitterDistance = 330;
            this.modelsPreviewSplitContainer.TabIndex = 15;
            // 
            // menusTabControl
            // 
            this.menusTabControl.Controls.Add(this.entitiesTabPage);
            this.menusTabControl.Controls.Add(this.texturesTabPage);
            this.menusTabControl.Controls.Add(this.vramTabPage);
            this.menusTabControl.Controls.Add(this.animationsTabPage);
            this.menusTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menusTabControl.Location = new System.Drawing.Point(0, 24);
            this.menusTabControl.Name = "menusTabControl";
            this.menusTabControl.SelectedIndex = 0;
            this.menusTabControl.Size = new System.Drawing.Size(1008, 599);
            this.menusTabControl.TabIndex = 1;
            this.menusTabControl.SelectedIndexChanged += new System.EventHandler(this.menusTabControl_SelectedIndexChanged);
            // 
            // texturesTabPage
            // 
            this.texturesTabPage.BackColor = System.Drawing.SystemColors.Window;
            this.texturesTabPage.Controls.Add(this.texturesPreviewSplitContainer);
            this.texturesTabPage.Location = new System.Drawing.Point(4, 22);
            this.texturesTabPage.Name = "texturesTabPage";
            this.texturesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.texturesTabPage.Size = new System.Drawing.Size(1000, 573);
            this.texturesTabPage.TabIndex = 1;
            this.texturesTabPage.Text = "Textures";
            // 
            // texturesPreviewSplitContainer
            // 
            this.texturesPreviewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturesPreviewSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.texturesPreviewSplitContainer.Name = "texturesPreviewSplitContainer";
            // 
            // texturesPreviewSplitContainer.Panel1
            // 
            this.texturesPreviewSplitContainer.Panel1.Controls.Add(this.texturesSideSplitContainer);
            // 
            // texturesPreviewSplitContainer.Panel2
            // 
            this.texturesPreviewSplitContainer.Panel2.Controls.Add(this.texturePreviewer);
            this.texturesPreviewSplitContainer.Size = new System.Drawing.Size(994, 567);
            this.texturesPreviewSplitContainer.SplitterDistance = 330;
            this.texturesPreviewSplitContainer.TabIndex = 17;
            // 
            // texturesSideSplitContainer
            // 
            this.texturesSideSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturesSideSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.texturesSideSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.texturesSideSplitContainer.Name = "texturesSideSplitContainer";
            this.texturesSideSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // texturesSideSplitContainer.Panel1
            // 
            this.texturesSideSplitContainer.Panel1.Controls.Add(this.texturesListView);
            // 
            // texturesSideSplitContainer.Panel2
            // 
            this.texturesSideSplitContainer.Panel2.Controls.Add(this.texturePropertyGridTableLayoutPanel);
            this.texturesSideSplitContainer.Size = new System.Drawing.Size(330, 567);
            this.texturesSideSplitContainer.SplitterDistance = 350;
            this.texturesSideSplitContainer.TabIndex = 0;
            // 
            // texturePropertyGridTableLayoutPanel
            // 
            this.texturePropertyGridTableLayoutPanel.ColumnCount = 1;
            this.texturePropertyGridTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.texturePropertyGridTableLayoutPanel.Controls.Add(this.drawToVRAMButton, 0, 2);
            this.texturePropertyGridTableLayoutPanel.Controls.Add(this.texturePropertyGrid, 0, 0);
            this.texturePropertyGridTableLayoutPanel.Controls.Add(this.exportBitmapButton, 0, 1);
            this.texturePropertyGridTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturePropertyGridTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.texturePropertyGridTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.texturePropertyGridTableLayoutPanel.Name = "texturePropertyGridTableLayoutPanel";
            this.texturePropertyGridTableLayoutPanel.RowCount = 3;
            this.texturePropertyGridTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.texturePropertyGridTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.texturePropertyGridTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.texturePropertyGridTableLayoutPanel.Size = new System.Drawing.Size(330, 213);
            this.texturePropertyGridTableLayoutPanel.TabIndex = 1;
            // 
            // drawToVRAMButton
            // 
            this.drawToVRAMButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.drawToVRAMButton.Location = new System.Drawing.Point(0, 179);
            this.drawToVRAMButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.drawToVRAMButton.Name = "drawToVRAMButton";
            this.drawToVRAMButton.Size = new System.Drawing.Size(330, 34);
            this.drawToVRAMButton.TabIndex = 13;
            this.drawToVRAMButton.Text = "Draw to VRAM";
            this.drawToVRAMButton.UseVisualStyleBackColor = true;
            this.drawToVRAMButton.Click += new System.EventHandler(this.drawSelectedToVRAM_Click);
            // 
            // texturePropertyGrid
            // 
            this.texturePropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturePropertyGrid.HelpVisible = false;
            this.texturePropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.texturePropertyGrid.Margin = new System.Windows.Forms.Padding(0);
            this.texturePropertyGrid.Name = "texturePropertyGrid";
            this.texturePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.texturePropertyGrid.Size = new System.Drawing.Size(330, 139);
            this.texturePropertyGrid.TabIndex = 11;
            this.texturePropertyGrid.ToolbarVisible = false;
            this.texturePropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.texturePropertyGrid_PropertyValueChanged);
            // 
            // exportBitmapButton
            // 
            this.exportBitmapButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.exportBitmapButton.Location = new System.Drawing.Point(0, 142);
            this.exportBitmapButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.exportBitmapButton.Name = "exportBitmapButton";
            this.exportBitmapButton.Size = new System.Drawing.Size(330, 34);
            this.exportBitmapButton.TabIndex = 12;
            this.exportBitmapButton.Text = "Export Selected Textures";
            this.exportBitmapButton.UseVisualStyleBackColor = true;
            this.exportBitmapButton.Click += new System.EventHandler(this.exportSelectedTextures_Click);
            // 
            // vramTabPage
            // 
            this.vramTabPage.BackColor = System.Drawing.SystemColors.Window;
            this.vramTabPage.Controls.Add(this.vramPreviewSplitContainer);
            this.vramTabPage.Location = new System.Drawing.Point(4, 22);
            this.vramTabPage.Name = "vramTabPage";
            this.vramTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.vramTabPage.Size = new System.Drawing.Size(1000, 573);
            this.vramTabPage.TabIndex = 2;
            this.vramTabPage.Text = "VRAM";
            // 
            // vramPreviewSplitContainer
            // 
            this.vramPreviewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vramPreviewSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.vramPreviewSplitContainer.Name = "vramPreviewSplitContainer";
            // 
            // vramPreviewSplitContainer.Panel1
            // 
            this.vramPreviewSplitContainer.Panel1.Controls.Add(this.vramSideSplitContainer);
            // 
            // vramPreviewSplitContainer.Panel2
            // 
            this.vramPreviewSplitContainer.Panel2.Controls.Add(this.vramPreviewer);
            this.vramPreviewSplitContainer.Size = new System.Drawing.Size(994, 567);
            this.vramPreviewSplitContainer.SplitterDistance = 330;
            this.vramPreviewSplitContainer.TabIndex = 13;
            // 
            // vramSideSplitContainer
            // 
            this.vramSideSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vramSideSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.vramSideSplitContainer.IsSplitterFixed = true;
            this.vramSideSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.vramSideSplitContainer.Name = "vramSideSplitContainer";
            this.vramSideSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // vramSideSplitContainer.Panel1
            // 
            this.vramSideSplitContainer.Panel1.Controls.Add(this.vramListBox);
            // 
            // vramSideSplitContainer.Panel2
            // 
            this.vramSideSplitContainer.Panel2.Controls.Add(this.vramButtonsTableLayoutPanel);
            this.vramSideSplitContainer.Size = new System.Drawing.Size(330, 567);
            this.vramSideSplitContainer.SplitterDistance = 492;
            this.vramSideSplitContainer.TabIndex = 0;
            // 
            // vramListBox
            // 
            this.vramListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vramListBox.FormattingEnabled = true;
            this.vramListBox.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31"});
            this.vramListBox.Location = new System.Drawing.Point(0, 0);
            this.vramListBox.Margin = new System.Windows.Forms.Padding(0);
            this.vramListBox.Name = "vramListBox";
            this.vramListBox.Size = new System.Drawing.Size(330, 492);
            this.vramListBox.TabIndex = 10;
            this.vramListBox.SelectedIndexChanged += new System.EventHandler(this.vramComboBox_SelectedIndexChanged);
            // 
            // vramButtonsTableLayoutPanel
            // 
            this.vramButtonsTableLayoutPanel.ColumnCount = 1;
            this.vramButtonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.vramButtonsTableLayoutPanel.Controls.Add(this.vramGotoPageButton, 0, 0);
            this.vramButtonsTableLayoutPanel.Controls.Add(this.vramClearPageButton, 0, 1);
            this.vramButtonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vramButtonsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.vramButtonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.vramButtonsTableLayoutPanel.Name = "vramButtonsTableLayoutPanel";
            this.vramButtonsTableLayoutPanel.RowCount = 2;
            this.vramButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.vramButtonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.vramButtonsTableLayoutPanel.Size = new System.Drawing.Size(330, 71);
            this.vramButtonsTableLayoutPanel.TabIndex = 13;
            // 
            // vramGotoPageButton
            // 
            this.vramGotoPageButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.vramGotoPageButton.Location = new System.Drawing.Point(0, 0);
            this.vramGotoPageButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.vramGotoPageButton.Name = "vramGotoPageButton";
            this.vramGotoPageButton.Size = new System.Drawing.Size(330, 34);
            this.vramGotoPageButton.TabIndex = 11;
            this.vramGotoPageButton.Text = "Go to Page";
            this.vramGotoPageButton.UseVisualStyleBackColor = true;
            this.vramGotoPageButton.Click += new System.EventHandler(this.gotoVRAMPageButton_Click);
            // 
            // vramClearPageButton
            // 
            this.vramClearPageButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.vramClearPageButton.Location = new System.Drawing.Point(0, 37);
            this.vramClearPageButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.vramClearPageButton.Name = "vramClearPageButton";
            this.vramClearPageButton.Size = new System.Drawing.Size(330, 34);
            this.vramClearPageButton.TabIndex = 12;
            this.vramClearPageButton.Text = "Clear Page";
            this.vramClearPageButton.UseVisualStyleBackColor = true;
            this.vramClearPageButton.Click += new System.EventHandler(this.clearVRAMPage_Click);
            // 
            // animationsTabPage
            // 
            this.animationsTabPage.BackColor = System.Drawing.SystemColors.Window;
            this.animationsTabPage.Controls.Add(this.animationsPreviewSplitContainer);
            this.animationsTabPage.Location = new System.Drawing.Point(4, 22);
            this.animationsTabPage.Name = "animationsTabPage";
            this.animationsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.animationsTabPage.Size = new System.Drawing.Size(1000, 573);
            this.animationsTabPage.TabIndex = 3;
            this.animationsTabPage.Text = "Animations";
            // 
            // animationsPreviewSplitContainer
            // 
            this.animationsPreviewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsPreviewSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.animationsPreviewSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.animationsPreviewSplitContainer.Name = "animationsPreviewSplitContainer";
            // 
            // animationsPreviewSplitContainer.Panel1
            // 
            this.animationsPreviewSplitContainer.Panel1.Controls.Add(this.animationsSideSplitContainer);
            // 
            // animationsPreviewSplitContainer.Panel2
            // 
            this.animationsPreviewSplitContainer.Panel2.BackColor = System.Drawing.Color.LightSkyBlue;
            this.animationsPreviewSplitContainer.Panel2.Controls.Add(this.animationPreviewPanel);
            this.animationsPreviewSplitContainer.Panel2.Controls.Add(this.animationGroupBoxMarginPanel);
            this.animationsPreviewSplitContainer.Size = new System.Drawing.Size(994, 567);
            this.animationsPreviewSplitContainer.SplitterDistance = 330;
            this.animationsPreviewSplitContainer.TabIndex = 16;
            // 
            // animationGroupBoxMarginPanel
            // 
            this.animationGroupBoxMarginPanel.AutoSize = true;
            this.animationGroupBoxMarginPanel.BackColor = System.Drawing.SystemColors.Window;
            this.animationGroupBoxMarginPanel.Controls.Add(this.animationGroupBox);
            this.animationGroupBoxMarginPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.animationGroupBoxMarginPanel.Location = new System.Drawing.Point(0, 436);
            this.animationGroupBoxMarginPanel.Name = "animationGroupBoxMarginPanel";
            this.animationGroupBoxMarginPanel.Padding = new System.Windows.Forms.Padding(3);
            this.animationGroupBoxMarginPanel.Size = new System.Drawing.Size(660, 131);
            this.animationGroupBoxMarginPanel.TabIndex = 32;
            // 
            // animationGroupBox
            // 
            this.animationGroupBox.AutoSize = true;
            this.animationGroupBox.Controls.Add(this.animationControlsTableLayoutPanel);
            this.animationGroupBox.Controls.Add(this.animationPlayButton);
            this.animationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationGroupBox.Location = new System.Drawing.Point(3, 3);
            this.animationGroupBox.Name = "animationGroupBox";
            this.animationGroupBox.Size = new System.Drawing.Size(654, 125);
            this.animationGroupBox.TabIndex = 30;
            this.animationGroupBox.TabStop = false;
            this.animationGroupBox.Text = "Animation";
            // 
            // animationControlsTableLayoutPanel
            // 
            this.animationControlsTableLayoutPanel.AutoSize = true;
            this.animationControlsTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.animationControlsTableLayoutPanel.ColumnCount = 2;
            this.animationControlsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.animationControlsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.animationControlsTableLayoutPanel.Controls.Add(this.animationProgressNameLabel, 0, 1);
            this.animationControlsTableLayoutPanel.Controls.Add(this.animationSpeedLabel, 0, 0);
            this.animationControlsTableLayoutPanel.Controls.Add(this.animationControlsProgressTableLayoutPanel, 1, 1);
            this.animationControlsTableLayoutPanel.Controls.Add(this.animationSpeedFlowLayoutPanel, 1, 0);
            this.animationControlsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.animationControlsTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.animationControlsTableLayoutPanel.Name = "animationControlsTableLayoutPanel";
            this.animationControlsTableLayoutPanel.RowCount = 2;
            this.animationControlsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.animationControlsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.animationControlsTableLayoutPanel.Size = new System.Drawing.Size(648, 72);
            this.animationControlsTableLayoutPanel.TabIndex = 22;
            // 
            // animationProgressNameLabel
            // 
            this.animationProgressNameLabel.AutoSize = true;
            this.animationProgressNameLabel.BackColor = System.Drawing.SystemColors.Window;
            this.animationProgressNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.animationProgressNameLabel.Location = new System.Drawing.Point(3, 27);
            this.animationProgressNameLabel.Name = "animationProgressNameLabel";
            this.animationProgressNameLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 24);
            this.animationProgressNameLabel.Size = new System.Drawing.Size(51, 45);
            this.animationProgressNameLabel.TabIndex = 23;
            this.animationProgressNameLabel.Text = "Progress:";
            this.animationProgressNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationSpeedLabel
            // 
            this.animationSpeedLabel.AutoSize = true;
            this.animationSpeedLabel.BackColor = System.Drawing.SystemColors.Window;
            this.animationSpeedLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.animationSpeedLabel.Location = new System.Drawing.Point(3, 0);
            this.animationSpeedLabel.Name = "animationSpeedLabel";
            this.animationSpeedLabel.Size = new System.Drawing.Size(41, 27);
            this.animationSpeedLabel.TabIndex = 21;
            this.animationSpeedLabel.Text = "Speed:";
            this.animationSpeedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationControlsProgressTableLayoutPanel
            // 
            this.animationControlsProgressTableLayoutPanel.ColumnCount = 2;
            this.animationControlsProgressTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.animationControlsProgressTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.animationControlsProgressTableLayoutPanel.Controls.Add(this.animationFrameTrackBar, 0, 0);
            this.animationControlsProgressTableLayoutPanel.Controls.Add(this.animationProgressLabel, 1, 0);
            this.animationControlsProgressTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationControlsProgressTableLayoutPanel.Location = new System.Drawing.Point(60, 27);
            this.animationControlsProgressTableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.animationControlsProgressTableLayoutPanel.Name = "animationControlsProgressTableLayoutPanel";
            this.animationControlsProgressTableLayoutPanel.RowCount = 1;
            this.animationControlsProgressTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.animationControlsProgressTableLayoutPanel.Size = new System.Drawing.Size(585, 45);
            this.animationControlsProgressTableLayoutPanel.TabIndex = 1;
            // 
            // animationFrameTrackBar
            // 
            this.animationFrameTrackBar.BackColor = System.Drawing.SystemColors.Window;
            this.animationFrameTrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationFrameTrackBar.Location = new System.Drawing.Point(0, 0);
            this.animationFrameTrackBar.Margin = new System.Windows.Forms.Padding(0);
            this.animationFrameTrackBar.Name = "animationFrameTrackBar";
            this.animationFrameTrackBar.Size = new System.Drawing.Size(510, 45);
            this.animationFrameTrackBar.TabIndex = 33;
            this.animationFrameTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.animationFrameTrackBar.Scroll += new System.EventHandler(this.animationFrameTrackBar_Scroll);
            // 
            // animationProgressLabel
            // 
            this.animationProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.animationProgressLabel.BackColor = System.Drawing.SystemColors.Window;
            this.animationProgressLabel.Location = new System.Drawing.Point(510, 0);
            this.animationProgressLabel.Margin = new System.Windows.Forms.Padding(0);
            this.animationProgressLabel.Name = "animationProgressLabel";
            this.animationProgressLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 24);
            this.animationProgressLabel.Size = new System.Drawing.Size(75, 45);
            this.animationProgressLabel.TabIndex = 25;
            this.animationProgressLabel.Text = "0/0";
            this.animationProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationSpeedFlowLayoutPanel
            // 
            this.animationSpeedFlowLayoutPanel.AutoSize = true;
            this.animationSpeedFlowLayoutPanel.Controls.Add(this.animationSpeedNumericUpDown);
            this.animationSpeedFlowLayoutPanel.Controls.Add(this.animationPlayModeFlowLayoutPanel);
            this.animationSpeedFlowLayoutPanel.Controls.Add(this.animationReverseFlowLayoutPanel);
            this.animationSpeedFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationSpeedFlowLayoutPanel.Location = new System.Drawing.Point(57, 0);
            this.animationSpeedFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.animationSpeedFlowLayoutPanel.Name = "animationSpeedFlowLayoutPanel";
            this.animationSpeedFlowLayoutPanel.Size = new System.Drawing.Size(591, 27);
            this.animationSpeedFlowLayoutPanel.TabIndex = 0;
            this.animationSpeedFlowLayoutPanel.WrapContents = false;
            // 
            // animationSpeedNumericUpDown
            // 
            this.animationSpeedNumericUpDown.BackColor = System.Drawing.SystemColors.Window;
            this.animationSpeedNumericUpDown.DecimalPlaces = 2;
            this.animationSpeedNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.animationSpeedNumericUpDown.Location = new System.Drawing.Point(12, 3);
            this.animationSpeedNumericUpDown.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
            this.animationSpeedNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.animationSpeedNumericUpDown.Name = "animationSpeedNumericUpDown";
            this.animationSpeedNumericUpDown.Size = new System.Drawing.Size(70, 20);
            this.animationSpeedNumericUpDown.TabIndex = 0;
            this.animationSpeedNumericUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.animationSpeedNumericUpDown.ValueChanged += new System.EventHandler(this.animationSpeedNumericUpDown_ValueChanged);
            // 
            // animationPlayModeFlowLayoutPanel
            // 
            this.animationPlayModeFlowLayoutPanel.AutoSize = true;
            this.animationPlayModeFlowLayoutPanel.Controls.Add(this.animationPlayModeLabel);
            this.animationPlayModeFlowLayoutPanel.Controls.Add(this.animationLoopModeComboBox);
            this.animationPlayModeFlowLayoutPanel.Location = new System.Drawing.Point(85, 0);
            this.animationPlayModeFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.animationPlayModeFlowLayoutPanel.Name = "animationPlayModeFlowLayoutPanel";
            this.animationPlayModeFlowLayoutPanel.Size = new System.Drawing.Size(182, 27);
            this.animationPlayModeFlowLayoutPanel.TabIndex = 26;
            // 
            // animationPlayModeLabel
            // 
            this.animationPlayModeLabel.AutoSize = true;
            this.animationPlayModeLabel.BackColor = System.Drawing.SystemColors.Window;
            this.animationPlayModeLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.animationPlayModeLabel.Location = new System.Drawing.Point(3, 0);
            this.animationPlayModeLabel.Name = "animationPlayModeLabel";
            this.animationPlayModeLabel.Size = new System.Drawing.Size(60, 27);
            this.animationPlayModeLabel.TabIndex = 23;
            this.animationPlayModeLabel.Text = "Play Mode:";
            this.animationPlayModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationLoopModeComboBox
            // 
            this.animationLoopModeComboBox.BackColor = System.Drawing.SystemColors.Window;
            this.animationLoopModeComboBox.FormattingEnabled = true;
            this.animationLoopModeComboBox.Items.AddRange(new object[] {
            "Once",
            "Loop",
            "Loop (Unsynced)",
            "Mirror Once",
            "Mirror Loop"});
            this.animationLoopModeComboBox.Location = new System.Drawing.Point(69, 3);
            this.animationLoopModeComboBox.Name = "animationLoopModeComboBox";
            this.animationLoopModeComboBox.Size = new System.Drawing.Size(110, 21);
            this.animationLoopModeComboBox.TabIndex = 1;
            this.animationLoopModeComboBox.SelectedIndexChanged += new System.EventHandler(this.animationLoopModeComboBox_SelectedIndexChanged);
            // 
            // animationReverseFlowLayoutPanel
            // 
            this.animationReverseFlowLayoutPanel.AutoSize = true;
            this.animationReverseFlowLayoutPanel.Controls.Add(this.animationReverseLabel);
            this.animationReverseFlowLayoutPanel.Controls.Add(this.animationReverseCheckBox);
            this.animationReverseFlowLayoutPanel.Location = new System.Drawing.Point(267, 0);
            this.animationReverseFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.animationReverseFlowLayoutPanel.Name = "animationReverseFlowLayoutPanel";
            this.animationReverseFlowLayoutPanel.Size = new System.Drawing.Size(77, 27);
            this.animationReverseFlowLayoutPanel.TabIndex = 27;
            // 
            // animationReverseLabel
            // 
            this.animationReverseLabel.AutoSize = true;
            this.animationReverseLabel.BackColor = System.Drawing.SystemColors.Window;
            this.animationReverseLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.animationReverseLabel.Location = new System.Drawing.Point(3, 0);
            this.animationReverseLabel.Name = "animationReverseLabel";
            this.animationReverseLabel.Size = new System.Drawing.Size(50, 27);
            this.animationReverseLabel.TabIndex = 26;
            this.animationReverseLabel.Text = "Reverse:";
            this.animationReverseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationReverseCheckBox
            // 
            this.animationReverseCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.animationReverseCheckBox.AutoSize = true;
            this.animationReverseCheckBox.BackColor = System.Drawing.SystemColors.Window;
            this.animationReverseCheckBox.Location = new System.Drawing.Point(57, 5);
            this.animationReverseCheckBox.Margin = new System.Windows.Forms.Padding(1, 5, 3, 5);
            this.animationReverseCheckBox.Name = "animationReverseCheckBox";
            this.animationReverseCheckBox.Padding = new System.Windows.Forms.Padding(2, 2, 0, 1);
            this.animationReverseCheckBox.Size = new System.Drawing.Size(17, 17);
            this.animationReverseCheckBox.TabIndex = 2;
            this.animationReverseCheckBox.UseVisualStyleBackColor = false;
            this.animationReverseCheckBox.CheckedChanged += new System.EventHandler(this.animationReverseCheckBox_CheckedChanged);
            // 
            // animationPlayButton
            // 
            this.animationPlayButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.animationPlayButton.Enabled = false;
            this.animationPlayButton.Location = new System.Drawing.Point(3, 88);
            this.animationPlayButton.Name = "animationPlayButton";
            this.animationPlayButton.Size = new System.Drawing.Size(648, 34);
            this.animationPlayButton.TabIndex = 34;
            this.animationPlayButton.Text = "Play Animation";
            this.animationPlayButton.UseVisualStyleBackColor = true;
            this.animationPlayButton.Click += new System.EventHandler(this.animationPlayButton_Click);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.modelsToolStripMenuItem,
            this.texturesToolStripMenuItem,
            this.vramToolStripMenuItem,
            this.animationsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(2);
            this.mainMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.mainMenuStrip.Size = new System.Drawing.Size(1008, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startScanToolStripMenuItem,
            this.clearScanResultsToolStripMenuItem,
            this.pauseScanningToolStripMenuItem,
            this.stopScanningToolStripMenuItem,
            this.toolStripSeparator11,
            this.showFPSToolStripMenuItem,
            this.fastWindowResizeToolStripMenuItem,
            this.showModelsStatusBarToolStripMenuItem,
            this.showSideBarToolStripMenuItem,
            this.showUIToolStripMenuItem,
            this.toolStripSeparator10,
            this.defaultSettingsToolStripMenuItem,
            this.loadSettingsToolStripMenuItem,
            this.saveSettingsToolStripMenuItem,
            this.advancedSettingsToolStripMenuItem,
            this.toolStripSeparator16,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // startScanToolStripMenuItem
            // 
            this.startScanToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Start;
            this.startScanToolStripMenuItem.Name = "startScanToolStripMenuItem";
            this.startScanToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.startScanToolStripMenuItem.Text = "Start Scan...";
            this.startScanToolStripMenuItem.Click += new System.EventHandler(this.startScanToolStripMenuItem_Click);
            // 
            // clearScanResultsToolStripMenuItem
            // 
            this.clearScanResultsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Cross;
            this.clearScanResultsToolStripMenuItem.Name = "clearScanResultsToolStripMenuItem";
            this.clearScanResultsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.clearScanResultsToolStripMenuItem.Text = "Clear Scan Results";
            this.clearScanResultsToolStripMenuItem.Click += new System.EventHandler(this.clearScanResultsToolStripMenuItem_Click);
            // 
            // pauseScanningToolStripMenuItem
            // 
            this.pauseScanningToolStripMenuItem.CheckOnClick = true;
            this.pauseScanningToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Pause;
            this.pauseScanningToolStripMenuItem.Name = "pauseScanningToolStripMenuItem";
            this.pauseScanningToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.pauseScanningToolStripMenuItem.Text = "Pause Scanning";
            this.pauseScanningToolStripMenuItem.CheckedChanged += new System.EventHandler(this.pauseScanningToolStripMenuItem_CheckedChanged);
            // 
            // stopScanningToolStripMenuItem
            // 
            this.stopScanningToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Stop;
            this.stopScanningToolStripMenuItem.Name = "stopScanningToolStripMenuItem";
            this.stopScanningToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.stopScanningToolStripMenuItem.Text = "Stop Scanning";
            this.stopScanningToolStripMenuItem.Click += new System.EventHandler(this.stopScanningToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(204, 6);
            // 
            // showFPSToolStripMenuItem
            // 
            this.showFPSToolStripMenuItem.CheckOnClick = true;
            this.showFPSToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.File_ShowFPS;
            this.showFPSToolStripMenuItem.Name = "showFPSToolStripMenuItem";
            this.showFPSToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showFPSToolStripMenuItem.Text = "Show FPS";
            this.showFPSToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showFPSToolStripMenuItem_CheckedChanged);
            // 
            // fastWindowResizeToolStripMenuItem
            // 
            this.fastWindowResizeToolStripMenuItem.CheckOnClick = true;
            this.fastWindowResizeToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.File_FastWindowResize;
            this.fastWindowResizeToolStripMenuItem.Name = "fastWindowResizeToolStripMenuItem";
            this.fastWindowResizeToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.fastWindowResizeToolStripMenuItem.Text = "Fast Window Resize";
            // 
            // showModelsStatusBarToolStripMenuItem
            // 
            this.showModelsStatusBarToolStripMenuItem.CheckOnClick = true;
            this.showModelsStatusBarToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.File_ShowStatusBar;
            this.showModelsStatusBarToolStripMenuItem.Name = "showModelsStatusBarToolStripMenuItem";
            this.showModelsStatusBarToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showModelsStatusBarToolStripMenuItem.Text = "Show Models Status Bar";
            this.showModelsStatusBarToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showModelsStatusBarToolStripMenuItem_CheckedChanged);
            // 
            // showSideBarToolStripMenuItem
            // 
            this.showSideBarToolStripMenuItem.Checked = true;
            this.showSideBarToolStripMenuItem.CheckOnClick = true;
            this.showSideBarToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showSideBarToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.File_ShowSideBar;
            this.showSideBarToolStripMenuItem.Name = "showSideBarToolStripMenuItem";
            this.showSideBarToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.showSideBarToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showSideBarToolStripMenuItem.Text = "Show Side Bar";
            this.showSideBarToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showSideBarToolStripMenuItem_CheckedChanged);
            // 
            // showUIToolStripMenuItem
            // 
            this.showUIToolStripMenuItem.Checked = true;
            this.showUIToolStripMenuItem.CheckOnClick = true;
            this.showUIToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showUIToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.File_ShowUI;
            this.showUIToolStripMenuItem.Name = "showUIToolStripMenuItem";
            this.showUIToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.showUIToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.showUIToolStripMenuItem.Text = "Show UI";
            this.showUIToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showUIToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(204, 6);
            // 
            // defaultSettingsToolStripMenuItem
            // 
            this.defaultSettingsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Settings_Defaults;
            this.defaultSettingsToolStripMenuItem.Name = "defaultSettingsToolStripMenuItem";
            this.defaultSettingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.defaultSettingsToolStripMenuItem.Text = "Reset Settings to Defaults";
            this.defaultSettingsToolStripMenuItem.Click += new System.EventHandler(this.defaultSettingsToolStripMenuItem_Click);
            // 
            // loadSettingsToolStripMenuItem
            // 
            this.loadSettingsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Settings_Reload;
            this.loadSettingsToolStripMenuItem.Name = "loadSettingsToolStripMenuItem";
            this.loadSettingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.loadSettingsToolStripMenuItem.Text = "Reload Settings";
            this.loadSettingsToolStripMenuItem.Click += new System.EventHandler(this.loadSettingsToolStripMenuItem_Click);
            // 
            // saveSettingsToolStripMenuItem
            // 
            this.saveSettingsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Settings_Save;
            this.saveSettingsToolStripMenuItem.Name = "saveSettingsToolStripMenuItem";
            this.saveSettingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveSettingsToolStripMenuItem.Text = "Save Current Settings";
            this.saveSettingsToolStripMenuItem.Click += new System.EventHandler(this.saveSettingsToolStripMenuItem_Click);
            // 
            // advancedSettingsToolStripMenuItem
            // 
            this.advancedSettingsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Settings_Advanced;
            this.advancedSettingsToolStripMenuItem.Name = "advancedSettingsToolStripMenuItem";
            this.advancedSettingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.advancedSettingsToolStripMenuItem.Text = "Advanced Settings...";
            this.advancedSettingsToolStripMenuItem.Click += new System.EventHandler(this.advancedSettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(204, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Exit;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // modelsToolStripMenuItem
            // 
            this.modelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportModelsToolStripMenuItem,
            this.checkedModelsToolStripMenuItem,
            this.toolStripSeparator2,
            this.resetTransformToolStripMenuItem,
            this.gizmoToolToolStripMenuItem,
            this.selectionModeToolStripMenuItem,
            this.toolStripSeparator12,
            this.drawModeToolStripMenuItem,
            this.showBoundsToolStripMenuItem,
            this.showSkeletonToolStripMenuItem,
            this.enableLightToolStripMenuItem,
            this.enableTexturesToolStripMenuItem,
            this.enableVertexColorToolStripMenuItem,
            this.enableSemiTransparencyToolStripMenuItem,
            this.forceDoubleSidedToolStripMenuItem,
            this.autoAttachLimbsToolStripMenuItem,
            this.toolStripSeparator14,
            this.subModelVisibilityToolStripMenuItem,
            this.autoFocusToolStripMenuItem,
            this.toolStripSeparator7,
            this.setAmbientColorToolStripMenuItem,
            this.setBackgroundColorToolStripMenuItem,
            this.setWireframeVerticesColorToolStripMenuItem,
            this.toolStripSeparator8,
            this.lineRendererToolStripMenuItem});
            this.modelsToolStripMenuItem.Name = "modelsToolStripMenuItem";
            this.modelsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.modelsToolStripMenuItem.Text = "Models";
            // 
            // exportModelsToolStripMenuItem
            // 
            this.exportModelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedModelsToolStripMenuItem,
            this.exportAllModelsToolStripMenuItem});
            this.exportModelsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Export;
            this.exportModelsToolStripMenuItem.Name = "exportModelsToolStripMenuItem";
            this.exportModelsToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.exportModelsToolStripMenuItem.Text = "Export Models";
            // 
            // exportSelectedModelsToolStripMenuItem
            // 
            this.exportSelectedModelsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Export;
            this.exportSelectedModelsToolStripMenuItem.Name = "exportSelectedModelsToolStripMenuItem";
            this.exportSelectedModelsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.exportSelectedModelsToolStripMenuItem.Text = "Export Selected Models...";
            this.exportSelectedModelsToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedModels_Click);
            // 
            // exportAllModelsToolStripMenuItem
            // 
            this.exportAllModelsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Export_All;
            this.exportAllModelsToolStripMenuItem.Name = "exportAllModelsToolStripMenuItem";
            this.exportAllModelsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.exportAllModelsToolStripMenuItem.Text = "Export All Models...";
            this.exportAllModelsToolStripMenuItem.Click += new System.EventHandler(this.exportAllModels_Click);
            // 
            // checkedModelsToolStripMenuItem
            // 
            this.checkedModelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllModelsToolStripMenuItem,
            this.uncheckAllModelsToolStripMenuItem});
            this.checkedModelsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.CheckBoxes;
            this.checkedModelsToolStripMenuItem.Name = "checkedModelsToolStripMenuItem";
            this.checkedModelsToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.checkedModelsToolStripMenuItem.Text = "Checked Models";
            // 
            // checkAllModelsToolStripMenuItem
            // 
            this.checkAllModelsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.CheckBox_Checked;
            this.checkAllModelsToolStripMenuItem.Name = "checkAllModelsToolStripMenuItem";
            this.checkAllModelsToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.checkAllModelsToolStripMenuItem.Text = "Check All";
            this.checkAllModelsToolStripMenuItem.Click += new System.EventHandler(this.checkAllModelsToolStripMenuItem_Click);
            // 
            // uncheckAllModelsToolStripMenuItem
            // 
            this.uncheckAllModelsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.CheckBox_Unchecked;
            this.uncheckAllModelsToolStripMenuItem.Name = "uncheckAllModelsToolStripMenuItem";
            this.uncheckAllModelsToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.uncheckAllModelsToolStripMenuItem.Text = "Uncheck All";
            this.uncheckAllModelsToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllModelsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(222, 6);
            // 
            // resetTransformToolStripMenuItem
            // 
            this.resetTransformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetWholeModelToolStripMenuItem,
            this.resetSelectedModelToolStripMenuItem});
            this.resetTransformToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_ResetTransform;
            this.resetTransformToolStripMenuItem.Name = "resetTransformToolStripMenuItem";
            this.resetTransformToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.resetTransformToolStripMenuItem.Text = "Reset Transform";
            // 
            // resetWholeModelToolStripMenuItem
            // 
            this.resetWholeModelToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_ResetTransform_Whole;
            this.resetWholeModelToolStripMenuItem.Name = "resetWholeModelToolStripMenuItem";
            this.resetWholeModelToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.resetWholeModelToolStripMenuItem.Text = "Reset Whole Model";
            this.resetWholeModelToolStripMenuItem.Click += new System.EventHandler(this.resetWholeModelToolStripMenuItem_Click);
            // 
            // resetSelectedModelToolStripMenuItem
            // 
            this.resetSelectedModelToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_ResetTransform_Selected;
            this.resetSelectedModelToolStripMenuItem.Name = "resetSelectedModelToolStripMenuItem";
            this.resetSelectedModelToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.resetSelectedModelToolStripMenuItem.Text = "Reset Selected Model";
            this.resetSelectedModelToolStripMenuItem.Click += new System.EventHandler(this.resetSelectedModelToolStripMenuItem_Click);
            // 
            // gizmoToolToolStripMenuItem
            // 
            this.gizmoToolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gizmoToolNoneToolStripMenuItem,
            this.gizmoToolTranslateToolStripMenuItem,
            this.gizmoToolRotateToolStripMenuItem,
            this.gizmoToolScaleToolStripMenuItem});
            this.gizmoToolToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_GizmoTool;
            this.gizmoToolToolStripMenuItem.Name = "gizmoToolToolStripMenuItem";
            this.gizmoToolToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.gizmoToolToolStripMenuItem.Text = "Gizmo Tool";
            // 
            // gizmoToolNoneToolStripMenuItem
            // 
            this.gizmoToolNoneToolStripMenuItem.Name = "gizmoToolNoneToolStripMenuItem";
            this.gizmoToolNoneToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.gizmoToolNoneToolStripMenuItem.Text = "None";
            this.gizmoToolNoneToolStripMenuItem.Click += new System.EventHandler(this.gizmoToolNoneToolStripMenuItem_Click);
            // 
            // gizmoToolTranslateToolStripMenuItem
            // 
            this.gizmoToolTranslateToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Gizmo_Translate;
            this.gizmoToolTranslateToolStripMenuItem.Name = "gizmoToolTranslateToolStripMenuItem";
            this.gizmoToolTranslateToolStripMenuItem.ShortcutKeyDisplayString = "W";
            this.gizmoToolTranslateToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.gizmoToolTranslateToolStripMenuItem.Text = "Translate";
            this.gizmoToolTranslateToolStripMenuItem.Click += new System.EventHandler(this.gizmoToolTranslateToolStripMenuItem_Click);
            // 
            // gizmoToolRotateToolStripMenuItem
            // 
            this.gizmoToolRotateToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Gizmo_Rotate;
            this.gizmoToolRotateToolStripMenuItem.Name = "gizmoToolRotateToolStripMenuItem";
            this.gizmoToolRotateToolStripMenuItem.ShortcutKeyDisplayString = "E";
            this.gizmoToolRotateToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.gizmoToolRotateToolStripMenuItem.Text = "Rotate";
            this.gizmoToolRotateToolStripMenuItem.Click += new System.EventHandler(this.gizmoToolRotateToolStripMenuItem_Click);
            // 
            // gizmoToolScaleToolStripMenuItem
            // 
            this.gizmoToolScaleToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Gizmo_Scale;
            this.gizmoToolScaleToolStripMenuItem.Name = "gizmoToolScaleToolStripMenuItem";
            this.gizmoToolScaleToolStripMenuItem.ShortcutKeyDisplayString = "R";
            this.gizmoToolScaleToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.gizmoToolScaleToolStripMenuItem.Text = "Scale";
            this.gizmoToolScaleToolStripMenuItem.Click += new System.EventHandler(this.gizmoToolScaleToolStripMenuItem_Click);
            // 
            // selectionModeToolStripMenuItem
            // 
            this.selectionModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectionModeNoneToolStripMenuItem,
            this.selectionModeBoundsToolStripMenuItem,
            this.selectionModeTriangleToolStripMenuItem});
            this.selectionModeToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_SelectionMode;
            this.selectionModeToolStripMenuItem.Name = "selectionModeToolStripMenuItem";
            this.selectionModeToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.selectionModeToolStripMenuItem.Text = "Selection Mode";
            // 
            // selectionModeNoneToolStripMenuItem
            // 
            this.selectionModeNoneToolStripMenuItem.Name = "selectionModeNoneToolStripMenuItem";
            this.selectionModeNoneToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.selectionModeNoneToolStripMenuItem.Text = "None";
            this.selectionModeNoneToolStripMenuItem.Click += new System.EventHandler(this.selectionModeNoneToolStripMenuItem_Click);
            // 
            // selectionModeBoundsToolStripMenuItem
            // 
            this.selectionModeBoundsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_SelectionMode_Bounds;
            this.selectionModeBoundsToolStripMenuItem.Name = "selectionModeBoundsToolStripMenuItem";
            this.selectionModeBoundsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.selectionModeBoundsToolStripMenuItem.Text = "Bounds Intersection";
            this.selectionModeBoundsToolStripMenuItem.Click += new System.EventHandler(this.selectionModeBoundsToolStripMenuItem_Click);
            // 
            // selectionModeTriangleToolStripMenuItem
            // 
            this.selectionModeTriangleToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_SelectionMode_Triangle;
            this.selectionModeTriangleToolStripMenuItem.Name = "selectionModeTriangleToolStripMenuItem";
            this.selectionModeTriangleToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.selectionModeTriangleToolStripMenuItem.Text = "Triangle Intersection";
            this.selectionModeTriangleToolStripMenuItem.Click += new System.EventHandler(this.selectionModeTriangleToolStripMenuItem_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(222, 6);
            // 
            // drawModeToolStripMenuItem
            // 
            this.drawModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawModeFacesToolStripMenuItem,
            this.drawModeWireframeToolStripMenuItem,
            this.drawModeVerticesToolStripMenuItem,
            this.drawModeSolidWireframeVerticesToolStripMenuItem});
            this.drawModeToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_DrawMode;
            this.drawModeToolStripMenuItem.Name = "drawModeToolStripMenuItem";
            this.drawModeToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.drawModeToolStripMenuItem.Text = "Draw Mode";
            // 
            // drawModeFacesToolStripMenuItem
            // 
            this.drawModeFacesToolStripMenuItem.CheckOnClick = true;
            this.drawModeFacesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.DrawMode_Faces;
            this.drawModeFacesToolStripMenuItem.Name = "drawModeFacesToolStripMenuItem";
            this.drawModeFacesToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.drawModeFacesToolStripMenuItem.Text = "Faces";
            this.drawModeFacesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.drawModeFacesToolStripMenuItem_CheckedChanged);
            // 
            // drawModeWireframeToolStripMenuItem
            // 
            this.drawModeWireframeToolStripMenuItem.CheckOnClick = true;
            this.drawModeWireframeToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.DrawMode_Wireframe;
            this.drawModeWireframeToolStripMenuItem.Name = "drawModeWireframeToolStripMenuItem";
            this.drawModeWireframeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.drawModeWireframeToolStripMenuItem.Text = "Wireframe";
            this.drawModeWireframeToolStripMenuItem.CheckedChanged += new System.EventHandler(this.drawModeWireframeToolStripMenuItem_CheckedChanged);
            // 
            // drawModeVerticesToolStripMenuItem
            // 
            this.drawModeVerticesToolStripMenuItem.CheckOnClick = true;
            this.drawModeVerticesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.DrawMode_Vertices;
            this.drawModeVerticesToolStripMenuItem.Name = "drawModeVerticesToolStripMenuItem";
            this.drawModeVerticesToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.drawModeVerticesToolStripMenuItem.Text = "Vertices";
            this.drawModeVerticesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.drawModeVerticesToolStripMenuItem_CheckedChanged);
            // 
            // drawModeSolidWireframeVerticesToolStripMenuItem
            // 
            this.drawModeSolidWireframeVerticesToolStripMenuItem.CheckOnClick = true;
            this.drawModeSolidWireframeVerticesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.DrawMode_SolidWireframeVertices;
            this.drawModeSolidWireframeVerticesToolStripMenuItem.Name = "drawModeSolidWireframeVerticesToolStripMenuItem";
            this.drawModeSolidWireframeVerticesToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.drawModeSolidWireframeVerticesToolStripMenuItem.Text = "Solid Wireframe/Vertices";
            this.drawModeSolidWireframeVerticesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.drawModeSolidWireframeVerticesToolStripMenuItem_CheckedChanged);
            // 
            // showBoundsToolStripMenuItem
            // 
            this.showBoundsToolStripMenuItem.CheckOnClick = true;
            this.showBoundsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_ShowBounds;
            this.showBoundsToolStripMenuItem.Name = "showBoundsToolStripMenuItem";
            this.showBoundsToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showBoundsToolStripMenuItem.Text = "Show Bounds";
            this.showBoundsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showBoundsToolStripMenuItem_CheckedChanged);
            // 
            // showSkeletonToolStripMenuItem
            // 
            this.showSkeletonToolStripMenuItem.CheckOnClick = true;
            this.showSkeletonToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_ShowSkeleton;
            this.showSkeletonToolStripMenuItem.Name = "showSkeletonToolStripMenuItem";
            this.showSkeletonToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.showSkeletonToolStripMenuItem.Text = "Show Skeleton";
            this.showSkeletonToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showSkeletonToolStripMenuItem_CheckedChanged);
            // 
            // enableLightToolStripMenuItem
            // 
            this.enableLightToolStripMenuItem.CheckOnClick = true;
            this.enableLightToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_EnableLight;
            this.enableLightToolStripMenuItem.Name = "enableLightToolStripMenuItem";
            this.enableLightToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.enableLightToolStripMenuItem.Text = "Enable Light";
            this.enableLightToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableLightToolStripMenuItem_CheckedChanged);
            // 
            // enableTexturesToolStripMenuItem
            // 
            this.enableTexturesToolStripMenuItem.CheckOnClick = true;
            this.enableTexturesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_EnableTextures;
            this.enableTexturesToolStripMenuItem.Name = "enableTexturesToolStripMenuItem";
            this.enableTexturesToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.enableTexturesToolStripMenuItem.Text = "Enable Textures";
            this.enableTexturesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableTexturesToolStripMenuItem_CheckedChanged);
            // 
            // enableVertexColorToolStripMenuItem
            // 
            this.enableVertexColorToolStripMenuItem.CheckOnClick = true;
            this.enableVertexColorToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_EnableVertexColor;
            this.enableVertexColorToolStripMenuItem.Name = "enableVertexColorToolStripMenuItem";
            this.enableVertexColorToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.enableVertexColorToolStripMenuItem.Text = "Enable Vertex Color";
            this.enableVertexColorToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableVertexColorToolStripMenuItem_CheckedChanged);
            // 
            // enableSemiTransparencyToolStripMenuItem
            // 
            this.enableSemiTransparencyToolStripMenuItem.CheckOnClick = true;
            this.enableSemiTransparencyToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_EnableSemiTransparency;
            this.enableSemiTransparencyToolStripMenuItem.Name = "enableSemiTransparencyToolStripMenuItem";
            this.enableSemiTransparencyToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.enableSemiTransparencyToolStripMenuItem.Text = "Enable Semi-Transparency";
            this.enableSemiTransparencyToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableSemiTransparencyToolStripMenuItem_CheckedChanged);
            // 
            // forceDoubleSidedToolStripMenuItem
            // 
            this.forceDoubleSidedToolStripMenuItem.CheckOnClick = true;
            this.forceDoubleSidedToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_ForceDoubleSided;
            this.forceDoubleSidedToolStripMenuItem.Name = "forceDoubleSidedToolStripMenuItem";
            this.forceDoubleSidedToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.forceDoubleSidedToolStripMenuItem.Text = "Force Double-Sided";
            this.forceDoubleSidedToolStripMenuItem.CheckedChanged += new System.EventHandler(this.forceDoubleSidedToolStripMenuItem_CheckedChanged);
            // 
            // autoAttachLimbsToolStripMenuItem
            // 
            this.autoAttachLimbsToolStripMenuItem.CheckOnClick = true;
            this.autoAttachLimbsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_AutoAttachLimbs;
            this.autoAttachLimbsToolStripMenuItem.Name = "autoAttachLimbsToolStripMenuItem";
            this.autoAttachLimbsToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.autoAttachLimbsToolStripMenuItem.Text = "Auto Attach Limbs";
            this.autoAttachLimbsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoAttachLimbsToolStripMenuItem_CheckedChanged);
            this.autoAttachLimbsToolStripMenuItem.CheckStateChanged += new System.EventHandler(this.autoAttachLimbsToolStripMenuItem_CheckStateChanged);
            this.autoAttachLimbsToolStripMenuItem.Click += new System.EventHandler(this.autoAttachLimbsToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(222, 6);
            // 
            // subModelVisibilityToolStripMenuItem
            // 
            this.subModelVisibilityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.subModelVisibilityAllToolStripMenuItem,
            this.subModelVisibilitySelectedToolStripMenuItem,
            this.subModelVisibilityWithSameTMDIDToolStripMenuItem});
            this.subModelVisibilityToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_SubModelVisibility;
            this.subModelVisibilityToolStripMenuItem.Name = "subModelVisibilityToolStripMenuItem";
            this.subModelVisibilityToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.subModelVisibilityToolStripMenuItem.Text = "Sub-Model Visibility";
            // 
            // subModelVisibilityAllToolStripMenuItem
            // 
            this.subModelVisibilityAllToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_SubModelVisibility_All;
            this.subModelVisibilityAllToolStripMenuItem.Name = "subModelVisibilityAllToolStripMenuItem";
            this.subModelVisibilityAllToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.subModelVisibilityAllToolStripMenuItem.Text = "All";
            this.subModelVisibilityAllToolStripMenuItem.Click += new System.EventHandler(this.subModelVisibilityAllToolStripMenuItem_Click);
            // 
            // subModelVisibilitySelectedToolStripMenuItem
            // 
            this.subModelVisibilitySelectedToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_SubModelVisibility_Selected;
            this.subModelVisibilitySelectedToolStripMenuItem.Name = "subModelVisibilitySelectedToolStripMenuItem";
            this.subModelVisibilitySelectedToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.subModelVisibilitySelectedToolStripMenuItem.Text = "Selected";
            this.subModelVisibilitySelectedToolStripMenuItem.Click += new System.EventHandler(this.subModelVisibilitySelectedToolStripMenuItem_Click);
            // 
            // subModelVisibilityWithSameTMDIDToolStripMenuItem
            // 
            this.subModelVisibilityWithSameTMDIDToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_SubModelVisibility_WithSameTMDID;
            this.subModelVisibilityWithSameTMDIDToolStripMenuItem.Name = "subModelVisibilityWithSameTMDIDToolStripMenuItem";
            this.subModelVisibilityWithSameTMDIDToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.subModelVisibilityWithSameTMDIDToolStripMenuItem.Text = "With Same TMD ID";
            this.subModelVisibilityWithSameTMDIDToolStripMenuItem.Click += new System.EventHandler(this.subModelVisibilityWithSameTMDIDToolStripMenuItem_Click);
            // 
            // autoFocusToolStripMenuItem
            // 
            this.autoFocusToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoFocusOnRootModelToolStripMenuItem,
            this.autoFocusOnSubModelToolStripMenuItem,
            this.autoFocusIncludeWholeModelToolStripMenuItem,
            this.autoFocusIncludeCheckedModelsToolStripMenuItem,
            this.autoFocusResetCameraRotationToolStripMenuItem});
            this.autoFocusToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Focus;
            this.autoFocusToolStripMenuItem.Name = "autoFocusToolStripMenuItem";
            this.autoFocusToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.autoFocusToolStripMenuItem.Text = "Auto Focus";
            // 
            // autoFocusOnRootModelToolStripMenuItem
            // 
            this.autoFocusOnRootModelToolStripMenuItem.CheckOnClick = true;
            this.autoFocusOnRootModelToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Focus_AutoFocusOnRootModel;
            this.autoFocusOnRootModelToolStripMenuItem.Name = "autoFocusOnRootModelToolStripMenuItem";
            this.autoFocusOnRootModelToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.autoFocusOnRootModelToolStripMenuItem.Text = "Auto Focus on Root Model";
            this.autoFocusOnRootModelToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoFocusOnRootModelToolStripMenuItem_CheckedChanged);
            // 
            // autoFocusOnSubModelToolStripMenuItem
            // 
            this.autoFocusOnSubModelToolStripMenuItem.CheckOnClick = true;
            this.autoFocusOnSubModelToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Focus_AutoFocusOnSubModel;
            this.autoFocusOnSubModelToolStripMenuItem.Name = "autoFocusOnSubModelToolStripMenuItem";
            this.autoFocusOnSubModelToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.autoFocusOnSubModelToolStripMenuItem.Text = "Auto Focus on Sub-Model";
            this.autoFocusOnSubModelToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoFocusOnSubModelToolStripMenuItem_CheckedChanged);
            // 
            // autoFocusIncludeWholeModelToolStripMenuItem
            // 
            this.autoFocusIncludeWholeModelToolStripMenuItem.CheckOnClick = true;
            this.autoFocusIncludeWholeModelToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Focus_IncludeWholeModel;
            this.autoFocusIncludeWholeModelToolStripMenuItem.Name = "autoFocusIncludeWholeModelToolStripMenuItem";
            this.autoFocusIncludeWholeModelToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.autoFocusIncludeWholeModelToolStripMenuItem.Text = "Include Whole Model";
            this.autoFocusIncludeWholeModelToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoFocusIncludeWholeModelToolStripMenuItem_CheckedChanged);
            // 
            // autoFocusIncludeCheckedModelsToolStripMenuItem
            // 
            this.autoFocusIncludeCheckedModelsToolStripMenuItem.CheckOnClick = true;
            this.autoFocusIncludeCheckedModelsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Focus_IncludeCheckedModels;
            this.autoFocusIncludeCheckedModelsToolStripMenuItem.Name = "autoFocusIncludeCheckedModelsToolStripMenuItem";
            this.autoFocusIncludeCheckedModelsToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.autoFocusIncludeCheckedModelsToolStripMenuItem.Text = "Include Checked Models";
            this.autoFocusIncludeCheckedModelsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoFocusIncludeCheckedModelsToolStripMenuItem_CheckedChanged);
            // 
            // autoFocusResetCameraRotationToolStripMenuItem
            // 
            this.autoFocusResetCameraRotationToolStripMenuItem.CheckOnClick = true;
            this.autoFocusResetCameraRotationToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_Focus_ResetCameraRotation;
            this.autoFocusResetCameraRotationToolStripMenuItem.Name = "autoFocusResetCameraRotationToolStripMenuItem";
            this.autoFocusResetCameraRotationToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.autoFocusResetCameraRotationToolStripMenuItem.Text = "Reset Camera Rotation";
            this.autoFocusResetCameraRotationToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoFocusResetCameraRotationToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(222, 6);
            // 
            // setAmbientColorToolStripMenuItem
            // 
            this.setAmbientColorToolStripMenuItem.Name = "setAmbientColorToolStripMenuItem";
            this.setAmbientColorToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.setAmbientColorToolStripMenuItem.Text = "Set Ambient Color";
            this.setAmbientColorToolStripMenuItem.Click += new System.EventHandler(this.setAmbientColorToolStripMenuItem_Click);
            // 
            // setBackgroundColorToolStripMenuItem
            // 
            this.setBackgroundColorToolStripMenuItem.Name = "setBackgroundColorToolStripMenuItem";
            this.setBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.setBackgroundColorToolStripMenuItem.Text = "Set Background Color";
            this.setBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.setBackgroundColorToolStripMenuItem_Click);
            // 
            // setWireframeVerticesColorToolStripMenuItem
            // 
            this.setWireframeVerticesColorToolStripMenuItem.Name = "setWireframeVerticesColorToolStripMenuItem";
            this.setWireframeVerticesColorToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.setWireframeVerticesColorToolStripMenuItem.Text = "Set Wireframe/Vertices Color";
            this.setWireframeVerticesColorToolStripMenuItem.Click += new System.EventHandler(this.setSolidWireframeVerticesColorToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(222, 6);
            // 
            // lineRendererToolStripMenuItem
            // 
            this.lineRendererToolStripMenuItem.CheckOnClick = true;
            this.lineRendererToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Models_LineRenderer;
            this.lineRendererToolStripMenuItem.Name = "lineRendererToolStripMenuItem";
            this.lineRendererToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.lineRendererToolStripMenuItem.Text = "Line Renderer";
            this.lineRendererToolStripMenuItem.CheckedChanged += new System.EventHandler(this.lineRendererToolStripMenuItem_CheckedChanged);
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportTexturesToolStripMenuItem,
            this.toolStripSeparator3,
            this.drawToVRAMToolStripMenuItem,
            this.drawAllToVRAMToolStripMenuItem,
            this.findByPageToolStripMenuItem,
            this.clearSearchToolStripMenuItem,
            this.setPaletteIndexToolStripMenuItem,
            this.toolStripSeparator1,
            this.showTexturePaletteToolStripMenuItem,
            this.showTextureSemiTransparencyToolStripMenuItem,
            this.showTextureUVsToolStripMenuItem,
            this.toolStripSeparator15,
            this.showMissingTexturesToolStripMenuItem,
            this.autoDrawModelTexturesToolStripMenuItem,
            this.autoPackModelTexturesToolStripMenuItem,
            this.toolStripSeparator6,
            this.setMaskColorToolStripMenuItem});
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.texturesToolStripMenuItem.Text = "Textures";
            // 
            // exportTexturesToolStripMenuItem
            // 
            this.exportTexturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedTexturesToolStripMenuItem,
            this.exportAllTexturesToolStripMenuItem});
            this.exportTexturesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_Export;
            this.exportTexturesToolStripMenuItem.Name = "exportTexturesToolStripMenuItem";
            this.exportTexturesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.exportTexturesToolStripMenuItem.Text = "Export Textures";
            // 
            // exportSelectedTexturesToolStripMenuItem
            // 
            this.exportSelectedTexturesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_Export_Selected;
            this.exportSelectedTexturesToolStripMenuItem.Name = "exportSelectedTexturesToolStripMenuItem";
            this.exportSelectedTexturesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.exportSelectedTexturesToolStripMenuItem.Text = "Export Selected Textures";
            this.exportSelectedTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedTextures_Click);
            // 
            // exportAllTexturesToolStripMenuItem
            // 
            this.exportAllTexturesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_Export_All;
            this.exportAllTexturesToolStripMenuItem.Name = "exportAllTexturesToolStripMenuItem";
            this.exportAllTexturesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.exportAllTexturesToolStripMenuItem.Text = "Export All Textures";
            this.exportAllTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportAllTextures_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(203, 6);
            // 
            // drawToVRAMToolStripMenuItem
            // 
            this.drawToVRAMToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_DrawToVRAM;
            this.drawToVRAMToolStripMenuItem.Name = "drawToVRAMToolStripMenuItem";
            this.drawToVRAMToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.drawToVRAMToolStripMenuItem.Text = "Draw to VRAM";
            this.drawToVRAMToolStripMenuItem.Click += new System.EventHandler(this.drawSelectedToVRAM_Click);
            // 
            // drawAllToVRAMToolStripMenuItem
            // 
            this.drawAllToVRAMToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_DrawToVRAM_All;
            this.drawAllToVRAMToolStripMenuItem.Name = "drawAllToVRAMToolStripMenuItem";
            this.drawAllToVRAMToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.drawAllToVRAMToolStripMenuItem.Text = "Draw All to VRAM";
            this.drawAllToVRAMToolStripMenuItem.Click += new System.EventHandler(this.drawAllToVRAM_Click);
            // 
            // findByPageToolStripMenuItem
            // 
            this.findByPageToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Find;
            this.findByPageToolStripMenuItem.Name = "findByPageToolStripMenuItem";
            this.findByPageToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.findByPageToolStripMenuItem.Text = "Find by Page";
            this.findByPageToolStripMenuItem.Click += new System.EventHandler(this.findTextureByVRAMPage_Click);
            // 
            // clearSearchToolStripMenuItem
            // 
            this.clearSearchToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.FindClear;
            this.clearSearchToolStripMenuItem.Name = "clearSearchToolStripMenuItem";
            this.clearSearchToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.clearSearchToolStripMenuItem.Text = "Clear Find Results";
            this.clearSearchToolStripMenuItem.Click += new System.EventHandler(this.clearTextureFindResults_Click);
            // 
            // setPaletteIndexToolStripMenuItem
            // 
            this.setPaletteIndexToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_SetPaletteIndex;
            this.setPaletteIndexToolStripMenuItem.Name = "setPaletteIndexToolStripMenuItem";
            this.setPaletteIndexToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.setPaletteIndexToolStripMenuItem.Text = "Set CLUT Index";
            this.setPaletteIndexToolStripMenuItem.Click += new System.EventHandler(this.setPaletteIndexToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(203, 6);
            // 
            // showTexturePaletteToolStripMenuItem
            // 
            this.showTexturePaletteToolStripMenuItem.CheckOnClick = true;
            this.showTexturePaletteToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_ShowPalette;
            this.showTexturePaletteToolStripMenuItem.Name = "showTexturePaletteToolStripMenuItem";
            this.showTexturePaletteToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.showTexturePaletteToolStripMenuItem.Text = "Show Palette";
            this.showTexturePaletteToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showTexturePaletteToolStripMenuItem_CheckedChanged);
            // 
            // showTextureSemiTransparencyToolStripMenuItem
            // 
            this.showTextureSemiTransparencyToolStripMenuItem.CheckOnClick = true;
            this.showTextureSemiTransparencyToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_ShowSemiTransparency;
            this.showTextureSemiTransparencyToolStripMenuItem.Name = "showTextureSemiTransparencyToolStripMenuItem";
            this.showTextureSemiTransparencyToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.showTextureSemiTransparencyToolStripMenuItem.Text = "Show Semi-Transparency";
            this.showTextureSemiTransparencyToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showTextureSemiTransparencyToolStripMenuItem_CheckedChanged);
            // 
            // showTextureUVsToolStripMenuItem
            // 
            this.showTextureUVsToolStripMenuItem.CheckOnClick = true;
            this.showTextureUVsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_ShowUVs;
            this.showTextureUVsToolStripMenuItem.Name = "showTextureUVsToolStripMenuItem";
            this.showTextureUVsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.showTextureUVsToolStripMenuItem.Text = "Show UVs";
            this.showTextureUVsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showTextureUVsToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(203, 6);
            // 
            // showMissingTexturesToolStripMenuItem
            // 
            this.showMissingTexturesToolStripMenuItem.CheckOnClick = true;
            this.showMissingTexturesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_ShowMissingTextures;
            this.showMissingTexturesToolStripMenuItem.Name = "showMissingTexturesToolStripMenuItem";
            this.showMissingTexturesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.showMissingTexturesToolStripMenuItem.Text = "Show Missing Textures";
            this.showMissingTexturesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showMissingTexturesToolStripMenuItem_CheckedChanged);
            // 
            // autoDrawModelTexturesToolStripMenuItem
            // 
            this.autoDrawModelTexturesToolStripMenuItem.CheckOnClick = true;
            this.autoDrawModelTexturesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_AutoDrawTextures;
            this.autoDrawModelTexturesToolStripMenuItem.Name = "autoDrawModelTexturesToolStripMenuItem";
            this.autoDrawModelTexturesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.autoDrawModelTexturesToolStripMenuItem.Text = "Auto Draw Textures";
            this.autoDrawModelTexturesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoDrawModelTexturesToolStripMenuItem_CheckedChanged);
            // 
            // autoPackModelTexturesToolStripMenuItem
            // 
            this.autoPackModelTexturesToolStripMenuItem.CheckOnClick = true;
            this.autoPackModelTexturesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Textures_AutoPackTextures;
            this.autoPackModelTexturesToolStripMenuItem.Name = "autoPackModelTexturesToolStripMenuItem";
            this.autoPackModelTexturesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.autoPackModelTexturesToolStripMenuItem.Text = "Auto Pack Textures";
            this.autoPackModelTexturesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoPackModelTexturesToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(203, 6);
            // 
            // setMaskColorToolStripMenuItem
            // 
            this.setMaskColorToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.setMaskColorToolStripMenuItem.Name = "setMaskColorToolStripMenuItem";
            this.setMaskColorToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.setMaskColorToolStripMenuItem.Text = "Set Mask Color";
            this.setMaskColorToolStripMenuItem.Click += new System.EventHandler(this.setMaskColorToolStripMenuItem_Click);
            // 
            // vramToolStripMenuItem
            // 
            this.vramToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportVRAMPagesToolStripMenuItem,
            this.toolStripSeparator9,
            this.clearPageToolStripMenuItem,
            this.clearAllPagesToolStripMenuItem,
            this.toolStripSeparator4,
            this.showVRAMSemiTransparencyToolStripMenuItem,
            this.showVRAMUVsToolStripMenuItem});
            this.vramToolStripMenuItem.Name = "vramToolStripMenuItem";
            this.vramToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.vramToolStripMenuItem.Text = "VRAM";
            // 
            // exportVRAMPagesToolStripMenuItem
            // 
            this.exportVRAMPagesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedVRAMPageToolStripMenuItem,
            this.exportDrawnToVRAMPagesToolStripMenuItem,
            this.exportAllVRAMPagesToolStripMenuItem});
            this.exportVRAMPagesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_Export;
            this.exportVRAMPagesToolStripMenuItem.Name = "exportVRAMPagesToolStripMenuItem";
            this.exportVRAMPagesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.exportVRAMPagesToolStripMenuItem.Text = "Export Pages";
            // 
            // exportSelectedVRAMPageToolStripMenuItem
            // 
            this.exportSelectedVRAMPageToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_Export_Selected;
            this.exportSelectedVRAMPageToolStripMenuItem.Name = "exportSelectedVRAMPageToolStripMenuItem";
            this.exportSelectedVRAMPageToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exportSelectedVRAMPageToolStripMenuItem.Text = "Export Selected Page";
            this.exportSelectedVRAMPageToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedVRAMPage_Click);
            // 
            // exportDrawnToVRAMPagesToolStripMenuItem
            // 
            this.exportDrawnToVRAMPagesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_Export_DrawnTo;
            this.exportDrawnToVRAMPagesToolStripMenuItem.Name = "exportDrawnToVRAMPagesToolStripMenuItem";
            this.exportDrawnToVRAMPagesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exportDrawnToVRAMPagesToolStripMenuItem.Text = "Export Drawn-to Pages";
            this.exportDrawnToVRAMPagesToolStripMenuItem.Click += new System.EventHandler(this.exportDrawnToVRAMPages_Click);
            // 
            // exportAllVRAMPagesToolStripMenuItem
            // 
            this.exportAllVRAMPagesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_Export_All;
            this.exportAllVRAMPagesToolStripMenuItem.Name = "exportAllVRAMPagesToolStripMenuItem";
            this.exportAllVRAMPagesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exportAllVRAMPagesToolStripMenuItem.Text = "Export All Pages";
            this.exportAllVRAMPagesToolStripMenuItem.Click += new System.EventHandler(this.exportAllVRAMPages_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(203, 6);
            // 
            // clearPageToolStripMenuItem
            // 
            this.clearPageToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_Clear_Selected;
            this.clearPageToolStripMenuItem.Name = "clearPageToolStripMenuItem";
            this.clearPageToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.clearPageToolStripMenuItem.Text = "Clear Page";
            this.clearPageToolStripMenuItem.Click += new System.EventHandler(this.clearVRAMPage_Click);
            // 
            // clearAllPagesToolStripMenuItem
            // 
            this.clearAllPagesToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_Clear_All;
            this.clearAllPagesToolStripMenuItem.Name = "clearAllPagesToolStripMenuItem";
            this.clearAllPagesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.clearAllPagesToolStripMenuItem.Text = "Clear All Pages";
            this.clearAllPagesToolStripMenuItem.Click += new System.EventHandler(this.clearAllVRAMPages_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(203, 6);
            // 
            // showVRAMSemiTransparencyToolStripMenuItem
            // 
            this.showVRAMSemiTransparencyToolStripMenuItem.CheckOnClick = true;
            this.showVRAMSemiTransparencyToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_ShowSemiTransparency;
            this.showVRAMSemiTransparencyToolStripMenuItem.Name = "showVRAMSemiTransparencyToolStripMenuItem";
            this.showVRAMSemiTransparencyToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.showVRAMSemiTransparencyToolStripMenuItem.Text = "Show Semi-Transparency";
            this.showVRAMSemiTransparencyToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showVRAMSemiTransparencyToolStripMenuItem_CheckedChanged);
            // 
            // showVRAMUVsToolStripMenuItem
            // 
            this.showVRAMUVsToolStripMenuItem.CheckOnClick = true;
            this.showVRAMUVsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.VRAM_ShowUVs;
            this.showVRAMUVsToolStripMenuItem.Name = "showVRAMUVsToolStripMenuItem";
            this.showVRAMUVsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.showVRAMUVsToolStripMenuItem.Text = "Show UVs";
            this.showVRAMUVsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showVRAMUVsToolStripMenuItem_CheckedChanged);
            // 
            // animationsToolStripMenuItem
            // 
            this.animationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkedAnimationsToolStripMenuItem,
            this.toolStripSeparator13,
            this.autoPlayAnimationsToolStripMenuItem,
            this.autoSelectAnimationModelToolStripMenuItem,
            this.toolStripSeparator17,
            this.showTMDBindingsToolStripMenuItem});
            this.animationsToolStripMenuItem.Name = "animationsToolStripMenuItem";
            this.animationsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.animationsToolStripMenuItem.Text = "Animations";
            // 
            // checkedAnimationsToolStripMenuItem
            // 
            this.checkedAnimationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllAnimationsToolStripMenuItem,
            this.uncheckAllAnimationsToolStripMenuItem});
            this.checkedAnimationsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.CheckBoxes;
            this.checkedAnimationsToolStripMenuItem.Name = "checkedAnimationsToolStripMenuItem";
            this.checkedAnimationsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.checkedAnimationsToolStripMenuItem.Text = "Checked Animations";
            // 
            // checkAllAnimationsToolStripMenuItem
            // 
            this.checkAllAnimationsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.CheckBox_Checked;
            this.checkAllAnimationsToolStripMenuItem.Name = "checkAllAnimationsToolStripMenuItem";
            this.checkAllAnimationsToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.checkAllAnimationsToolStripMenuItem.Text = "Check All";
            this.checkAllAnimationsToolStripMenuItem.Click += new System.EventHandler(this.checkAllAnimationsToolStripMenuItem_Click);
            // 
            // uncheckAllAnimationsToolStripMenuItem
            // 
            this.uncheckAllAnimationsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.CheckBox_Unchecked;
            this.uncheckAllAnimationsToolStripMenuItem.Name = "uncheckAllAnimationsToolStripMenuItem";
            this.uncheckAllAnimationsToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.uncheckAllAnimationsToolStripMenuItem.Text = "Uncheck All";
            this.uncheckAllAnimationsToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllAnimationsToolStripMenuItem_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(181, 6);
            // 
            // autoPlayAnimationsToolStripMenuItem
            // 
            this.autoPlayAnimationsToolStripMenuItem.CheckOnClick = true;
            this.autoPlayAnimationsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Animations_AutoPlayAnimation;
            this.autoPlayAnimationsToolStripMenuItem.Name = "autoPlayAnimationsToolStripMenuItem";
            this.autoPlayAnimationsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.autoPlayAnimationsToolStripMenuItem.Text = "Auto Play Animation";
            this.autoPlayAnimationsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoPlayAnimationsToolStripMenuItem_CheckedChanged);
            // 
            // autoSelectAnimationModelToolStripMenuItem
            // 
            this.autoSelectAnimationModelToolStripMenuItem.CheckOnClick = true;
            this.autoSelectAnimationModelToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Animations_AutoSelectModel;
            this.autoSelectAnimationModelToolStripMenuItem.Name = "autoSelectAnimationModelToolStripMenuItem";
            this.autoSelectAnimationModelToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.autoSelectAnimationModelToolStripMenuItem.Text = "Auto Select Model";
            this.autoSelectAnimationModelToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoSelectAnimationModelToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(181, 6);
            // 
            // showTMDBindingsToolStripMenuItem
            // 
            this.showTMDBindingsToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Animations_EditTMDBindings;
            this.showTMDBindingsToolStripMenuItem.Name = "showTMDBindingsToolStripMenuItem";
            this.showTMDBindingsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.showTMDBindingsToolStripMenuItem.Text = "Edit TMD Bindings";
            this.showTMDBindingsToolStripMenuItem.Click += new System.EventHandler(this.showTMDBindingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.videoTutorialToolStripMenuItem,
            this.compatibilityListToolStripMenuItem,
            this.viewOnGitHubToolStripMenuItem,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // videoTutorialToolStripMenuItem
            // 
            this.videoTutorialToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Logo_YouTube;
            this.videoTutorialToolStripMenuItem.Name = "videoTutorialToolStripMenuItem";
            this.videoTutorialToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.videoTutorialToolStripMenuItem.Text = "Video Tutorial";
            this.videoTutorialToolStripMenuItem.Click += new System.EventHandler(this.videoTutorialToolStripMenuItem_Click);
            // 
            // compatibilityListToolStripMenuItem
            // 
            this.compatibilityListToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Help_CompatibilityList;
            this.compatibilityListToolStripMenuItem.Name = "compatibilityListToolStripMenuItem";
            this.compatibilityListToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.compatibilityListToolStripMenuItem.Text = "Compatibility List";
            this.compatibilityListToolStripMenuItem.Click += new System.EventHandler(this.compatibilityListToolStripMenuItem_Click);
            // 
            // viewOnGitHubToolStripMenuItem
            // 
            this.viewOnGitHubToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Logo_GitHub;
            this.viewOnGitHubToolStripMenuItem.Name = "viewOnGitHubToolStripMenuItem";
            this.viewOnGitHubToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.viewOnGitHubToolStripMenuItem.Text = "View on GitHub";
            this.viewOnGitHubToolStripMenuItem.Click += new System.EventHandler(this.viewOnGitHubToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::PSXPrev.Properties.Resources.Information;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusTotalFilesProgressBar,
            this.statusCurrentFileProgressBar,
            this.statusMessageLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 649);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.ShowItemToolTips = true;
            this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip";
            // 
            // statusTotalFilesProgressBar
            // 
            this.statusTotalFilesProgressBar.Name = "statusTotalFilesProgressBar";
            this.statusTotalFilesProgressBar.Size = new System.Drawing.Size(100, 16);
            this.statusTotalFilesProgressBar.ToolTipText = "Total Files";
            // 
            // statusCurrentFileProgressBar
            // 
            this.statusCurrentFileProgressBar.Name = "statusCurrentFileProgressBar";
            this.statusCurrentFileProgressBar.Size = new System.Drawing.Size(100, 16);
            this.statusCurrentFileProgressBar.ToolTipText = "Current File";
            // 
            // statusMessageLabel
            // 
            this.statusMessageLabel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
            this.statusMessageLabel.Name = "statusMessageLabel";
            this.statusMessageLabel.Size = new System.Drawing.Size(48, 17);
            this.statusMessageLabel.Text = "Waiting";
            this.statusMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sceneControlsFlowLayoutPanel
            // 
            this.sceneControlsFlowLayoutPanel.AutoSize = true;
            this.sceneControlsFlowLayoutPanel.Controls.Add(this.lightRotationFlowLayoutPanel);
            this.sceneControlsFlowLayoutPanel.Controls.Add(this.lightIntensityFlowLayoutPanel4);
            this.sceneControlsFlowLayoutPanel.Controls.Add(this.cameraFOVFlowLayoutPanel);
            this.sceneControlsFlowLayoutPanel.Controls.Add(this.gizmoSnapFlowLayoutPanel);
            this.sceneControlsFlowLayoutPanel.Controls.Add(this.wireframeVertexSizeFlowLayoutPanel);
            this.sceneControlsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sceneControlsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.sceneControlsFlowLayoutPanel.Location = new System.Drawing.Point(0, 623);
            this.sceneControlsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.sceneControlsFlowLayoutPanel.Name = "sceneControlsFlowLayoutPanel";
            this.sceneControlsFlowLayoutPanel.Size = new System.Drawing.Size(1008, 26);
            this.sceneControlsFlowLayoutPanel.TabIndex = 21;
            // 
            // lightRotationFlowLayoutPanel
            // 
            this.lightRotationFlowLayoutPanel.AutoSize = true;
            this.lightRotationFlowLayoutPanel.Controls.Add(this.lightRotationLabel);
            this.lightRotationFlowLayoutPanel.Controls.Add(this.lightYawNumericUpDown);
            this.lightRotationFlowLayoutPanel.Controls.Add(this.lightPitchNumericUpDown);
            this.lightRotationFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lightRotationFlowLayoutPanel.Location = new System.Drawing.Point(828, 0);
            this.lightRotationFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.lightRotationFlowLayoutPanel.Name = "lightRotationFlowLayoutPanel";
            this.lightRotationFlowLayoutPanel.Size = new System.Drawing.Size(180, 26);
            this.lightRotationFlowLayoutPanel.TabIndex = 104;
            // 
            // lightRotationLabel
            // 
            this.lightRotationLabel.AutoSize = true;
            this.lightRotationLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lightRotationLabel.Location = new System.Drawing.Point(3, 0);
            this.lightRotationLabel.Name = "lightRotationLabel";
            this.lightRotationLabel.Size = new System.Drawing.Size(76, 26);
            this.lightRotationLabel.TabIndex = 15;
            this.lightRotationLabel.Text = "Light Rotation:";
            this.lightRotationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lightYawNumericUpDown
            // 
            this.lightYawNumericUpDown.Location = new System.Drawing.Point(85, 3);
            this.lightYawNumericUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.lightYawNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.lightYawNumericUpDown.Name = "lightYawNumericUpDown";
            this.lightYawNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.lightYawNumericUpDown.TabIndex = 0;
            this.toolTip.SetToolTip(this.lightYawNumericUpDown, "Yaw");
            this.lightYawNumericUpDown.ValueChanged += new System.EventHandler(this.lightYawNumericUpDown_ValueChanged);
            // 
            // lightPitchNumericUpDown
            // 
            this.lightPitchNumericUpDown.Location = new System.Drawing.Point(134, 3);
            this.lightPitchNumericUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.lightPitchNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.lightPitchNumericUpDown.Name = "lightPitchNumericUpDown";
            this.lightPitchNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.lightPitchNumericUpDown.TabIndex = 1;
            this.toolTip.SetToolTip(this.lightPitchNumericUpDown, "Pitch");
            this.lightPitchNumericUpDown.ValueChanged += new System.EventHandler(this.lightPitchNumericUpDown_ValueChanged);
            // 
            // lightIntensityFlowLayoutPanel4
            // 
            this.lightIntensityFlowLayoutPanel4.AutoSize = true;
            this.lightIntensityFlowLayoutPanel4.Controls.Add(this.lightIntensityLabel);
            this.lightIntensityFlowLayoutPanel4.Controls.Add(this.lightIntensityNumericUpDown);
            this.lightIntensityFlowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lightIntensityFlowLayoutPanel4.Location = new System.Drawing.Point(691, 0);
            this.lightIntensityFlowLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.lightIntensityFlowLayoutPanel4.Name = "lightIntensityFlowLayoutPanel4";
            this.lightIntensityFlowLayoutPanel4.Size = new System.Drawing.Size(137, 26);
            this.lightIntensityFlowLayoutPanel4.TabIndex = 103;
            // 
            // lightIntensityLabel
            // 
            this.lightIntensityLabel.AutoSize = true;
            this.lightIntensityLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lightIntensityLabel.Location = new System.Drawing.Point(3, 0);
            this.lightIntensityLabel.Name = "lightIntensityLabel";
            this.lightIntensityLabel.Size = new System.Drawing.Size(75, 26);
            this.lightIntensityLabel.TabIndex = 19;
            this.lightIntensityLabel.Text = "Light Intensity:";
            this.lightIntensityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lightIntensityNumericUpDown
            // 
            this.lightIntensityNumericUpDown.DecimalPlaces = 2;
            this.lightIntensityNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.lightIntensityNumericUpDown.Location = new System.Drawing.Point(84, 3);
            this.lightIntensityNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.lightIntensityNumericUpDown.Name = "lightIntensityNumericUpDown";
            this.lightIntensityNumericUpDown.Size = new System.Drawing.Size(50, 20);
            this.lightIntensityNumericUpDown.TabIndex = 0;
            this.lightIntensityNumericUpDown.ValueChanged += new System.EventHandler(this.lightIntensityNumericUpDown_ValueChanged);
            // 
            // cameraFOVFlowLayoutPanel
            // 
            this.cameraFOVFlowLayoutPanel.AutoSize = true;
            this.cameraFOVFlowLayoutPanel.Controls.Add(this.cameraFOVLabel);
            this.cameraFOVFlowLayoutPanel.Controls.Add(this.cameraFOVUpDown);
            this.cameraFOVFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cameraFOVFlowLayoutPanel.Location = new System.Drawing.Point(605, 0);
            this.cameraFOVFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.cameraFOVFlowLayoutPanel.Name = "cameraFOVFlowLayoutPanel";
            this.cameraFOVFlowLayoutPanel.Size = new System.Drawing.Size(86, 26);
            this.cameraFOVFlowLayoutPanel.TabIndex = 102;
            // 
            // cameraFOVLabel
            // 
            this.cameraFOVLabel.AutoSize = true;
            this.cameraFOVLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.cameraFOVLabel.Location = new System.Drawing.Point(3, 0);
            this.cameraFOVLabel.Name = "cameraFOVLabel";
            this.cameraFOVLabel.Size = new System.Drawing.Size(31, 26);
            this.cameraFOVLabel.TabIndex = 24;
            this.cameraFOVLabel.Text = "FOV:";
            this.cameraFOVLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cameraFOVUpDown
            // 
            this.cameraFOVUpDown.Location = new System.Drawing.Point(40, 3);
            this.cameraFOVUpDown.Maximum = new decimal(new int[] {
            160,
            0,
            0,
            0});
            this.cameraFOVUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cameraFOVUpDown.Name = "cameraFOVUpDown";
            this.cameraFOVUpDown.Size = new System.Drawing.Size(43, 20);
            this.cameraFOVUpDown.TabIndex = 0;
            this.cameraFOVUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cameraFOVUpDown.ValueChanged += new System.EventHandler(this.cameraFOVUpDown_ValueChanged);
            // 
            // gizmoSnapFlowLayoutPanel
            // 
            this.gizmoSnapFlowLayoutPanel.AutoSize = true;
            this.gizmoSnapFlowLayoutPanel.Controls.Add(this.gizmoSnapLabel);
            this.gizmoSnapFlowLayoutPanel.Controls.Add(this.gridSnapUpDown);
            this.gizmoSnapFlowLayoutPanel.Controls.Add(this.angleSnapUpDown);
            this.gizmoSnapFlowLayoutPanel.Controls.Add(this.scaleSnapUpDown);
            this.gizmoSnapFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gizmoSnapFlowLayoutPanel.Location = new System.Drawing.Point(396, 0);
            this.gizmoSnapFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.gizmoSnapFlowLayoutPanel.Name = "gizmoSnapFlowLayoutPanel";
            this.gizmoSnapFlowLayoutPanel.Size = new System.Drawing.Size(209, 26);
            this.gizmoSnapFlowLayoutPanel.TabIndex = 101;
            // 
            // gizmoSnapLabel
            // 
            this.gizmoSnapLabel.AutoSize = true;
            this.gizmoSnapLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.gizmoSnapLabel.Location = new System.Drawing.Point(3, 0);
            this.gizmoSnapLabel.Name = "gizmoSnapLabel";
            this.gizmoSnapLabel.Size = new System.Drawing.Size(35, 26);
            this.gizmoSnapLabel.TabIndex = 17;
            this.gizmoSnapLabel.Text = "Snap:";
            this.gizmoSnapLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gridSnapUpDown
            // 
            this.gridSnapUpDown.DecimalPlaces = 1;
            this.gridSnapUpDown.Location = new System.Drawing.Point(44, 3);
            this.gridSnapUpDown.Name = "gridSnapUpDown";
            this.gridSnapUpDown.Size = new System.Drawing.Size(50, 20);
            this.gridSnapUpDown.TabIndex = 0;
            this.gridSnapUpDown.ValueChanged += new System.EventHandler(this.gridSnapUpDown_ValueChanged);
            // 
            // angleSnapUpDown
            // 
            this.angleSnapUpDown.DecimalPlaces = 1;
            this.angleSnapUpDown.Location = new System.Drawing.Point(100, 3);
            this.angleSnapUpDown.Name = "angleSnapUpDown";
            this.angleSnapUpDown.Size = new System.Drawing.Size(50, 20);
            this.angleSnapUpDown.TabIndex = 1;
            this.toolTip.SetToolTip(this.angleSnapUpDown, "Angle in Degrees");
            this.angleSnapUpDown.ValueChanged += new System.EventHandler(this.angleSnapUpDown_ValueChanged);
            // 
            // scaleSnapUpDown
            // 
            this.scaleSnapUpDown.DecimalPlaces = 2;
            this.scaleSnapUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.scaleSnapUpDown.Location = new System.Drawing.Point(156, 3);
            this.scaleSnapUpDown.Name = "scaleSnapUpDown";
            this.scaleSnapUpDown.Size = new System.Drawing.Size(50, 20);
            this.scaleSnapUpDown.TabIndex = 2;
            this.scaleSnapUpDown.ValueChanged += new System.EventHandler(this.scaleSnapUpDown_ValueChanged);
            // 
            // wireframeVertexSizeFlowLayoutPanel
            // 
            this.wireframeVertexSizeFlowLayoutPanel.AutoSize = true;
            this.wireframeVertexSizeFlowLayoutPanel.Controls.Add(this.wireframeVertexSizeLabel);
            this.wireframeVertexSizeFlowLayoutPanel.Controls.Add(this.wireframeSizeUpDown);
            this.wireframeVertexSizeFlowLayoutPanel.Controls.Add(this.vertexSizeUpDown);
            this.wireframeVertexSizeFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.wireframeVertexSizeFlowLayoutPanel.Location = new System.Drawing.Point(242, 0);
            this.wireframeVertexSizeFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.wireframeVertexSizeFlowLayoutPanel.Name = "wireframeVertexSizeFlowLayoutPanel";
            this.wireframeVertexSizeFlowLayoutPanel.Size = new System.Drawing.Size(154, 26);
            this.wireframeVertexSizeFlowLayoutPanel.TabIndex = 100;
            // 
            // wireframeVertexSizeLabel
            // 
            this.wireframeVertexSizeLabel.AutoSize = true;
            this.wireframeVertexSizeLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.wireframeVertexSizeLabel.Location = new System.Drawing.Point(3, 0);
            this.wireframeVertexSizeLabel.Name = "wireframeVertexSizeLabel";
            this.wireframeVertexSizeLabel.Size = new System.Drawing.Size(56, 26);
            this.wireframeVertexSizeLabel.TabIndex = 21;
            this.wireframeVertexSizeLabel.Text = "W/V Size:";
            this.wireframeVertexSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // wireframeSizeUpDown
            // 
            this.wireframeSizeUpDown.Location = new System.Drawing.Point(65, 3);
            this.wireframeSizeUpDown.Name = "wireframeSizeUpDown";
            this.wireframeSizeUpDown.Size = new System.Drawing.Size(40, 20);
            this.wireframeSizeUpDown.TabIndex = 0;
            this.toolTip.SetToolTip(this.wireframeSizeUpDown, "Wireframe");
            this.wireframeSizeUpDown.ValueChanged += new System.EventHandler(this.wireframeSizeUpDown_ValueChanged);
            // 
            // vertexSizeUpDown
            // 
            this.vertexSizeUpDown.Location = new System.Drawing.Point(111, 3);
            this.vertexSizeUpDown.Name = "vertexSizeUpDown";
            this.vertexSizeUpDown.Size = new System.Drawing.Size(40, 20);
            this.vertexSizeUpDown.TabIndex = 1;
            this.toolTip.SetToolTip(this.vertexSizeUpDown, "Vertex");
            this.vertexSizeUpDown.ValueChanged += new System.EventHandler(this.vertexSizeUpDown_ValueChanged);
            // 
            // animationsSideSplitContainer
            // 
            this.animationsSideSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsSideSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.animationsSideSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.animationsSideSplitContainer.Name = "animationsSideSplitContainer";
            this.animationsSideSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // animationsSideSplitContainer.Panel1
            // 
            this.animationsSideSplitContainer.Panel1.Controls.Add(this.animationsTreeView);
            // 
            // animationsSideSplitContainer.Panel2
            // 
            this.animationsSideSplitContainer.Panel2.Controls.Add(this.animationPropertyGrid);
            this.animationsSideSplitContainer.Size = new System.Drawing.Size(330, 567);
            this.animationsSideSplitContainer.SplitterDistance = 400;
            this.animationsSideSplitContainer.TabIndex = 0;
            // 
            // animationPropertyGrid
            // 
            this.animationPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationPropertyGrid.HelpVisible = false;
            this.animationPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.animationPropertyGrid.Name = "animationPropertyGrid";
            this.animationPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.animationPropertyGrid.Size = new System.Drawing.Size(330, 163);
            this.animationPropertyGrid.TabIndex = 11;
            this.animationPropertyGrid.ToolbarVisible = false;
            this.animationPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.animationPropertyGrid_PropertyValueChanged);
            // 
            // animationPreviewPanel
            // 
            this.animationPreviewPanel.BackColor = System.Drawing.Color.LightSkyBlue;
            this.animationPreviewPanel.Controls.Add(this.animationPreviewer);
            this.animationPreviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationPreviewPanel.Location = new System.Drawing.Point(0, 0);
            this.animationPreviewPanel.Name = "animationPreviewPanel";
            this.animationPreviewPanel.Size = new System.Drawing.Size(660, 436);
            this.animationPreviewPanel.TabIndex = 33;
            // 
            // modelsSideSplitContainer
            // 
            this.modelsSideSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelsSideSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.modelsSideSplitContainer.Name = "modelsSideSplitContainer";
            this.modelsSideSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // modelsSideSplitContainer.Panel1
            // 
            this.modelsSideSplitContainer.Panel1.Controls.Add(this.entitiesTreeView);
            // 
            // modelsSideSplitContainer.Panel2
            // 
            this.modelsSideSplitContainer.Panel2.Controls.Add(this.modelPropertyGridTableLayoutPanel);
            this.modelsSideSplitContainer.Size = new System.Drawing.Size(330, 567);
            this.modelsSideSplitContainer.SplitterDistance = 200;
            this.modelsSideSplitContainer.TabIndex = 2;
            // 
            // entitiesTreeView
            // 
            this.entitiesTreeView.CheckBoxes = true;
            this.entitiesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entitiesTreeView.HideSelection = false;
            this.entitiesTreeView.Location = new System.Drawing.Point(0, 0);
            this.entitiesTreeView.Name = "entitiesTreeView";
            this.entitiesTreeView.Size = new System.Drawing.Size(330, 200);
            this.entitiesTreeView.TabIndex = 10;
            this.entitiesTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.entitiesTreeView_AfterCheck);
            this.entitiesTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.entitiesTreeView_BeforeExpand);
            this.entitiesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.entitiesTreeView_AfterSelect);
            // 
            // modelPropertyGridTableLayoutPanel
            // 
            this.modelPropertyGridTableLayoutPanel.ColumnCount = 1;
            this.modelPropertyGridTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.modelPropertyGridTableLayoutPanel.Controls.Add(this.exportSelectedModelsButton, 0, 1);
            this.modelPropertyGridTableLayoutPanel.Controls.Add(this.modelPropertyGrid, 0, 0);
            this.modelPropertyGridTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelPropertyGridTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.modelPropertyGridTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.modelPropertyGridTableLayoutPanel.Name = "modelPropertyGridTableLayoutPanel";
            this.modelPropertyGridTableLayoutPanel.RowCount = 2;
            this.modelPropertyGridTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.modelPropertyGridTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.modelPropertyGridTableLayoutPanel.Size = new System.Drawing.Size(330, 363);
            this.modelPropertyGridTableLayoutPanel.TabIndex = 1;
            // 
            // exportSelectedModelsButton
            // 
            this.exportSelectedModelsButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.exportSelectedModelsButton.Location = new System.Drawing.Point(0, 329);
            this.exportSelectedModelsButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.exportSelectedModelsButton.Name = "exportSelectedModelsButton";
            this.exportSelectedModelsButton.Size = new System.Drawing.Size(330, 34);
            this.exportSelectedModelsButton.TabIndex = 12;
            this.exportSelectedModelsButton.Text = "Export Selected Models";
            this.exportSelectedModelsButton.UseVisualStyleBackColor = true;
            this.exportSelectedModelsButton.Click += new System.EventHandler(this.exportSelectedModels_Click);
            // 
            // modelPropertyGrid
            // 
            this.modelPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelPropertyGrid.HelpVisible = false;
            this.modelPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.modelPropertyGrid.Margin = new System.Windows.Forms.Padding(0);
            this.modelPropertyGrid.Name = "modelPropertyGrid";
            this.modelPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.modelPropertyGrid.Size = new System.Drawing.Size(330, 326);
            this.modelPropertyGrid.TabIndex = 11;
            this.modelPropertyGrid.ToolbarVisible = false;
            this.modelPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.modelPropertyGrid_PropertyValueChanged);
            // 
            // scenePreviewer
            // 
            this.scenePreviewer.BackColor = System.Drawing.Color.LightSkyBlue;
            this.scenePreviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scenePreviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scenePreviewer.Location = new System.Drawing.Point(0, 0);
            this.scenePreviewer.Name = "scenePreviewer";
            this.scenePreviewer.ShowStatusBar = false;
            this.scenePreviewer.Size = new System.Drawing.Size(660, 567);
            this.scenePreviewer.TabIndex = 14;
            // 
            // texturesListView
            // 
            this.texturesListView.AllowItemReorder = false;
            this.texturesListView.AutoRotateThumbnails = false;
            this.texturesListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            imageListViewColumnHeader1.Comparer = null;
            imageListViewColumnHeader1.DisplayIndex = 0;
            imageListViewColumnHeader1.Grouper = null;
            imageListViewColumnHeader1.Key = "Found";
            imageListViewColumnHeader1.Text = "Found";
            imageListViewColumnHeader1.Type = Manina.Windows.Forms.ColumnType.Custom;
            imageListViewColumnHeader2.Comparer = null;
            imageListViewColumnHeader2.DisplayIndex = 1;
            imageListViewColumnHeader2.Grouper = null;
            imageListViewColumnHeader2.Key = "Textures";
            imageListViewColumnHeader2.Text = "Textures";
            imageListViewColumnHeader2.Type = Manina.Windows.Forms.ColumnType.Custom;
            this.texturesListView.Columns.AddRange(new Manina.Windows.Forms.ImageListView.ImageListViewColumnHeader[] {
            imageListViewColumnHeader1,
            imageListViewColumnHeader2});
            this.texturesListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.texturesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturesListView.GroupColumn = 1;
            this.texturesListView.GroupOrder = Manina.Windows.Forms.SortOrder.Ascending;
            this.texturesListView.Location = new System.Drawing.Point(0, 0);
            this.texturesListView.Name = "texturesListView";
            this.texturesListView.PersistentCacheDirectory = "";
            this.texturesListView.PersistentCacheSize = ((long)(100));
            this.texturesListView.Size = new System.Drawing.Size(330, 350);
            this.texturesListView.SortColumn = 1;
            this.texturesListView.SortOrder = Manina.Windows.Forms.SortOrder.Ascending;
            this.texturesListView.TabIndex = 10;
            this.texturesListView.ThumbnailSize = new System.Drawing.Size(80, 80);
            this.texturesListView.UseWIC = true;
            this.texturesListView.SelectionChanged += new System.EventHandler(this.texturesListView_SelectedIndexChanged);
            // 
            // texturePreviewer
            // 
            this.texturePreviewer.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.texturePreviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.texturePreviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturePreviewer.Location = new System.Drawing.Point(0, 0);
            this.texturePreviewer.Name = "texturePreviewer";
            this.texturePreviewer.Size = new System.Drawing.Size(660, 567);
            this.texturePreviewer.TabIndex = 14;
            // 
            // vramPreviewer
            // 
            this.vramPreviewer.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.vramPreviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.vramPreviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vramPreviewer.Location = new System.Drawing.Point(0, 0);
            this.vramPreviewer.Name = "vramPreviewer";
            this.vramPreviewer.Size = new System.Drawing.Size(660, 567);
            this.vramPreviewer.TabIndex = 14;
            // 
            // animationsTreeView
            // 
            this.animationsTreeView.CheckBoxes = true;
            this.animationsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsTreeView.HideSelection = false;
            this.animationsTreeView.Location = new System.Drawing.Point(0, 0);
            this.animationsTreeView.Name = "animationsTreeView";
            this.animationsTreeView.Size = new System.Drawing.Size(330, 400);
            this.animationsTreeView.TabIndex = 10;
            this.animationsTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.animationsTreeView_BeforeExpand);
            this.animationsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.animationsTreeView_AfterSelect);
            // 
            // animationPreviewer
            // 
            this.animationPreviewer.BackColor = System.Drawing.Color.LightSkyBlue;
            this.animationPreviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.animationPreviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationPreviewer.Location = new System.Drawing.Point(0, 0);
            this.animationPreviewer.Name = "animationPreviewer";
            this.animationPreviewer.ShowStatusBar = false;
            this.animationPreviewer.Size = new System.Drawing.Size(660, 436);
            this.animationPreviewer.TabIndex = 14;
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 671);
            this.Controls.Add(this.menusTabControl);
            this.Controls.Add(this.sceneControlsFlowLayoutPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "PreviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PSXPrev";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.previewForm_FormClosing);
            this.Load += new System.EventHandler(this.previewForm_Load);
            this.Shown += new System.EventHandler(this.previewForm_Shown);
            this.ResizeBegin += new System.EventHandler(this.previewForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.previewForm_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.previewForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.previewForm_KeyUp);
            this.entitiesTabPage.ResumeLayout(false);
            this.modelsPreviewSplitContainer.Panel1.ResumeLayout(false);
            this.modelsPreviewSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.modelsPreviewSplitContainer)).EndInit();
            this.modelsPreviewSplitContainer.ResumeLayout(false);
            this.menusTabControl.ResumeLayout(false);
            this.texturesTabPage.ResumeLayout(false);
            this.texturesPreviewSplitContainer.Panel1.ResumeLayout(false);
            this.texturesPreviewSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.texturesPreviewSplitContainer)).EndInit();
            this.texturesPreviewSplitContainer.ResumeLayout(false);
            this.texturesSideSplitContainer.Panel1.ResumeLayout(false);
            this.texturesSideSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.texturesSideSplitContainer)).EndInit();
            this.texturesSideSplitContainer.ResumeLayout(false);
            this.texturePropertyGridTableLayoutPanel.ResumeLayout(false);
            this.vramTabPage.ResumeLayout(false);
            this.vramPreviewSplitContainer.Panel1.ResumeLayout(false);
            this.vramPreviewSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.vramPreviewSplitContainer)).EndInit();
            this.vramPreviewSplitContainer.ResumeLayout(false);
            this.vramSideSplitContainer.Panel1.ResumeLayout(false);
            this.vramSideSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.vramSideSplitContainer)).EndInit();
            this.vramSideSplitContainer.ResumeLayout(false);
            this.vramButtonsTableLayoutPanel.ResumeLayout(false);
            this.animationsTabPage.ResumeLayout(false);
            this.animationsPreviewSplitContainer.Panel1.ResumeLayout(false);
            this.animationsPreviewSplitContainer.Panel2.ResumeLayout(false);
            this.animationsPreviewSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationsPreviewSplitContainer)).EndInit();
            this.animationsPreviewSplitContainer.ResumeLayout(false);
            this.animationGroupBoxMarginPanel.ResumeLayout(false);
            this.animationGroupBoxMarginPanel.PerformLayout();
            this.animationGroupBox.ResumeLayout(false);
            this.animationGroupBox.PerformLayout();
            this.animationControlsTableLayoutPanel.ResumeLayout(false);
            this.animationControlsTableLayoutPanel.PerformLayout();
            this.animationControlsProgressTableLayoutPanel.ResumeLayout(false);
            this.animationControlsProgressTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationFrameTrackBar)).EndInit();
            this.animationSpeedFlowLayoutPanel.ResumeLayout(false);
            this.animationSpeedFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationSpeedNumericUpDown)).EndInit();
            this.animationPlayModeFlowLayoutPanel.ResumeLayout(false);
            this.animationPlayModeFlowLayoutPanel.PerformLayout();
            this.animationReverseFlowLayoutPanel.ResumeLayout(false);
            this.animationReverseFlowLayoutPanel.PerformLayout();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.sceneControlsFlowLayoutPanel.ResumeLayout(false);
            this.sceneControlsFlowLayoutPanel.PerformLayout();
            this.lightRotationFlowLayoutPanel.ResumeLayout(false);
            this.lightRotationFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightYawNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightPitchNumericUpDown)).EndInit();
            this.lightIntensityFlowLayoutPanel4.ResumeLayout(false);
            this.lightIntensityFlowLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightIntensityNumericUpDown)).EndInit();
            this.cameraFOVFlowLayoutPanel.ResumeLayout(false);
            this.cameraFOVFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraFOVUpDown)).EndInit();
            this.gizmoSnapFlowLayoutPanel.ResumeLayout(false);
            this.gizmoSnapFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSnapUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.angleSnapUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scaleSnapUpDown)).EndInit();
            this.wireframeVertexSizeFlowLayoutPanel.ResumeLayout(false);
            this.wireframeVertexSizeFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wireframeSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vertexSizeUpDown)).EndInit();
            this.animationsSideSplitContainer.Panel1.ResumeLayout(false);
            this.animationsSideSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.animationsSideSplitContainer)).EndInit();
            this.animationsSideSplitContainer.ResumeLayout(false);
            this.animationPreviewPanel.ResumeLayout(false);
            this.modelsSideSplitContainer.Panel1.ResumeLayout(false);
            this.modelsSideSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.modelsSideSplitContainer)).EndInit();
            this.modelsSideSplitContainer.ResumeLayout(false);
            this.modelPropertyGridTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl menusTabControl;
        private System.Windows.Forms.TabPage entitiesTabPage;
        private System.Windows.Forms.TabPage texturesTabPage;
        private System.Windows.Forms.Button exportSelectedModelsButton;
        private System.Windows.Forms.Button exportBitmapButton;
        private PSXPrev.Forms.Controls.ExtendedTreeView entitiesTreeView;
        private System.Windows.Forms.TabPage vramTabPage;
        private PSXPrev.Forms.Controls.TexturePreviewer texturePreviewer;
        private System.Windows.Forms.Button drawToVRAMButton;
        private System.Windows.Forms.PropertyGrid modelPropertyGrid;
        private System.Windows.Forms.PropertyGrid texturePropertyGrid;
        private System.Windows.Forms.Button vramClearPageButton;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem modelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawToVRAMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findByPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearPageToolStripMenuItem;
        private PSXPrev.Forms.Controls.ExtendedImageListView texturesListView;
        private System.Windows.Forms.ToolStripMenuItem clearSearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllPagesToolStripMenuItem;
        private System.Windows.Forms.TabPage animationsTabPage;
        private System.Windows.Forms.ToolStripMenuItem animationsToolStripMenuItem;
        private System.Windows.Forms.PropertyGrid animationPropertyGrid;
        private PSXPrev.Forms.Controls.ExtendedTreeView animationsTreeView;
        private System.Windows.Forms.Button animationPlayButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusMessageLabel;
        private System.Windows.Forms.ToolStripMenuItem showBoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showVRAMUVsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem videoTutorialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoAttachLimbsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compatibilityListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.Label animationSpeedLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem setMaskColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableLightToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem setAmbientColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setBackgroundColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem lineRendererToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetTransformToolStripMenuItem;
        private System.Windows.Forms.GroupBox animationGroupBox;
        private System.Windows.Forms.TableLayoutPanel animationControlsTableLayoutPanel;
        private System.Windows.Forms.SplitContainer texturesPreviewSplitContainer;
        private System.Windows.Forms.SplitContainer texturesSideSplitContainer;
        private System.Windows.Forms.SplitContainer modelsPreviewSplitContainer;
        private System.Windows.Forms.SplitContainer modelsSideSplitContainer;
        private System.Windows.Forms.SplitContainer animationsSideSplitContainer;
        private System.Windows.Forms.SplitContainer animationsPreviewSplitContainer;
        private System.Windows.Forms.TableLayoutPanel modelPropertyGridTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel texturePropertyGridTableLayoutPanel;
        private System.Windows.Forms.Label animationProgressNameLabel;
        private System.Windows.Forms.ToolStripMenuItem enableSemiTransparencyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forceDoubleSidedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseScanningToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripMenuItem autoDrawModelTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem autoSelectAnimationModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoPlayAnimationsToolStripMenuItem;
        private System.Windows.Forms.SplitContainer vramPreviewSplitContainer;
        private System.Windows.Forms.SplitContainer vramSideSplitContainer;
        private System.Windows.Forms.ListBox vramListBox;
        private PSXPrev.Forms.Controls.TexturePreviewer vramPreviewer;
        private System.Windows.Forms.TableLayoutPanel vramButtonsTableLayoutPanel;
        private System.Windows.Forms.Button vramGotoPageButton;
        private System.Windows.Forms.TrackBar animationFrameTrackBar;
        private System.Windows.Forms.TableLayoutPanel animationControlsProgressTableLayoutPanel;
        private System.Windows.Forms.Label animationProgressLabel;
        private System.Windows.Forms.FlowLayoutPanel sceneControlsFlowLayoutPanel;
        private System.Windows.Forms.ToolStripMenuItem stopScanningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTMDBindingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewOnGitHubToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.FlowLayoutPanel lightRotationFlowLayoutPanel;
        private System.Windows.Forms.Label lightRotationLabel;
        private System.Windows.Forms.NumericUpDown lightYawNumericUpDown;
        private System.Windows.Forms.NumericUpDown lightPitchNumericUpDown;
        private System.Windows.Forms.FlowLayoutPanel lightIntensityFlowLayoutPanel4;
        private System.Windows.Forms.Label lightIntensityLabel;
        private System.Windows.Forms.NumericUpDown lightIntensityNumericUpDown;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetWholeModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetSelectedModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem exportTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportVRAMPagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedVRAMPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllVRAMPagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawAllToVRAMToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel animationSpeedFlowLayoutPanel;
        private System.Windows.Forms.NumericUpDown animationSpeedNumericUpDown;
        private System.Windows.Forms.ToolStripMenuItem exportDrawnToVRAMPagesToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel animationPlayModeFlowLayoutPanel;
        private System.Windows.Forms.Label animationPlayModeLabel;
        private System.Windows.Forms.ComboBox animationLoopModeComboBox;
        private System.Windows.Forms.FlowLayoutPanel animationReverseFlowLayoutPanel;
        private System.Windows.Forms.Label animationReverseLabel;
        private System.Windows.Forms.CheckBox animationReverseCheckBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripMenuItem defaultSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem loadSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startScanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearScanResultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem drawModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawModeFacesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawModeWireframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawModeVerticesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawModeSolidWireframeVerticesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gizmoToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gizmoToolNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gizmoToolTranslateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gizmoToolRotateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gizmoToolScaleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setWireframeVerticesColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableTexturesToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel cameraFOVFlowLayoutPanel;
        private System.Windows.Forms.Label cameraFOVLabel;
        private System.Windows.Forms.NumericUpDown cameraFOVUpDown;
        private System.Windows.Forms.FlowLayoutPanel gizmoSnapFlowLayoutPanel;
        private System.Windows.Forms.Label gizmoSnapLabel;
        private System.Windows.Forms.NumericUpDown gridSnapUpDown;
        private System.Windows.Forms.FlowLayoutPanel wireframeVertexSizeFlowLayoutPanel;
        private System.Windows.Forms.Label wireframeVertexSizeLabel;
        private System.Windows.Forms.NumericUpDown wireframeSizeUpDown;
        private System.Windows.Forms.NumericUpDown vertexSizeUpDown;
        private System.Windows.Forms.NumericUpDown angleSnapUpDown;
        private System.Windows.Forms.NumericUpDown scaleSnapUpDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripProgressBar statusTotalFilesProgressBar;
        private System.Windows.Forms.ToolStripProgressBar statusCurrentFileProgressBar;
        private System.Windows.Forms.ToolStripMenuItem fastWindowResizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSideBarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPaletteIndexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoPackModelTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkedModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkAllModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkedAnimationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkAllAnimationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllAnimationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem subModelVisibilityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFPSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableVertexColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subModelVisibilityAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subModelVisibilitySelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subModelVisibilityWithSameTMDIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFocusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFocusOnRootModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFocusOnSubModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFocusIncludeCheckedModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFocusIncludeWholeModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem showMissingTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSkeletonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFocusResetCameraRotationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectionModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectionModeNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectionModeBoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectionModeTriangleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripMenuItem showTextureSemiTransparencyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTexturePaletteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showUIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showVRAMSemiTransparencyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTextureUVsToolStripMenuItem;
        private System.Windows.Forms.Panel animationGroupBoxMarginPanel;
        private Controls.ScenePreviewer scenePreviewer;
        private System.Windows.Forms.ToolStripMenuItem showModelsStatusBarToolStripMenuItem;
        private Controls.ScenePreviewer animationPreviewer;
        private System.Windows.Forms.Panel animationPreviewPanel;
    }
}
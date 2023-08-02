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
            this.modelsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.entitiesTreeView = new System.Windows.Forms.TreeView();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.exportSelectedModelsButton = new System.Windows.Forms.Button();
            this.modelPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.menusTabControl = new System.Windows.Forms.TabControl();
            this.bitmapsTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.texturesListView = new Manina.Windows.Forms.ImageListView();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.drawToVRAMButton = new System.Windows.Forms.Button();
            this.texturePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.exportBitmapButton = new System.Windows.Forms.Button();
            this.texturesZoomLabel = new System.Windows.Forms.Label();
            this.texturePanel = new System.Windows.Forms.Panel();
            this.texturePreviewPictureBox = new System.Windows.Forms.PictureBox();
            this.vramTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.vramListBox = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.gotoPageButton = new System.Windows.Forms.Button();
            this.btnClearPage = new System.Windows.Forms.Button();
            this.vramZoomLabel = new System.Windows.Forms.Label();
            this.vramPanel = new System.Windows.Forms.Panel();
            this.vramPagePictureBox = new System.Windows.Forms.PictureBox();
            this.animationsTabPage = new System.Windows.Forms.TabPage();
            this.animationsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.animationsTreeView = new System.Windows.Forms.TreeView();
            this.animationPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.animationsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.animationGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.animationFrameTrackBar = new System.Windows.Forms.TrackBar();
            this.animationProgressLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel7 = new System.Windows.Forms.FlowLayoutPanel();
            this.animationSpeedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel8 = new System.Windows.Forms.FlowLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.animationLoopModeComboBox = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel9 = new System.Windows.Forms.FlowLayoutPanel();
            this.label9 = new System.Windows.Forms.Label();
            this.animationReverseCheckBox = new System.Windows.Forms.CheckBox();
            this.animationPlayButtonx = new System.Windows.Forms.Button();
            this.exportModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllModelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseScanningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopScanningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.defaultSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetTransformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetWholeModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.wireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verticesOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showGizmosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableSemiTransparencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forceDoubleSidedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAttachLimbsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.setAmbientColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.autoDrawModelTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.setMaskColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vRAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportVRAMPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedVRAMPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportDrawnToVRAMPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllVRAMPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.clearPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.showUVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoPlayAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoSelectAnimationModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.showTMDBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoTutorialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compatibilityListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewOnGitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.messageToolStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.gridSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.lightYawNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.lightPitchNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.lightIntensityNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.vertexSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.cameraFOVUpDown = new System.Windows.Forms.NumericUpDown();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.loadSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.entitiesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modelsSplitContainer)).BeginInit();
            this.modelsSplitContainer.Panel1.SuspendLayout();
            this.modelsSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.menusTabControl.SuspendLayout();
            this.bitmapsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.texturePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texturePreviewPictureBox)).BeginInit();
            this.vramTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).BeginInit();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.vramPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vramPagePictureBox)).BeginInit();
            this.animationsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationsSplitContainer)).BeginInit();
            this.animationsSplitContainer.Panel1.SuspendLayout();
            this.animationsSplitContainer.Panel2.SuspendLayout();
            this.animationsSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.animationsTableLayoutPanel.SuspendLayout();
            this.animationGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationFrameTrackBar)).BeginInit();
            this.flowLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationSpeedNumericUpDown)).BeginInit();
            this.flowLayoutPanel8.SuspendLayout();
            this.flowLayoutPanel9.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSizeNumericUpDown)).BeginInit();
            this.flowLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightYawNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightPitchNumericUpDown)).BeginInit();
            this.flowLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightIntensityNumericUpDown)).BeginInit();
            this.flowLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vertexSizeUpDown)).BeginInit();
            this.flowLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraFOVUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // entitiesTabPage
            // 
            this.entitiesTabPage.Controls.Add(this.modelsSplitContainer);
            this.entitiesTabPage.Location = new System.Drawing.Point(4, 22);
            this.entitiesTabPage.Name = "entitiesTabPage";
            this.entitiesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.entitiesTabPage.Size = new System.Drawing.Size(1000, 573);
            this.entitiesTabPage.TabIndex = 0;
            this.entitiesTabPage.Text = "Models";
            this.entitiesTabPage.UseVisualStyleBackColor = true;
            // 
            // modelsSplitContainer
            // 
            this.modelsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelsSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.modelsSplitContainer.Name = "modelsSplitContainer";
            // 
            // modelsSplitContainer.Panel1
            // 
            this.modelsSplitContainer.Panel1.Controls.Add(this.splitContainer6);
            this.modelsSplitContainer.Size = new System.Drawing.Size(994, 567);
            this.modelsSplitContainer.SplitterDistance = 330;
            this.modelsSplitContainer.TabIndex = 15;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.entitiesTreeView);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.tableLayoutPanel5);
            this.splitContainer6.Size = new System.Drawing.Size(330, 567);
            this.splitContainer6.SplitterDistance = 200;
            this.splitContainer6.TabIndex = 0;
            // 
            // entitiesTreeView
            // 
            this.entitiesTreeView.CheckBoxes = true;
            this.entitiesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entitiesTreeView.HideSelection = false;
            this.entitiesTreeView.Location = new System.Drawing.Point(0, 0);
            this.entitiesTreeView.Name = "entitiesTreeView";
            this.entitiesTreeView.Size = new System.Drawing.Size(330, 200);
            this.entitiesTreeView.TabIndex = 9;
            this.entitiesTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.entitiesTreeView_AfterCheck);
            this.entitiesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.entitiesTreeView_AfterSelect);
            this.entitiesTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.entitiesTreeView_NodeMouseClick);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.exportSelectedModelsButton, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.modelPropertyGrid, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.Size = new System.Drawing.Size(330, 363);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // exportSelectedModelsButton
            // 
            this.exportSelectedModelsButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.exportSelectedModelsButton.Location = new System.Drawing.Point(0, 329);
            this.exportSelectedModelsButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.exportSelectedModelsButton.Name = "exportSelectedModelsButton";
            this.exportSelectedModelsButton.Size = new System.Drawing.Size(330, 34);
            this.exportSelectedModelsButton.TabIndex = 8;
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
            this.modelPropertyGrid.TabIndex = 14;
            this.modelPropertyGrid.ToolbarVisible = false;
            this.modelPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.modelPropertyGrid_PropertyValueChanged);
            // 
            // menusTabControl
            // 
            this.menusTabControl.Controls.Add(this.entitiesTabPage);
            this.menusTabControl.Controls.Add(this.bitmapsTabPage);
            this.menusTabControl.Controls.Add(this.vramTabPage);
            this.menusTabControl.Controls.Add(this.animationsTabPage);
            this.menusTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menusTabControl.Location = new System.Drawing.Point(0, 24);
            this.menusTabControl.Name = "menusTabControl";
            this.menusTabControl.SelectedIndex = 0;
            this.menusTabControl.Size = new System.Drawing.Size(1008, 599);
            this.menusTabControl.TabIndex = 3;
            this.menusTabControl.SelectedIndexChanged += new System.EventHandler(this.menusTabControl_SelectedIndexChanged);
            // 
            // bitmapsTabPage
            // 
            this.bitmapsTabPage.Controls.Add(this.splitContainer2);
            this.bitmapsTabPage.Location = new System.Drawing.Point(4, 22);
            this.bitmapsTabPage.Name = "bitmapsTabPage";
            this.bitmapsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.bitmapsTabPage.Size = new System.Drawing.Size(1000, 573);
            this.bitmapsTabPage.TabIndex = 1;
            this.bitmapsTabPage.Text = "Textures";
            this.bitmapsTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.texturesZoomLabel);
            this.splitContainer2.Panel2.Controls.Add(this.texturePanel);
            this.splitContainer2.Size = new System.Drawing.Size(994, 567);
            this.splitContainer2.SplitterDistance = 330;
            this.splitContainer2.TabIndex = 17;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.texturesListView);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tableLayoutPanel4);
            this.splitContainer3.Size = new System.Drawing.Size(330, 567);
            this.splitContainer3.SplitterDistance = 350;
            this.splitContainer3.TabIndex = 0;
            // 
            // texturesListView
            // 
            this.texturesListView.AllowItemReorder = false;
            this.texturesListView.AutoRotateThumbnails = false;
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
            this.texturesListView.TabIndex = 16;
            this.texturesListView.ThumbnailSize = new System.Drawing.Size(80, 80);
            this.texturesListView.UseWIC = true;
            this.texturesListView.SelectionChanged += new System.EventHandler(this.texturesListView_SelectedIndexChanged);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.drawToVRAMButton, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.texturePropertyGrid, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.exportBitmapButton, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(330, 213);
            this.tableLayoutPanel4.TabIndex = 1;
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
            this.texturePropertyGrid.TabIndex = 15;
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
            this.exportBitmapButton.TabIndex = 9;
            this.exportBitmapButton.Text = "Export Selected Textures";
            this.exportBitmapButton.UseVisualStyleBackColor = true;
            this.exportBitmapButton.Click += new System.EventHandler(this.exportSelectedTextures_Click);
            // 
            // texturesZoomLabel
            // 
            this.texturesZoomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.texturesZoomLabel.BackColor = System.Drawing.SystemColors.Control;
            this.texturesZoomLabel.Location = new System.Drawing.Point(611, 0);
            this.texturesZoomLabel.Margin = new System.Windows.Forms.Padding(0);
            this.texturesZoomLabel.Name = "texturesZoomLabel";
            this.texturesZoomLabel.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.texturesZoomLabel.Size = new System.Drawing.Size(50, 30);
            this.texturesZoomLabel.TabIndex = 10;
            this.texturesZoomLabel.Text = "100%";
            this.texturesZoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // texturePanel
            // 
            this.texturePanel.AutoScroll = true;
            this.texturePanel.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.texturePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.texturePanel.Controls.Add(this.texturePreviewPictureBox);
            this.texturePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturePanel.Location = new System.Drawing.Point(0, 0);
            this.texturePanel.Name = "texturePanel";
            this.texturePanel.Size = new System.Drawing.Size(660, 567);
            this.texturePanel.TabIndex = 11;
            // 
            // texturePreviewPictureBox
            // 
            this.texturePreviewPictureBox.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.texturePreviewPictureBox.Location = new System.Drawing.Point(0, 0);
            this.texturePreviewPictureBox.Name = "texturePreviewPictureBox";
            this.texturePreviewPictureBox.Size = new System.Drawing.Size(256, 256);
            this.texturePreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.texturePreviewPictureBox.TabIndex = 9;
            this.texturePreviewPictureBox.TabStop = false;
            this.texturePreviewPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.texturePreviewPictureBox_Paint);
            // 
            // vramTabPage
            // 
            this.vramTabPage.Controls.Add(this.splitContainer4);
            this.vramTabPage.Location = new System.Drawing.Point(4, 22);
            this.vramTabPage.Name = "vramTabPage";
            this.vramTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.vramTabPage.Size = new System.Drawing.Size(1000, 573);
            this.vramTabPage.TabIndex = 2;
            this.vramTabPage.Text = "VRAM";
            this.vramTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(3, 3);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.splitContainer7);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.vramZoomLabel);
            this.splitContainer4.Panel2.Controls.Add(this.vramPanel);
            this.splitContainer4.Size = new System.Drawing.Size(994, 567);
            this.splitContainer4.SplitterDistance = 330;
            this.splitContainer4.TabIndex = 13;
            // 
            // splitContainer7
            // 
            this.splitContainer7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer7.IsSplitterFixed = true;
            this.splitContainer7.Location = new System.Drawing.Point(0, 0);
            this.splitContainer7.Name = "splitContainer7";
            this.splitContainer7.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.Controls.Add(this.vramListBox);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.tableLayoutPanel3);
            this.splitContainer7.Size = new System.Drawing.Size(330, 567);
            this.splitContainer7.SplitterDistance = 492;
            this.splitContainer7.TabIndex = 0;
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
            this.vramListBox.TabIndex = 0;
            this.vramListBox.SelectedIndexChanged += new System.EventHandler(this.vramComboBox_SelectedIndexChanged);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.gotoPageButton, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnClearPage, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(330, 71);
            this.tableLayoutPanel3.TabIndex = 13;
            // 
            // gotoPageButton
            // 
            this.gotoPageButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.gotoPageButton.Location = new System.Drawing.Point(0, 0);
            this.gotoPageButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.gotoPageButton.Name = "gotoPageButton";
            this.gotoPageButton.Size = new System.Drawing.Size(330, 34);
            this.gotoPageButton.TabIndex = 13;
            this.gotoPageButton.Text = "Go to Page";
            this.gotoPageButton.UseVisualStyleBackColor = true;
            this.gotoPageButton.Click += new System.EventHandler(this.gotoPageButton_Click);
            // 
            // btnClearPage
            // 
            this.btnClearPage.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnClearPage.Location = new System.Drawing.Point(0, 37);
            this.btnClearPage.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.btnClearPage.Name = "btnClearPage";
            this.btnClearPage.Size = new System.Drawing.Size(330, 34);
            this.btnClearPage.TabIndex = 12;
            this.btnClearPage.Text = "Clear Page";
            this.btnClearPage.UseVisualStyleBackColor = true;
            this.btnClearPage.Click += new System.EventHandler(this.btnClearPage_Click);
            // 
            // vramZoomLabel
            // 
            this.vramZoomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.vramZoomLabel.BackColor = System.Drawing.SystemColors.Control;
            this.vramZoomLabel.Location = new System.Drawing.Point(611, 0);
            this.vramZoomLabel.Margin = new System.Windows.Forms.Padding(0);
            this.vramZoomLabel.Name = "vramZoomLabel";
            this.vramZoomLabel.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.vramZoomLabel.Size = new System.Drawing.Size(50, 30);
            this.vramZoomLabel.TabIndex = 11;
            this.vramZoomLabel.Text = "100%";
            this.vramZoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // vramPanel
            // 
            this.vramPanel.AutoScroll = true;
            this.vramPanel.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.vramPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.vramPanel.Controls.Add(this.vramPagePictureBox);
            this.vramPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vramPanel.Location = new System.Drawing.Point(0, 0);
            this.vramPanel.Name = "vramPanel";
            this.vramPanel.Size = new System.Drawing.Size(660, 567);
            this.vramPanel.TabIndex = 10;
            // 
            // vramPagePictureBox
            // 
            this.vramPagePictureBox.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.vramPagePictureBox.Location = new System.Drawing.Point(0, 0);
            this.vramPagePictureBox.Name = "vramPagePictureBox";
            this.vramPagePictureBox.Size = new System.Drawing.Size(256, 256);
            this.vramPagePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.vramPagePictureBox.TabIndex = 9;
            this.vramPagePictureBox.TabStop = false;
            this.vramPagePictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.vramPagePictureBox_Paint);
            // 
            // animationsTabPage
            // 
            this.animationsTabPage.Controls.Add(this.animationsSplitContainer);
            this.animationsTabPage.Location = new System.Drawing.Point(4, 22);
            this.animationsTabPage.Name = "animationsTabPage";
            this.animationsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.animationsTabPage.Size = new System.Drawing.Size(1000, 573);
            this.animationsTabPage.TabIndex = 3;
            this.animationsTabPage.Text = "Animations";
            this.animationsTabPage.UseVisualStyleBackColor = true;
            // 
            // animationsSplitContainer
            // 
            this.animationsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.animationsSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.animationsSplitContainer.Name = "animationsSplitContainer";
            // 
            // animationsSplitContainer.Panel1
            // 
            this.animationsSplitContainer.Panel1.Controls.Add(this.splitContainer5);
            // 
            // animationsSplitContainer.Panel2
            // 
            this.animationsSplitContainer.Panel2.Controls.Add(this.animationsTableLayoutPanel);
            this.animationsSplitContainer.Size = new System.Drawing.Size(994, 567);
            this.animationsSplitContainer.SplitterDistance = 330;
            this.animationsSplitContainer.TabIndex = 16;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.animationsTreeView);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.animationPropertyGrid);
            this.splitContainer5.Size = new System.Drawing.Size(330, 567);
            this.splitContainer5.SplitterDistance = 400;
            this.splitContainer5.TabIndex = 0;
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
            this.animationsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.animationsTreeView_AfterSelect);
            // 
            // animationPropertyGrid
            // 
            this.animationPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationPropertyGrid.HelpVisible = false;
            this.animationPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.animationPropertyGrid.Name = "animationPropertyGrid";
            this.animationPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.animationPropertyGrid.Size = new System.Drawing.Size(330, 163);
            this.animationPropertyGrid.TabIndex = 15;
            this.animationPropertyGrid.ToolbarVisible = false;
            this.animationPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.animationPropertyGrid_PropertyValueChanged);
            // 
            // animationsTableLayoutPanel
            // 
            this.animationsTableLayoutPanel.ColumnCount = 1;
            this.animationsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.animationsTableLayoutPanel.Controls.Add(this.animationGroupBox, 0, 1);
            this.animationsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.animationsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.animationsTableLayoutPanel.Name = "animationsTableLayoutPanel";
            this.animationsTableLayoutPanel.RowCount = 2;
            this.animationsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.animationsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.animationsTableLayoutPanel.Size = new System.Drawing.Size(660, 567);
            this.animationsTableLayoutPanel.TabIndex = 3;
            // 
            // animationGroupBox
            // 
            this.animationGroupBox.AutoSize = true;
            this.animationGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.animationGroupBox.Controls.Add(this.animationPlayButtonx);
            this.animationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationGroupBox.Location = new System.Drawing.Point(3, 439);
            this.animationGroupBox.Name = "animationGroupBox";
            this.animationGroupBox.Size = new System.Drawing.Size(654, 125);
            this.animationGroupBox.TabIndex = 1;
            this.animationGroupBox.TabStop = false;
            this.animationGroupBox.Text = "Animation";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel6, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel7, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(648, 72);
            this.tableLayoutPanel2.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Left;
            this.label6.Location = new System.Drawing.Point(3, 27);
            this.label6.Name = "label6";
            this.label6.Padding = new System.Windows.Forms.Padding(0, 0, 0, 24);
            this.label6.Size = new System.Drawing.Size(51, 45);
            this.label6.TabIndex = 23;
            this.label6.Text = "Progress:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 27);
            this.label3.TabIndex = 21;
            this.label3.Text = "Speed:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.Controls.Add(this.animationFrameTrackBar, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.animationProgressLabel, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(60, 27);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.Size = new System.Drawing.Size(585, 45);
            this.tableLayoutPanel6.TabIndex = 24;
            // 
            // animationFrameTrackBar
            // 
            this.animationFrameTrackBar.BackColor = System.Drawing.SystemColors.Window;
            this.animationFrameTrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationFrameTrackBar.Location = new System.Drawing.Point(0, 0);
            this.animationFrameTrackBar.Margin = new System.Windows.Forms.Padding(0);
            this.animationFrameTrackBar.Name = "animationFrameTrackBar";
            this.animationFrameTrackBar.Size = new System.Drawing.Size(531, 45);
            this.animationFrameTrackBar.TabIndex = 24;
            this.animationFrameTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.animationFrameTrackBar.Scroll += new System.EventHandler(this.animationFrameTrackBar_Scroll);
            // 
            // animationProgressLabel
            // 
            this.animationProgressLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.animationProgressLabel.Location = new System.Drawing.Point(531, 0);
            this.animationProgressLabel.Margin = new System.Windows.Forms.Padding(0);
            this.animationProgressLabel.Name = "animationProgressLabel";
            this.animationProgressLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 24);
            this.animationProgressLabel.Size = new System.Drawing.Size(54, 45);
            this.animationProgressLabel.TabIndex = 25;
            this.animationProgressLabel.Text = "0/0";
            this.animationProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel7
            // 
            this.flowLayoutPanel7.AutoSize = true;
            this.flowLayoutPanel7.Controls.Add(this.animationSpeedNumericUpDown);
            this.flowLayoutPanel7.Controls.Add(this.flowLayoutPanel8);
            this.flowLayoutPanel7.Controls.Add(this.flowLayoutPanel9);
            this.flowLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel7.Location = new System.Drawing.Point(57, 0);
            this.flowLayoutPanel7.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel7.Name = "flowLayoutPanel7";
            this.flowLayoutPanel7.Size = new System.Drawing.Size(591, 27);
            this.flowLayoutPanel7.TabIndex = 25;
            // 
            // animationSpeedNumericUpDown
            // 
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
            this.animationSpeedNumericUpDown.TabIndex = 3;
            this.animationSpeedNumericUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.animationSpeedNumericUpDown.ValueChanged += new System.EventHandler(this.animationSpeedNumericUpDown_ValueChanged);
            // 
            // flowLayoutPanel8
            // 
            this.flowLayoutPanel8.AutoSize = true;
            this.flowLayoutPanel8.Controls.Add(this.label8);
            this.flowLayoutPanel8.Controls.Add(this.animationLoopModeComboBox);
            this.flowLayoutPanel8.Location = new System.Drawing.Point(85, 0);
            this.flowLayoutPanel8.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel8.Name = "flowLayoutPanel8";
            this.flowLayoutPanel8.Size = new System.Drawing.Size(182, 27);
            this.flowLayoutPanel8.TabIndex = 26;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Left;
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 27);
            this.label8.TabIndex = 23;
            this.label8.Text = "Play Mode:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationLoopModeComboBox
            // 
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
            this.animationLoopModeComboBox.TabIndex = 24;
            this.animationLoopModeComboBox.SelectedIndexChanged += new System.EventHandler(this.animationLoopModeComboBox_SelectedIndexChanged);
            // 
            // flowLayoutPanel9
            // 
            this.flowLayoutPanel9.AutoSize = true;
            this.flowLayoutPanel9.Controls.Add(this.label9);
            this.flowLayoutPanel9.Controls.Add(this.animationReverseCheckBox);
            this.flowLayoutPanel9.Location = new System.Drawing.Point(267, 0);
            this.flowLayoutPanel9.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel9.Name = "flowLayoutPanel9";
            this.flowLayoutPanel9.Size = new System.Drawing.Size(77, 22);
            this.flowLayoutPanel9.TabIndex = 27;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Left;
            this.label9.Location = new System.Drawing.Point(3, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 22);
            this.label9.TabIndex = 26;
            this.label9.Text = "Reverse:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationReverseCheckBox
            // 
            this.animationReverseCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.animationReverseCheckBox.AutoSize = true;
            this.animationReverseCheckBox.Location = new System.Drawing.Point(59, 5);
            this.animationReverseCheckBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.animationReverseCheckBox.Name = "animationReverseCheckBox";
            this.animationReverseCheckBox.Size = new System.Drawing.Size(15, 14);
            this.animationReverseCheckBox.TabIndex = 27;
            this.animationReverseCheckBox.UseVisualStyleBackColor = true;
            this.animationReverseCheckBox.CheckedChanged += new System.EventHandler(this.animationReverseCheckBox_CheckedChanged);
            // 
            // animationPlayButtonx
            // 
            this.animationPlayButtonx.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.animationPlayButtonx.Enabled = false;
            this.animationPlayButtonx.Location = new System.Drawing.Point(3, 88);
            this.animationPlayButtonx.Name = "animationPlayButtonx";
            this.animationPlayButtonx.Size = new System.Drawing.Size(648, 34);
            this.animationPlayButtonx.TabIndex = 16;
            this.animationPlayButtonx.Text = "Play Animation";
            this.animationPlayButtonx.UseVisualStyleBackColor = true;
            this.animationPlayButtonx.Click += new System.EventHandler(this.animationPlayButton_Click);
            // 
            // exportModelsToolStripMenuItem
            // 
            this.exportModelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedModelsToolStripMenuItem,
            this.exportAllModelsToolStripMenuItem});
            this.exportModelsToolStripMenuItem.Name = "exportModelsToolStripMenuItem";
            this.exportModelsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.exportModelsToolStripMenuItem.Text = "Export Models";
            // 
            // exportSelectedModelsToolStripMenuItem
            // 
            this.exportSelectedModelsToolStripMenuItem.Name = "exportSelectedModelsToolStripMenuItem";
            this.exportSelectedModelsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.exportSelectedModelsToolStripMenuItem.Text = "Export Selected Models";
            this.exportSelectedModelsToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedModels_Click);
            // 
            // exportAllModelsToolStripMenuItem
            // 
            this.exportAllModelsToolStripMenuItem.Name = "exportAllModelsToolStripMenuItem";
            this.exportAllModelsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.exportAllModelsToolStripMenuItem.Text = "Export All Models";
            this.exportAllModelsToolStripMenuItem.Click += new System.EventHandler(this.exportAllModels_Click);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.modelsToolStripMenuItem,
            this.texturesToolStripMenuItem,
            this.vRAMToolStripMenuItem,
            this.animationsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(2);
            this.mainMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.mainMenuStrip.Size = new System.Drawing.Size(1008, 24);
            this.mainMenuStrip.TabIndex = 5;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pauseScanningToolStripMenuItem,
            this.stopScanningToolStripMenuItem,
            this.toolStripSeparator10,
            this.defaultSettingsToolStripMenuItem,
            this.loadSettingsToolStripMenuItem,
            this.saveSettingsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // pauseScanningToolStripMenuItem
            // 
            this.pauseScanningToolStripMenuItem.CheckOnClick = true;
            this.pauseScanningToolStripMenuItem.Name = "pauseScanningToolStripMenuItem";
            this.pauseScanningToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.pauseScanningToolStripMenuItem.Text = "Pause Scanning";
            this.pauseScanningToolStripMenuItem.CheckedChanged += new System.EventHandler(this.pauseScanningToolStripMenuItem_CheckedChanged);
            // 
            // stopScanningToolStripMenuItem
            // 
            this.stopScanningToolStripMenuItem.Name = "stopScanningToolStripMenuItem";
            this.stopScanningToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.stopScanningToolStripMenuItem.Text = "Stop Scanning";
            this.stopScanningToolStripMenuItem.Click += new System.EventHandler(this.stopScanningToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(204, 6);
            // 
            // defaultSettingsToolStripMenuItem
            // 
            this.defaultSettingsToolStripMenuItem.Name = "defaultSettingsToolStripMenuItem";
            this.defaultSettingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.defaultSettingsToolStripMenuItem.Text = "Reset Settings to Defaults";
            this.defaultSettingsToolStripMenuItem.Click += new System.EventHandler(this.defaultSettingsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(204, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // modelsToolStripMenuItem
            // 
            this.modelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportModelsToolStripMenuItem,
            this.resetTransformToolStripMenuItem,
            this.toolStripSeparator2,
            this.wireframeToolStripMenuItem,
            this.verticesOnlyToolStripMenuItem,
            this.showGizmosToolStripMenuItem,
            this.showBoundsToolStripMenuItem,
            this.enableLightToolStripMenuItem,
            this.enableSemiTransparencyToolStripMenuItem,
            this.forceDoubleSidedToolStripMenuItem,
            this.autoAttachLimbsToolStripMenuItem,
            this.toolStripSeparator7,
            this.setAmbientColorToolStripMenuItem,
            this.setBackgroundColorToolStripMenuItem,
            this.toolStripSeparator8,
            this.lineRendererToolStripMenuItem});
            this.modelsToolStripMenuItem.Name = "modelsToolStripMenuItem";
            this.modelsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.modelsToolStripMenuItem.Text = "Models";
            // 
            // resetTransformToolStripMenuItem
            // 
            this.resetTransformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetWholeModelToolStripMenuItem,
            this.resetSelectedModelToolStripMenuItem});
            this.resetTransformToolStripMenuItem.Name = "resetTransformToolStripMenuItem";
            this.resetTransformToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.resetTransformToolStripMenuItem.Text = "Reset Transform";
            // 
            // resetWholeModelToolStripMenuItem
            // 
            this.resetWholeModelToolStripMenuItem.Name = "resetWholeModelToolStripMenuItem";
            this.resetWholeModelToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.resetWholeModelToolStripMenuItem.Text = "Reset Whole Model";
            this.resetWholeModelToolStripMenuItem.Click += new System.EventHandler(this.resetWholeModelToolStripMenuItem_Click);
            // 
            // resetSelectedModelToolStripMenuItem
            // 
            this.resetSelectedModelToolStripMenuItem.Name = "resetSelectedModelToolStripMenuItem";
            this.resetSelectedModelToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.resetSelectedModelToolStripMenuItem.Text = "Reset Selected Model";
            this.resetSelectedModelToolStripMenuItem.Click += new System.EventHandler(this.resetSelectedModelToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(209, 6);
            // 
            // wireframeToolStripMenuItem
            // 
            this.wireframeToolStripMenuItem.CheckOnClick = true;
            this.wireframeToolStripMenuItem.Name = "wireframeToolStripMenuItem";
            this.wireframeToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.wireframeToolStripMenuItem.Text = "Wireframe";
            this.wireframeToolStripMenuItem.CheckedChanged += new System.EventHandler(this.wireframeToolStripMenuItem_CheckedChanged);
            // 
            // verticesOnlyToolStripMenuItem
            // 
            this.verticesOnlyToolStripMenuItem.CheckOnClick = true;
            this.verticesOnlyToolStripMenuItem.Name = "verticesOnlyToolStripMenuItem";
            this.verticesOnlyToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.verticesOnlyToolStripMenuItem.Text = "Vertices Only";
            this.verticesOnlyToolStripMenuItem.CheckedChanged += new System.EventHandler(this.verticesOnlyToolStripMenuItem_CheckedChanged);
            // 
            // showGizmosToolStripMenuItem
            // 
            this.showGizmosToolStripMenuItem.CheckOnClick = true;
            this.showGizmosToolStripMenuItem.Name = "showGizmosToolStripMenuItem";
            this.showGizmosToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.showGizmosToolStripMenuItem.Text = "Show Gizmos";
            this.showGizmosToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showGizmosToolStripMenuItem_CheckedChanged);
            // 
            // showBoundsToolStripMenuItem
            // 
            this.showBoundsToolStripMenuItem.CheckOnClick = true;
            this.showBoundsToolStripMenuItem.Name = "showBoundsToolStripMenuItem";
            this.showBoundsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.showBoundsToolStripMenuItem.Text = "Show Bounds";
            this.showBoundsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showBoundsToolStripMenuItem_CheckedChanged);
            // 
            // enableLightToolStripMenuItem
            // 
            this.enableLightToolStripMenuItem.CheckOnClick = true;
            this.enableLightToolStripMenuItem.Name = "enableLightToolStripMenuItem";
            this.enableLightToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.enableLightToolStripMenuItem.Text = "Enable Light";
            this.enableLightToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableLightToolStripMenuItem_CheckedChanged);
            // 
            // enableSemiTransparencyToolStripMenuItem
            // 
            this.enableSemiTransparencyToolStripMenuItem.CheckOnClick = true;
            this.enableSemiTransparencyToolStripMenuItem.Name = "enableSemiTransparencyToolStripMenuItem";
            this.enableSemiTransparencyToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.enableSemiTransparencyToolStripMenuItem.Text = "Enable Semi-Transparency";
            this.enableSemiTransparencyToolStripMenuItem.CheckedChanged += new System.EventHandler(this.enableTransparencyToolStripMenuItem_CheckedChanged);
            // 
            // forceDoubleSidedToolStripMenuItem
            // 
            this.forceDoubleSidedToolStripMenuItem.CheckOnClick = true;
            this.forceDoubleSidedToolStripMenuItem.Name = "forceDoubleSidedToolStripMenuItem";
            this.forceDoubleSidedToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.forceDoubleSidedToolStripMenuItem.Text = "Force Double-Sided";
            this.forceDoubleSidedToolStripMenuItem.CheckedChanged += new System.EventHandler(this.forceDoubleSidedToolStripMenuItem_CheckedChanged);
            // 
            // autoAttachLimbsToolStripMenuItem
            // 
            this.autoAttachLimbsToolStripMenuItem.CheckOnClick = true;
            this.autoAttachLimbsToolStripMenuItem.Name = "autoAttachLimbsToolStripMenuItem";
            this.autoAttachLimbsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.autoAttachLimbsToolStripMenuItem.Text = "Auto Attach Limbs";
            this.autoAttachLimbsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoAttachLimbsToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(209, 6);
            // 
            // setAmbientColorToolStripMenuItem
            // 
            this.setAmbientColorToolStripMenuItem.Name = "setAmbientColorToolStripMenuItem";
            this.setAmbientColorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.setAmbientColorToolStripMenuItem.Text = "Set Ambient Color";
            this.setAmbientColorToolStripMenuItem.Click += new System.EventHandler(this.setAmbientColorToolStripMenuItem_Click);
            // 
            // setBackgroundColorToolStripMenuItem
            // 
            this.setBackgroundColorToolStripMenuItem.Name = "setBackgroundColorToolStripMenuItem";
            this.setBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.setBackgroundColorToolStripMenuItem.Text = "Set Background Color";
            this.setBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.setBackgroundColorToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(209, 6);
            // 
            // lineRendererToolStripMenuItem
            // 
            this.lineRendererToolStripMenuItem.CheckOnClick = true;
            this.lineRendererToolStripMenuItem.Name = "lineRendererToolStripMenuItem";
            this.lineRendererToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
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
            this.toolStripSeparator1,
            this.autoDrawModelTexturesToolStripMenuItem,
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
            this.exportTexturesToolStripMenuItem.Name = "exportTexturesToolStripMenuItem";
            this.exportTexturesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportTexturesToolStripMenuItem.Text = "Export Textures";
            // 
            // exportSelectedTexturesToolStripMenuItem
            // 
            this.exportSelectedTexturesToolStripMenuItem.Name = "exportSelectedTexturesToolStripMenuItem";
            this.exportSelectedTexturesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.exportSelectedTexturesToolStripMenuItem.Text = "Export Selected Textures";
            this.exportSelectedTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedTextures_Click);
            // 
            // exportAllTexturesToolStripMenuItem
            // 
            this.exportAllTexturesToolStripMenuItem.Name = "exportAllTexturesToolStripMenuItem";
            this.exportAllTexturesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.exportAllTexturesToolStripMenuItem.Text = "Export All Textures";
            this.exportAllTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportAllTextures_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // drawToVRAMToolStripMenuItem
            // 
            this.drawToVRAMToolStripMenuItem.Name = "drawToVRAMToolStripMenuItem";
            this.drawToVRAMToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.drawToVRAMToolStripMenuItem.Text = "Draw to VRAM";
            this.drawToVRAMToolStripMenuItem.Click += new System.EventHandler(this.drawSelectedToVRAM_Click);
            // 
            // drawAllToVRAMToolStripMenuItem
            // 
            this.drawAllToVRAMToolStripMenuItem.Name = "drawAllToVRAMToolStripMenuItem";
            this.drawAllToVRAMToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.drawAllToVRAMToolStripMenuItem.Text = "Draw All to VRAM";
            this.drawAllToVRAMToolStripMenuItem.Click += new System.EventHandler(this.drawAllToVRAM_Click);
            // 
            // findByPageToolStripMenuItem
            // 
            this.findByPageToolStripMenuItem.Name = "findByPageToolStripMenuItem";
            this.findByPageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.findByPageToolStripMenuItem.Text = "Find by Page";
            this.findByPageToolStripMenuItem.Click += new System.EventHandler(this.findByPageToolStripMenuItem_Click);
            // 
            // clearSearchToolStripMenuItem
            // 
            this.clearSearchToolStripMenuItem.Name = "clearSearchToolStripMenuItem";
            this.clearSearchToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.clearSearchToolStripMenuItem.Text = "Clear Results";
            this.clearSearchToolStripMenuItem.Click += new System.EventHandler(this.clearSearchToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // autoDrawModelTexturesToolStripMenuItem
            // 
            this.autoDrawModelTexturesToolStripMenuItem.CheckOnClick = true;
            this.autoDrawModelTexturesToolStripMenuItem.Name = "autoDrawModelTexturesToolStripMenuItem";
            this.autoDrawModelTexturesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.autoDrawModelTexturesToolStripMenuItem.Text = "Auto Draw Textures";
            this.autoDrawModelTexturesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoDrawModelTexturesToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(177, 6);
            // 
            // setMaskColorToolStripMenuItem
            // 
            this.setMaskColorToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.setMaskColorToolStripMenuItem.Name = "setMaskColorToolStripMenuItem";
            this.setMaskColorToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.setMaskColorToolStripMenuItem.Text = "Set Mask Color";
            this.setMaskColorToolStripMenuItem.Click += new System.EventHandler(this.setMaskColorToolStripMenuItem_Click);
            // 
            // vRAMToolStripMenuItem
            // 
            this.vRAMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportVRAMPagesToolStripMenuItem,
            this.toolStripSeparator9,
            this.clearPageToolStripMenuItem,
            this.clearAllPagesToolStripMenuItem,
            this.toolStripSeparator4,
            this.showUVToolStripMenuItem});
            this.vRAMToolStripMenuItem.Name = "vRAMToolStripMenuItem";
            this.vRAMToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.vRAMToolStripMenuItem.Text = "VRAM";
            // 
            // exportVRAMPagesToolStripMenuItem
            // 
            this.exportVRAMPagesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedVRAMPageToolStripMenuItem,
            this.exportDrawnToVRAMPagesToolStripMenuItem,
            this.exportAllVRAMPagesToolStripMenuItem});
            this.exportVRAMPagesToolStripMenuItem.Name = "exportVRAMPagesToolStripMenuItem";
            this.exportVRAMPagesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exportVRAMPagesToolStripMenuItem.Text = "Export Pages";
            // 
            // exportSelectedVRAMPageToolStripMenuItem
            // 
            this.exportSelectedVRAMPageToolStripMenuItem.Name = "exportSelectedVRAMPageToolStripMenuItem";
            this.exportSelectedVRAMPageToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exportSelectedVRAMPageToolStripMenuItem.Text = "Export Selected Page";
            this.exportSelectedVRAMPageToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedVRAMPage_Click);
            // 
            // exportDrawnToVRAMPagesToolStripMenuItem
            // 
            this.exportDrawnToVRAMPagesToolStripMenuItem.Name = "exportDrawnToVRAMPagesToolStripMenuItem";
            this.exportDrawnToVRAMPagesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exportDrawnToVRAMPagesToolStripMenuItem.Text = "Export Drawn-to Pages";
            this.exportDrawnToVRAMPagesToolStripMenuItem.Click += new System.EventHandler(this.exportDrawnToVRAMPages_Click);
            // 
            // exportAllVRAMPagesToolStripMenuItem
            // 
            this.exportAllVRAMPagesToolStripMenuItem.Name = "exportAllVRAMPagesToolStripMenuItem";
            this.exportAllVRAMPagesToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exportAllVRAMPagesToolStripMenuItem.Text = "Export All Pages";
            this.exportAllVRAMPagesToolStripMenuItem.Click += new System.EventHandler(this.exportAllVRAMPages_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(149, 6);
            // 
            // clearPageToolStripMenuItem
            // 
            this.clearPageToolStripMenuItem.Name = "clearPageToolStripMenuItem";
            this.clearPageToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.clearPageToolStripMenuItem.Text = "Clear Page";
            this.clearPageToolStripMenuItem.Click += new System.EventHandler(this.btnClearPage_Click);
            // 
            // clearAllPagesToolStripMenuItem
            // 
            this.clearAllPagesToolStripMenuItem.Name = "clearAllPagesToolStripMenuItem";
            this.clearAllPagesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.clearAllPagesToolStripMenuItem.Text = "Clear All Pages";
            this.clearAllPagesToolStripMenuItem.Click += new System.EventHandler(this.clearAllPagesToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(149, 6);
            // 
            // showUVToolStripMenuItem
            // 
            this.showUVToolStripMenuItem.CheckOnClick = true;
            this.showUVToolStripMenuItem.Name = "showUVToolStripMenuItem";
            this.showUVToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showUVToolStripMenuItem.Text = "Show UV";
            this.showUVToolStripMenuItem.Click += new System.EventHandler(this.showUVToolStripMenuItem_Click);
            // 
            // animationsToolStripMenuItem
            // 
            this.animationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoPlayAnimationsToolStripMenuItem,
            this.autoSelectAnimationModelToolStripMenuItem,
            this.toolStripMenuItem2,
            this.showTMDBindingsToolStripMenuItem});
            this.animationsToolStripMenuItem.Name = "animationsToolStripMenuItem";
            this.animationsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.animationsToolStripMenuItem.Text = "Animations";
            // 
            // autoPlayAnimationsToolStripMenuItem
            // 
            this.autoPlayAnimationsToolStripMenuItem.CheckOnClick = true;
            this.autoPlayAnimationsToolStripMenuItem.Name = "autoPlayAnimationsToolStripMenuItem";
            this.autoPlayAnimationsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.autoPlayAnimationsToolStripMenuItem.Text = "Auto Play Animation";
            this.autoPlayAnimationsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoPlayAnimationsToolStripMenuItem_CheckedChanged);
            // 
            // autoSelectAnimationModelToolStripMenuItem
            // 
            this.autoSelectAnimationModelToolStripMenuItem.CheckOnClick = true;
            this.autoSelectAnimationModelToolStripMenuItem.Name = "autoSelectAnimationModelToolStripMenuItem";
            this.autoSelectAnimationModelToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.autoSelectAnimationModelToolStripMenuItem.Text = "Auto Select Model";
            this.autoSelectAnimationModelToolStripMenuItem.CheckedChanged += new System.EventHandler(this.autoSelectAnimationModelToolStripMenuItem_CheckedChanged);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(181, 6);
            // 
            // showTMDBindingsToolStripMenuItem
            // 
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
            this.videoTutorialToolStripMenuItem.Name = "videoTutorialToolStripMenuItem";
            this.videoTutorialToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.videoTutorialToolStripMenuItem.Text = "Video Tutorial";
            this.videoTutorialToolStripMenuItem.Click += new System.EventHandler(this.videoTutorialToolStripMenuItem_Click);
            // 
            // compatibilityListToolStripMenuItem
            // 
            this.compatibilityListToolStripMenuItem.Name = "compatibilityListToolStripMenuItem";
            this.compatibilityListToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.compatibilityListToolStripMenuItem.Text = "Compatibility List";
            this.compatibilityListToolStripMenuItem.Click += new System.EventHandler(this.compatibilityListToolStripMenuItem_Click);
            // 
            // viewOnGitHubToolStripMenuItem
            // 
            this.viewOnGitHubToolStripMenuItem.Name = "viewOnGitHubToolStripMenuItem";
            this.viewOnGitHubToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.viewOnGitHubToolStripMenuItem.Text = "View on GitHub";
            this.viewOnGitHubToolStripMenuItem.Click += new System.EventHandler(this.viewOnGitHubToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(164, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.messageToolStripLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 649);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.Resize += new System.EventHandler(this.statusStrip1_Resize);
            // 
            // messageToolStripLabel
            // 
            this.messageToolStripLabel.AutoSize = false;
            this.messageToolStripLabel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
            this.messageToolStripLabel.Name = "messageToolStripLabel";
            this.messageToolStripLabel.Size = new System.Drawing.Size(200, 17);
            this.messageToolStripLabel.Text = "Waiting";
            this.messageToolStripLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel1);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel4);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel5);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel6);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 623);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(1008, 26);
            this.flowLayoutPanel2.TabIndex = 21;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.gridSizeNumericUpDown);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(898, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(110, 26);
            this.flowLayoutPanel1.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 26);
            this.label2.TabIndex = 17;
            this.label2.Text = "Grid Align:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gridSizeNumericUpDown
            // 
            this.gridSizeNumericUpDown.Location = new System.Drawing.Point(64, 3);
            this.gridSizeNumericUpDown.Name = "gridSizeNumericUpDown";
            this.gridSizeNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.gridSizeNumericUpDown.TabIndex = 18;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.Controls.Add(this.label1);
            this.flowLayoutPanel3.Controls.Add(this.lightYawNumericUpDown);
            this.flowLayoutPanel3.Controls.Add(this.lightPitchNumericUpDown);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(718, 0);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(180, 26);
            this.flowLayoutPanel3.TabIndex = 25;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 26);
            this.label1.TabIndex = 15;
            this.label1.Text = "Light Rotation:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.lightYawNumericUpDown.TabIndex = 21;
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
            this.lightPitchNumericUpDown.TabIndex = 22;
            this.lightPitchNumericUpDown.ValueChanged += new System.EventHandler(this.lightPitchNumericUpDown_ValueChanged);
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.AutoSize = true;
            this.flowLayoutPanel4.Controls.Add(this.label4);
            this.flowLayoutPanel4.Controls.Add(this.lightIntensityNumericUpDown);
            this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel4.Location = new System.Drawing.Point(588, 0);
            this.flowLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(130, 26);
            this.flowLayoutPanel4.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 26);
            this.label4.TabIndex = 19;
            this.label4.Text = "Light Intensity:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lightIntensityNumericUpDown
            // 
            this.lightIntensityNumericUpDown.Location = new System.Drawing.Point(84, 3);
            this.lightIntensityNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.lightIntensityNumericUpDown.Name = "lightIntensityNumericUpDown";
            this.lightIntensityNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.lightIntensityNumericUpDown.TabIndex = 20;
            this.lightIntensityNumericUpDown.ValueChanged += new System.EventHandler(this.lightIntensityNumericUpDown_ValueChanged);
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.AutoSize = true;
            this.flowLayoutPanel5.Controls.Add(this.label5);
            this.flowLayoutPanel5.Controls.Add(this.vertexSizeUpDown);
            this.flowLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel5.Location = new System.Drawing.Point(470, 0);
            this.flowLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(118, 26);
            this.flowLayoutPanel5.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Left;
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 26);
            this.label5.TabIndex = 21;
            this.label5.Text = "Vertex Size:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // vertexSizeUpDown
            // 
            this.vertexSizeUpDown.Location = new System.Drawing.Point(72, 3);
            this.vertexSizeUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.vertexSizeUpDown.Name = "vertexSizeUpDown";
            this.vertexSizeUpDown.Size = new System.Drawing.Size(43, 20);
            this.vertexSizeUpDown.TabIndex = 22;
            this.vertexSizeUpDown.ValueChanged += new System.EventHandler(this.vertexSizeUpDown_ValueChanged);
            // 
            // flowLayoutPanel6
            // 
            this.flowLayoutPanel6.AutoSize = true;
            this.flowLayoutPanel6.Controls.Add(this.label7);
            this.flowLayoutPanel6.Controls.Add(this.cameraFOVUpDown);
            this.flowLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel6.Location = new System.Drawing.Point(384, 0);
            this.flowLayoutPanel6.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel6.Name = "flowLayoutPanel6";
            this.flowLayoutPanel6.Size = new System.Drawing.Size(86, 26);
            this.flowLayoutPanel6.TabIndex = 28;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Left;
            this.label7.Location = new System.Drawing.Point(3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 26);
            this.label7.TabIndex = 24;
            this.label7.Text = "FOV:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.cameraFOVUpDown.TabIndex = 25;
            this.cameraFOVUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cameraFOVUpDown.ValueChanged += new System.EventHandler(this.cameraFOVUpDown_ValueChanged);
            // 
            // loadSettingsToolStripMenuItem
            // 
            this.loadSettingsToolStripMenuItem.Name = "loadSettingsToolStripMenuItem";
            this.loadSettingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.loadSettingsToolStripMenuItem.Text = "Reload Settings";
            this.loadSettingsToolStripMenuItem.Click += new System.EventHandler(this.loadSettingsToolStripMenuItem_Click);
            // 
            // saveSettingsToolStripMenuItem
            // 
            this.saveSettingsToolStripMenuItem.Name = "saveSettingsToolStripMenuItem";
            this.saveSettingsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.saveSettingsToolStripMenuItem.Text = "Save Current Settings";
            this.saveSettingsToolStripMenuItem.Click += new System.EventHandler(this.saveSettingsToolStripMenuItem_Click);
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 671);
            this.Controls.Add(this.menusTabControl);
            this.Controls.Add(this.flowLayoutPanel2);
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
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.previewForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.previewForm_KeyUp);
            this.entitiesTabPage.ResumeLayout(false);
            this.modelsSplitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.modelsSplitContainer)).EndInit();
            this.modelsSplitContainer.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.menusTabControl.ResumeLayout(false);
            this.bitmapsTabPage.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.texturePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.texturePreviewPictureBox)).EndInit();
            this.vramTabPage.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).EndInit();
            this.splitContainer7.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.vramPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.vramPagePictureBox)).EndInit();
            this.animationsTabPage.ResumeLayout(false);
            this.animationsSplitContainer.Panel1.ResumeLayout(false);
            this.animationsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.animationsSplitContainer)).EndInit();
            this.animationsSplitContainer.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.animationsTableLayoutPanel.ResumeLayout(false);
            this.animationsTableLayoutPanel.PerformLayout();
            this.animationGroupBox.ResumeLayout(false);
            this.animationGroupBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationFrameTrackBar)).EndInit();
            this.flowLayoutPanel7.ResumeLayout(false);
            this.flowLayoutPanel7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationSpeedNumericUpDown)).EndInit();
            this.flowLayoutPanel8.ResumeLayout(false);
            this.flowLayoutPanel8.PerformLayout();
            this.flowLayoutPanel9.ResumeLayout(false);
            this.flowLayoutPanel9.PerformLayout();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSizeNumericUpDown)).EndInit();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightYawNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightPitchNumericUpDown)).EndInit();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightIntensityNumericUpDown)).EndInit();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vertexSizeUpDown)).EndInit();
            this.flowLayoutPanel6.ResumeLayout(false);
            this.flowLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraFOVUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl menusTabControl;
        private System.Windows.Forms.TabPage entitiesTabPage;
        private System.Windows.Forms.TabPage bitmapsTabPage;
        private System.Windows.Forms.Button exportSelectedModelsButton;
        private System.Windows.Forms.Button exportBitmapButton;
        private System.Windows.Forms.TreeView entitiesTreeView;
        private System.Windows.Forms.TabPage vramTabPage;
        private System.Windows.Forms.PictureBox vramPagePictureBox;
        private System.Windows.Forms.Panel texturePanel;
        private System.Windows.Forms.PictureBox texturePreviewPictureBox;
        private System.Windows.Forms.Button drawToVRAMButton;
        private System.Windows.Forms.PropertyGrid modelPropertyGrid;
        private System.Windows.Forms.PropertyGrid texturePropertyGrid;
        private System.Windows.Forms.Button btnClearPage;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem modelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vRAMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportModelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawToVRAMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findByPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearPageToolStripMenuItem;
        private Manina.Windows.Forms.ImageListView texturesListView;
        private System.Windows.Forms.ToolStripMenuItem clearSearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wireframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllPagesToolStripMenuItem;
        private System.Windows.Forms.TabPage animationsTabPage;
        private System.Windows.Forms.ToolStripMenuItem animationsToolStripMenuItem;
        private System.Windows.Forms.PropertyGrid animationPropertyGrid;
        private System.Windows.Forms.TreeView animationsTreeView;
        private System.Windows.Forms.Button animationPlayButtonx;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel messageToolStripLabel;
        private System.Windows.Forms.ToolStripMenuItem showGizmosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showUVToolStripMenuItem;
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
        private System.Windows.Forms.Label label3;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer modelsSplitContainer;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer animationsSplitContainer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripMenuItem enableSemiTransparencyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forceDoubleSidedToolStripMenuItem;
        private System.Windows.Forms.Label texturesZoomLabel;
        private System.Windows.Forms.ToolStripMenuItem verticesOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseScanningToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem autoDrawModelTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem autoSelectAnimationModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoPlayAnimationsToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.ListBox vramListBox;
        private System.Windows.Forms.Panel vramPanel;
        private System.Windows.Forms.Label vramZoomLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button gotoPageButton;
        private System.Windows.Forms.TableLayoutPanel animationsTableLayoutPanel;
        private System.Windows.Forms.TrackBar animationFrameTrackBar;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label animationProgressLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.ToolStripMenuItem stopScanningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTMDBindingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewOnGitHubToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown gridSizeNumericUpDown;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown lightYawNumericUpDown;
        private System.Windows.Forms.NumericUpDown lightPitchNumericUpDown;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown lightIntensityNumericUpDown;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown vertexSizeUpDown;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown cameraFOVUpDown;
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
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel7;
        private System.Windows.Forms.NumericUpDown animationSpeedNumericUpDown;
        private System.Windows.Forms.ToolStripMenuItem exportDrawnToVRAMPagesToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel8;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox animationLoopModeComboBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel9;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox animationReverseCheckBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripMenuItem defaultSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem loadSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSettingsToolStripMenuItem;
    }
}
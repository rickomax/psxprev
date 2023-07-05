namespace PSXPrev
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Found", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewForm));
            this.entitiesTabPage = new System.Windows.Forms.TabPage();
            this.modelsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.entitiesTreeView = new System.Windows.Forms.TreeView();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.exportEntityButton = new System.Windows.Forms.Button();
            this.modelPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.menusTabControl = new System.Windows.Forms.TabControl();
            this.bitmapsTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.texturesListView = new System.Windows.Forms.ListView();
            this.thumbsImageList = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.drawToVRAMButton = new System.Windows.Forms.Button();
            this.texturePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.exportBitmapButton = new System.Windows.Forms.Button();
            this.texturePanel = new System.Windows.Forms.Panel();
            this.texturePreviewPictureBox = new System.Windows.Forms.PictureBox();
            this.vramTabPage = new System.Windows.Forms.TabPage();
            this.btnClearPage = new System.Windows.Forms.Button();
            this.vramPageLabel = new System.Windows.Forms.Label();
            this.vramComboBox = new System.Windows.Forms.ComboBox();
            this.vramPagePictureBox = new System.Windows.Forms.PictureBox();
            this.animationsTabPage = new System.Windows.Forms.TabPage();
            this.animationsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.animationsTreeView = new System.Windows.Forms.TreeView();
            this.animationPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.label3 = new System.Windows.Forms.Label();
            this.animationSpeedTrackbar = new System.Windows.Forms.TrackBar();
            this.animationPlayButton = new System.Windows.Forms.Button();
            this.cmsModelExport = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miOBJ = new System.Windows.Forms.ToolStripMenuItem();
            this.miOBJVC = new System.Windows.Forms.ToolStripMenuItem();
            this.miOBJMerged = new System.Windows.Forms.ToolStripMenuItem();
            this.miOBJVCMerged = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetTransformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsResetTransform = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetWholeModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSelectedModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.wireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showGizmosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAttachLimbsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.setAmbientColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.lineRendererToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.drawToVRAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findByPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.setMaskColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vRAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.showUVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSkeletonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoTutorialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compatibilityListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.gridSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.lightPitchNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.lightYawNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.lightRollNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.lightIntensityNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.animationFrameLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.vramPagePictureBox)).BeginInit();
            this.animationsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationsSplitContainer)).BeginInit();
            this.animationsSplitContainer.Panel1.SuspendLayout();
            this.animationsSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationSpeedTrackbar)).BeginInit();
            this.cmsModelExport.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.cmsResetTransform.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSizeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightPitchNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightYawNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightRollNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightIntensityNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // entitiesTabPage
            // 
            this.entitiesTabPage.Controls.Add(this.modelsSplitContainer);
            this.entitiesTabPage.Location = new System.Drawing.Point(4, 22);
            this.entitiesTabPage.Name = "entitiesTabPage";
            this.entitiesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.entitiesTabPage.Size = new System.Drawing.Size(760, 599);
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
            this.modelsSplitContainer.Size = new System.Drawing.Size(754, 593);
            this.modelsSplitContainer.SplitterDistance = 251;
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
            this.splitContainer6.Size = new System.Drawing.Size(251, 593);
            this.splitContainer6.SplitterDistance = 148;
            this.splitContainer6.TabIndex = 0;
            // 
            // entitiesTreeView
            // 
            this.entitiesTreeView.CheckBoxes = true;
            this.entitiesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entitiesTreeView.HideSelection = false;
            this.entitiesTreeView.Location = new System.Drawing.Point(0, 0);
            this.entitiesTreeView.Name = "entitiesTreeView";
            this.entitiesTreeView.Size = new System.Drawing.Size(251, 148);
            this.entitiesTreeView.TabIndex = 9;
            this.entitiesTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.entitiesTreeView_AfterCheck);
            this.entitiesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.entitiesTreeView_AfterSelect);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.exportEntityButton, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.modelPropertyGrid, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.Size = new System.Drawing.Size(251, 441);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // exportEntityButton
            // 
            this.exportEntityButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.exportEntityButton.Location = new System.Drawing.Point(0, 407);
            this.exportEntityButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.exportEntityButton.Name = "exportEntityButton";
            this.exportEntityButton.Size = new System.Drawing.Size(251, 34);
            this.exportEntityButton.TabIndex = 8;
            this.exportEntityButton.Text = "Export Checked Models";
            this.exportEntityButton.UseVisualStyleBackColor = true;
            this.exportEntityButton.Click += new System.EventHandler(this.exportEntityButton_Click);
            // 
            // modelPropertyGrid
            // 
            this.modelPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelPropertyGrid.HelpVisible = false;
            this.modelPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.modelPropertyGrid.Margin = new System.Windows.Forms.Padding(0);
            this.modelPropertyGrid.Name = "modelPropertyGrid";
            this.modelPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.modelPropertyGrid.Size = new System.Drawing.Size(251, 404);
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
            this.menusTabControl.Location = new System.Drawing.Point(0, 0);
            this.menusTabControl.Name = "menusTabControl";
            this.menusTabControl.SelectedIndex = 0;
            this.menusTabControl.Size = new System.Drawing.Size(768, 625);
            this.menusTabControl.TabIndex = 3;
            this.menusTabControl.SelectedIndexChanged += new System.EventHandler(this.menusTabControl_SelectedIndexChanged);
            // 
            // bitmapsTabPage
            // 
            this.bitmapsTabPage.Controls.Add(this.splitContainer2);
            this.bitmapsTabPage.Location = new System.Drawing.Point(4, 22);
            this.bitmapsTabPage.Name = "bitmapsTabPage";
            this.bitmapsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.bitmapsTabPage.Size = new System.Drawing.Size(760, 599);
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
            this.splitContainer2.Panel2.Controls.Add(this.texturePanel);
            this.splitContainer2.Size = new System.Drawing.Size(754, 593);
            this.splitContainer2.SplitterDistance = 251;
            this.splitContainer2.TabIndex = 17;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.splitContainer3.Size = new System.Drawing.Size(251, 593);
            this.splitContainer3.SplitterDistance = 148;
            this.splitContainer3.TabIndex = 0;
            // 
            // texturesListView
            // 
            this.texturesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "Found";
            listViewGroup1.Name = "foundListViewGroup";
            this.texturesListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.texturesListView.HideSelection = false;
            this.texturesListView.LargeImageList = this.thumbsImageList;
            this.texturesListView.Location = new System.Drawing.Point(0, 0);
            this.texturesListView.Name = "texturesListView";
            this.texturesListView.Size = new System.Drawing.Size(251, 148);
            this.texturesListView.TabIndex = 16;
            this.texturesListView.UseCompatibleStateImageBehavior = false;
            this.texturesListView.SelectedIndexChanged += new System.EventHandler(this.texturesListView_SelectedIndexChanged);
            // 
            // thumbsImageList
            // 
            this.thumbsImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.thumbsImageList.ImageSize = new System.Drawing.Size(64, 64);
            this.thumbsImageList.TransparentColor = System.Drawing.Color.Transparent;
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(251, 441);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // drawToVRAMButton
            // 
            this.drawToVRAMButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.drawToVRAMButton.Location = new System.Drawing.Point(0, 407);
            this.drawToVRAMButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.drawToVRAMButton.Name = "drawToVRAMButton";
            this.drawToVRAMButton.Size = new System.Drawing.Size(251, 34);
            this.drawToVRAMButton.TabIndex = 13;
            this.drawToVRAMButton.Text = "Draw to VRAM";
            this.drawToVRAMButton.UseVisualStyleBackColor = true;
            this.drawToVRAMButton.Click += new System.EventHandler(this.drawToVRAMButton_Click);
            // 
            // texturePropertyGrid
            // 
            this.texturePropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturePropertyGrid.HelpVisible = false;
            this.texturePropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.texturePropertyGrid.Margin = new System.Windows.Forms.Padding(0);
            this.texturePropertyGrid.Name = "texturePropertyGrid";
            this.texturePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.texturePropertyGrid.Size = new System.Drawing.Size(251, 367);
            this.texturePropertyGrid.TabIndex = 15;
            this.texturePropertyGrid.ToolbarVisible = false;
            this.texturePropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.texturePropertyGrid_PropertyValueChanged);
            // 
            // exportBitmapButton
            // 
            this.exportBitmapButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.exportBitmapButton.Location = new System.Drawing.Point(0, 370);
            this.exportBitmapButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.exportBitmapButton.Name = "exportBitmapButton";
            this.exportBitmapButton.Size = new System.Drawing.Size(251, 34);
            this.exportBitmapButton.TabIndex = 9;
            this.exportBitmapButton.Text = "Export Selected Textures";
            this.exportBitmapButton.UseVisualStyleBackColor = true;
            this.exportBitmapButton.Click += new System.EventHandler(this.exportBitmapButton_Click);
            // 
            // texturePanel
            // 
            this.texturePanel.AutoScroll = true;
            this.texturePanel.Controls.Add(this.texturePreviewPictureBox);
            this.texturePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturePanel.Location = new System.Drawing.Point(0, 0);
            this.texturePanel.Name = "texturePanel";
            this.texturePanel.Size = new System.Drawing.Size(499, 593);
            this.texturePanel.TabIndex = 11;
            // 
            // texturePreviewPictureBox
            // 
            this.texturePreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.texturePreviewPictureBox.Location = new System.Drawing.Point(0, 0);
            this.texturePreviewPictureBox.Name = "texturePreviewPictureBox";
            this.texturePreviewPictureBox.Size = new System.Drawing.Size(256, 256);
            this.texturePreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.texturePreviewPictureBox.TabIndex = 9;
            this.texturePreviewPictureBox.TabStop = false;
            // 
            // vramTabPage
            // 
            this.vramTabPage.Controls.Add(this.btnClearPage);
            this.vramTabPage.Controls.Add(this.vramPageLabel);
            this.vramTabPage.Controls.Add(this.vramComboBox);
            this.vramTabPage.Controls.Add(this.vramPagePictureBox);
            this.vramTabPage.Location = new System.Drawing.Point(4, 22);
            this.vramTabPage.Name = "vramTabPage";
            this.vramTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.vramTabPage.Size = new System.Drawing.Size(760, 599);
            this.vramTabPage.TabIndex = 2;
            this.vramTabPage.Text = "VRAM";
            this.vramTabPage.UseVisualStyleBackColor = true;
            // 
            // btnClearPage
            // 
            this.btnClearPage.Location = new System.Drawing.Point(177, 20);
            this.btnClearPage.Name = "btnClearPage";
            this.btnClearPage.Size = new System.Drawing.Size(81, 23);
            this.btnClearPage.TabIndex = 12;
            this.btnClearPage.Text = "Clear Page";
            this.btnClearPage.UseVisualStyleBackColor = true;
            this.btnClearPage.Click += new System.EventHandler(this.btnClearPage_Click);
            // 
            // vramPageLabel
            // 
            this.vramPageLabel.AutoSize = true;
            this.vramPageLabel.Location = new System.Drawing.Point(-1, 3);
            this.vramPageLabel.Name = "vramPageLabel";
            this.vramPageLabel.Size = new System.Drawing.Size(66, 13);
            this.vramPageLabel.TabIndex = 11;
            this.vramPageLabel.Text = "VRAM Page";
            // 
            // vramComboBox
            // 
            this.vramComboBox.FormattingEnabled = true;
            this.vramComboBox.Items.AddRange(new object[] {
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
            this.vramComboBox.Location = new System.Drawing.Point(1, 21);
            this.vramComboBox.Name = "vramComboBox";
            this.vramComboBox.Size = new System.Drawing.Size(172, 21);
            this.vramComboBox.TabIndex = 10;
            this.vramComboBox.Text = "Select";
            this.vramComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // vramPagePictureBox
            // 
            this.vramPagePictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.vramPagePictureBox.Location = new System.Drawing.Point(1, 47);
            this.vramPagePictureBox.Name = "vramPagePictureBox";
            this.vramPagePictureBox.Size = new System.Drawing.Size(256, 256);
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
            this.animationsTabPage.Size = new System.Drawing.Size(760, 599);
            this.animationsTabPage.TabIndex = 3;
            this.animationsTabPage.Text = "Animations";
            this.animationsTabPage.UseVisualStyleBackColor = true;
            // 
            // animationsSplitContainer
            // 
            this.animationsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.animationsSplitContainer.Name = "animationsSplitContainer";
            // 
            // animationsSplitContainer.Panel1
            // 
            this.animationsSplitContainer.Panel1.Controls.Add(this.splitContainer5);
            this.animationsSplitContainer.Size = new System.Drawing.Size(754, 593);
            this.animationsSplitContainer.SplitterDistance = 251;
            this.animationsSplitContainer.TabIndex = 16;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.splitContainer5.Size = new System.Drawing.Size(251, 593);
            this.splitContainer5.SplitterDistance = 147;
            this.splitContainer5.TabIndex = 0;
            // 
            // animationsTreeView
            // 
            this.animationsTreeView.CheckBoxes = true;
            this.animationsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsTreeView.HideSelection = false;
            this.animationsTreeView.Location = new System.Drawing.Point(0, 0);
            this.animationsTreeView.Name = "animationsTreeView";
            this.animationsTreeView.Size = new System.Drawing.Size(251, 147);
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
            this.animationPropertyGrid.Size = new System.Drawing.Size(251, 442);
            this.animationPropertyGrid.TabIndex = 15;
            this.animationPropertyGrid.ToolbarVisible = false;
            this.animationPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.animationPropertyGrid_PropertyValueChanged);
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 26);
            this.label3.TabIndex = 21;
            this.label3.Text = "Speed:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // animationSpeedTrackbar
            // 
            this.animationSpeedTrackbar.AutoSize = false;
            this.animationSpeedTrackbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationSpeedTrackbar.Location = new System.Drawing.Point(118, 3);
            this.animationSpeedTrackbar.Maximum = 1000;
            this.animationSpeedTrackbar.Minimum = 10;
            this.animationSpeedTrackbar.Name = "animationSpeedTrackbar";
            this.animationSpeedTrackbar.Size = new System.Drawing.Size(109, 20);
            this.animationSpeedTrackbar.TabIndex = 17;
            this.animationSpeedTrackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.animationSpeedTrackbar.Value = 100;
            this.animationSpeedTrackbar.Scroll += new System.EventHandler(this.animationSpeedTrackbar_Scroll);
            // 
            // animationPlayButton
            // 
            this.animationPlayButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.animationPlayButton.Enabled = false;
            this.animationPlayButton.Location = new System.Drawing.Point(3, 71);
            this.animationPlayButton.Name = "animationPlayButton";
            this.animationPlayButton.Size = new System.Drawing.Size(230, 34);
            this.animationPlayButton.TabIndex = 16;
            this.animationPlayButton.Text = "Play Animation";
            this.animationPlayButton.UseVisualStyleBackColor = true;
            this.animationPlayButton.Click += new System.EventHandler(this.animationPlayButton_Click);
            // 
            // cmsModelExport
            // 
            this.cmsModelExport.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miOBJ,
            this.miOBJVC,
            this.miOBJMerged,
            this.miOBJVCMerged});
            this.cmsModelExport.Name = "cmsModelExport";
            this.cmsModelExport.OwnerItem = this.exportSelectedToolStripMenuItem;
            this.cmsModelExport.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.cmsModelExport.Size = new System.Drawing.Size(355, 92);
            this.cmsModelExport.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cmsModelExport_ItemClicked);
            // 
            // miOBJ
            // 
            this.miOBJ.Name = "miOBJ";
            this.miOBJ.Size = new System.Drawing.Size(354, 22);
            this.miOBJ.Text = "Wavefront .OBJ";
            // 
            // miOBJVC
            // 
            this.miOBJVC.Name = "miOBJVC";
            this.miOBJVC.Size = new System.Drawing.Size(354, 22);
            this.miOBJVC.Text = "Wavefront .OBJ (Experimental Vertex Color)";
            // 
            // miOBJMerged
            // 
            this.miOBJMerged.Name = "miOBJMerged";
            this.miOBJMerged.Size = new System.Drawing.Size(354, 22);
            this.miOBJMerged.Text = "Wavefront .OBJ - Merged";
            // 
            // miOBJVCMerged
            // 
            this.miOBJVCMerged.Name = "miOBJVCMerged";
            this.miOBJVCMerged.Size = new System.Drawing.Size(354, 22);
            this.miOBJVCMerged.Text = "Wavefront .OBJ - Merged (Experimental Vertex Color)";
            // 
            // exportSelectedToolStripMenuItem
            // 
            this.exportSelectedToolStripMenuItem.DropDown = this.cmsModelExport;
            this.exportSelectedToolStripMenuItem.Name = "exportSelectedToolStripMenuItem";
            this.exportSelectedToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.exportSelectedToolStripMenuItem.Text = "Export Selected";
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
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // modelsToolStripMenuItem
            // 
            this.modelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedToolStripMenuItem,
            this.resetTransformToolStripMenuItem,
            this.toolStripSeparator2,
            this.wireframeToolStripMenuItem,
            this.showGizmosToolStripMenuItem,
            this.showBoundsToolStripMenuItem,
            this.enableLightToolStripMenuItem,
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
            this.resetTransformToolStripMenuItem.DropDown = this.cmsResetTransform;
            this.resetTransformToolStripMenuItem.Name = "resetTransformToolStripMenuItem";
            this.resetTransformToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.resetTransformToolStripMenuItem.Text = "Reset Transform";
            // 
            // cmsResetTransform
            // 
            this.cmsResetTransform.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetWholeModelToolStripMenuItem,
            this.resetSelectedModelToolStripMenuItem});
            this.cmsResetTransform.Name = "cmsResetTransform";
            this.cmsResetTransform.OwnerItem = this.resetTransformToolStripMenuItem;
            this.cmsResetTransform.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.cmsResetTransform.Size = new System.Drawing.Size(187, 48);
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
            this.toolStripSeparator2.Size = new System.Drawing.Size(186, 6);
            // 
            // wireframeToolStripMenuItem
            // 
            this.wireframeToolStripMenuItem.CheckOnClick = true;
            this.wireframeToolStripMenuItem.Name = "wireframeToolStripMenuItem";
            this.wireframeToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.wireframeToolStripMenuItem.Text = "Wireframe";
            this.wireframeToolStripMenuItem.CheckedChanged += new System.EventHandler(this.wireframeToolStripMenuItem_CheckedChanged);
            this.wireframeToolStripMenuItem.Click += new System.EventHandler(this.wireframeToolStripMenuItem_Click);
            // 
            // showGizmosToolStripMenuItem
            // 
            this.showGizmosToolStripMenuItem.Checked = true;
            this.showGizmosToolStripMenuItem.CheckOnClick = true;
            this.showGizmosToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGizmosToolStripMenuItem.Name = "showGizmosToolStripMenuItem";
            this.showGizmosToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.showGizmosToolStripMenuItem.Text = "Show Gizmos";
            this.showGizmosToolStripMenuItem.Click += new System.EventHandler(this.showGizmosToolStripMenuItem_Click);
            // 
            // showBoundsToolStripMenuItem
            // 
            this.showBoundsToolStripMenuItem.Checked = true;
            this.showBoundsToolStripMenuItem.CheckOnClick = true;
            this.showBoundsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBoundsToolStripMenuItem.Name = "showBoundsToolStripMenuItem";
            this.showBoundsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.showBoundsToolStripMenuItem.Text = "Show Bounds";
            this.showBoundsToolStripMenuItem.Click += new System.EventHandler(this.showBoundsToolStripMenuItem_Click);
            // 
            // enableLightToolStripMenuItem
            // 
            this.enableLightToolStripMenuItem.Checked = true;
            this.enableLightToolStripMenuItem.CheckOnClick = true;
            this.enableLightToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableLightToolStripMenuItem.Name = "enableLightToolStripMenuItem";
            this.enableLightToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.enableLightToolStripMenuItem.Text = "Enable Light";
            this.enableLightToolStripMenuItem.Click += new System.EventHandler(this.enableLightToolStripMenuItem_Click);
            // 
            // autoAttachLimbsToolStripMenuItem
            // 
            this.autoAttachLimbsToolStripMenuItem.CheckOnClick = true;
            this.autoAttachLimbsToolStripMenuItem.Name = "autoAttachLimbsToolStripMenuItem";
            this.autoAttachLimbsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.autoAttachLimbsToolStripMenuItem.Text = "Auto Attach Limbs";
            this.autoAttachLimbsToolStripMenuItem.Click += new System.EventHandler(this.autoAttachLimbsToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(186, 6);
            // 
            // setAmbientColorToolStripMenuItem
            // 
            this.setAmbientColorToolStripMenuItem.Name = "setAmbientColorToolStripMenuItem";
            this.setAmbientColorToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.setAmbientColorToolStripMenuItem.Text = "Set Ambient Color";
            this.setAmbientColorToolStripMenuItem.Click += new System.EventHandler(this.setAmbientColorToolStripMenuItem_Click);
            // 
            // setBackgroundColorToolStripMenuItem
            // 
            this.setBackgroundColorToolStripMenuItem.Name = "setBackgroundColorToolStripMenuItem";
            this.setBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.setBackgroundColorToolStripMenuItem.Text = "Set Background Color";
            this.setBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.setBackgroundColorToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(186, 6);
            // 
            // lineRendererToolStripMenuItem
            // 
            this.lineRendererToolStripMenuItem.CheckOnClick = true;
            this.lineRendererToolStripMenuItem.Name = "lineRendererToolStripMenuItem";
            this.lineRendererToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.lineRendererToolStripMenuItem.Text = "Line Renderer";
            this.lineRendererToolStripMenuItem.CheckedChanged += new System.EventHandler(this.lineRendererToolStripMenuItem_CheckedChanged);
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedToolStripMenuItem1,
            this.toolStripSeparator3,
            this.drawToVRAMToolStripMenuItem,
            this.findByPageToolStripMenuItem,
            this.clearSearchToolStripMenuItem,
            this.toolStripSeparator1,
            this.setMaskColorToolStripMenuItem});
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.texturesToolStripMenuItem.Text = "Textures";
            // 
            // exportSelectedToolStripMenuItem1
            // 
            this.exportSelectedToolStripMenuItem1.Name = "exportSelectedToolStripMenuItem1";
            this.exportSelectedToolStripMenuItem1.Size = new System.Drawing.Size(155, 22);
            this.exportSelectedToolStripMenuItem1.Text = "Export Selected";
            this.exportSelectedToolStripMenuItem1.Click += new System.EventHandler(this.exportBitmapButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(152, 6);
            // 
            // drawToVRAMToolStripMenuItem
            // 
            this.drawToVRAMToolStripMenuItem.Name = "drawToVRAMToolStripMenuItem";
            this.drawToVRAMToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.drawToVRAMToolStripMenuItem.Text = "Draw to VRAM";
            this.drawToVRAMToolStripMenuItem.Click += new System.EventHandler(this.drawToVRAMButton_Click);
            // 
            // findByPageToolStripMenuItem
            // 
            this.findByPageToolStripMenuItem.Name = "findByPageToolStripMenuItem";
            this.findByPageToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.findByPageToolStripMenuItem.Text = "Find by Page";
            this.findByPageToolStripMenuItem.Click += new System.EventHandler(this.findByPageToolStripMenuItem_Click);
            // 
            // clearSearchToolStripMenuItem
            // 
            this.clearSearchToolStripMenuItem.Name = "clearSearchToolStripMenuItem";
            this.clearSearchToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.clearSearchToolStripMenuItem.Text = "Clear Results";
            this.clearSearchToolStripMenuItem.Click += new System.EventHandler(this.clearSearchToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // setMaskColorToolStripMenuItem
            // 
            this.setMaskColorToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.setMaskColorToolStripMenuItem.Name = "setMaskColorToolStripMenuItem";
            this.setMaskColorToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.setMaskColorToolStripMenuItem.Text = "Set Mask Color";
            this.setMaskColorToolStripMenuItem.Click += new System.EventHandler(this.setMaskColorToolStripMenuItem_Click);
            // 
            // vRAMToolStripMenuItem
            // 
            this.vRAMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearPageToolStripMenuItem,
            this.clearAllPagesToolStripMenuItem,
            this.toolStripSeparator4,
            this.showUVToolStripMenuItem});
            this.vRAMToolStripMenuItem.Name = "vRAMToolStripMenuItem";
            this.vRAMToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.vRAMToolStripMenuItem.Text = "VRAM";
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
            this.showUVToolStripMenuItem.Checked = true;
            this.showUVToolStripMenuItem.CheckOnClick = true;
            this.showUVToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showUVToolStripMenuItem.Name = "showUVToolStripMenuItem";
            this.showUVToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.showUVToolStripMenuItem.Text = "Show UV";
            this.showUVToolStripMenuItem.Click += new System.EventHandler(this.showUVToolStripMenuItem_Click);
            // 
            // animationsToolStripMenuItem
            // 
            this.animationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showSkeletonToolStripMenuItem});
            this.animationsToolStripMenuItem.Name = "animationsToolStripMenuItem";
            this.animationsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.animationsToolStripMenuItem.Text = "Animations";
            // 
            // showSkeletonToolStripMenuItem
            // 
            this.showSkeletonToolStripMenuItem.CheckOnClick = true;
            this.showSkeletonToolStripMenuItem.Name = "showSkeletonToolStripMenuItem";
            this.showSkeletonToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.showSkeletonToolStripMenuItem.Text = "Show Skeleton";
            this.showSkeletonToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showSkeletonToolStripMenuItem_CheckedChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.videoTutorialToolStripMenuItem,
            this.compatibilityListToolStripMenuItem,
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
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 649);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.Resize += new System.EventHandler(this.statusStrip1_Resize);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(200, 17);
            this.toolStripStatusLabel1.Text = "Waiting";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.AutoSize = false;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(200, 16);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 26);
            this.label1.TabIndex = 14;
            this.label1.Text = "Light Rotation:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gridSizeNumericUpDown
            // 
            this.gridSizeNumericUpDown.Location = new System.Drawing.Point(86, 3);
            this.gridSizeNumericUpDown.Name = "gridSizeNumericUpDown";
            this.gridSizeNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.gridSizeNumericUpDown.TabIndex = 15;
            this.gridSizeNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 26);
            this.label2.TabIndex = 16;
            this.label2.Text = "Grid Size:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lightPitchNumericUpDown
            // 
            this.lightPitchNumericUpDown.Location = new System.Drawing.Point(3, 3);
            this.lightPitchNumericUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.lightPitchNumericUpDown.Name = "lightPitchNumericUpDown";
            this.lightPitchNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.lightPitchNumericUpDown.TabIndex = 17;
            this.lightPitchNumericUpDown.Value = new decimal(new int[] {
            135,
            0,
            0,
            0});
            this.lightPitchNumericUpDown.ValueChanged += new System.EventHandler(this.lightPitchNumericUpDown_ValueChanged);
            // 
            // lightYawNumericUpDown
            // 
            this.lightYawNumericUpDown.Location = new System.Drawing.Point(52, 3);
            this.lightYawNumericUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.lightYawNumericUpDown.Name = "lightYawNumericUpDown";
            this.lightYawNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.lightYawNumericUpDown.TabIndex = 18;
            this.lightYawNumericUpDown.Value = new decimal(new int[] {
            135,
            0,
            0,
            0});
            this.lightYawNumericUpDown.ValueChanged += new System.EventHandler(this.lightYawNumericUpDown_ValueChanged);
            // 
            // lightRollNumericUpDown
            // 
            this.lightRollNumericUpDown.Location = new System.Drawing.Point(101, 3);
            this.lightRollNumericUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.lightRollNumericUpDown.Name = "lightRollNumericUpDown";
            this.lightRollNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.lightRollNumericUpDown.TabIndex = 19;
            this.lightRollNumericUpDown.ValueChanged += new System.EventHandler(this.lightRollNumericUpDown_ValueChanged);
            // 
            // lightIntensityNumericUpDown
            // 
            this.lightIntensityNumericUpDown.Location = new System.Drawing.Point(86, 55);
            this.lightIntensityNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.lightIntensityNumericUpDown.Name = "lightIntensityNumericUpDown";
            this.lightIntensityNumericUpDown.Size = new System.Drawing.Size(43, 20);
            this.lightIntensityNumericUpDown.TabIndex = 19;
            this.lightIntensityNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.lightIntensityNumericUpDown.ValueChanged += new System.EventHandler(this.lightIntensityNumericUpDown_ValueChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.menusTabControl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1008, 625);
            this.splitContainer1.SplitterDistance = 768;
            this.splitContainer1.TabIndex = 20;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.tableLayoutPanel2);
            this.groupBox2.Controls.Add(this.animationPlayButton);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 97);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(236, 108);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Animation";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.animationFrameLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.animationSpeedTrackbar, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(230, 55);
            this.tableLayoutPanel2.TabIndex = 22;
            // 
            // animationFrameLabel
            // 
            this.animationFrameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationFrameLabel.Location = new System.Drawing.Point(118, 26);
            this.animationFrameLabel.Name = "animationFrameLabel";
            this.animationFrameLabel.Size = new System.Drawing.Size(109, 29);
            this.animationFrameLabel.TabIndex = 22;
            this.animationFrameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(236, 97);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 147F));
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lightIntensityNumericUpDown, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.gridSizeNumericUpDown, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(230, 78);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 26);
            this.label4.TabIndex = 18;
            this.label4.Text = "Light Intensity:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.lightPitchNumericUpDown);
            this.flowLayoutPanel1.Controls.Add(this.lightYawNumericUpDown);
            this.flowLayoutPanel1.Controls.Add(this.lightRollNumericUpDown);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(83, 26);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(147, 26);
            this.flowLayoutPanel1.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 29);
            this.label6.TabIndex = 23;
            this.label6.Text = "Progress:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 671);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "PreviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PSXPrev";
            this.Load += new System.EventHandler(this.previewForm_Load);
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
            this.texturePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texturePreviewPictureBox)).EndInit();
            this.vramTabPage.ResumeLayout(false);
            this.vramTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vramPagePictureBox)).EndInit();
            this.animationsTabPage.ResumeLayout(false);
            this.animationsSplitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.animationsSplitContainer)).EndInit();
            this.animationsSplitContainer.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.animationSpeedTrackbar)).EndInit();
            this.cmsModelExport.ResumeLayout(false);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.cmsResetTransform.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSizeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightPitchNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightYawNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightRollNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lightIntensityNumericUpDown)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl menusTabControl;
        private System.Windows.Forms.TabPage entitiesTabPage;
        private System.Windows.Forms.TabPage bitmapsTabPage;
        private System.Windows.Forms.Button exportEntityButton;
        private System.Windows.Forms.Button exportBitmapButton;
        private System.Windows.Forms.TreeView entitiesTreeView;
        private System.Windows.Forms.TabPage vramTabPage;
        private System.Windows.Forms.PictureBox vramPagePictureBox;
        private System.Windows.Forms.Panel texturePanel;
        private System.Windows.Forms.PictureBox texturePreviewPictureBox;
        private System.Windows.Forms.Button drawToVRAMButton;
        private System.Windows.Forms.Label vramPageLabel;
        private System.Windows.Forms.ComboBox vramComboBox;
        private System.Windows.Forms.PropertyGrid modelPropertyGrid;
        private System.Windows.Forms.PropertyGrid texturePropertyGrid;
        private System.Windows.Forms.Button btnClearPage;
        private System.Windows.Forms.ContextMenuStrip cmsModelExport;
        private System.Windows.Forms.ToolStripMenuItem miOBJ;
        private System.Windows.Forms.ToolStripMenuItem miOBJVC;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem modelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vRAMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem drawToVRAMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findByPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearPageToolStripMenuItem;
        private System.Windows.Forms.ListView texturesListView;
        private System.Windows.Forms.ImageList thumbsImageList;
        private System.Windows.Forms.ToolStripMenuItem clearSearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wireframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllPagesToolStripMenuItem;
        private System.Windows.Forms.TabPage animationsTabPage;
        private System.Windows.Forms.ToolStripMenuItem animationsToolStripMenuItem;
        private System.Windows.Forms.PropertyGrid animationPropertyGrid;
        private System.Windows.Forms.TreeView animationsTreeView;
        private System.Windows.Forms.Button animationPlayButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem miOBJMerged;
        private System.Windows.Forms.ToolStripMenuItem miOBJVCMerged;
        private System.Windows.Forms.ToolStripMenuItem showGizmosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showUVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSkeletonToolStripMenuItem;
        private System.Windows.Forms.TrackBar animationSpeedTrackbar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem videoTutorialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoAttachLimbsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compatibilityListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown gridSizeNumericUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown lightPitchNumericUpDown;
        private System.Windows.Forms.NumericUpDown lightYawNumericUpDown;
        private System.Windows.Forms.NumericUpDown lightRollNumericUpDown;
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
        private System.Windows.Forms.NumericUpDown lightIntensityNumericUpDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem lineRendererToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetTransformToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsResetTransform;
        private System.Windows.Forms.ToolStripMenuItem resetWholeModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetSelectedModelToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label animationFrameLabel;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer modelsSplitContainer;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer animationsSplitContainer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label6;
    }
}
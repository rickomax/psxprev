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
            this.modelPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.entitiesTreeView = new System.Windows.Forms.TreeView();
            this.exportEntityButton = new System.Windows.Forms.Button();
            this.menusTabControl = new System.Windows.Forms.TabControl();
            this.bitmapsTabPage = new System.Windows.Forms.TabPage();
            this.texturesListView = new System.Windows.Forms.ListView();
            this.thumbsImageList = new System.Windows.Forms.ImageList(this.components);
            this.texturePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.drawToVRAMButton = new System.Windows.Forms.Button();
            this.texturePanel = new System.Windows.Forms.Panel();
            this.texturePreviewPictureBox = new System.Windows.Forms.PictureBox();
            this.exportBitmapButton = new System.Windows.Forms.Button();
            this.vramTabPage = new System.Windows.Forms.TabPage();
            this.btnClearPage = new System.Windows.Forms.Button();
            this.vramPageLabel = new System.Windows.Forms.Label();
            this.vramComboBox = new System.Windows.Forms.ComboBox();
            this.vramPagePictureBox = new System.Windows.Forms.PictureBox();
            this.cmsModelExport = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miOBJ = new System.Windows.Forms.ToolStripMenuItem();
            this.miOBJVC = new System.Windows.Forms.ToolStripMenuItem();
            this.miPLY = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.viewTooStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.drawToVRAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findByPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vRAMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllPagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redrawTimer = new System.Windows.Forms.Timer(this.components);
            this.entitiesTabPage.SuspendLayout();
            this.menusTabControl.SuspendLayout();
            this.bitmapsTabPage.SuspendLayout();
            this.texturePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texturePreviewPictureBox)).BeginInit();
            this.vramTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vramPagePictureBox)).BeginInit();
            this.cmsModelExport.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // entitiesTabPage
            // 
            this.entitiesTabPage.Controls.Add(this.modelPropertyGrid);
            this.entitiesTabPage.Controls.Add(this.entitiesTreeView);
            this.entitiesTabPage.Controls.Add(this.exportEntityButton);
            this.entitiesTabPage.Location = new System.Drawing.Point(4, 22);
            this.entitiesTabPage.Name = "entitiesTabPage";
            this.entitiesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.entitiesTabPage.Size = new System.Drawing.Size(983, 605);
            this.entitiesTabPage.TabIndex = 0;
            this.entitiesTabPage.Text = "Models";
            this.entitiesTabPage.UseVisualStyleBackColor = true;
            // 
            // modelPropertyGrid
            // 
            this.modelPropertyGrid.HelpVisible = false;
            this.modelPropertyGrid.Location = new System.Drawing.Point(1, 406);
            this.modelPropertyGrid.Name = "modelPropertyGrid";
            this.modelPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.modelPropertyGrid.Size = new System.Drawing.Size(175, 161);
            this.modelPropertyGrid.TabIndex = 14;
            this.modelPropertyGrid.ToolbarVisible = false;
            this.modelPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.modelPropertyGrid_PropertyValueChanged);
            // 
            // entitiesTreeView
            // 
            this.entitiesTreeView.CheckBoxes = true;
            this.entitiesTreeView.HideSelection = false;
            this.entitiesTreeView.Location = new System.Drawing.Point(1, 3);
            this.entitiesTreeView.Name = "entitiesTreeView";
            this.entitiesTreeView.Size = new System.Drawing.Size(175, 399);
            this.entitiesTreeView.TabIndex = 9;
            this.entitiesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.entitiesTreeView_AfterSelect);
            // 
            // exportEntityButton
            // 
            this.exportEntityButton.Location = new System.Drawing.Point(0, 570);
            this.exportEntityButton.Name = "exportEntityButton";
            this.exportEntityButton.Size = new System.Drawing.Size(177, 34);
            this.exportEntityButton.TabIndex = 8;
            this.exportEntityButton.Text = "Export Selected";
            this.exportEntityButton.UseVisualStyleBackColor = true;
            this.exportEntityButton.Click += new System.EventHandler(this.exportEntityButton_Click);
            // 
            // menusTabControl
            // 
            this.menusTabControl.Controls.Add(this.entitiesTabPage);
            this.menusTabControl.Controls.Add(this.bitmapsTabPage);
            this.menusTabControl.Controls.Add(this.vramTabPage);
            this.menusTabControl.Location = new System.Drawing.Point(0, 24);
            this.menusTabControl.Name = "menusTabControl";
            this.menusTabControl.SelectedIndex = 0;
            this.menusTabControl.Size = new System.Drawing.Size(991, 631);
            this.menusTabControl.TabIndex = 3;
            this.menusTabControl.SelectedIndexChanged += new System.EventHandler(this.menusTabControl_SelectedIndexChanged);
            // 
            // bitmapsTabPage
            // 
            this.bitmapsTabPage.Controls.Add(this.texturesListView);
            this.bitmapsTabPage.Controls.Add(this.texturePropertyGrid);
            this.bitmapsTabPage.Controls.Add(this.drawToVRAMButton);
            this.bitmapsTabPage.Controls.Add(this.texturePanel);
            this.bitmapsTabPage.Controls.Add(this.exportBitmapButton);
            this.bitmapsTabPage.Location = new System.Drawing.Point(4, 22);
            this.bitmapsTabPage.Name = "bitmapsTabPage";
            this.bitmapsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.bitmapsTabPage.Size = new System.Drawing.Size(983, 605);
            this.bitmapsTabPage.TabIndex = 1;
            this.bitmapsTabPage.Text = "Textures";
            this.bitmapsTabPage.UseVisualStyleBackColor = true;
            // 
            // texturesListView
            // 
            listViewGroup1.Header = "Found";
            listViewGroup1.Name = "foundListViewGroup";
            this.texturesListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.texturesListView.HideSelection = false;
            this.texturesListView.LargeImageList = this.thumbsImageList;
            this.texturesListView.Location = new System.Drawing.Point(1, 3);
            this.texturesListView.Name = "texturesListView";
            this.texturesListView.Size = new System.Drawing.Size(175, 355);
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
            // texturePropertyGrid
            // 
            this.texturePropertyGrid.HelpVisible = false;
            this.texturePropertyGrid.Location = new System.Drawing.Point(1, 362);
            this.texturePropertyGrid.Name = "texturePropertyGrid";
            this.texturePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.texturePropertyGrid.Size = new System.Drawing.Size(175, 169);
            this.texturePropertyGrid.TabIndex = 15;
            this.texturePropertyGrid.ToolbarVisible = false;
            this.texturePropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.texturePropertyGrid_PropertyValueChanged);
            // 
            // drawToVRAMButton
            // 
            this.drawToVRAMButton.Location = new System.Drawing.Point(0, 570);
            this.drawToVRAMButton.Name = "drawToVRAMButton";
            this.drawToVRAMButton.Size = new System.Drawing.Size(177, 34);
            this.drawToVRAMButton.TabIndex = 13;
            this.drawToVRAMButton.Text = "Draw to VRAM";
            this.drawToVRAMButton.UseVisualStyleBackColor = true;
            this.drawToVRAMButton.Click += new System.EventHandler(this.drawToVRAMButton_Click);
            // 
            // texturePanel
            // 
            this.texturePanel.AutoScroll = true;
            this.texturePanel.Controls.Add(this.texturePreviewPictureBox);
            this.texturePanel.Location = new System.Drawing.Point(180, 3);
            this.texturePanel.Name = "texturePanel";
            this.texturePanel.Size = new System.Drawing.Size(758, 600);
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
            // exportBitmapButton
            // 
            this.exportBitmapButton.Location = new System.Drawing.Point(0, 534);
            this.exportBitmapButton.Name = "exportBitmapButton";
            this.exportBitmapButton.Size = new System.Drawing.Size(177, 34);
            this.exportBitmapButton.TabIndex = 9;
            this.exportBitmapButton.Text = "Export Selected";
            this.exportBitmapButton.UseVisualStyleBackColor = true;
            this.exportBitmapButton.Click += new System.EventHandler(this.exportBitmapButton_Click);
            // 
            // vramTabPage
            // 
            this.vramTabPage.Controls.Add(this.btnClearPage);
            this.vramTabPage.Controls.Add(this.vramPageLabel);
            this.vramTabPage.Controls.Add(this.vramComboBox);
            this.vramTabPage.Controls.Add(this.vramPagePictureBox);
            this.vramTabPage.Location = new System.Drawing.Point(4, 22);
            this.vramTabPage.Name = "vramTabPage";
            this.vramTabPage.Size = new System.Drawing.Size(983, 605);
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
            // 
            // cmsModelExport
            // 
            this.cmsModelExport.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miOBJ,
            this.miOBJVC,
            this.miPLY});
            this.cmsModelExport.Name = "cmsModelExport";
            this.cmsModelExport.OwnerItem = this.exportSelectedToolStripMenuItem;
            this.cmsModelExport.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.cmsModelExport.Size = new System.Drawing.Size(302, 70);
            this.cmsModelExport.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cmsModelExport_ItemClicked);
            // 
            // miOBJ
            // 
            this.miOBJ.Name = "miOBJ";
            this.miOBJ.Size = new System.Drawing.Size(301, 22);
            this.miOBJ.Text = "Wavefront .OBJ (Accepts Groups)";
            // 
            // miOBJVC
            // 
            this.miOBJVC.Name = "miOBJVC";
            this.miOBJVC.Size = new System.Drawing.Size(301, 22);
            this.miOBJVC.Text = "Wavefront .OBJ (Experimental Vertex Color)";
            // 
            // miPLY
            // 
            this.miPLY.Name = "miPLY";
            this.miPLY.Size = new System.Drawing.Size(301, 22);
            this.miPLY.Text = "Stanford .PLY (Accepts Vertex Color)";
            // 
            // exportSelectedToolStripMenuItem
            // 
            this.exportSelectedToolStripMenuItem.DropDown = this.cmsModelExport;
            this.exportSelectedToolStripMenuItem.Name = "exportSelectedToolStripMenuItem";
            this.exportSelectedToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.exportSelectedToolStripMenuItem.Text = "Export Selected";
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewTooStripMenuItem,
            this.modelsToolStripMenuItem,
            this.texturesToolStripMenuItem,
            this.vRAMToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(2);
            this.mainMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.mainMenuStrip.Size = new System.Drawing.Size(993, 24);
            this.mainMenuStrip.TabIndex = 5;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // viewTooStripMenuItem
            // 
            this.viewTooStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wireframeToolStripMenuItem});
            this.viewTooStripMenuItem.Name = "viewTooStripMenuItem";
            this.viewTooStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewTooStripMenuItem.Text = "View";
            // 
            // wireframeToolStripMenuItem
            // 
            this.wireframeToolStripMenuItem.CheckOnClick = true;
            this.wireframeToolStripMenuItem.Name = "wireframeToolStripMenuItem";
            this.wireframeToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.wireframeToolStripMenuItem.Text = "Wireframe";
            this.wireframeToolStripMenuItem.Click += new System.EventHandler(this.wireframeToolStripMenuItem_Click);
            // 
            // modelsToolStripMenuItem
            // 
            this.modelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedToolStripMenuItem});
            this.modelsToolStripMenuItem.Name = "modelsToolStripMenuItem";
            this.modelsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.modelsToolStripMenuItem.Text = "Models";
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSelectedToolStripMenuItem1,
            this.drawToVRAMToolStripMenuItem,
            this.findByPageToolStripMenuItem,
            this.clearSearchToolStripMenuItem});
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.texturesToolStripMenuItem.Text = "Textures";
            // 
            // exportSelectedToolStripMenuItem1
            // 
            this.exportSelectedToolStripMenuItem1.Name = "exportSelectedToolStripMenuItem1";
            this.exportSelectedToolStripMenuItem1.Size = new System.Drawing.Size(154, 22);
            this.exportSelectedToolStripMenuItem1.Text = "Export Selected";
            this.exportSelectedToolStripMenuItem1.Click += new System.EventHandler(this.exportBitmapButton_Click);
            // 
            // drawToVRAMToolStripMenuItem
            // 
            this.drawToVRAMToolStripMenuItem.Name = "drawToVRAMToolStripMenuItem";
            this.drawToVRAMToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.drawToVRAMToolStripMenuItem.Text = "Draw to VRAM";
            this.drawToVRAMToolStripMenuItem.Click += new System.EventHandler(this.drawToVRAMButton_Click);
            // 
            // findByPageToolStripMenuItem
            // 
            this.findByPageToolStripMenuItem.Name = "findByPageToolStripMenuItem";
            this.findByPageToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.findByPageToolStripMenuItem.Text = "Find by Page";
            this.findByPageToolStripMenuItem.Click += new System.EventHandler(this.findByPageToolStripMenuItem_Click);
            // 
            // clearSearchToolStripMenuItem
            // 
            this.clearSearchToolStripMenuItem.Name = "clearSearchToolStripMenuItem";
            this.clearSearchToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.clearSearchToolStripMenuItem.Text = "Clear Results";
            this.clearSearchToolStripMenuItem.Click += new System.EventHandler(this.clearSearchToolStripMenuItem_Click);
            // 
            // vRAMToolStripMenuItem
            // 
            this.vRAMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearPageToolStripMenuItem,
            this.clearAllPagesToolStripMenuItem});
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
            // redrawTimer
            // 
            this.redrawTimer.Interval = 16;
            this.redrawTimer.Tick += new System.EventHandler(this.redrawTimer_Tick);
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 654);
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.menusTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.MaximizeBox = false;
            this.Name = "PreviewForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PSXPrev Alpha 0.7";
            this.Load += new System.EventHandler(this.previewForm_Load);
            this.entitiesTabPage.ResumeLayout(false);
            this.menusTabControl.ResumeLayout(false);
            this.bitmapsTabPage.ResumeLayout(false);
            this.texturePanel.ResumeLayout(false);
            this.texturePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texturePreviewPictureBox)).EndInit();
            this.vramTabPage.ResumeLayout(false);
            this.vramTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vramPagePictureBox)).EndInit();
            this.cmsModelExport.ResumeLayout(false);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem miPLY;
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
        private System.Windows.Forms.ToolStripMenuItem clearAllPagesToolStripMenuItem;
        private System.Windows.Forms.Timer redrawTimer;
        private System.Windows.Forms.ToolStripMenuItem viewTooStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wireframeToolStripMenuItem;

    }
}
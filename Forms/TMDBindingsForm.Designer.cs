namespace PSXPrev.Forms
{
    partial class TMDBindingsForm
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
            this.bindingPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bindingPropertyGrid
            // 
            this.bindingPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bindingPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.bindingPropertyGrid.Name = "bindingPropertyGrid";
            this.bindingPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.bindingPropertyGrid.Size = new System.Drawing.Size(274, 351);
            this.bindingPropertyGrid.TabIndex = 0;
            this.bindingPropertyGrid.ToolbarVisible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(274, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadBindingsToolStripMenuItem,
            this.saveBindingsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadBindingsToolStripMenuItem
            // 
            this.loadBindingsToolStripMenuItem.Enabled = false;
            this.loadBindingsToolStripMenuItem.Name = "loadBindingsToolStripMenuItem";
            this.loadBindingsToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.loadBindingsToolStripMenuItem.Text = "Load Bindings";
            // 
            // saveBindingsToolStripMenuItem
            // 
            this.saveBindingsToolStripMenuItem.Enabled = false;
            this.saveBindingsToolStripMenuItem.Name = "saveBindingsToolStripMenuItem";
            this.saveBindingsToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.saveBindingsToolStripMenuItem.Text = "Save Bindings";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyBindingsToolStripMenuItem,
            this.pasteBindingsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // copyBindingsToolStripMenuItem
            // 
            this.copyBindingsToolStripMenuItem.Name = "copyBindingsToolStripMenuItem";
            this.copyBindingsToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.copyBindingsToolStripMenuItem.Text = "Copy Bindings";
            this.copyBindingsToolStripMenuItem.Click += new System.EventHandler(this.copyBindingsToolStripMenuItem_Click);
            // 
            // pasteBindingsToolStripMenuItem
            // 
            this.pasteBindingsToolStripMenuItem.Name = "pasteBindingsToolStripMenuItem";
            this.pasteBindingsToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.pasteBindingsToolStripMenuItem.Text = "Paste Bindings";
            this.pasteBindingsToolStripMenuItem.Click += new System.EventHandler(this.pasteBindingsToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(274, 71);
            this.label1.TabIndex = 2;
            this.label1.Text = "At the left column, you have the original TMD ID assigned to the Animation nodes." +
    "\r\nAt the right column, you can assign a new TMD ID value to the current item.";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.bindingPropertyGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(274, 426);
            this.splitContainer1.SplitterDistance = 351;
            this.splitContainer1.TabIndex = 3;
            // 
            // TMDBindingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TMDBindingsForm";
            this.Text = "TMD Bindings";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TMDBindingForm_FormClosed);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid bindingPropertyGrid;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyBindingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteBindingsToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadBindingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBindingsToolStripMenuItem;
    }
}
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
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteBindingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bindingPropertyGrid
            // 
            this.bindingPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bindingPropertyGrid.Location = new System.Drawing.Point(0, 24);
            this.bindingPropertyGrid.Name = "bindingPropertyGrid";
            this.bindingPropertyGrid.Size = new System.Drawing.Size(274, 426);
            this.bindingPropertyGrid.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(274, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
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
            this.copyBindingsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyBindingsToolStripMenuItem.Text = "Copy Bindings";
            this.copyBindingsToolStripMenuItem.Click += new System.EventHandler(this.copyBindingsToolStripMenuItem_Click);
            // 
            // pasteBindingsToolStripMenuItem
            // 
            this.pasteBindingsToolStripMenuItem.Name = "pasteBindingsToolStripMenuItem";
            this.pasteBindingsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pasteBindingsToolStripMenuItem.Text = "Paste Bindings";
            this.pasteBindingsToolStripMenuItem.Click += new System.EventHandler(this.pasteBindingsToolStripMenuItem_Click);
            // 
            // TMDBindingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 450);
            this.Controls.Add(this.bindingPropertyGrid);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TMDBindingsForm";
            this.Text = "TMD Bindings";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TMDBindingForm_FormClosed);
            this.Load += new System.EventHandler(this.TMDBindingForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid bindingPropertyGrid;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyBindingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteBindingsToolStripMenuItem;
    }
}
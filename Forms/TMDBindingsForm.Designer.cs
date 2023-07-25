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
            this.SuspendLayout();
            // 
            // bindingPropertyGrid
            // 
            this.bindingPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bindingPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.bindingPropertyGrid.Name = "bindingPropertyGrid";
            this.bindingPropertyGrid.Size = new System.Drawing.Size(274, 450);
            this.bindingPropertyGrid.TabIndex = 0;
            // 
            // TMDBindingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 450);
            this.Controls.Add(this.bindingPropertyGrid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TMDBindingsForm";
            this.Text = "TMD Bindings";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TMDBindingForm_FormClosed);
            this.Load += new System.EventHandler(this.TMDBindingForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid bindingPropertyGrid;
    }
}
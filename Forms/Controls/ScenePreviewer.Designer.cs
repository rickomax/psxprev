namespace PSXPrev.Forms.Controls
{
    partial class ScenePreviewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.camPositionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.camRotationStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.camDistanceStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.antiFlicker = new System.Windows.Forms.PictureBox();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.antiFlicker)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.camPositionStatusLabel,
            this.camRotationStatusLabel,
            this.camDistanceStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 360);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new System.Drawing.Size(382, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            this.statusStrip.MouseDown += new System.Windows.Forms.MouseEventHandler(this.statusStrip_MouseDown);
            // 
            // camPositionStatusLabel
            // 
            this.camPositionStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.camPositionStatusLabel.Name = "camPositionStatusLabel";
            this.camPositionStatusLabel.Size = new System.Drawing.Size(69, 17);
            this.camPositionStatusLabel.Text = "Camera:  ,  ,";
            this.camPositionStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // camRotationStatusLabel
            // 
            this.camRotationStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.camRotationStatusLabel.Name = "camRotationStatusLabel";
            this.camRotationStatusLabel.Size = new System.Drawing.Size(64, 17);
            this.camRotationStatusLabel.Text = "Rotation:  ,";
            this.camRotationStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.camRotationStatusLabel.ToolTipText = "Yaw, Pitch";
            // 
            // camDistanceStatusLabel
            // 
            this.camDistanceStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.camDistanceStatusLabel.Name = "camDistanceStatusLabel";
            this.camDistanceStatusLabel.Size = new System.Drawing.Size(61, 17);
            this.camDistanceStatusLabel.Text = "Distance:  ";
            this.camDistanceStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // antiFlicker
            // 
            this.antiFlicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.antiFlicker.Location = new System.Drawing.Point(0, 0);
            this.antiFlicker.Name = "antiFlicker";
            this.antiFlicker.Size = new System.Drawing.Size(382, 360);
            this.antiFlicker.TabIndex = 2;
            this.antiFlicker.TabStop = false;
            // 
            // ScenePreviewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSkyBlue;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.antiFlicker);
            this.Controls.Add(this.statusStrip);
            this.Name = "ScenePreviewer";
            this.Size = new System.Drawing.Size(382, 382);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.antiFlicker)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel camRotationStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel camDistanceStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel camPositionStatusLabel;
        private System.Windows.Forms.PictureBox antiFlicker;
    }
}

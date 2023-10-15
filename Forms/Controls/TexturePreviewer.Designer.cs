namespace PSXPrev.Forms.Controls
{
    partial class TexturePreviewer
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
            this.zoomLabel = new System.Windows.Forms.Label();
            this.anchorPanel = new System.Windows.Forms.Panel();
            this.previewPanel = new PSXPrev.Forms.Controls.ExtendedPanel();
            this.previewPictureBox = new System.Windows.Forms.PictureBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.positionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.colorStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.transparencyStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.paletteIndexStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.zoomStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.anchorPanel.SuspendLayout();
            this.previewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // zoomLabel
            // 
            this.zoomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zoomLabel.BackColor = System.Drawing.SystemColors.Control;
            this.zoomLabel.Location = new System.Drawing.Point(322, 0);
            this.zoomLabel.Margin = new System.Windows.Forms.Padding(0);
            this.zoomLabel.Name = "zoomLabel";
            this.zoomLabel.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.zoomLabel.Size = new System.Drawing.Size(60, 30);
            this.zoomLabel.TabIndex = 15;
            this.zoomLabel.Text = "100%";
            this.zoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.zoomLabel.Visible = false;
            // 
            // anchorPanel
            // 
            this.anchorPanel.Controls.Add(this.zoomLabel);
            this.anchorPanel.Controls.Add(this.previewPanel);
            this.anchorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.anchorPanel.Location = new System.Drawing.Point(0, 0);
            this.anchorPanel.Name = "anchorPanel";
            this.anchorPanel.Size = new System.Drawing.Size(382, 360);
            this.anchorPanel.TabIndex = 16;
            // 
            // previewPanel
            // 
            this.previewPanel.AllowFocus = true;
            this.previewPanel.AllowMouseDragging = true;
            this.previewPanel.AllowMouseWheelScrolling = false;
            this.previewPanel.AutoScroll = true;
            this.previewPanel.Controls.Add(this.previewPictureBox);
            this.previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewPanel.Location = new System.Drawing.Point(0, 0);
            this.previewPanel.Name = "previewPanel";
            this.previewPanel.Size = new System.Drawing.Size(382, 360);
            this.previewPanel.TabIndex = 14;
            this.previewPanel.TabStop = true;
            this.previewPanel.MouseWheelEx += new System.Windows.Forms.MouseEventHandler(this.previewPanel_MouseWheelEx);
            // 
            // previewPictureBox
            // 
            this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
            this.previewPictureBox.Name = "previewPictureBox";
            this.previewPictureBox.Size = new System.Drawing.Size(256, 256);
            this.previewPictureBox.TabIndex = 10;
            this.previewPictureBox.TabStop = false;
            this.previewPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.previewPictureBox_Paint);
            this.previewPictureBox.MouseLeave += new System.EventHandler(this.previewPictureBox_MouseLeave);
            this.previewPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.previewPictureBox_MouseMove);
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.positionStatusLabel,
            this.colorStatusLabel,
            this.transparencyStatusLabel,
            this.paletteIndexStatusLabel,
            this.zoomStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 360);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(382, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 17;
            this.statusStrip.Text = "statusStrip1";
            this.statusStrip.MouseDown += new System.Windows.Forms.MouseEventHandler(this.statusStrip_MouseDown);
            // 
            // positionStatusLabel
            // 
            this.positionStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.positionStatusLabel.Name = "positionStatusLabel";
            this.positionStatusLabel.Size = new System.Drawing.Size(62, 17);
            this.positionStatusLabel.Text = "Position:  ,";
            this.positionStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // colorStatusLabel
            // 
            this.colorStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 4, 2);
            this.colorStatusLabel.Name = "colorStatusLabel";
            this.colorStatusLabel.Size = new System.Drawing.Size(54, 17);
            this.colorStatusLabel.Text = "Color:  , ,";
            this.colorStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // transparencyStatusLabel
            // 
            this.transparencyStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 8, 2);
            this.transparencyStatusLabel.Name = "transparencyStatusLabel";
            this.transparencyStatusLabel.Size = new System.Drawing.Size(13, 17);
            this.transparencyStatusLabel.Text = "S";
            // 
            // paletteIndexStatusLabel
            // 
            this.paletteIndexStatusLabel.Name = "paletteIndexStatusLabel";
            this.paletteIndexStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.paletteIndexStatusLabel.Text = "Index:";
            this.paletteIndexStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // zoomStatusLabel
            // 
            this.zoomStatusLabel.Name = "zoomStatusLabel";
            this.zoomStatusLabel.Size = new System.Drawing.Size(148, 17);
            this.zoomStatusLabel.Spring = true;
            this.zoomStatusLabel.Text = "Zoom:  100%";
            this.zoomStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TexturePreviewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.anchorPanel);
            this.Controls.Add(this.statusStrip);
            this.Name = "TexturePreviewer";
            this.Size = new System.Drawing.Size(382, 382);
            this.anchorPanel.ResumeLayout(false);
            this.previewPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ExtendedPanel previewPanel;
        private System.Windows.Forms.PictureBox previewPictureBox;
        private System.Windows.Forms.Label zoomLabel;
        private System.Windows.Forms.Panel anchorPanel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel positionStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel colorStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel paletteIndexStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel transparencyStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel zoomStatusLabel;
    }
}

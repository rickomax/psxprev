namespace PSXPrev.Forms
{
    partial class ExportModelsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportModelsForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.exportingModelsLabel = new System.Windows.Forms.Label();
            this.selectFolderButton = new System.Windows.Forms.Button();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.formatGLTF2RadioButton = new System.Windows.Forms.RadioButton();
            this.formatPLYRadioButton = new System.Windows.Forms.RadioButton();
            this.formatOBJRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.optionTiledTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.optionShareTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.optionRedrawTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.texturesSingleRadioButton = new System.Windows.Forms.RadioButton();
            this.texturesIndividualRadioButton = new System.Windows.Forms.RadioButton();
            this.texturesOffRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.optionAttachLimbsCheckBox = new System.Windows.Forms.CheckBox();
            this.optionExperimentalVertexColorCheckBox = new System.Windows.Forms.CheckBox();
            this.optionMergeModelsCheckBox = new System.Windows.Forms.CheckBox();
            this.exportButton = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkedAnimationsListBox = new System.Windows.Forms.ListBox();
            this.animationsOnRadioButton = new System.Windows.Forms.RadioButton();
            this.animationsOffRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.exportingModelsLabel);
            this.groupBox1.Controls.Add(this.selectFolderButton);
            this.groupBox1.Controls.Add(this.fileNameTextBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(394, 77);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Folder";
            // 
            // exportingModelsLabel
            // 
            this.exportingModelsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exportingModelsLabel.Location = new System.Drawing.Point(182, 50);
            this.exportingModelsLabel.Name = "exportingModelsLabel";
            this.exportingModelsLabel.Size = new System.Drawing.Size(200, 13);
            this.exportingModelsLabel.TabIndex = 4;
            this.exportingModelsLabel.Text = "Exporting 0 Models";
            this.exportingModelsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // selectFolderButton
            // 
            this.selectFolderButton.Location = new System.Drawing.Point(12, 45);
            this.selectFolderButton.Name = "selectFolderButton";
            this.selectFolderButton.Size = new System.Drawing.Size(79, 23);
            this.selectFolderButton.TabIndex = 1;
            this.selectFolderButton.Text = "Select Folder";
            this.selectFolderButton.Click += new System.EventHandler(this.selectFolderButton_Click);
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNameTextBox.Location = new System.Drawing.Point(12, 19);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.Size = new System.Drawing.Size(370, 20);
            this.fileNameTextBox.TabIndex = 0;
            this.fileNameTextBox.TextChanged += new System.EventHandler(this.fileNameTextBox_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.formatGLTF2RadioButton);
            this.groupBox2.Controls.Add(this.formatPLYRadioButton);
            this.groupBox2.Controls.Add(this.formatOBJRadioButton);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 77);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(394, 47);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Format";
            // 
            // formatGLTF2RadioButton
            // 
            this.formatGLTF2RadioButton.AutoSize = true;
            this.formatGLTF2RadioButton.Location = new System.Drawing.Point(216, 19);
            this.formatGLTF2RadioButton.Name = "formatGLTF2RadioButton";
            this.formatGLTF2RadioButton.Size = new System.Drawing.Size(97, 17);
            this.formatGLTF2RadioButton.TabIndex = 4;
            this.formatGLTF2RadioButton.Tag = "glTF2";
            this.formatGLTF2RadioButton.Text = "Khronos .glTF2";
            this.formatGLTF2RadioButton.UseVisualStyleBackColor = true;
            this.formatGLTF2RadioButton.CheckedChanged += new System.EventHandler(this.formatRadioButtons_CheckedChanged);
            // 
            // formatPLYRadioButton
            // 
            this.formatPLYRadioButton.AutoSize = true;
            this.formatPLYRadioButton.Location = new System.Drawing.Point(119, 19);
            this.formatPLYRadioButton.Name = "formatPLYRadioButton";
            this.formatPLYRadioButton.Size = new System.Drawing.Size(91, 17);
            this.formatPLYRadioButton.TabIndex = 3;
            this.formatPLYRadioButton.Tag = "PLY";
            this.formatPLYRadioButton.Text = "Stanford .PLY";
            this.formatPLYRadioButton.UseVisualStyleBackColor = true;
            this.formatPLYRadioButton.CheckedChanged += new System.EventHandler(this.formatRadioButtons_CheckedChanged);
            // 
            // formatOBJRadioButton
            // 
            this.formatOBJRadioButton.AutoSize = true;
            this.formatOBJRadioButton.Checked = true;
            this.formatOBJRadioButton.Location = new System.Drawing.Point(12, 19);
            this.formatOBJRadioButton.Name = "formatOBJRadioButton";
            this.formatOBJRadioButton.Size = new System.Drawing.Size(101, 17);
            this.formatOBJRadioButton.TabIndex = 2;
            this.formatOBJRadioButton.TabStop = true;
            this.formatOBJRadioButton.Tag = "OBJ";
            this.formatOBJRadioButton.Text = "Wavefront .OBJ";
            this.formatOBJRadioButton.UseVisualStyleBackColor = true;
            this.formatOBJRadioButton.CheckedChanged += new System.EventHandler(this.formatRadioButtons_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.optionTiledTexturesCheckBox);
            this.groupBox3.Controls.Add(this.optionShareTexturesCheckBox);
            this.groupBox3.Controls.Add(this.optionRedrawTexturesCheckBox);
            this.groupBox3.Controls.Add(this.texturesSingleRadioButton);
            this.groupBox3.Controls.Add(this.texturesIndividualRadioButton);
            this.groupBox3.Controls.Add(this.texturesOffRadioButton);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(0, 124);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(394, 80);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Textures";
            // 
            // optionTiledTexturesCheckBox
            // 
            this.optionTiledTexturesCheckBox.AutoSize = true;
            this.optionTiledTexturesCheckBox.Checked = true;
            this.optionTiledTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionTiledTexturesCheckBox.Location = new System.Drawing.Point(269, 53);
            this.optionTiledTexturesCheckBox.Name = "optionTiledTexturesCheckBox";
            this.optionTiledTexturesCheckBox.Size = new System.Drawing.Size(82, 17);
            this.optionTiledTexturesCheckBox.TabIndex = 9;
            this.optionTiledTexturesCheckBox.Text = "Export Tiled";
            this.optionTiledTexturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionShareTexturesCheckBox
            // 
            this.optionShareTexturesCheckBox.AutoSize = true;
            this.optionShareTexturesCheckBox.Checked = true;
            this.optionShareTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionShareTexturesCheckBox.Location = new System.Drawing.Point(127, 53);
            this.optionShareTexturesCheckBox.Name = "optionShareTexturesCheckBox";
            this.optionShareTexturesCheckBox.Size = new System.Drawing.Size(136, 17);
            this.optionShareTexturesCheckBox.TabIndex = 8;
            this.optionShareTexturesCheckBox.Text = "Share Between Models";
            this.optionShareTexturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionRedrawTexturesCheckBox
            // 
            this.optionRedrawTexturesCheckBox.AutoSize = true;
            this.optionRedrawTexturesCheckBox.Location = new System.Drawing.Point(12, 53);
            this.optionRedrawTexturesCheckBox.Name = "optionRedrawTexturesCheckBox";
            this.optionRedrawTexturesCheckBox.Size = new System.Drawing.Size(109, 17);
            this.optionRedrawTexturesCheckBox.TabIndex = 7;
            this.optionRedrawTexturesCheckBox.Text = "Redraw to VRAM";
            this.optionRedrawTexturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // texturesSingleRadioButton
            // 
            this.texturesSingleRadioButton.AutoSize = true;
            this.texturesSingleRadioButton.Location = new System.Drawing.Point(246, 19);
            this.texturesSingleRadioButton.Name = "texturesSingleRadioButton";
            this.texturesSingleRadioButton.Size = new System.Drawing.Size(93, 17);
            this.texturesSingleRadioButton.TabIndex = 6;
            this.texturesSingleRadioButton.Text = "Single Texture";
            this.texturesSingleRadioButton.UseVisualStyleBackColor = true;
            this.texturesSingleRadioButton.CheckedChanged += new System.EventHandler(this.texturesRadioButtons_CheckedChanged);
            // 
            // texturesIndividualRadioButton
            // 
            this.texturesIndividualRadioButton.AutoSize = true;
            this.texturesIndividualRadioButton.Checked = true;
            this.texturesIndividualRadioButton.Location = new System.Drawing.Point(119, 19);
            this.texturesIndividualRadioButton.Name = "texturesIndividualRadioButton";
            this.texturesIndividualRadioButton.Size = new System.Drawing.Size(114, 17);
            this.texturesIndividualRadioButton.TabIndex = 5;
            this.texturesIndividualRadioButton.TabStop = true;
            this.texturesIndividualRadioButton.Text = "Individual Textures";
            this.texturesIndividualRadioButton.UseVisualStyleBackColor = true;
            this.texturesIndividualRadioButton.CheckedChanged += new System.EventHandler(this.texturesRadioButtons_CheckedChanged);
            // 
            // texturesOffRadioButton
            // 
            this.texturesOffRadioButton.AutoSize = true;
            this.texturesOffRadioButton.Location = new System.Drawing.Point(12, 19);
            this.texturesOffRadioButton.Name = "texturesOffRadioButton";
            this.texturesOffRadioButton.Size = new System.Drawing.Size(83, 17);
            this.texturesOffRadioButton.TabIndex = 4;
            this.texturesOffRadioButton.Text = "Don\'t Export";
            this.texturesOffRadioButton.UseVisualStyleBackColor = true;
            this.texturesOffRadioButton.CheckedChanged += new System.EventHandler(this.texturesRadioButtons_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.optionAttachLimbsCheckBox);
            this.groupBox4.Controls.Add(this.optionExperimentalVertexColorCheckBox);
            this.groupBox4.Controls.Add(this.optionMergeModelsCheckBox);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox4.Location = new System.Drawing.Point(0, 204);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(394, 46);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Options";
            // 
            // optionAttachLimbsCheckBox
            // 
            this.optionAttachLimbsCheckBox.AutoSize = true;
            this.optionAttachLimbsCheckBox.Checked = true;
            this.optionAttachLimbsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionAttachLimbsCheckBox.Location = new System.Drawing.Point(111, 19);
            this.optionAttachLimbsCheckBox.Name = "optionAttachLimbsCheckBox";
            this.optionAttachLimbsCheckBox.Size = new System.Drawing.Size(87, 17);
            this.optionAttachLimbsCheckBox.TabIndex = 11;
            this.optionAttachLimbsCheckBox.Text = "Attach Limbs";
            this.optionAttachLimbsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionExperimentalVertexColorCheckBox
            // 
            this.optionExperimentalVertexColorCheckBox.AutoSize = true;
            this.optionExperimentalVertexColorCheckBox.Checked = true;
            this.optionExperimentalVertexColorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionExperimentalVertexColorCheckBox.Location = new System.Drawing.Point(204, 19);
            this.optionExperimentalVertexColorCheckBox.Name = "optionExperimentalVertexColorCheckBox";
            this.optionExperimentalVertexColorCheckBox.Size = new System.Drawing.Size(172, 17);
            this.optionExperimentalVertexColorCheckBox.TabIndex = 12;
            this.optionExperimentalVertexColorCheckBox.Text = "Experimental .OBJ Vertex Color";
            this.optionExperimentalVertexColorCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionMergeModelsCheckBox
            // 
            this.optionMergeModelsCheckBox.AutoSize = true;
            this.optionMergeModelsCheckBox.Location = new System.Drawing.Point(12, 19);
            this.optionMergeModelsCheckBox.Name = "optionMergeModelsCheckBox";
            this.optionMergeModelsCheckBox.Size = new System.Drawing.Size(93, 17);
            this.optionMergeModelsCheckBox.TabIndex = 10;
            this.optionMergeModelsCheckBox.Text = "Merge Models";
            this.optionMergeModelsCheckBox.UseVisualStyleBackColor = true;
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.exportButton.Enabled = false;
            this.exportButton.Location = new System.Drawing.Point(307, 443);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 6;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 10000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.checkedAnimationsListBox);
            this.groupBox5.Controls.Add(this.animationsOnRadioButton);
            this.groupBox5.Controls.Add(this.animationsOffRadioButton);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox5.Location = new System.Drawing.Point(0, 250);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(394, 186);
            this.groupBox5.TabIndex = 7;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Animations";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Checked Animations (Under Animations tab):";
            // 
            // checkedAnimationsListBox
            // 
            this.checkedAnimationsListBox.FormattingEnabled = true;
            this.checkedAnimationsListBox.Location = new System.Drawing.Point(12, 74);
            this.checkedAnimationsListBox.Name = "checkedAnimationsListBox";
            this.checkedAnimationsListBox.Size = new System.Drawing.Size(370, 69);
            this.checkedAnimationsListBox.TabIndex = 7;
            // 
            // animationsOnRadioButton
            // 
            this.animationsOnRadioButton.AutoSize = true;
            this.animationsOnRadioButton.Enabled = false;
            this.animationsOnRadioButton.Location = new System.Drawing.Point(101, 19);
            this.animationsOnRadioButton.Name = "animationsOnRadioButton";
            this.animationsOnRadioButton.Size = new System.Drawing.Size(101, 17);
            this.animationsOnRadioButton.TabIndex = 6;
            this.animationsOnRadioButton.Text = "Export Checked";
            this.animationsOnRadioButton.UseVisualStyleBackColor = true;
            // 
            // animationsOffRadioButton
            // 
            this.animationsOffRadioButton.AutoSize = true;
            this.animationsOffRadioButton.Checked = true;
            this.animationsOffRadioButton.Enabled = false;
            this.animationsOffRadioButton.Location = new System.Drawing.Point(12, 19);
            this.animationsOffRadioButton.Name = "animationsOffRadioButton";
            this.animationsOffRadioButton.Size = new System.Drawing.Size(83, 17);
            this.animationsOffRadioButton.TabIndex = 5;
            this.animationsOffRadioButton.TabStop = true;
            this.animationsOffRadioButton.Text = "Don\'t Export";
            this.animationsOffRadioButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 149);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(366, 30);
            this.label2.TabIndex = 9;
            this.label2.Text = "Before exporting Animations, please make sure the animations you have checked are" +
    " playable";
            // 
            // ExportModelsForm
            // 
            this.AcceptButton = this.exportButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 473);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportModelsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PSXPrev Export Models";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExportModelsForm_FormClosed);
            this.Load += new System.EventHandler(this.ExportModelsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button selectFolderButton;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton formatPLYRadioButton;
        private System.Windows.Forms.RadioButton formatOBJRadioButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton texturesSingleRadioButton;
        private System.Windows.Forms.RadioButton texturesIndividualRadioButton;
        private System.Windows.Forms.RadioButton texturesOffRadioButton;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox optionRedrawTexturesCheckBox;
        private System.Windows.Forms.CheckBox optionExperimentalVertexColorCheckBox;
        private System.Windows.Forms.CheckBox optionMergeModelsCheckBox;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.CheckBox optionAttachLimbsCheckBox;
        private System.Windows.Forms.Label exportingModelsLabel;
        private System.Windows.Forms.CheckBox optionShareTexturesCheckBox;
        private System.Windows.Forms.CheckBox optionTiledTexturesCheckBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.RadioButton formatGLTF2RadioButton;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox checkedAnimationsListBox;
        private System.Windows.Forms.RadioButton animationsOnRadioButton;
        private System.Windows.Forms.RadioButton animationsOffRadioButton;
        private System.Windows.Forms.Label label2;
    }
}
namespace PSXPrev.Forms
{
    partial class ScannerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScannerForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.selectFolderButton = new System.Windows.Forms.Button();
            this.selectISOButton = new System.Windows.Forms.Button();
            this.selectFileButton = new System.Windows.Forms.Button();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.scanBFFCheckBox = new System.Windows.Forms.CheckBox();
            this.scanANCheckBox = new System.Windows.Forms.CheckBox();
            this.scanPSXCheckBox = new System.Windows.Forms.CheckBox();
            this.scanMODCheckBox = new System.Windows.Forms.CheckBox();
            this.scanPMDCheckBox = new System.Windows.Forms.CheckBox();
            this.scanHMDCheckBox = new System.Windows.Forms.CheckBox();
            this.scanTODCheckBox = new System.Windows.Forms.CheckBox();
            this.scanTIMCheckBox = new System.Windows.Forms.CheckBox();
            this.scanVDFCheckBox = new System.Windows.Forms.CheckBox();
            this.scanTMDCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.optionIgnoreHMDVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.optionNoOffsetCheckBox = new System.Windows.Forms.CheckBox();
            this.optionShowErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.optionAutoAttachLimbsCheckBox = new System.Windows.Forms.CheckBox();
            this.optionDrawAllToVRAMCheckBox = new System.Windows.Forms.CheckBox();
            this.optionIgnoreTMDVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.optionDebugCheckBox = new System.Windows.Forms.CheckBox();
            this.optionNoVerboseCheckBox = new System.Windows.Forms.CheckBox();
            this.optionLogToFileCheckBox = new System.Windows.Forms.CheckBox();
            this.scanButton = new System.Windows.Forms.Button();
            this.optionIgnoreTIMVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.selectFolderButton);
            this.groupBox1.Controls.Add(this.selectISOButton);
            this.groupBox1.Controls.Add(this.selectFileButton);
            this.groupBox1.Controls.Add(this.fileNameTextBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(384, 77);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filename";
            // 
            // selectFolderButton
            // 
            this.selectFolderButton.Location = new System.Drawing.Point(167, 45);
            this.selectFolderButton.Name = "selectFolderButton";
            this.selectFolderButton.Size = new System.Drawing.Size(79, 23);
            this.selectFolderButton.TabIndex = 3;
            this.selectFolderButton.Text = "Select Folder";
            this.selectFolderButton.Click += new System.EventHandler(this.selectFolderButton_Click);
            // 
            // selectISOButton
            // 
            this.selectISOButton.Location = new System.Drawing.Point(86, 45);
            this.selectISOButton.Name = "selectISOButton";
            this.selectISOButton.Size = new System.Drawing.Size(75, 23);
            this.selectISOButton.TabIndex = 2;
            this.selectISOButton.Text = "Select ISO";
            this.selectISOButton.UseVisualStyleBackColor = true;
            this.selectISOButton.Click += new System.EventHandler(this.selectISOButton_Click);
            // 
            // selectFileButton
            // 
            this.selectFileButton.Location = new System.Drawing.Point(12, 45);
            this.selectFileButton.Name = "selectFileButton";
            this.selectFileButton.Size = new System.Drawing.Size(68, 23);
            this.selectFileButton.TabIndex = 1;
            this.selectFileButton.Text = "Select File";
            this.selectFileButton.UseVisualStyleBackColor = true;
            this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.Location = new System.Drawing.Point(12, 19);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.Size = new System.Drawing.Size(360, 20);
            this.fileNameTextBox.TabIndex = 0;
            this.fileNameTextBox.TextChanged += new System.EventHandler(this.fileNameTextBox_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.filterTextBox);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 77);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(384, 58);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filter";
            // 
            // filterTextBox
            // 
            this.filterTextBox.Location = new System.Drawing.Point(12, 21);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(75, 20);
            this.filterTextBox.TabIndex = 1;
            this.filterTextBox.Text = "*.*";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.scanBFFCheckBox);
            this.groupBox3.Controls.Add(this.scanANCheckBox);
            this.groupBox3.Controls.Add(this.scanPSXCheckBox);
            this.groupBox3.Controls.Add(this.scanMODCheckBox);
            this.groupBox3.Controls.Add(this.scanPMDCheckBox);
            this.groupBox3.Controls.Add(this.scanHMDCheckBox);
            this.groupBox3.Controls.Add(this.scanTODCheckBox);
            this.groupBox3.Controls.Add(this.scanTIMCheckBox);
            this.groupBox3.Controls.Add(this.scanVDFCheckBox);
            this.groupBox3.Controls.Add(this.scanTMDCheckBox);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(0, 135);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(384, 93);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Scanners";
            // 
            // scanBFFCheckBox
            // 
            this.scanBFFCheckBox.AutoSize = true;
            this.scanBFFCheckBox.Location = new System.Drawing.Point(310, 19);
            this.scanBFFCheckBox.Name = "scanBFFCheckBox";
            this.scanBFFCheckBox.Size = new System.Drawing.Size(45, 17);
            this.scanBFFCheckBox.TabIndex = 10;
            this.scanBFFCheckBox.Text = "BFF";
            this.scanBFFCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanANCheckBox
            // 
            this.scanANCheckBox.AutoSize = true;
            this.scanANCheckBox.Location = new System.Drawing.Point(210, 42);
            this.scanANCheckBox.Name = "scanANCheckBox";
            this.scanANCheckBox.Size = new System.Drawing.Size(84, 17);
            this.scanANCheckBox.TabIndex = 9;
            this.scanANCheckBox.Text = "Scan for AN";
            this.scanANCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanPSXCheckBox
            // 
            this.scanPSXCheckBox.AutoSize = true;
            this.scanPSXCheckBox.Location = new System.Drawing.Point(210, 65);
            this.scanPSXCheckBox.Name = "scanPSXCheckBox";
            this.scanPSXCheckBox.Size = new System.Drawing.Size(90, 17);
            this.scanPSXCheckBox.TabIndex = 8;
            this.scanPSXCheckBox.Text = "Scan for PSX";
            this.scanPSXCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanMODCheckBox
            // 
            this.scanMODCheckBox.AutoSize = true;
            this.scanMODCheckBox.Location = new System.Drawing.Point(109, 65);
            this.scanMODCheckBox.Name = "scanMODCheckBox";
            this.scanMODCheckBox.Size = new System.Drawing.Size(94, 17);
            this.scanMODCheckBox.TabIndex = 7;
            this.scanMODCheckBox.Text = "Scan for MOD";
            this.scanMODCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanPMDCheckBox
            // 
            this.scanPMDCheckBox.AutoSize = true;
            this.scanPMDCheckBox.Location = new System.Drawing.Point(109, 19);
            this.scanPMDCheckBox.Name = "scanPMDCheckBox";
            this.scanPMDCheckBox.Size = new System.Drawing.Size(93, 17);
            this.scanPMDCheckBox.TabIndex = 6;
            this.scanPMDCheckBox.Text = "Scan for PMD";
            this.scanPMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanHMDCheckBox
            // 
            this.scanHMDCheckBox.AutoSize = true;
            this.scanHMDCheckBox.Location = new System.Drawing.Point(210, 19);
            this.scanHMDCheckBox.Name = "scanHMDCheckBox";
            this.scanHMDCheckBox.Size = new System.Drawing.Size(94, 17);
            this.scanHMDCheckBox.TabIndex = 5;
            this.scanHMDCheckBox.Text = "Scan for HMD";
            this.scanHMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanTODCheckBox
            // 
            this.scanTODCheckBox.AutoSize = true;
            this.scanTODCheckBox.Location = new System.Drawing.Point(10, 42);
            this.scanTODCheckBox.Name = "scanTODCheckBox";
            this.scanTODCheckBox.Size = new System.Drawing.Size(92, 17);
            this.scanTODCheckBox.TabIndex = 4;
            this.scanTODCheckBox.Text = "Scan for TOD";
            this.scanTODCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanTIMCheckBox
            // 
            this.scanTIMCheckBox.AutoSize = true;
            this.scanTIMCheckBox.Location = new System.Drawing.Point(10, 65);
            this.scanTIMCheckBox.Name = "scanTIMCheckBox";
            this.scanTIMCheckBox.Size = new System.Drawing.Size(88, 17);
            this.scanTIMCheckBox.TabIndex = 2;
            this.scanTIMCheckBox.Text = "Scan for TIM";
            this.scanTIMCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanVDFCheckBox
            // 
            this.scanVDFCheckBox.AutoSize = true;
            this.scanVDFCheckBox.Location = new System.Drawing.Point(109, 42);
            this.scanVDFCheckBox.Name = "scanVDFCheckBox";
            this.scanVDFCheckBox.Size = new System.Drawing.Size(90, 17);
            this.scanVDFCheckBox.TabIndex = 1;
            this.scanVDFCheckBox.Text = "Scan for VDF";
            this.scanVDFCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanTMDCheckBox
            // 
            this.scanTMDCheckBox.AutoSize = true;
            this.scanTMDCheckBox.Location = new System.Drawing.Point(10, 19);
            this.scanTMDCheckBox.Name = "scanTMDCheckBox";
            this.scanTMDCheckBox.Size = new System.Drawing.Size(93, 17);
            this.scanTMDCheckBox.TabIndex = 0;
            this.scanTMDCheckBox.Text = "Scan for TMD";
            this.scanTMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.optionIgnoreTIMVersionCheckBox);
            this.groupBox4.Controls.Add(this.optionIgnoreHMDVersionCheckBox);
            this.groupBox4.Controls.Add(this.optionNoOffsetCheckBox);
            this.groupBox4.Controls.Add(this.optionShowErrorsCheckBox);
            this.groupBox4.Controls.Add(this.optionAutoAttachLimbsCheckBox);
            this.groupBox4.Controls.Add(this.optionDrawAllToVRAMCheckBox);
            this.groupBox4.Controls.Add(this.optionIgnoreTMDVersionCheckBox);
            this.groupBox4.Controls.Add(this.optionDebugCheckBox);
            this.groupBox4.Controls.Add(this.optionNoVerboseCheckBox);
            this.groupBox4.Controls.Add(this.optionLogToFileCheckBox);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox4.Location = new System.Drawing.Point(0, 228);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(384, 92);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Options";
            // 
            // optionIgnoreHMDVersionCheckBox
            // 
            this.optionIgnoreHMDVersionCheckBox.AutoSize = true;
            this.optionIgnoreHMDVersionCheckBox.Location = new System.Drawing.Point(129, 65);
            this.optionIgnoreHMDVersionCheckBox.Name = "optionIgnoreHMDVersionCheckBox";
            this.optionIgnoreHMDVersionCheckBox.Size = new System.Drawing.Size(113, 17);
            this.optionIgnoreHMDVersionCheckBox.TabIndex = 10;
            this.optionIgnoreHMDVersionCheckBox.Text = "Skip HMD Version";
            this.optionIgnoreHMDVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionNoOffsetCheckBox
            // 
            this.optionNoOffsetCheckBox.AutoSize = true;
            this.optionNoOffsetCheckBox.Location = new System.Drawing.Point(222, 42);
            this.optionNoOffsetCheckBox.Name = "optionNoOffsetCheckBox";
            this.optionNoOffsetCheckBox.Size = new System.Drawing.Size(131, 17);
            this.optionNoOffsetCheckBox.TabIndex = 9;
            this.optionNoOffsetCheckBox.Text = "Only Scan Start of File";
            this.optionNoOffsetCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionShowErrorsCheckBox
            // 
            this.optionShowErrorsCheckBox.AutoSize = true;
            this.optionShowErrorsCheckBox.Location = new System.Drawing.Point(286, 19);
            this.optionShowErrorsCheckBox.Name = "optionShowErrorsCheckBox";
            this.optionShowErrorsCheckBox.Size = new System.Drawing.Size(83, 17);
            this.optionShowErrorsCheckBox.TabIndex = 8;
            this.optionShowErrorsCheckBox.Text = "Show Errors";
            this.optionShowErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionAutoAttachLimbsCheckBox
            // 
            this.optionAutoAttachLimbsCheckBox.AutoSize = true;
            this.optionAutoAttachLimbsCheckBox.Location = new System.Drawing.Point(129, 42);
            this.optionAutoAttachLimbsCheckBox.Name = "optionAutoAttachLimbsCheckBox";
            this.optionAutoAttachLimbsCheckBox.Size = new System.Drawing.Size(87, 17);
            this.optionAutoAttachLimbsCheckBox.TabIndex = 7;
            this.optionAutoAttachLimbsCheckBox.Text = "Attach Limbs";
            this.optionAutoAttachLimbsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionDrawAllToVRAMCheckBox
            // 
            this.optionDrawAllToVRAMCheckBox.AutoSize = true;
            this.optionDrawAllToVRAMCheckBox.Location = new System.Drawing.Point(12, 42);
            this.optionDrawAllToVRAMCheckBox.Name = "optionDrawAllToVRAMCheckBox";
            this.optionDrawAllToVRAMCheckBox.Size = new System.Drawing.Size(111, 17);
            this.optionDrawAllToVRAMCheckBox.TabIndex = 6;
            this.optionDrawAllToVRAMCheckBox.Text = "Draw All to VRAM";
            this.optionDrawAllToVRAMCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionIgnoreTMDVersionCheckBox
            // 
            this.optionIgnoreTMDVersionCheckBox.AutoSize = true;
            this.optionIgnoreTMDVersionCheckBox.Location = new System.Drawing.Point(12, 65);
            this.optionIgnoreTMDVersionCheckBox.Name = "optionIgnoreTMDVersionCheckBox";
            this.optionIgnoreTMDVersionCheckBox.Size = new System.Drawing.Size(112, 17);
            this.optionIgnoreTMDVersionCheckBox.TabIndex = 4;
            this.optionIgnoreTMDVersionCheckBox.Text = "Skip TMD Version";
            this.optionIgnoreTMDVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionDebugCheckBox
            // 
            this.optionDebugCheckBox.AutoSize = true;
            this.optionDebugCheckBox.Location = new System.Drawing.Point(222, 19);
            this.optionDebugCheckBox.Name = "optionDebugCheckBox";
            this.optionDebugCheckBox.Size = new System.Drawing.Size(58, 17);
            this.optionDebugCheckBox.TabIndex = 3;
            this.optionDebugCheckBox.Text = "Debug";
            this.optionDebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionNoVerboseCheckBox
            // 
            this.optionNoVerboseCheckBox.AutoSize = true;
            this.optionNoVerboseCheckBox.Location = new System.Drawing.Point(129, 19);
            this.optionNoVerboseCheckBox.Name = "optionNoVerboseCheckBox";
            this.optionNoVerboseCheckBox.Size = new System.Drawing.Size(82, 17);
            this.optionNoVerboseCheckBox.TabIndex = 2;
            this.optionNoVerboseCheckBox.Text = "No-Verbose";
            this.optionNoVerboseCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionLogToFileCheckBox
            // 
            this.optionLogToFileCheckBox.AutoSize = true;
            this.optionLogToFileCheckBox.Location = new System.Drawing.Point(12, 19);
            this.optionLogToFileCheckBox.Name = "optionLogToFileCheckBox";
            this.optionLogToFileCheckBox.Size = new System.Drawing.Size(91, 17);
            this.optionLogToFileCheckBox.TabIndex = 1;
            this.optionLogToFileCheckBox.Text = "Generate Log";
            this.optionLogToFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // scanButton
            // 
            this.scanButton.Enabled = false;
            this.scanButton.Location = new System.Drawing.Point(297, 326);
            this.scanButton.Name = "scanButton";
            this.scanButton.Size = new System.Drawing.Size(75, 23);
            this.scanButton.TabIndex = 4;
            this.scanButton.Text = "Scan";
            this.scanButton.UseVisualStyleBackColor = true;
            this.scanButton.Click += new System.EventHandler(this.scanButton_Click);
            // 
            // optionIgnoreTIMVersionCheckBox
            // 
            this.optionIgnoreTIMVersionCheckBox.AutoSize = true;
            this.optionIgnoreTIMVersionCheckBox.Location = new System.Drawing.Point(246, 65);
            this.optionIgnoreTIMVersionCheckBox.Name = "optionIgnoreTIMVersionCheckBox";
            this.optionIgnoreTIMVersionCheckBox.Size = new System.Drawing.Size(107, 17);
            this.optionIgnoreTIMVersionCheckBox.TabIndex = 11;
            this.optionIgnoreTIMVersionCheckBox.Text = "Skip TIM Version";
            this.optionIgnoreTIMVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScannerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 356);
            this.Controls.Add(this.scanButton);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ScannerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PSXPrev Launcher";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button selectFolderButton;
        private System.Windows.Forms.Button selectISOButton;
        private System.Windows.Forms.Button selectFileButton;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox filterTextBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox scanHMDCheckBox;
        private System.Windows.Forms.CheckBox scanTODCheckBox;
        private System.Windows.Forms.CheckBox scanTIMCheckBox;
        private System.Windows.Forms.CheckBox scanVDFCheckBox;
        private System.Windows.Forms.CheckBox scanTMDCheckBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox optionDebugCheckBox;
        private System.Windows.Forms.CheckBox optionNoVerboseCheckBox;
        private System.Windows.Forms.CheckBox optionLogToFileCheckBox;
        private System.Windows.Forms.Button scanButton;
        private System.Windows.Forms.CheckBox scanPMDCheckBox;
        private System.Windows.Forms.CheckBox scanMODCheckBox;
        private System.Windows.Forms.CheckBox scanPSXCheckBox;
        private System.Windows.Forms.CheckBox scanANCheckBox;
        private System.Windows.Forms.CheckBox optionIgnoreTMDVersionCheckBox;
        private System.Windows.Forms.CheckBox scanBFFCheckBox;
        private System.Windows.Forms.CheckBox optionAutoAttachLimbsCheckBox;
        private System.Windows.Forms.CheckBox optionDrawAllToVRAMCheckBox;
        private System.Windows.Forms.CheckBox optionShowErrorsCheckBox;
        private System.Windows.Forms.CheckBox optionNoOffsetCheckBox;
        private System.Windows.Forms.CheckBox optionIgnoreHMDVersionCheckBox;
        private System.Windows.Forms.CheckBox optionIgnoreTIMVersionCheckBox;
    }
}
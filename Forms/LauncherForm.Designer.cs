namespace PSXPrev.Forms
{
    partial class LauncherForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LauncherForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SelectFolderButton = new System.Windows.Forms.Button();
            this.SelectISOButton = new System.Windows.Forms.Button();
            this.SelectFileButton = new System.Windows.Forms.Button();
            this.FilenameText = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.FilterText = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.scanForAnCheckBox = new System.Windows.Forms.CheckBox();
            this.psxCheckBox = new System.Windows.Forms.CheckBox();
            this.crocCheckBox = new System.Windows.Forms.CheckBox();
            this.PMDCheckBox = new System.Windows.Forms.CheckBox();
            this.hmdCheckBox = new System.Windows.Forms.CheckBox();
            this.TODCheckBox = new System.Windows.Forms.CheckBox();
            this.TIMCheckBox = new System.Windows.Forms.CheckBox();
            this.VDFCheckBox = new System.Windows.Forms.CheckBox();
            this.TMDCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ignoreVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.DebugCheckBox = new System.Windows.Forms.CheckBox();
            this.NoVerboseCheckBox = new System.Windows.Forms.CheckBox();
            this.LogCheckBox = new System.Windows.Forms.CheckBox();
            this.ScanButton = new System.Windows.Forms.Button();
            this.scanForBffCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SelectFolderButton);
            this.groupBox1.Controls.Add(this.SelectISOButton);
            this.groupBox1.Controls.Add(this.SelectFileButton);
            this.groupBox1.Controls.Add(this.FilenameText);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(384, 77);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filename";
            // 
            // SelectFolderButton
            // 
            this.SelectFolderButton.Location = new System.Drawing.Point(167, 45);
            this.SelectFolderButton.Name = "SelectFolderButton";
            this.SelectFolderButton.Size = new System.Drawing.Size(79, 23);
            this.SelectFolderButton.TabIndex = 3;
            this.SelectFolderButton.Text = "Select Folder";
            this.SelectFolderButton.Click += new System.EventHandler(this.SelectFolderButton_Click);
            // 
            // SelectISOButton
            // 
            this.SelectISOButton.Location = new System.Drawing.Point(86, 45);
            this.SelectISOButton.Name = "SelectISOButton";
            this.SelectISOButton.Size = new System.Drawing.Size(75, 23);
            this.SelectISOButton.TabIndex = 2;
            this.SelectISOButton.Text = "Select ISO";
            this.SelectISOButton.UseVisualStyleBackColor = true;
            this.SelectISOButton.Click += new System.EventHandler(this.SelectISOButton_Click);
            // 
            // SelectFileButton
            // 
            this.SelectFileButton.Location = new System.Drawing.Point(12, 45);
            this.SelectFileButton.Name = "SelectFileButton";
            this.SelectFileButton.Size = new System.Drawing.Size(68, 23);
            this.SelectFileButton.TabIndex = 1;
            this.SelectFileButton.Text = "Select File";
            this.SelectFileButton.UseVisualStyleBackColor = true;
            this.SelectFileButton.Click += new System.EventHandler(this.SelectFileButton_Click);
            // 
            // FilenameText
            // 
            this.FilenameText.Location = new System.Drawing.Point(12, 19);
            this.FilenameText.Name = "FilenameText";
            this.FilenameText.Size = new System.Drawing.Size(360, 20);
            this.FilenameText.TabIndex = 0;
            this.FilenameText.TextChanged += new System.EventHandler(this.FilenameText_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.FilterText);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 77);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(384, 58);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filter";
            // 
            // FilterText
            // 
            this.FilterText.Location = new System.Drawing.Point(12, 21);
            this.FilterText.Name = "FilterText";
            this.FilterText.Size = new System.Drawing.Size(75, 20);
            this.FilterText.TabIndex = 1;
            this.FilterText.Text = "*.*";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.scanForBffCheckBox);
            this.groupBox3.Controls.Add(this.scanForAnCheckBox);
            this.groupBox3.Controls.Add(this.psxCheckBox);
            this.groupBox3.Controls.Add(this.crocCheckBox);
            this.groupBox3.Controls.Add(this.PMDCheckBox);
            this.groupBox3.Controls.Add(this.hmdCheckBox);
            this.groupBox3.Controls.Add(this.TODCheckBox);
            this.groupBox3.Controls.Add(this.TIMCheckBox);
            this.groupBox3.Controls.Add(this.VDFCheckBox);
            this.groupBox3.Controls.Add(this.TMDCheckBox);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(0, 135);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(384, 93);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Scanners";
            // 
            // scanForAnCheckBox
            // 
            this.scanForAnCheckBox.AutoSize = true;
            this.scanForAnCheckBox.Location = new System.Drawing.Point(210, 42);
            this.scanForAnCheckBox.Name = "scanForAnCheckBox";
            this.scanForAnCheckBox.Size = new System.Drawing.Size(84, 17);
            this.scanForAnCheckBox.TabIndex = 9;
            this.scanForAnCheckBox.Text = "Scan for AN";
            this.scanForAnCheckBox.UseVisualStyleBackColor = true;
            // 
            // psxCheckBox
            // 
            this.psxCheckBox.AutoSize = true;
            this.psxCheckBox.Location = new System.Drawing.Point(210, 65);
            this.psxCheckBox.Name = "psxCheckBox";
            this.psxCheckBox.Size = new System.Drawing.Size(90, 17);
            this.psxCheckBox.TabIndex = 8;
            this.psxCheckBox.Text = "Scan for PSX";
            this.psxCheckBox.UseVisualStyleBackColor = true;
            // 
            // crocCheckBox
            // 
            this.crocCheckBox.AutoSize = true;
            this.crocCheckBox.Location = new System.Drawing.Point(109, 65);
            this.crocCheckBox.Name = "crocCheckBox";
            this.crocCheckBox.Size = new System.Drawing.Size(91, 17);
            this.crocCheckBox.TabIndex = 7;
            this.crocCheckBox.Text = "Scan for Croc";
            this.crocCheckBox.UseVisualStyleBackColor = true;
            // 
            // PMDCheckBox
            // 
            this.PMDCheckBox.AutoSize = true;
            this.PMDCheckBox.Location = new System.Drawing.Point(109, 19);
            this.PMDCheckBox.Name = "PMDCheckBox";
            this.PMDCheckBox.Size = new System.Drawing.Size(93, 17);
            this.PMDCheckBox.TabIndex = 6;
            this.PMDCheckBox.Text = "Scan for PMD";
            this.PMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // hmdCheckBox
            // 
            this.hmdCheckBox.AutoSize = true;
            this.hmdCheckBox.Location = new System.Drawing.Point(210, 19);
            this.hmdCheckBox.Name = "hmdCheckBox";
            this.hmdCheckBox.Size = new System.Drawing.Size(94, 17);
            this.hmdCheckBox.TabIndex = 5;
            this.hmdCheckBox.Text = "Scan for HMD";
            this.hmdCheckBox.UseVisualStyleBackColor = true;
            // 
            // TODCheckBox
            // 
            this.TODCheckBox.AutoSize = true;
            this.TODCheckBox.Location = new System.Drawing.Point(10, 42);
            this.TODCheckBox.Name = "TODCheckBox";
            this.TODCheckBox.Size = new System.Drawing.Size(92, 17);
            this.TODCheckBox.TabIndex = 4;
            this.TODCheckBox.Text = "Scan for TOD";
            this.TODCheckBox.UseVisualStyleBackColor = true;
            // 
            // TIMCheckBox
            // 
            this.TIMCheckBox.AutoSize = true;
            this.TIMCheckBox.Location = new System.Drawing.Point(10, 65);
            this.TIMCheckBox.Name = "TIMCheckBox";
            this.TIMCheckBox.Size = new System.Drawing.Size(88, 17);
            this.TIMCheckBox.TabIndex = 2;
            this.TIMCheckBox.Text = "Scan for TIM";
            this.TIMCheckBox.UseVisualStyleBackColor = true;
            // 
            // VDFCheckBox
            // 
            this.VDFCheckBox.AutoSize = true;
            this.VDFCheckBox.Location = new System.Drawing.Point(109, 42);
            this.VDFCheckBox.Name = "VDFCheckBox";
            this.VDFCheckBox.Size = new System.Drawing.Size(90, 17);
            this.VDFCheckBox.TabIndex = 1;
            this.VDFCheckBox.Text = "Scan for VDF";
            this.VDFCheckBox.UseVisualStyleBackColor = true;
            // 
            // TMDCheckBox
            // 
            this.TMDCheckBox.AutoSize = true;
            this.TMDCheckBox.Location = new System.Drawing.Point(10, 19);
            this.TMDCheckBox.Name = "TMDCheckBox";
            this.TMDCheckBox.Size = new System.Drawing.Size(93, 17);
            this.TMDCheckBox.TabIndex = 0;
            this.TMDCheckBox.Text = "Scan for TMD";
            this.TMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.ignoreVersionCheckBox);
            this.groupBox4.Controls.Add(this.DebugCheckBox);
            this.groupBox4.Controls.Add(this.NoVerboseCheckBox);
            this.groupBox4.Controls.Add(this.LogCheckBox);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox4.Location = new System.Drawing.Point(0, 228);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(384, 46);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Options";
            // 
            // ignoreVersionCheckBox
            // 
            this.ignoreVersionCheckBox.AutoSize = true;
            this.ignoreVersionCheckBox.Location = new System.Drawing.Point(261, 19);
            this.ignoreVersionCheckBox.Name = "ignoreVersionCheckBox";
            this.ignoreVersionCheckBox.Size = new System.Drawing.Size(121, 17);
            this.ignoreVersionCheckBox.TabIndex = 4;
            this.ignoreVersionCheckBox.Text = "Ignore TMD Version";
            this.ignoreVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // DebugCheckBox
            // 
            this.DebugCheckBox.AutoSize = true;
            this.DebugCheckBox.Location = new System.Drawing.Point(197, 19);
            this.DebugCheckBox.Name = "DebugCheckBox";
            this.DebugCheckBox.Size = new System.Drawing.Size(58, 17);
            this.DebugCheckBox.TabIndex = 3;
            this.DebugCheckBox.Text = "Debug";
            this.DebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // NoVerboseCheckBox
            // 
            this.NoVerboseCheckBox.AutoSize = true;
            this.NoVerboseCheckBox.Location = new System.Drawing.Point(109, 19);
            this.NoVerboseCheckBox.Name = "NoVerboseCheckBox";
            this.NoVerboseCheckBox.Size = new System.Drawing.Size(82, 17);
            this.NoVerboseCheckBox.TabIndex = 2;
            this.NoVerboseCheckBox.Text = "No-Verbose";
            this.NoVerboseCheckBox.UseVisualStyleBackColor = true;
            // 
            // LogCheckBox
            // 
            this.LogCheckBox.AutoSize = true;
            this.LogCheckBox.Location = new System.Drawing.Point(12, 19);
            this.LogCheckBox.Name = "LogCheckBox";
            this.LogCheckBox.Size = new System.Drawing.Size(91, 17);
            this.LogCheckBox.TabIndex = 1;
            this.LogCheckBox.Text = "Generate Log";
            this.LogCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScanButton
            // 
            this.ScanButton.Enabled = false;
            this.ScanButton.Location = new System.Drawing.Point(297, 280);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(75, 23);
            this.ScanButton.TabIndex = 4;
            this.ScanButton.Text = "Scan";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // scanForBffCheckBox
            // 
            this.scanForBffCheckBox.AutoSize = true;
            this.scanForBffCheckBox.Location = new System.Drawing.Point(310, 19);
            this.scanForBffCheckBox.Name = "scanForBffCheckBox";
            this.scanForBffCheckBox.Size = new System.Drawing.Size(45, 17);
            this.scanForBffCheckBox.TabIndex = 10;
            this.scanForBffCheckBox.Text = "BFF";
            this.scanForBffCheckBox.UseVisualStyleBackColor = true;
            // 
            // LauncherForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 310);
            this.Controls.Add(this.ScanButton);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "LauncherForm";
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
        private System.Windows.Forms.Button SelectFolderButton;
        private System.Windows.Forms.Button SelectISOButton;
        private System.Windows.Forms.Button SelectFileButton;
        private System.Windows.Forms.TextBox FilenameText;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox FilterText;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox hmdCheckBox;
        private System.Windows.Forms.CheckBox TODCheckBox;
        private System.Windows.Forms.CheckBox TIMCheckBox;
        private System.Windows.Forms.CheckBox VDFCheckBox;
        private System.Windows.Forms.CheckBox TMDCheckBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox DebugCheckBox;
        private System.Windows.Forms.CheckBox NoVerboseCheckBox;
        private System.Windows.Forms.CheckBox LogCheckBox;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.CheckBox PMDCheckBox;
        private System.Windows.Forms.CheckBox crocCheckBox;
        private System.Windows.Forms.CheckBox psxCheckBox;
        private System.Windows.Forms.CheckBox scanForAnCheckBox;
        private System.Windows.Forms.CheckBox ignoreVersionCheckBox;
        private System.Windows.Forms.CheckBox scanForBffCheckBox;
    }
}
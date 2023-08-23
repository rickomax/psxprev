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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScannerForm));
            this.fileGroupBox = new System.Windows.Forms.GroupBox();
            this.filterUseRegexCheckBox = new System.Windows.Forms.CheckBox();
            this.filterLabel = new System.Windows.Forms.Label();
            this.selectBINButton = new System.Windows.Forms.Button();
            this.selectFolderButton = new System.Windows.Forms.Button();
            this.selectISOButton = new System.Windows.Forms.Button();
            this.selectFileButton = new System.Windows.Forms.Button();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.checkBFFCheckBox = new System.Windows.Forms.CheckBox();
            this.checkPSXCheckBox = new System.Windows.Forms.CheckBox();
            this.checkMODCheckBox = new System.Windows.Forms.CheckBox();
            this.checkHMDCheckBox = new System.Windows.Forms.CheckBox();
            this.optionOldUVAlignmentCheckBox = new System.Windows.Forms.CheckBox();
            this.optionAsyncScanCheckBox = new System.Windows.Forms.CheckBox();
            this.binContentsCheckBox = new System.Windows.Forms.CheckBox();
            this.isoContentsCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetNextCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetAlignUpDown = new System.Windows.Forms.NumericUpDown();
            this.offsetStartOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetStartCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetStopCheckBox = new System.Windows.Forms.CheckBox();
            this.optionDrawAllToVRAMCheckBox = new System.Windows.Forms.CheckBox();
            this.binSectorCheckBox = new System.Windows.Forms.CheckBox();
            this.formatsGroupBox = new System.Windows.Forms.GroupBox();
            this.animationsLabel = new System.Windows.Forms.Label();
            this.texturesLabel = new System.Windows.Forms.Label();
            this.modelsLabel = new System.Windows.Forms.Label();
            this.checkANCheckBox = new System.Windows.Forms.CheckBox();
            this.checkPMDCheckBox = new System.Windows.Forms.CheckBox();
            this.checkTODCheckBox = new System.Windows.Forms.CheckBox();
            this.checkTIMCheckBox = new System.Windows.Forms.CheckBox();
            this.checkVDFCheckBox = new System.Windows.Forms.CheckBox();
            this.checkTMDCheckBox = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.optionShowErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.optionDebugCheckBox = new System.Windows.Forms.CheckBox();
            this.optionNoVerboseCheckBox = new System.Windows.Forms.CheckBox();
            this.optionLogToFileCheckBox = new System.Windows.Forms.CheckBox();
            this.optionIgnoreTIMVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.optionIgnoreHMDVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.optionIgnoreTMDVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.showAdvancedMarginPanel = new System.Windows.Forms.Panel();
            this.showAdvancedButton = new System.Windows.Forms.Button();
            this.advancedOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.binSectorInvalidLabel = new System.Windows.Forms.Label();
            this.binSectorSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.binSectorStartUpDown = new System.Windows.Forms.NumericUpDown();
            this.advancedOffsetGroupBox = new System.Windows.Forms.GroupBox();
            this.offsetAlignLabel = new System.Windows.Forms.Label();
            this.offsetStartUpDown = new System.Windows.Forms.NumericUpDown();
            this.offsetStopUpDown = new System.Windows.Forms.NumericUpDown();
            this.scanButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.scanCancelMarginFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.fileGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.offsetAlignUpDown)).BeginInit();
            this.formatsGroupBox.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            this.showAdvancedMarginPanel.SuspendLayout();
            this.advancedOptionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.binSectorSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.binSectorStartUpDown)).BeginInit();
            this.advancedOffsetGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.offsetStartUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.offsetStopUpDown)).BeginInit();
            this.scanCancelMarginFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileGroupBox
            // 
            this.fileGroupBox.Controls.Add(this.filterUseRegexCheckBox);
            this.fileGroupBox.Controls.Add(this.filterLabel);
            this.fileGroupBox.Controls.Add(this.selectBINButton);
            this.fileGroupBox.Controls.Add(this.selectFolderButton);
            this.fileGroupBox.Controls.Add(this.selectISOButton);
            this.fileGroupBox.Controls.Add(this.selectFileButton);
            this.fileGroupBox.Controls.Add(this.filePathTextBox);
            this.fileGroupBox.Controls.Add(this.filterTextBox);
            this.fileGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.fileGroupBox.Location = new System.Drawing.Point(0, 0);
            this.fileGroupBox.Name = "fileGroupBox";
            this.fileGroupBox.Size = new System.Drawing.Size(394, 105);
            this.fileGroupBox.TabIndex = 0;
            this.fileGroupBox.TabStop = false;
            this.fileGroupBox.Text = "Files";
            // 
            // filterUseRegexCheckBox
            // 
            this.filterUseRegexCheckBox.AutoSize = true;
            this.filterUseRegexCheckBox.Enabled = false;
            this.filterUseRegexCheckBox.Location = new System.Drawing.Point(204, 77);
            this.filterUseRegexCheckBox.Name = "filterUseRegexCheckBox";
            this.filterUseRegexCheckBox.Size = new System.Drawing.Size(139, 17);
            this.filterUseRegexCheckBox.TabIndex = 6;
            this.filterUseRegexCheckBox.Text = "Use Regular Expression";
            this.toolTip.SetToolTip(this.filterUseRegexCheckBox, "Use regular expressions to filter files instead of wildcard patterns.");
            this.filterUseRegexCheckBox.UseVisualStyleBackColor = true;
            this.filterUseRegexCheckBox.Visible = false;
            // 
            // filterLabel
            // 
            this.filterLabel.AutoSize = true;
            this.filterLabel.Location = new System.Drawing.Point(6, 78);
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Size = new System.Drawing.Size(32, 13);
            this.filterLabel.TabIndex = 10;
            this.filterLabel.Text = "Filter:";
            // 
            // selectBINButton
            // 
            this.selectBINButton.Location = new System.Drawing.Point(173, 45);
            this.selectBINButton.Name = "selectBINButton";
            this.selectBINButton.Size = new System.Drawing.Size(75, 23);
            this.selectBINButton.TabIndex = 3;
            this.selectBINButton.Text = "Select BIN";
            this.selectBINButton.UseVisualStyleBackColor = true;
            this.selectBINButton.Click += new System.EventHandler(this.selectBINButton_Click);
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
            // selectISOButton
            // 
            this.selectISOButton.Location = new System.Drawing.Point(254, 45);
            this.selectISOButton.Name = "selectISOButton";
            this.selectISOButton.Size = new System.Drawing.Size(75, 23);
            this.selectISOButton.TabIndex = 4;
            this.selectISOButton.Text = "Select ISO";
            this.selectISOButton.UseVisualStyleBackColor = true;
            this.selectISOButton.Click += new System.EventHandler(this.selectISOButton_Click);
            // 
            // selectFileButton
            // 
            this.selectFileButton.Location = new System.Drawing.Point(99, 45);
            this.selectFileButton.Name = "selectFileButton";
            this.selectFileButton.Size = new System.Drawing.Size(68, 23);
            this.selectFileButton.TabIndex = 2;
            this.selectFileButton.Text = "Select File";
            this.selectFileButton.UseVisualStyleBackColor = true;
            this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filePathTextBox.Location = new System.Drawing.Point(12, 19);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.Size = new System.Drawing.Size(370, 20);
            this.filePathTextBox.TabIndex = 0;
            this.filePathTextBox.TextChanged += new System.EventHandler(this.filePathTextBox_TextChanged);
            // 
            // filterTextBox
            // 
            this.filterTextBox.Location = new System.Drawing.Point(44, 75);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(150, 20);
            this.filterTextBox.TabIndex = 5;
            this.filterTextBox.Text = "*.*";
            this.toolTip.SetToolTip(this.filterTextBox, "Wildcard pattern to match files.");
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 10000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // checkBFFCheckBox
            // 
            this.checkBFFCheckBox.AutoSize = true;
            this.checkBFFCheckBox.Location = new System.Drawing.Point(339, 19);
            this.checkBFFCheckBox.Name = "checkBFFCheckBox";
            this.checkBFFCheckBox.Size = new System.Drawing.Size(45, 17);
            this.checkBFFCheckBox.TabIndex = 5;
            this.checkBFFCheckBox.Text = "BFF";
            this.toolTip.SetToolTip(this.checkBFFCheckBox, "Work in Progress");
            this.checkBFFCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkPSXCheckBox
            // 
            this.checkPSXCheckBox.AutoSize = true;
            this.checkPSXCheckBox.Location = new System.Drawing.Point(284, 19);
            this.checkPSXCheckBox.Name = "checkPSXCheckBox";
            this.checkPSXCheckBox.Size = new System.Drawing.Size(47, 17);
            this.checkPSXCheckBox.TabIndex = 4;
            this.checkPSXCheckBox.Text = "PSX";
            this.toolTip.SetToolTip(this.checkPSXCheckBox, "Tony Hawk\'s Pro Scater / Apocalypse / Spiderman");
            this.checkPSXCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkMODCheckBox
            // 
            this.checkMODCheckBox.AutoSize = true;
            this.checkMODCheckBox.Location = new System.Drawing.Point(229, 19);
            this.checkMODCheckBox.Name = "checkMODCheckBox";
            this.checkMODCheckBox.Size = new System.Drawing.Size(51, 17);
            this.checkMODCheckBox.TabIndex = 3;
            this.checkMODCheckBox.Text = "MOD";
            this.toolTip.SetToolTip(this.checkMODCheckBox, "Croc");
            this.checkMODCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkHMDCheckBox
            // 
            this.checkHMDCheckBox.AutoSize = true;
            this.checkHMDCheckBox.Location = new System.Drawing.Point(119, 19);
            this.checkHMDCheckBox.Name = "checkHMDCheckBox";
            this.checkHMDCheckBox.Size = new System.Drawing.Size(51, 17);
            this.checkHMDCheckBox.TabIndex = 1;
            this.checkHMDCheckBox.Text = "HMD";
            this.toolTip.SetToolTip(this.checkHMDCheckBox, "Includes Textures and Animations");
            this.checkHMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionOldUVAlignmentCheckBox
            // 
            this.optionOldUVAlignmentCheckBox.AutoSize = true;
            this.optionOldUVAlignmentCheckBox.Location = new System.Drawing.Point(135, 42);
            this.optionOldUVAlignmentCheckBox.Name = "optionOldUVAlignmentCheckBox";
            this.optionOldUVAlignmentCheckBox.Size = new System.Drawing.Size(109, 17);
            this.optionOldUVAlignmentCheckBox.TabIndex = 4;
            this.optionOldUVAlignmentCheckBox.Text = "Old UV Alignment";
            this.toolTip.SetToolTip(this.optionOldUVAlignmentCheckBox, "PSXPrev originally used UV alignment that\r\nranged from 0-256, however this was in" +
        "correct,\r\nand 0-255 is now used by default.");
            this.optionOldUVAlignmentCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionAsyncScanCheckBox
            // 
            this.optionAsyncScanCheckBox.AutoSize = true;
            this.optionAsyncScanCheckBox.Location = new System.Drawing.Point(258, 42);
            this.optionAsyncScanCheckBox.Name = "optionAsyncScanCheckBox";
            this.optionAsyncScanCheckBox.Size = new System.Drawing.Size(83, 17);
            this.optionAsyncScanCheckBox.TabIndex = 5;
            this.optionAsyncScanCheckBox.Text = "Async Scan";
            this.toolTip.SetToolTip(this.optionAsyncScanCheckBox, "All formats for the current file will scan at the same time.");
            this.optionAsyncScanCheckBox.UseVisualStyleBackColor = true;
            // 
            // binContentsCheckBox
            // 
            this.binContentsCheckBox.AutoSize = true;
            this.binContentsCheckBox.Location = new System.Drawing.Point(12, 42);
            this.binContentsCheckBox.Name = "binContentsCheckBox";
            this.binContentsCheckBox.Size = new System.Drawing.Size(117, 17);
            this.binContentsCheckBox.TabIndex = 3;
            this.binContentsCheckBox.Text = "Scan BIN Contents";
            this.toolTip.SetToolTip(this.binContentsCheckBox, "Scan raw PS1 .BIN files without needing\r\nto reformat them. (experimental)");
            this.binContentsCheckBox.UseVisualStyleBackColor = true;
            this.binContentsCheckBox.CheckedChanged += new System.EventHandler(this.binContentsCheckBox_CheckedChanged);
            // 
            // isoContentsCheckBox
            // 
            this.isoContentsCheckBox.AutoSize = true;
            this.isoContentsCheckBox.Location = new System.Drawing.Point(135, 42);
            this.isoContentsCheckBox.Name = "isoContentsCheckBox";
            this.isoContentsCheckBox.Size = new System.Drawing.Size(117, 17);
            this.isoContentsCheckBox.TabIndex = 4;
            this.isoContentsCheckBox.Text = "Scan ISO Contents";
            this.toolTip.SetToolTip(this.isoContentsCheckBox, "Scan the contents of .ISO files.");
            this.isoContentsCheckBox.UseVisualStyleBackColor = true;
            // 
            // offsetNextCheckBox
            // 
            this.offsetNextCheckBox.AutoSize = true;
            this.offsetNextCheckBox.Location = new System.Drawing.Point(162, 47);
            this.offsetNextCheckBox.Name = "offsetNextCheckBox";
            this.offsetNextCheckBox.Size = new System.Drawing.Size(181, 17);
            this.offsetNextCheckBox.TabIndex = 6;
            this.offsetNextCheckBox.Text = "Next Offset at End of Last Match";
            this.toolTip.SetToolTip(this.offsetNextCheckBox, "When a scanner finds a valid file, the next offset will start\r\nat the end of that" +
        " file, instead of incrementing by Align.");
            this.offsetNextCheckBox.UseVisualStyleBackColor = true;
            // 
            // offsetAlignUpDown
            // 
            this.offsetAlignUpDown.Location = new System.Drawing.Point(279, 19);
            this.offsetAlignUpDown.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.offsetAlignUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.offsetAlignUpDown.Name = "offsetAlignUpDown";
            this.offsetAlignUpDown.Size = new System.Drawing.Size(60, 20);
            this.offsetAlignUpDown.TabIndex = 3;
            this.toolTip.SetToolTip(this.offsetAlignUpDown, "Increasing offset alignment can reduce scan times of\r\nlarge files. Usually this s" +
        "hould be a power of two.");
            this.offsetAlignUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // offsetStartOnlyCheckBox
            // 
            this.offsetStartOnlyCheckBox.AutoSize = true;
            this.offsetStartOnlyCheckBox.Location = new System.Drawing.Point(162, 20);
            this.offsetStartOnlyCheckBox.Name = "offsetStartOnlyCheckBox";
            this.offsetStartOnlyCheckBox.Size = new System.Drawing.Size(72, 17);
            this.offsetStartOnlyCheckBox.TabIndex = 2;
            this.offsetStartOnlyCheckBox.Text = "Start Only";
            this.toolTip.SetToolTip(this.offsetStartOnlyCheckBox, "Scan exactly at the Start offset and no higher.");
            this.offsetStartOnlyCheckBox.UseVisualStyleBackColor = true;
            this.offsetStartOnlyCheckBox.CheckedChanged += new System.EventHandler(this.offsetStartOnlyCheckBox_CheckedChanged);
            // 
            // offsetStartCheckBox
            // 
            this.offsetStartCheckBox.AutoSize = true;
            this.offsetStartCheckBox.Location = new System.Drawing.Point(12, 20);
            this.offsetStartCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.offsetStartCheckBox.Name = "offsetStartCheckBox";
            this.offsetStartCheckBox.Size = new System.Drawing.Size(51, 17);
            this.offsetStartCheckBox.TabIndex = 0;
            this.offsetStartCheckBox.Text = "Start:";
            this.toolTip.SetToolTip(this.offsetStartCheckBox, "Start scanning files at the hexadecimal offset.");
            this.offsetStartCheckBox.UseVisualStyleBackColor = true;
            this.offsetStartCheckBox.CheckedChanged += new System.EventHandler(this.offsetStartCheckBox_CheckedChanged);
            // 
            // offsetStopCheckBox
            // 
            this.offsetStopCheckBox.AutoSize = true;
            this.offsetStopCheckBox.Location = new System.Drawing.Point(12, 47);
            this.offsetStopCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.offsetStopCheckBox.Name = "offsetStopCheckBox";
            this.offsetStopCheckBox.Size = new System.Drawing.Size(51, 17);
            this.offsetStopCheckBox.TabIndex = 4;
            this.offsetStopCheckBox.Text = "Stop:";
            this.toolTip.SetToolTip(this.offsetStopCheckBox, "Stop scanning files at the hexadecimal offset.");
            this.offsetStopCheckBox.UseVisualStyleBackColor = true;
            this.offsetStopCheckBox.CheckedChanged += new System.EventHandler(this.offsetStopCheckBox_CheckedChanged);
            // 
            // optionDrawAllToVRAMCheckBox
            // 
            this.optionDrawAllToVRAMCheckBox.AutoSize = true;
            this.optionDrawAllToVRAMCheckBox.Location = new System.Drawing.Point(12, 42);
            this.optionDrawAllToVRAMCheckBox.Name = "optionDrawAllToVRAMCheckBox";
            this.optionDrawAllToVRAMCheckBox.Size = new System.Drawing.Size(111, 17);
            this.optionDrawAllToVRAMCheckBox.TabIndex = 3;
            this.optionDrawAllToVRAMCheckBox.Text = "Draw All to VRAM";
            this.toolTip.SetToolTip(this.optionDrawAllToVRAMCheckBox, "All loaded textures will be drawn \r\nto VRAM after the scan finishes.");
            this.optionDrawAllToVRAMCheckBox.UseVisualStyleBackColor = true;
            // 
            // binSectorCheckBox
            // 
            this.binSectorCheckBox.AutoSize = true;
            this.binSectorCheckBox.Enabled = false;
            this.binSectorCheckBox.Location = new System.Drawing.Point(12, 69);
            this.binSectorCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.binSectorCheckBox.Name = "binSectorCheckBox";
            this.binSectorCheckBox.Size = new System.Drawing.Size(131, 17);
            this.binSectorCheckBox.TabIndex = 6;
            this.binSectorCheckBox.Text = "BIN Sector Start/Size:";
            this.toolTip.SetToolTip(this.binSectorCheckBox, "Enter custom sector user start/size for raw PS1 .BIN files.\r\nThe default is 24, 2" +
        "048.");
            this.binSectorCheckBox.UseVisualStyleBackColor = true;
            this.binSectorCheckBox.CheckedChanged += new System.EventHandler(this.binSectorCheckBox_CheckedChanged);
            // 
            // formatsGroupBox
            // 
            this.formatsGroupBox.Controls.Add(this.animationsLabel);
            this.formatsGroupBox.Controls.Add(this.texturesLabel);
            this.formatsGroupBox.Controls.Add(this.modelsLabel);
            this.formatsGroupBox.Controls.Add(this.checkBFFCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkANCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkPSXCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkMODCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkPMDCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkHMDCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkTODCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkTIMCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkVDFCheckBox);
            this.formatsGroupBox.Controls.Add(this.checkTMDCheckBox);
            this.formatsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.formatsGroupBox.Location = new System.Drawing.Point(0, 105);
            this.formatsGroupBox.Name = "formatsGroupBox";
            this.formatsGroupBox.Size = new System.Drawing.Size(394, 70);
            this.formatsGroupBox.TabIndex = 1;
            this.formatsGroupBox.TabStop = false;
            this.formatsGroupBox.Text = "Formats";
            // 
            // animationsLabel
            // 
            this.animationsLabel.AutoSize = true;
            this.animationsLabel.Location = new System.Drawing.Point(161, 43);
            this.animationsLabel.Name = "animationsLabel";
            this.animationsLabel.Size = new System.Drawing.Size(61, 13);
            this.animationsLabel.TabIndex = 13;
            this.animationsLabel.Text = "Animations:";
            // 
            // texturesLabel
            // 
            this.texturesLabel.AutoSize = true;
            this.texturesLabel.Location = new System.Drawing.Point(6, 43);
            this.texturesLabel.Name = "texturesLabel";
            this.texturesLabel.Size = new System.Drawing.Size(51, 13);
            this.texturesLabel.TabIndex = 12;
            this.texturesLabel.Text = "Textures:";
            // 
            // modelsLabel
            // 
            this.modelsLabel.AutoSize = true;
            this.modelsLabel.Location = new System.Drawing.Point(6, 20);
            this.modelsLabel.Name = "modelsLabel";
            this.modelsLabel.Size = new System.Drawing.Size(44, 13);
            this.modelsLabel.TabIndex = 11;
            this.modelsLabel.Text = "Models:";
            // 
            // checkANCheckBox
            // 
            this.checkANCheckBox.AutoSize = true;
            this.checkANCheckBox.Location = new System.Drawing.Point(229, 42);
            this.checkANCheckBox.Name = "checkANCheckBox";
            this.checkANCheckBox.Size = new System.Drawing.Size(41, 17);
            this.checkANCheckBox.TabIndex = 7;
            this.checkANCheckBox.Text = "AN";
            this.checkANCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkPMDCheckBox
            // 
            this.checkPMDCheckBox.AutoSize = true;
            this.checkPMDCheckBox.Location = new System.Drawing.Point(174, 19);
            this.checkPMDCheckBox.Name = "checkPMDCheckBox";
            this.checkPMDCheckBox.Size = new System.Drawing.Size(50, 17);
            this.checkPMDCheckBox.TabIndex = 2;
            this.checkPMDCheckBox.Text = "PMD";
            this.checkPMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkTODCheckBox
            // 
            this.checkTODCheckBox.AutoSize = true;
            this.checkTODCheckBox.Location = new System.Drawing.Point(284, 42);
            this.checkTODCheckBox.Name = "checkTODCheckBox";
            this.checkTODCheckBox.Size = new System.Drawing.Size(49, 17);
            this.checkTODCheckBox.TabIndex = 8;
            this.checkTODCheckBox.Text = "TOD";
            this.checkTODCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkTIMCheckBox
            // 
            this.checkTIMCheckBox.AutoSize = true;
            this.checkTIMCheckBox.Location = new System.Drawing.Point(64, 42);
            this.checkTIMCheckBox.Name = "checkTIMCheckBox";
            this.checkTIMCheckBox.Size = new System.Drawing.Size(45, 17);
            this.checkTIMCheckBox.TabIndex = 6;
            this.checkTIMCheckBox.Text = "TIM";
            this.checkTIMCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkVDFCheckBox
            // 
            this.checkVDFCheckBox.AutoSize = true;
            this.checkVDFCheckBox.Location = new System.Drawing.Point(339, 42);
            this.checkVDFCheckBox.Name = "checkVDFCheckBox";
            this.checkVDFCheckBox.Size = new System.Drawing.Size(47, 17);
            this.checkVDFCheckBox.TabIndex = 9;
            this.checkVDFCheckBox.Text = "VDF";
            this.checkVDFCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkTMDCheckBox
            // 
            this.checkTMDCheckBox.AutoSize = true;
            this.checkTMDCheckBox.Location = new System.Drawing.Point(64, 19);
            this.checkTMDCheckBox.Name = "checkTMDCheckBox";
            this.checkTMDCheckBox.Size = new System.Drawing.Size(50, 17);
            this.checkTMDCheckBox.TabIndex = 0;
            this.checkTMDCheckBox.Text = "TMD";
            this.checkTMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.optionShowErrorsCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionOldUVAlignmentCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionDrawAllToVRAMCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionDebugCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionNoVerboseCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionLogToFileCheckBox);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 175);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(394, 69);
            this.optionsGroupBox.TabIndex = 2;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // optionShowErrorsCheckBox
            // 
            this.optionShowErrorsCheckBox.AutoSize = true;
            this.optionShowErrorsCheckBox.Location = new System.Drawing.Point(258, 19);
            this.optionShowErrorsCheckBox.Name = "optionShowErrorsCheckBox";
            this.optionShowErrorsCheckBox.Size = new System.Drawing.Size(89, 17);
            this.optionShowErrorsCheckBox.TabIndex = 2;
            this.optionShowErrorsCheckBox.Text = "Error Logging";
            this.optionShowErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionDebugCheckBox
            // 
            this.optionDebugCheckBox.AutoSize = true;
            this.optionDebugCheckBox.Location = new System.Drawing.Point(258, 42);
            this.optionDebugCheckBox.Name = "optionDebugCheckBox";
            this.optionDebugCheckBox.Size = new System.Drawing.Size(99, 17);
            this.optionDebugCheckBox.TabIndex = 5;
            this.optionDebugCheckBox.Text = "Debug Logging";
            this.optionDebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionNoVerboseCheckBox
            // 
            this.optionNoVerboseCheckBox.AutoSize = true;
            this.optionNoVerboseCheckBox.Location = new System.Drawing.Point(135, 19);
            this.optionNoVerboseCheckBox.Name = "optionNoVerboseCheckBox";
            this.optionNoVerboseCheckBox.Size = new System.Drawing.Size(97, 17);
            this.optionNoVerboseCheckBox.TabIndex = 1;
            this.optionNoVerboseCheckBox.Text = "Log to Console";
            this.optionNoVerboseCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionLogToFileCheckBox
            // 
            this.optionLogToFileCheckBox.AutoSize = true;
            this.optionLogToFileCheckBox.Location = new System.Drawing.Point(12, 19);
            this.optionLogToFileCheckBox.Name = "optionLogToFileCheckBox";
            this.optionLogToFileCheckBox.Size = new System.Drawing.Size(75, 17);
            this.optionLogToFileCheckBox.TabIndex = 0;
            this.optionLogToFileCheckBox.Text = "Log to File";
            this.optionLogToFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionIgnoreTIMVersionCheckBox
            // 
            this.optionIgnoreTIMVersionCheckBox.AutoSize = true;
            this.optionIgnoreTIMVersionCheckBox.Location = new System.Drawing.Point(258, 19);
            this.optionIgnoreTIMVersionCheckBox.Name = "optionIgnoreTIMVersionCheckBox";
            this.optionIgnoreTIMVersionCheckBox.Size = new System.Drawing.Size(107, 17);
            this.optionIgnoreTIMVersionCheckBox.TabIndex = 2;
            this.optionIgnoreTIMVersionCheckBox.Text = "Skip TIM Version";
            this.optionIgnoreTIMVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionIgnoreHMDVersionCheckBox
            // 
            this.optionIgnoreHMDVersionCheckBox.AutoSize = true;
            this.optionIgnoreHMDVersionCheckBox.Location = new System.Drawing.Point(135, 19);
            this.optionIgnoreHMDVersionCheckBox.Name = "optionIgnoreHMDVersionCheckBox";
            this.optionIgnoreHMDVersionCheckBox.Size = new System.Drawing.Size(113, 17);
            this.optionIgnoreHMDVersionCheckBox.TabIndex = 1;
            this.optionIgnoreHMDVersionCheckBox.Text = "Skip HMD Version";
            this.optionIgnoreHMDVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionIgnoreTMDVersionCheckBox
            // 
            this.optionIgnoreTMDVersionCheckBox.AutoSize = true;
            this.optionIgnoreTMDVersionCheckBox.Location = new System.Drawing.Point(12, 19);
            this.optionIgnoreTMDVersionCheckBox.Name = "optionIgnoreTMDVersionCheckBox";
            this.optionIgnoreTMDVersionCheckBox.Size = new System.Drawing.Size(112, 17);
            this.optionIgnoreTMDVersionCheckBox.TabIndex = 0;
            this.optionIgnoreTMDVersionCheckBox.Text = "Skip TMD Version";
            this.optionIgnoreTMDVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // showAdvancedMarginPanel
            // 
            this.showAdvancedMarginPanel.AutoSize = true;
            this.showAdvancedMarginPanel.Controls.Add(this.showAdvancedButton);
            this.showAdvancedMarginPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.showAdvancedMarginPanel.Location = new System.Drawing.Point(0, 244);
            this.showAdvancedMarginPanel.Name = "showAdvancedMarginPanel";
            this.showAdvancedMarginPanel.Padding = new System.Windows.Forms.Padding(3);
            this.showAdvancedMarginPanel.Size = new System.Drawing.Size(394, 29);
            this.showAdvancedMarginPanel.TabIndex = 3;
            // 
            // showAdvancedButton
            // 
            this.showAdvancedButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.showAdvancedButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showAdvancedButton.Location = new System.Drawing.Point(3, 3);
            this.showAdvancedButton.Name = "showAdvancedButton";
            this.showAdvancedButton.Size = new System.Drawing.Size(388, 23);
            this.showAdvancedButton.TabIndex = 0;
            this.showAdvancedButton.Text = "Show Advanced";
            this.showAdvancedButton.UseVisualStyleBackColor = true;
            this.showAdvancedButton.Click += new System.EventHandler(this.showAdvancedButton_Click);
            // 
            // advancedOptionsGroupBox
            // 
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorInvalidLabel);
            this.advancedOptionsGroupBox.Controls.Add(this.optionIgnoreTIMVersionCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.optionIgnoreHMDVersionCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorSizeUpDown);
            this.advancedOptionsGroupBox.Controls.Add(this.optionAsyncScanCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorStartUpDown);
            this.advancedOptionsGroupBox.Controls.Add(this.binContentsCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.optionIgnoreTMDVersionCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.isoContentsCheckBox);
            this.advancedOptionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.advancedOptionsGroupBox.Location = new System.Drawing.Point(0, 273);
            this.advancedOptionsGroupBox.Name = "advancedOptionsGroupBox";
            this.advancedOptionsGroupBox.Size = new System.Drawing.Size(394, 98);
            this.advancedOptionsGroupBox.TabIndex = 4;
            this.advancedOptionsGroupBox.TabStop = false;
            this.advancedOptionsGroupBox.Text = "Advanced Options";
            // 
            // binSectorInvalidLabel
            // 
            this.binSectorInvalidLabel.AutoSize = true;
            this.binSectorInvalidLabel.ForeColor = System.Drawing.Color.Firebrick;
            this.binSectorInvalidLabel.Location = new System.Drawing.Point(270, 65);
            this.binSectorInvalidLabel.Name = "binSectorInvalidLabel";
            this.binSectorInvalidLabel.Size = new System.Drawing.Size(103, 26);
            this.binSectorInvalidLabel.TabIndex = 17;
            this.binSectorInvalidLabel.Text = "Start + Size is bigger\r\nthan raw size (2352)";
            this.binSectorInvalidLabel.Visible = false;
            // 
            // binSectorSizeUpDown
            // 
            this.binSectorSizeUpDown.Enabled = false;
            this.binSectorSizeUpDown.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.binSectorSizeUpDown.Location = new System.Drawing.Point(206, 68);
            this.binSectorSizeUpDown.Maximum = new decimal(new int[] {
            2352,
            0,
            0,
            0});
            this.binSectorSizeUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.binSectorSizeUpDown.Name = "binSectorSizeUpDown";
            this.binSectorSizeUpDown.Size = new System.Drawing.Size(54, 20);
            this.binSectorSizeUpDown.TabIndex = 8;
            this.binSectorSizeUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.binSectorSizeUpDown.ValueChanged += new System.EventHandler(this.binSectorStartSizeUpDown_ValueChanged);
            // 
            // binSectorStartUpDown
            // 
            this.binSectorStartUpDown.Enabled = false;
            this.binSectorStartUpDown.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.binSectorStartUpDown.Location = new System.Drawing.Point(146, 68);
            this.binSectorStartUpDown.Maximum = new decimal(new int[] {
            2351,
            0,
            0,
            0});
            this.binSectorStartUpDown.Name = "binSectorStartUpDown";
            this.binSectorStartUpDown.Size = new System.Drawing.Size(54, 20);
            this.binSectorStartUpDown.TabIndex = 7;
            this.binSectorStartUpDown.ValueChanged += new System.EventHandler(this.binSectorStartSizeUpDown_ValueChanged);
            // 
            // advancedOffsetGroupBox
            // 
            this.advancedOffsetGroupBox.Controls.Add(this.offsetNextCheckBox);
            this.advancedOffsetGroupBox.Controls.Add(this.offsetAlignUpDown);
            this.advancedOffsetGroupBox.Controls.Add(this.offsetAlignLabel);
            this.advancedOffsetGroupBox.Controls.Add(this.offsetStartOnlyCheckBox);
            this.advancedOffsetGroupBox.Controls.Add(this.offsetStartCheckBox);
            this.advancedOffsetGroupBox.Controls.Add(this.offsetStartUpDown);
            this.advancedOffsetGroupBox.Controls.Add(this.offsetStopUpDown);
            this.advancedOffsetGroupBox.Controls.Add(this.offsetStopCheckBox);
            this.advancedOffsetGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.advancedOffsetGroupBox.Location = new System.Drawing.Point(0, 371);
            this.advancedOffsetGroupBox.Name = "advancedOffsetGroupBox";
            this.advancedOffsetGroupBox.Size = new System.Drawing.Size(394, 76);
            this.advancedOffsetGroupBox.TabIndex = 5;
            this.advancedOffsetGroupBox.TabStop = false;
            this.advancedOffsetGroupBox.Text = "Advanced File Offset";
            // 
            // offsetAlignLabel
            // 
            this.offsetAlignLabel.AutoSize = true;
            this.offsetAlignLabel.Location = new System.Drawing.Point(240, 21);
            this.offsetAlignLabel.Name = "offsetAlignLabel";
            this.offsetAlignLabel.Size = new System.Drawing.Size(33, 13);
            this.offsetAlignLabel.TabIndex = 16;
            this.offsetAlignLabel.Text = "Align:";
            // 
            // offsetStartUpDown
            // 
            this.offsetStartUpDown.Enabled = false;
            this.offsetStartUpDown.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.offsetStartUpDown.Hexadecimal = true;
            this.offsetStartUpDown.Location = new System.Drawing.Point(66, 19);
            this.offsetStartUpDown.Maximum = new decimal(new int[] {
            -1,
            -1,
            0,
            0});
            this.offsetStartUpDown.Name = "offsetStartUpDown";
            this.offsetStartUpDown.Size = new System.Drawing.Size(84, 20);
            this.offsetStartUpDown.TabIndex = 1;
            // 
            // offsetStopUpDown
            // 
            this.offsetStopUpDown.Enabled = false;
            this.offsetStopUpDown.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.offsetStopUpDown.Hexadecimal = true;
            this.offsetStopUpDown.Location = new System.Drawing.Point(66, 46);
            this.offsetStopUpDown.Maximum = new decimal(new int[] {
            -1,
            -1,
            0,
            0});
            this.offsetStopUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.offsetStopUpDown.Name = "offsetStopUpDown";
            this.offsetStopUpDown.Size = new System.Drawing.Size(84, 20);
            this.offsetStopUpDown.TabIndex = 5;
            this.offsetStopUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // scanButton
            // 
            this.scanButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.scanButton.Enabled = false;
            this.scanButton.Location = new System.Drawing.Point(227, 6);
            this.scanButton.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.scanButton.Name = "scanButton";
            this.scanButton.Size = new System.Drawing.Size(75, 23);
            this.scanButton.TabIndex = 0;
            this.scanButton.Text = "Scan";
            this.scanButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(308, 6);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(0);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // scanCancelMarginFlowLayoutPanel
            // 
            this.scanCancelMarginFlowLayoutPanel.Controls.Add(this.cancelButton);
            this.scanCancelMarginFlowLayoutPanel.Controls.Add(this.scanButton);
            this.scanCancelMarginFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.scanCancelMarginFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.scanCancelMarginFlowLayoutPanel.Location = new System.Drawing.Point(0, 447);
            this.scanCancelMarginFlowLayoutPanel.Name = "scanCancelMarginFlowLayoutPanel";
            this.scanCancelMarginFlowLayoutPanel.Padding = new System.Windows.Forms.Padding(0, 6, 11, 7);
            this.scanCancelMarginFlowLayoutPanel.Size = new System.Drawing.Size(394, 36);
            this.scanCancelMarginFlowLayoutPanel.TabIndex = 6;
            this.scanCancelMarginFlowLayoutPanel.WrapContents = false;
            // 
            // ScannerForm
            // 
            this.AcceptButton = this.scanButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(394, 491);
            this.Controls.Add(this.scanCancelMarginFlowLayoutPanel);
            this.Controls.Add(this.advancedOffsetGroupBox);
            this.Controls.Add(this.advancedOptionsGroupBox);
            this.Controls.Add(this.showAdvancedMarginPanel);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.formatsGroupBox);
            this.Controls.Add(this.fileGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(410, 39);
            this.Name = "ScannerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PSXPrev Scan Files";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ScannerForm_FormClosed);
            this.Load += new System.EventHandler(this.ScannerForm_Load);
            this.fileGroupBox.ResumeLayout(false);
            this.fileGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.offsetAlignUpDown)).EndInit();
            this.formatsGroupBox.ResumeLayout(false);
            this.formatsGroupBox.PerformLayout();
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.showAdvancedMarginPanel.ResumeLayout(false);
            this.advancedOptionsGroupBox.ResumeLayout(false);
            this.advancedOptionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.binSectorSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.binSectorStartUpDown)).EndInit();
            this.advancedOffsetGroupBox.ResumeLayout(false);
            this.advancedOffsetGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.offsetStartUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.offsetStopUpDown)).EndInit();
            this.scanCancelMarginFlowLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox fileGroupBox;
        private System.Windows.Forms.Button selectFolderButton;
        private System.Windows.Forms.Button selectISOButton;
        private System.Windows.Forms.Button selectFileButton;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.TextBox filterTextBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button selectBINButton;
        private System.Windows.Forms.CheckBox filterUseRegexCheckBox;
        private System.Windows.Forms.Label filterLabel;
        private System.Windows.Forms.GroupBox formatsGroupBox;
        private System.Windows.Forms.Label animationsLabel;
        private System.Windows.Forms.Label texturesLabel;
        private System.Windows.Forms.Label modelsLabel;
        private System.Windows.Forms.CheckBox checkBFFCheckBox;
        private System.Windows.Forms.CheckBox checkANCheckBox;
        private System.Windows.Forms.CheckBox checkPSXCheckBox;
        private System.Windows.Forms.CheckBox checkMODCheckBox;
        private System.Windows.Forms.CheckBox checkPMDCheckBox;
        private System.Windows.Forms.CheckBox checkHMDCheckBox;
        private System.Windows.Forms.CheckBox checkTODCheckBox;
        private System.Windows.Forms.CheckBox checkTIMCheckBox;
        private System.Windows.Forms.CheckBox checkVDFCheckBox;
        private System.Windows.Forms.CheckBox checkTMDCheckBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.CheckBox optionIgnoreTIMVersionCheckBox;
        private System.Windows.Forms.CheckBox optionIgnoreHMDVersionCheckBox;
        private System.Windows.Forms.CheckBox optionShowErrorsCheckBox;
        private System.Windows.Forms.CheckBox optionOldUVAlignmentCheckBox;
        private System.Windows.Forms.CheckBox optionDrawAllToVRAMCheckBox;
        private System.Windows.Forms.CheckBox optionIgnoreTMDVersionCheckBox;
        private System.Windows.Forms.CheckBox optionDebugCheckBox;
        private System.Windows.Forms.CheckBox optionNoVerboseCheckBox;
        private System.Windows.Forms.CheckBox optionLogToFileCheckBox;
        private System.Windows.Forms.Panel showAdvancedMarginPanel;
        private System.Windows.Forms.Button showAdvancedButton;
        private System.Windows.Forms.GroupBox advancedOptionsGroupBox;
        private System.Windows.Forms.CheckBox binSectorCheckBox;
        private System.Windows.Forms.NumericUpDown binSectorSizeUpDown;
        private System.Windows.Forms.CheckBox optionAsyncScanCheckBox;
        private System.Windows.Forms.NumericUpDown binSectorStartUpDown;
        private System.Windows.Forms.CheckBox binContentsCheckBox;
        private System.Windows.Forms.CheckBox isoContentsCheckBox;
        private System.Windows.Forms.GroupBox advancedOffsetGroupBox;
        private System.Windows.Forms.CheckBox offsetNextCheckBox;
        private System.Windows.Forms.NumericUpDown offsetAlignUpDown;
        private System.Windows.Forms.Label offsetAlignLabel;
        private System.Windows.Forms.CheckBox offsetStartOnlyCheckBox;
        private System.Windows.Forms.CheckBox offsetStartCheckBox;
        private System.Windows.Forms.NumericUpDown offsetStartUpDown;
        private System.Windows.Forms.NumericUpDown offsetStopUpDown;
        private System.Windows.Forms.CheckBox offsetStopCheckBox;
        private System.Windows.Forms.Button scanButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel scanCancelMarginFlowLayoutPanel;
        private System.Windows.Forms.Label binSectorInvalidLabel;
    }
}
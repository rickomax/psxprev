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
            this.historyBookmarkButton = new System.Windows.Forms.Button();
            this.historyRemoveButton = new System.Windows.Forms.Button();
            this.historyLabel = new System.Windows.Forms.Label();
            this.historyComboBox = new System.Windows.Forms.ComboBox();
            this.filterUseRegexCheckBox = new System.Windows.Forms.CheckBox();
            this.filterLabel = new System.Windows.Forms.Label();
            this.selectBINButton = new System.Windows.Forms.Button();
            this.selectFolderButton = new System.Windows.Forms.Button();
            this.selectISOButton = new System.Windows.Forms.Button();
            this.selectFileButton = new System.Windows.Forms.Button();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.formatBFFCheckBox = new System.Windows.Forms.CheckBox();
            this.formatPSXCheckBox = new System.Windows.Forms.CheckBox();
            this.formatMODCheckBox = new System.Windows.Forms.CheckBox();
            this.formatHMDCheckBox = new System.Windows.Forms.CheckBox();
            this.optionAsyncScanCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetNextCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetAlignUpDown = new System.Windows.Forms.NumericUpDown();
            this.offsetStartOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetStartCheckBox = new System.Windows.Forms.CheckBox();
            this.offsetStopCheckBox = new System.Windows.Forms.CheckBox();
            this.optionDrawAllToVRAMCheckBox = new System.Windows.Forms.CheckBox();
            this.binSectorCheckBox = new System.Windows.Forms.CheckBox();
            this.formatSPTCheckBox = new System.Windows.Forms.CheckBox();
            this.formatPMDCheckBox = new System.Windows.Forms.CheckBox();
            this.formatTODCheckBox = new System.Windows.Forms.CheckBox();
            this.formatTIMCheckBox = new System.Windows.Forms.CheckBox();
            this.formatTMDCheckBox = new System.Windows.Forms.CheckBox();
            this.formatPILCheckBox = new System.Windows.Forms.CheckBox();
            this.binScanComboBox = new System.Windows.Forms.ComboBox();
            this.formatVDFCheckBox = new System.Windows.Forms.CheckBox();
            this.isoScanComboBox = new System.Windows.Forms.ComboBox();
            this.unstrictTMDCheckBox = new System.Windows.Forms.CheckBox();
            this.unstrictHMDCheckBox = new System.Windows.Forms.CheckBox();
            this.unstrictPMDCheckBox = new System.Windows.Forms.CheckBox();
            this.unstrictTIMCheckBox = new System.Windows.Forms.CheckBox();
            this.formatsGroupBox = new System.Windows.Forms.GroupBox();
            this.formatANCheckBox = new System.Windows.Forms.CheckBox();
            this.animationsLabel = new System.Windows.Forms.Label();
            this.texturesLabel = new System.Windows.Forms.Label();
            this.modelsLabel = new System.Windows.Forms.Label();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.optionErrorLoggingCheckBox = new System.Windows.Forms.CheckBox();
            this.optionDebugLoggingCheckBox = new System.Windows.Forms.CheckBox();
            this.optionLogToConsoleCheckBox = new System.Windows.Forms.CheckBox();
            this.optionLogToFileCheckBox = new System.Windows.Forms.CheckBox();
            this.showAdvancedMarginPanel = new System.Windows.Forms.Panel();
            this.showAdvancedButton = new System.Windows.Forms.Button();
            this.advancedOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
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
            this.fileGroupBox.Controls.Add(this.historyBookmarkButton);
            this.fileGroupBox.Controls.Add(this.historyRemoveButton);
            this.fileGroupBox.Controls.Add(this.historyLabel);
            this.fileGroupBox.Controls.Add(this.historyComboBox);
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
            this.fileGroupBox.Size = new System.Drawing.Size(444, 133);
            this.fileGroupBox.TabIndex = 0;
            this.fileGroupBox.TabStop = false;
            this.fileGroupBox.Text = "Files";
            // 
            // historyBookmarkButton
            // 
            this.historyBookmarkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.historyBookmarkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.historyBookmarkButton.Location = new System.Drawing.Point(379, 18);
            this.historyBookmarkButton.Name = "historyBookmarkButton";
            this.historyBookmarkButton.Size = new System.Drawing.Size(24, 23);
            this.historyBookmarkButton.TabIndex = 1;
            this.historyBookmarkButton.Text = "+";
            this.toolTip.SetToolTip(this.historyBookmarkButton, "Bookmark history");
            this.historyBookmarkButton.UseVisualStyleBackColor = true;
            this.historyBookmarkButton.Click += new System.EventHandler(this.historyBookmarkButton_Click);
            // 
            // historyRemoveButton
            // 
            this.historyRemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.historyRemoveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.historyRemoveButton.Location = new System.Drawing.Point(409, 18);
            this.historyRemoveButton.Name = "historyRemoveButton";
            this.historyRemoveButton.Size = new System.Drawing.Size(24, 23);
            this.historyRemoveButton.TabIndex = 2;
            this.historyRemoveButton.Text = "−";
            this.toolTip.SetToolTip(this.historyRemoveButton, "Remove history");
            this.historyRemoveButton.UseVisualStyleBackColor = true;
            this.historyRemoveButton.Click += new System.EventHandler(this.historyRemoveButton_Click);
            // 
            // historyLabel
            // 
            this.historyLabel.AutoSize = true;
            this.historyLabel.Location = new System.Drawing.Point(6, 22);
            this.historyLabel.Name = "historyLabel";
            this.historyLabel.Size = new System.Drawing.Size(42, 13);
            this.historyLabel.TabIndex = 12;
            this.historyLabel.Text = "History:";
            // 
            // historyComboBox
            // 
            this.historyComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.historyComboBox.DropDownHeight = 275;
            this.historyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.historyComboBox.FormattingEnabled = true;
            this.historyComboBox.IntegralHeight = false;
            this.historyComboBox.ItemHeight = 13;
            this.historyComboBox.Location = new System.Drawing.Point(54, 19);
            this.historyComboBox.MaxDropDownItems = 21;
            this.historyComboBox.Name = "historyComboBox";
            this.historyComboBox.Size = new System.Drawing.Size(319, 21);
            this.historyComboBox.TabIndex = 0;
            this.historyComboBox.Tag = "NOTSETTING";
            this.toolTip.SetToolTip(this.historyComboBox, "Choose a previous scan from history");
            this.historyComboBox.SelectedIndexChanged += new System.EventHandler(this.historyComboBox_SelectedIndexChanged);
            // 
            // filterUseRegexCheckBox
            // 
            this.filterUseRegexCheckBox.AutoSize = true;
            this.filterUseRegexCheckBox.Location = new System.Drawing.Point(234, 105);
            this.filterUseRegexCheckBox.Name = "filterUseRegexCheckBox";
            this.filterUseRegexCheckBox.Size = new System.Drawing.Size(139, 17);
            this.filterUseRegexCheckBox.TabIndex = 9;
            this.filterUseRegexCheckBox.Text = "Use Regular Expression";
            this.toolTip.SetToolTip(this.filterUseRegexCheckBox, "Use Regular Expressions to filter files instead of wildcard patterns.\r\nRemember t" +
        "o include ^ and $ if you don\'t want the match to be open-ended.");
            this.filterUseRegexCheckBox.UseVisualStyleBackColor = true;
            this.filterUseRegexCheckBox.CheckedChanged += new System.EventHandler(this.filterUseRegexCheckBox_CheckedChanged);
            // 
            // filterLabel
            // 
            this.filterLabel.AutoSize = true;
            this.filterLabel.Location = new System.Drawing.Point(6, 106);
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Size = new System.Drawing.Size(32, 13);
            this.filterLabel.TabIndex = 10;
            this.filterLabel.Text = "Filter:";
            // 
            // selectBINButton
            // 
            this.selectBINButton.Location = new System.Drawing.Point(173, 73);
            this.selectBINButton.Name = "selectBINButton";
            this.selectBINButton.Size = new System.Drawing.Size(75, 23);
            this.selectBINButton.TabIndex = 6;
            this.selectBINButton.Text = "Select BIN";
            this.selectBINButton.UseVisualStyleBackColor = true;
            this.selectBINButton.Click += new System.EventHandler(this.selectBINButton_Click);
            // 
            // selectFolderButton
            // 
            this.selectFolderButton.Location = new System.Drawing.Point(12, 73);
            this.selectFolderButton.Name = "selectFolderButton";
            this.selectFolderButton.Size = new System.Drawing.Size(79, 23);
            this.selectFolderButton.TabIndex = 4;
            this.selectFolderButton.Text = "Select Folder";
            this.selectFolderButton.Click += new System.EventHandler(this.selectFolderButton_Click);
            // 
            // selectISOButton
            // 
            this.selectISOButton.Location = new System.Drawing.Point(254, 73);
            this.selectISOButton.Name = "selectISOButton";
            this.selectISOButton.Size = new System.Drawing.Size(75, 23);
            this.selectISOButton.TabIndex = 7;
            this.selectISOButton.Text = "Select ISO";
            this.selectISOButton.UseVisualStyleBackColor = true;
            this.selectISOButton.Click += new System.EventHandler(this.selectISOButton_Click);
            // 
            // selectFileButton
            // 
            this.selectFileButton.Location = new System.Drawing.Point(99, 73);
            this.selectFileButton.Name = "selectFileButton";
            this.selectFileButton.Size = new System.Drawing.Size(68, 23);
            this.selectFileButton.TabIndex = 5;
            this.selectFileButton.Text = "Select File";
            this.selectFileButton.UseVisualStyleBackColor = true;
            this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filePathTextBox.Location = new System.Drawing.Point(12, 47);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.Size = new System.Drawing.Size(420, 20);
            this.filePathTextBox.TabIndex = 3;
            this.filePathTextBox.TextChanged += new System.EventHandler(this.filePathTextBox_TextChanged);
            // 
            // filterTextBox
            // 
            this.filterTextBox.Location = new System.Drawing.Point(44, 103);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(180, 20);
            this.filterTextBox.TabIndex = 8;
            this.filterTextBox.Text = "*.*";
            this.toolTip.SetToolTip(this.filterTextBox, "Wildcard or Regex pattern to match files.");
            this.filterTextBox.TextChanged += new System.EventHandler(this.filterTextBox_TextChanged);
            this.filterTextBox.MouseEnter += new System.EventHandler(this.filterTextBox_MouseEnter);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 10000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // formatBFFCheckBox
            // 
            this.formatBFFCheckBox.AutoSize = true;
            this.formatBFFCheckBox.Location = new System.Drawing.Point(341, 19);
            this.formatBFFCheckBox.Name = "formatBFFCheckBox";
            this.formatBFFCheckBox.Size = new System.Drawing.Size(45, 17);
            this.formatBFFCheckBox.TabIndex = 5;
            this.formatBFFCheckBox.Tag = "format:BFF";
            this.formatBFFCheckBox.Text = "BFF";
            this.toolTip.SetToolTip(this.formatBFFCheckBox, "Blitz Games Models and Animations Format\r\nIncludes subformats: FMW, FMM, PSI\r\nFro" +
        "gger 2 / Chicken Run (Uses SPT textures)");
            this.formatBFFCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatPSXCheckBox
            // 
            this.formatPSXCheckBox.AutoSize = true;
            this.formatPSXCheckBox.Location = new System.Drawing.Point(287, 19);
            this.formatPSXCheckBox.Name = "formatPSXCheckBox";
            this.formatPSXCheckBox.Size = new System.Drawing.Size(47, 17);
            this.formatPSXCheckBox.TabIndex = 4;
            this.formatPSXCheckBox.Tag = "format:PSX";
            this.formatPSXCheckBox.Text = "PSX";
            this.toolTip.SetToolTip(this.formatPSXCheckBox, "Neversoft Model and Textures Format\r\nTony Hawk\'s Pro Skater / Apocalypse / Spider" +
        "man");
            this.formatPSXCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatMODCheckBox
            // 
            this.formatMODCheckBox.AutoSize = true;
            this.formatMODCheckBox.Location = new System.Drawing.Point(233, 19);
            this.formatMODCheckBox.Name = "formatMODCheckBox";
            this.formatMODCheckBox.Size = new System.Drawing.Size(51, 17);
            this.formatMODCheckBox.TabIndex = 3;
            this.formatMODCheckBox.Tag = "format:MOD";
            this.formatMODCheckBox.Text = "MOD";
            this.toolTip.SetToolTip(this.formatMODCheckBox, "Croc Model Format");
            this.formatMODCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatHMDCheckBox
            // 
            this.formatHMDCheckBox.AutoSize = true;
            this.formatHMDCheckBox.Location = new System.Drawing.Point(125, 19);
            this.formatHMDCheckBox.Name = "formatHMDCheckBox";
            this.formatHMDCheckBox.Size = new System.Drawing.Size(51, 17);
            this.formatHMDCheckBox.TabIndex = 1;
            this.formatHMDCheckBox.Tag = "format:HMD";
            this.formatHMDCheckBox.Text = "HMD";
            this.toolTip.SetToolTip(this.formatHMDCheckBox, "Standard Model, Textures, and Animations Format");
            this.formatHMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionAsyncScanCheckBox
            // 
            this.optionAsyncScanCheckBox.AutoSize = true;
            this.optionAsyncScanCheckBox.Location = new System.Drawing.Point(278, 44);
            this.optionAsyncScanCheckBox.Name = "optionAsyncScanCheckBox";
            this.optionAsyncScanCheckBox.Size = new System.Drawing.Size(83, 17);
            this.optionAsyncScanCheckBox.TabIndex = 5;
            this.optionAsyncScanCheckBox.Text = "Async Scan";
            this.toolTip.SetToolTip(this.optionAsyncScanCheckBox, "All formats for the current file will scan at the same time.");
            this.optionAsyncScanCheckBox.UseVisualStyleBackColor = true;
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
            this.optionDrawAllToVRAMCheckBox.Location = new System.Drawing.Point(258, 19);
            this.optionDrawAllToVRAMCheckBox.Name = "optionDrawAllToVRAMCheckBox";
            this.optionDrawAllToVRAMCheckBox.Size = new System.Drawing.Size(111, 17);
            this.optionDrawAllToVRAMCheckBox.TabIndex = 5;
            this.optionDrawAllToVRAMCheckBox.Text = "Draw All to VRAM";
            this.toolTip.SetToolTip(this.optionDrawAllToVRAMCheckBox, "All loaded textures will be drawn \r\nto VRAM after the scan finishes.");
            this.optionDrawAllToVRAMCheckBox.UseVisualStyleBackColor = true;
            // 
            // binSectorCheckBox
            // 
            this.binSectorCheckBox.AutoSize = true;
            this.binSectorCheckBox.Enabled = false;
            this.binSectorCheckBox.Location = new System.Drawing.Point(12, 73);
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
            // formatSPTCheckBox
            // 
            this.formatSPTCheckBox.AutoSize = true;
            this.formatSPTCheckBox.Location = new System.Drawing.Point(125, 42);
            this.formatSPTCheckBox.Name = "formatSPTCheckBox";
            this.formatSPTCheckBox.Size = new System.Drawing.Size(47, 17);
            this.formatSPTCheckBox.TabIndex = 8;
            this.formatSPTCheckBox.Tag = "format:SPT";
            this.formatSPTCheckBox.Text = "SPT";
            this.toolTip.SetToolTip(this.formatSPTCheckBox, resources.GetString("formatSPTCheckBox.ToolTip"));
            this.formatSPTCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatPMDCheckBox
            // 
            this.formatPMDCheckBox.AutoSize = true;
            this.formatPMDCheckBox.Location = new System.Drawing.Point(179, 19);
            this.formatPMDCheckBox.Name = "formatPMDCheckBox";
            this.formatPMDCheckBox.Size = new System.Drawing.Size(50, 17);
            this.formatPMDCheckBox.TabIndex = 2;
            this.formatPMDCheckBox.Tag = "format:PMD";
            this.formatPMDCheckBox.Text = "PMD";
            this.toolTip.SetToolTip(this.formatPMDCheckBox, "Standard Model Format");
            this.formatPMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatTODCheckBox
            // 
            this.formatTODCheckBox.AutoSize = true;
            this.formatTODCheckBox.Location = new System.Drawing.Point(125, 66);
            this.formatTODCheckBox.Name = "formatTODCheckBox";
            this.formatTODCheckBox.Size = new System.Drawing.Size(49, 17);
            this.formatTODCheckBox.TabIndex = 10;
            this.formatTODCheckBox.Tag = "format:TOD";
            this.formatTODCheckBox.Text = "TOD";
            this.toolTip.SetToolTip(this.formatTODCheckBox, "Standard Animation Format");
            this.formatTODCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatTIMCheckBox
            // 
            this.formatTIMCheckBox.AutoSize = true;
            this.formatTIMCheckBox.Location = new System.Drawing.Point(71, 42);
            this.formatTIMCheckBox.Name = "formatTIMCheckBox";
            this.formatTIMCheckBox.Size = new System.Drawing.Size(45, 17);
            this.formatTIMCheckBox.TabIndex = 7;
            this.formatTIMCheckBox.Tag = "format:TIM";
            this.formatTIMCheckBox.Text = "TIM";
            this.toolTip.SetToolTip(this.formatTIMCheckBox, "Standard Texture Format");
            this.formatTIMCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatTMDCheckBox
            // 
            this.formatTMDCheckBox.AutoSize = true;
            this.formatTMDCheckBox.Location = new System.Drawing.Point(71, 19);
            this.formatTMDCheckBox.Name = "formatTMDCheckBox";
            this.formatTMDCheckBox.Size = new System.Drawing.Size(50, 17);
            this.formatTMDCheckBox.TabIndex = 0;
            this.formatTMDCheckBox.Tag = "format:TMD";
            this.formatTMDCheckBox.Text = "TMD";
            this.toolTip.SetToolTip(this.formatTMDCheckBox, "Standard Model Format");
            this.formatTMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatPILCheckBox
            // 
            this.formatPILCheckBox.AutoSize = true;
            this.formatPILCheckBox.Location = new System.Drawing.Point(395, 19);
            this.formatPILCheckBox.Name = "formatPILCheckBox";
            this.formatPILCheckBox.Size = new System.Drawing.Size(42, 17);
            this.formatPILCheckBox.TabIndex = 6;
            this.formatPILCheckBox.Tag = "format:PIL";
            this.formatPILCheckBox.Text = "PIL";
            this.toolTip.SetToolTip(this.formatPILCheckBox, "Blitz Games Models and Animations Format\r\nIncludes subformats: PSI\r\nAction Man 2 " +
        "/ Frogger 2 / Chicken Run (Uses SPT textures)");
            this.formatPILCheckBox.UseVisualStyleBackColor = true;
            // 
            // binScanComboBox
            // 
            this.binScanComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.binScanComboBox.FormattingEnabled = true;
            this.binScanComboBox.Items.AddRange(new object[] {
            "Don\'t Scan BIN",
            "Scan BIN Contents",
            "Scan BIN Data"});
            this.binScanComboBox.Location = new System.Drawing.Point(12, 42);
            this.binScanComboBox.Name = "binScanComboBox";
            this.binScanComboBox.Size = new System.Drawing.Size(120, 21);
            this.binScanComboBox.TabIndex = 3;
            this.toolTip.SetToolTip(this.binScanComboBox, resources.GetString("binScanComboBox.ToolTip"));
            this.binScanComboBox.SelectedIndexChanged += new System.EventHandler(this.binScanComboBox_SelectedIndexChanged);
            // 
            // formatVDFCheckBox
            // 
            this.formatVDFCheckBox.AutoSize = true;
            this.formatVDFCheckBox.Location = new System.Drawing.Point(179, 66);
            this.formatVDFCheckBox.Name = "formatVDFCheckBox";
            this.formatVDFCheckBox.Size = new System.Drawing.Size(47, 17);
            this.formatVDFCheckBox.TabIndex = 11;
            this.formatVDFCheckBox.Tag = "format:VDF";
            this.formatVDFCheckBox.Text = "VDF";
            this.toolTip.SetToolTip(this.formatVDFCheckBox, "Vertex Diff Animation Format");
            this.formatVDFCheckBox.UseVisualStyleBackColor = true;
            // 
            // isoScanComboBox
            // 
            this.isoScanComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.isoScanComboBox.FormattingEnabled = true;
            this.isoScanComboBox.Items.AddRange(new object[] {
            "Don\'t Scan ISO",
            "Scan ISO Contents"});
            this.isoScanComboBox.Location = new System.Drawing.Point(145, 42);
            this.isoScanComboBox.Name = "isoScanComboBox";
            this.isoScanComboBox.Size = new System.Drawing.Size(120, 21);
            this.isoScanComboBox.TabIndex = 4;
            this.toolTip.SetToolTip(this.isoScanComboBox, "Scan the contents of .ISO files.");
            // 
            // unstrictTMDCheckBox
            // 
            this.unstrictTMDCheckBox.AutoSize = true;
            this.unstrictTMDCheckBox.Location = new System.Drawing.Point(88, 19);
            this.unstrictTMDCheckBox.Name = "unstrictTMDCheckBox";
            this.unstrictTMDCheckBox.Size = new System.Drawing.Size(50, 17);
            this.unstrictTMDCheckBox.TabIndex = 19;
            this.unstrictTMDCheckBox.Tag = "unstrict:TMD";
            this.unstrictTMDCheckBox.Text = "TMD";
            this.toolTip.SetToolTip(this.unstrictTMDCheckBox, "Support for non-standard TMD formats\r\nthat use a different version number");
            this.unstrictTMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // unstrictHMDCheckBox
            // 
            this.unstrictHMDCheckBox.AutoSize = true;
            this.unstrictHMDCheckBox.Location = new System.Drawing.Point(142, 19);
            this.unstrictHMDCheckBox.Name = "unstrictHMDCheckBox";
            this.unstrictHMDCheckBox.Size = new System.Drawing.Size(51, 17);
            this.unstrictHMDCheckBox.TabIndex = 20;
            this.unstrictHMDCheckBox.Tag = "unstrict:HMD";
            this.unstrictHMDCheckBox.Text = "HMD";
            this.toolTip.SetToolTip(this.unstrictHMDCheckBox, "Support for non-standard HMD formats\r\nthat use a different version number");
            this.unstrictHMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // unstrictPMDCheckBox
            // 
            this.unstrictPMDCheckBox.AutoSize = true;
            this.unstrictPMDCheckBox.Location = new System.Drawing.Point(196, 19);
            this.unstrictPMDCheckBox.Name = "unstrictPMDCheckBox";
            this.unstrictPMDCheckBox.Size = new System.Drawing.Size(50, 17);
            this.unstrictPMDCheckBox.TabIndex = 21;
            this.unstrictPMDCheckBox.Tag = "unstrict:PMD";
            this.unstrictPMDCheckBox.Text = "PMD";
            this.toolTip.SetToolTip(this.unstrictPMDCheckBox, "Support for non-standard PMD formats\r\nthat use a different version number");
            this.unstrictPMDCheckBox.UseVisualStyleBackColor = true;
            // 
            // unstrictTIMCheckBox
            // 
            this.unstrictTIMCheckBox.AutoSize = true;
            this.unstrictTIMCheckBox.Location = new System.Drawing.Point(250, 19);
            this.unstrictTIMCheckBox.Name = "unstrictTIMCheckBox";
            this.unstrictTIMCheckBox.Size = new System.Drawing.Size(45, 17);
            this.unstrictTIMCheckBox.TabIndex = 22;
            this.unstrictTIMCheckBox.Tag = "unstrict:TIM";
            this.unstrictTIMCheckBox.Text = "TIM";
            this.toolTip.SetToolTip(this.unstrictTIMCheckBox, "Support for non-standard TIM formats\r\nthat use a different version number");
            this.unstrictTIMCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatsGroupBox
            // 
            this.formatsGroupBox.Controls.Add(this.formatPILCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatSPTCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatANCheckBox);
            this.formatsGroupBox.Controls.Add(this.animationsLabel);
            this.formatsGroupBox.Controls.Add(this.texturesLabel);
            this.formatsGroupBox.Controls.Add(this.modelsLabel);
            this.formatsGroupBox.Controls.Add(this.formatBFFCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatPSXCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatMODCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatPMDCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatHMDCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatTODCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatTIMCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatVDFCheckBox);
            this.formatsGroupBox.Controls.Add(this.formatTMDCheckBox);
            this.formatsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.formatsGroupBox.Location = new System.Drawing.Point(0, 133);
            this.formatsGroupBox.Name = "formatsGroupBox";
            this.formatsGroupBox.Size = new System.Drawing.Size(444, 93);
            this.formatsGroupBox.TabIndex = 1;
            this.formatsGroupBox.TabStop = false;
            this.formatsGroupBox.Text = "Formats";
            // 
            // formatANCheckBox
            // 
            this.formatANCheckBox.AutoSize = true;
            this.formatANCheckBox.Location = new System.Drawing.Point(71, 66);
            this.formatANCheckBox.Name = "formatANCheckBox";
            this.formatANCheckBox.Size = new System.Drawing.Size(41, 17);
            this.formatANCheckBox.TabIndex = 9;
            this.formatANCheckBox.Tag = "format:AN";
            this.formatANCheckBox.Text = "AN";
            this.formatANCheckBox.UseVisualStyleBackColor = true;
            // 
            // animationsLabel
            // 
            this.animationsLabel.AutoSize = true;
            this.animationsLabel.Location = new System.Drawing.Point(6, 66);
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
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.optionErrorLoggingCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionDrawAllToVRAMCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionDebugLoggingCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionLogToConsoleCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionLogToFileCheckBox);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 226);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(444, 69);
            this.optionsGroupBox.TabIndex = 2;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // optionErrorLoggingCheckBox
            // 
            this.optionErrorLoggingCheckBox.AutoSize = true;
            this.optionErrorLoggingCheckBox.Enabled = false;
            this.optionErrorLoggingCheckBox.Location = new System.Drawing.Point(12, 42);
            this.optionErrorLoggingCheckBox.Name = "optionErrorLoggingCheckBox";
            this.optionErrorLoggingCheckBox.Size = new System.Drawing.Size(89, 17);
            this.optionErrorLoggingCheckBox.TabIndex = 3;
            this.optionErrorLoggingCheckBox.Text = "Error Logging";
            this.optionErrorLoggingCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionDebugLoggingCheckBox
            // 
            this.optionDebugLoggingCheckBox.AutoSize = true;
            this.optionDebugLoggingCheckBox.Enabled = false;
            this.optionDebugLoggingCheckBox.Location = new System.Drawing.Point(135, 42);
            this.optionDebugLoggingCheckBox.Name = "optionDebugLoggingCheckBox";
            this.optionDebugLoggingCheckBox.Size = new System.Drawing.Size(99, 17);
            this.optionDebugLoggingCheckBox.TabIndex = 4;
            this.optionDebugLoggingCheckBox.Text = "Debug Logging";
            this.optionDebugLoggingCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionLogToConsoleCheckBox
            // 
            this.optionLogToConsoleCheckBox.AutoSize = true;
            this.optionLogToConsoleCheckBox.Location = new System.Drawing.Point(135, 19);
            this.optionLogToConsoleCheckBox.Name = "optionLogToConsoleCheckBox";
            this.optionLogToConsoleCheckBox.Size = new System.Drawing.Size(97, 17);
            this.optionLogToConsoleCheckBox.TabIndex = 1;
            this.optionLogToConsoleCheckBox.Text = "Log to Console";
            this.optionLogToConsoleCheckBox.UseVisualStyleBackColor = true;
            this.optionLogToConsoleCheckBox.CheckedChanged += new System.EventHandler(this.optionLogToConsoleCheckBox_CheckedChanged);
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
            this.optionLogToFileCheckBox.CheckedChanged += new System.EventHandler(this.optionLogToFileCheckBox_CheckedChanged);
            // 
            // showAdvancedMarginPanel
            // 
            this.showAdvancedMarginPanel.AutoSize = true;
            this.showAdvancedMarginPanel.Controls.Add(this.showAdvancedButton);
            this.showAdvancedMarginPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.showAdvancedMarginPanel.Location = new System.Drawing.Point(0, 295);
            this.showAdvancedMarginPanel.Name = "showAdvancedMarginPanel";
            this.showAdvancedMarginPanel.Padding = new System.Windows.Forms.Padding(3);
            this.showAdvancedMarginPanel.Size = new System.Drawing.Size(444, 29);
            this.showAdvancedMarginPanel.TabIndex = 3;
            // 
            // showAdvancedButton
            // 
            this.showAdvancedButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.showAdvancedButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showAdvancedButton.Location = new System.Drawing.Point(3, 3);
            this.showAdvancedButton.Name = "showAdvancedButton";
            this.showAdvancedButton.Size = new System.Drawing.Size(438, 23);
            this.showAdvancedButton.TabIndex = 0;
            this.showAdvancedButton.Text = "Show Advanced";
            this.showAdvancedButton.UseVisualStyleBackColor = true;
            this.showAdvancedButton.Click += new System.EventHandler(this.showAdvancedButton_Click);
            // 
            // advancedOptionsGroupBox
            // 
            this.advancedOptionsGroupBox.Controls.Add(this.unstrictTIMCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.unstrictPMDCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.unstrictHMDCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.unstrictTMDCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.label1);
            this.advancedOptionsGroupBox.Controls.Add(this.isoScanComboBox);
            this.advancedOptionsGroupBox.Controls.Add(this.binScanComboBox);
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorInvalidLabel);
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorSizeUpDown);
            this.advancedOptionsGroupBox.Controls.Add(this.optionAsyncScanCheckBox);
            this.advancedOptionsGroupBox.Controls.Add(this.binSectorStartUpDown);
            this.advancedOptionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.advancedOptionsGroupBox.Location = new System.Drawing.Point(0, 324);
            this.advancedOptionsGroupBox.Name = "advancedOptionsGroupBox";
            this.advancedOptionsGroupBox.Size = new System.Drawing.Size(444, 102);
            this.advancedOptionsGroupBox.TabIndex = 4;
            this.advancedOptionsGroupBox.TabStop = false;
            this.advancedOptionsGroupBox.Text = "Advanced Options";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Ignore Version:";
            // 
            // binSectorInvalidLabel
            // 
            this.binSectorInvalidLabel.AutoSize = true;
            this.binSectorInvalidLabel.ForeColor = System.Drawing.Color.Firebrick;
            this.binSectorInvalidLabel.Location = new System.Drawing.Point(270, 69);
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
            this.binSectorSizeUpDown.Location = new System.Drawing.Point(206, 72);
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
            this.binSectorStartUpDown.Location = new System.Drawing.Point(146, 72);
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
            this.advancedOffsetGroupBox.Location = new System.Drawing.Point(0, 426);
            this.advancedOffsetGroupBox.Name = "advancedOffsetGroupBox";
            this.advancedOffsetGroupBox.Size = new System.Drawing.Size(444, 76);
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
            this.scanButton.Location = new System.Drawing.Point(277, 6);
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
            this.cancelButton.Location = new System.Drawing.Point(358, 6);
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
            this.scanCancelMarginFlowLayoutPanel.Location = new System.Drawing.Point(0, 502);
            this.scanCancelMarginFlowLayoutPanel.Name = "scanCancelMarginFlowLayoutPanel";
            this.scanCancelMarginFlowLayoutPanel.Padding = new System.Windows.Forms.Padding(0, 6, 11, 7);
            this.scanCancelMarginFlowLayoutPanel.Size = new System.Drawing.Size(444, 36);
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
            this.ClientSize = new System.Drawing.Size(444, 564);
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
            this.MinimumSize = new System.Drawing.Size(460, 39);
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
        private System.Windows.Forms.CheckBox formatBFFCheckBox;
        private System.Windows.Forms.CheckBox formatANCheckBox;
        private System.Windows.Forms.CheckBox formatPSXCheckBox;
        private System.Windows.Forms.CheckBox formatMODCheckBox;
        private System.Windows.Forms.CheckBox formatPMDCheckBox;
        private System.Windows.Forms.CheckBox formatHMDCheckBox;
        private System.Windows.Forms.CheckBox formatTODCheckBox;
        private System.Windows.Forms.CheckBox formatTIMCheckBox;
        private System.Windows.Forms.CheckBox formatVDFCheckBox;
        private System.Windows.Forms.CheckBox formatTMDCheckBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.CheckBox optionErrorLoggingCheckBox;
        private System.Windows.Forms.CheckBox optionDrawAllToVRAMCheckBox;
        private System.Windows.Forms.CheckBox optionDebugLoggingCheckBox;
        private System.Windows.Forms.CheckBox optionLogToConsoleCheckBox;
        private System.Windows.Forms.CheckBox optionLogToFileCheckBox;
        private System.Windows.Forms.Panel showAdvancedMarginPanel;
        private System.Windows.Forms.Button showAdvancedButton;
        private System.Windows.Forms.GroupBox advancedOptionsGroupBox;
        private System.Windows.Forms.CheckBox binSectorCheckBox;
        private System.Windows.Forms.NumericUpDown binSectorSizeUpDown;
        private System.Windows.Forms.CheckBox optionAsyncScanCheckBox;
        private System.Windows.Forms.NumericUpDown binSectorStartUpDown;
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
        private System.Windows.Forms.CheckBox formatSPTCheckBox;
        private System.Windows.Forms.ComboBox historyComboBox;
        private System.Windows.Forms.Label historyLabel;
        private System.Windows.Forms.Button historyBookmarkButton;
        private System.Windows.Forms.Button historyRemoveButton;
        private System.Windows.Forms.CheckBox formatPILCheckBox;
        private System.Windows.Forms.ComboBox isoScanComboBox;
        private System.Windows.Forms.ComboBox binScanComboBox;
        private System.Windows.Forms.CheckBox unstrictTIMCheckBox;
        private System.Windows.Forms.CheckBox unstrictPMDCheckBox;
        private System.Windows.Forms.CheckBox unstrictHMDCheckBox;
        private System.Windows.Forms.CheckBox unstrictTMDCheckBox;
        private System.Windows.Forms.Label label1;
    }
}
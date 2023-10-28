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
            this.filePathGroupBox = new System.Windows.Forms.GroupBox();
            this.exportingModelsLabel = new System.Windows.Forms.Label();
            this.selectFolderButton = new System.Windows.Forms.Button();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.formatGroupBox = new System.Windows.Forms.GroupBox();
            this.formatDAERadioButton = new System.Windows.Forms.RadioButton();
            this.formatGLTF2RadioButton = new System.Windows.Forms.RadioButton();
            this.formatPLYRadioButton = new System.Windows.Forms.RadioButton();
            this.formatOBJRadioButton = new System.Windows.Forms.RadioButton();
            this.texturesGroupBox = new System.Windows.Forms.GroupBox();
            this.optionTiledTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.optionShareTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.optionRedrawTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.texturesSingleRadioButton = new System.Windows.Forms.RadioButton();
            this.texturesIndividualRadioButton = new System.Windows.Forms.RadioButton();
            this.texturesOffRadioButton = new System.Windows.Forms.RadioButton();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.optionStrictFloatsCheckBox = new System.Windows.Forms.CheckBox();
            this.optionHumanReadableCheckBox = new System.Windows.Forms.CheckBox();
            this.optionVertexIndexReuseCheckBox = new System.Windows.Forms.CheckBox();
            this.optionModelGroupingComboBox = new System.Windows.Forms.ComboBox();
            this.optionAttachLimbsCheckBox = new System.Windows.Forms.CheckBox();
            this.optionExperimentalVertexColorCheckBox = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.animationsGroupBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkedAnimationsListBox = new System.Windows.Forms.ListBox();
            this.animationsOnRadioButton = new System.Windows.Forms.RadioButton();
            this.animationsOffRadioButton = new System.Windows.Forms.RadioButton();
            this.scanCancelMarginFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.filePathGroupBox.SuspendLayout();
            this.formatGroupBox.SuspendLayout();
            this.texturesGroupBox.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            this.animationsGroupBox.SuspendLayout();
            this.scanCancelMarginFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // filePathGroupBox
            // 
            this.filePathGroupBox.Controls.Add(this.exportingModelsLabel);
            this.filePathGroupBox.Controls.Add(this.selectFolderButton);
            this.filePathGroupBox.Controls.Add(this.filePathTextBox);
            this.filePathGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.filePathGroupBox.Location = new System.Drawing.Point(0, 0);
            this.filePathGroupBox.Name = "filePathGroupBox";
            this.filePathGroupBox.Size = new System.Drawing.Size(394, 77);
            this.filePathGroupBox.TabIndex = 0;
            this.filePathGroupBox.TabStop = false;
            this.filePathGroupBox.Text = "Folder";
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
            // formatGroupBox
            // 
            this.formatGroupBox.Controls.Add(this.formatDAERadioButton);
            this.formatGroupBox.Controls.Add(this.formatGLTF2RadioButton);
            this.formatGroupBox.Controls.Add(this.formatPLYRadioButton);
            this.formatGroupBox.Controls.Add(this.formatOBJRadioButton);
            this.formatGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.formatGroupBox.Location = new System.Drawing.Point(0, 77);
            this.formatGroupBox.Name = "formatGroupBox";
            this.formatGroupBox.Size = new System.Drawing.Size(394, 47);
            this.formatGroupBox.TabIndex = 1;
            this.formatGroupBox.TabStop = false;
            this.formatGroupBox.Text = "Format";
            // 
            // formatDAERadioButton
            // 
            this.formatDAERadioButton.AutoSize = true;
            this.formatDAERadioButton.Location = new System.Drawing.Point(192, 19);
            this.formatDAERadioButton.Name = "formatDAERadioButton";
            this.formatDAERadioButton.Size = new System.Drawing.Size(47, 17);
            this.formatDAERadioButton.TabIndex = 3;
            this.formatDAERadioButton.Tag = "DAE";
            this.formatDAERadioButton.Text = "DAE";
            this.toolTip.SetToolTip(this.formatDAERadioButton, "COLLADA Digital Asset Exchange Format");
            this.formatDAERadioButton.UseVisualStyleBackColor = true;
            this.formatDAERadioButton.CheckedChanged += new System.EventHandler(this.formatRadioButtons_CheckedChanged);
            // 
            // formatGLTF2RadioButton
            // 
            this.formatGLTF2RadioButton.AutoSize = true;
            this.formatGLTF2RadioButton.Location = new System.Drawing.Point(132, 19);
            this.formatGLTF2RadioButton.Name = "formatGLTF2RadioButton";
            this.formatGLTF2RadioButton.Size = new System.Drawing.Size(52, 17);
            this.formatGLTF2RadioButton.TabIndex = 2;
            this.formatGLTF2RadioButton.Tag = "glTF2";
            this.formatGLTF2RadioButton.Text = "glTF2";
            this.toolTip.SetToolTip(this.formatGLTF2RadioButton, "Khronos glTF2 Format");
            this.formatGLTF2RadioButton.UseVisualStyleBackColor = true;
            this.formatGLTF2RadioButton.CheckedChanged += new System.EventHandler(this.formatRadioButtons_CheckedChanged);
            // 
            // formatPLYRadioButton
            // 
            this.formatPLYRadioButton.AutoSize = true;
            this.formatPLYRadioButton.Location = new System.Drawing.Point(72, 19);
            this.formatPLYRadioButton.Name = "formatPLYRadioButton";
            this.formatPLYRadioButton.Size = new System.Drawing.Size(45, 17);
            this.formatPLYRadioButton.TabIndex = 1;
            this.formatPLYRadioButton.Tag = "PLY";
            this.formatPLYRadioButton.Text = "PLY";
            this.toolTip.SetToolTip(this.formatPLYRadioButton, "Stanford Polygon / Triangle Format");
            this.formatPLYRadioButton.UseVisualStyleBackColor = true;
            this.formatPLYRadioButton.CheckedChanged += new System.EventHandler(this.formatRadioButtons_CheckedChanged);
            // 
            // formatOBJRadioButton
            // 
            this.formatOBJRadioButton.AutoSize = true;
            this.formatOBJRadioButton.Checked = true;
            this.formatOBJRadioButton.Location = new System.Drawing.Point(12, 19);
            this.formatOBJRadioButton.Name = "formatOBJRadioButton";
            this.formatOBJRadioButton.Size = new System.Drawing.Size(45, 17);
            this.formatOBJRadioButton.TabIndex = 0;
            this.formatOBJRadioButton.TabStop = true;
            this.formatOBJRadioButton.Tag = "OBJ";
            this.formatOBJRadioButton.Text = "OBJ";
            this.toolTip.SetToolTip(this.formatOBJRadioButton, "Wavefront OBJ Format");
            this.formatOBJRadioButton.UseVisualStyleBackColor = true;
            this.formatOBJRadioButton.CheckedChanged += new System.EventHandler(this.formatRadioButtons_CheckedChanged);
            // 
            // texturesGroupBox
            // 
            this.texturesGroupBox.Controls.Add(this.optionTiledTexturesCheckBox);
            this.texturesGroupBox.Controls.Add(this.optionShareTexturesCheckBox);
            this.texturesGroupBox.Controls.Add(this.optionRedrawTexturesCheckBox);
            this.texturesGroupBox.Controls.Add(this.texturesSingleRadioButton);
            this.texturesGroupBox.Controls.Add(this.texturesIndividualRadioButton);
            this.texturesGroupBox.Controls.Add(this.texturesOffRadioButton);
            this.texturesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.texturesGroupBox.Location = new System.Drawing.Point(0, 124);
            this.texturesGroupBox.Name = "texturesGroupBox";
            this.texturesGroupBox.Size = new System.Drawing.Size(394, 80);
            this.texturesGroupBox.TabIndex = 2;
            this.texturesGroupBox.TabStop = false;
            this.texturesGroupBox.Text = "Textures";
            // 
            // optionTiledTexturesCheckBox
            // 
            this.optionTiledTexturesCheckBox.AutoSize = true;
            this.optionTiledTexturesCheckBox.Checked = true;
            this.optionTiledTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionTiledTexturesCheckBox.Location = new System.Drawing.Point(269, 53);
            this.optionTiledTexturesCheckBox.Name = "optionTiledTexturesCheckBox";
            this.optionTiledTexturesCheckBox.Size = new System.Drawing.Size(82, 17);
            this.optionTiledTexturesCheckBox.TabIndex = 5;
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
            this.optionShareTexturesCheckBox.TabIndex = 4;
            this.optionShareTexturesCheckBox.Text = "Share Between Models";
            this.toolTip.SetToolTip(this.optionShareTexturesCheckBox, "All exported models will reference the same\r\nexported texture files");
            this.optionShareTexturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionRedrawTexturesCheckBox
            // 
            this.optionRedrawTexturesCheckBox.AutoSize = true;
            this.optionRedrawTexturesCheckBox.Location = new System.Drawing.Point(12, 53);
            this.optionRedrawTexturesCheckBox.Name = "optionRedrawTexturesCheckBox";
            this.optionRedrawTexturesCheckBox.Size = new System.Drawing.Size(109, 17);
            this.optionRedrawTexturesCheckBox.TabIndex = 3;
            this.optionRedrawTexturesCheckBox.Text = "Redraw to VRAM";
            this.toolTip.SetToolTip(this.optionRedrawTexturesCheckBox, "Models with associated textures will draw these\r\ntextures to the VRAM pages befor" +
        "e exporting");
            this.optionRedrawTexturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // texturesSingleRadioButton
            // 
            this.texturesSingleRadioButton.AutoSize = true;
            this.texturesSingleRadioButton.Location = new System.Drawing.Point(246, 19);
            this.texturesSingleRadioButton.Name = "texturesSingleRadioButton";
            this.texturesSingleRadioButton.Size = new System.Drawing.Size(93, 17);
            this.texturesSingleRadioButton.TabIndex = 2;
            this.texturesSingleRadioButton.Text = "Single Texture";
            this.toolTip.SetToolTip(this.texturesSingleRadioButton, "Required textures pages (and optionally tiled textures)\r\nwill be combined into a " +
        "single file");
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
            this.texturesIndividualRadioButton.TabIndex = 1;
            this.texturesIndividualRadioButton.TabStop = true;
            this.texturesIndividualRadioButton.Text = "Individual Textures";
            this.toolTip.SetToolTip(this.texturesIndividualRadioButton, "Required texture pages (and optionally tiled textures)\r\nwill be exported as indiv" +
        "idual files");
            this.texturesIndividualRadioButton.UseVisualStyleBackColor = true;
            this.texturesIndividualRadioButton.CheckedChanged += new System.EventHandler(this.texturesRadioButtons_CheckedChanged);
            // 
            // texturesOffRadioButton
            // 
            this.texturesOffRadioButton.AutoSize = true;
            this.texturesOffRadioButton.Location = new System.Drawing.Point(12, 19);
            this.texturesOffRadioButton.Name = "texturesOffRadioButton";
            this.texturesOffRadioButton.Size = new System.Drawing.Size(83, 17);
            this.texturesOffRadioButton.TabIndex = 0;
            this.texturesOffRadioButton.Text = "Don\'t Export";
            this.texturesOffRadioButton.UseVisualStyleBackColor = true;
            this.texturesOffRadioButton.CheckedChanged += new System.EventHandler(this.texturesRadioButtons_CheckedChanged);
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.optionStrictFloatsCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionHumanReadableCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionVertexIndexReuseCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionModelGroupingComboBox);
            this.optionsGroupBox.Controls.Add(this.optionAttachLimbsCheckBox);
            this.optionsGroupBox.Controls.Add(this.optionExperimentalVertexColorCheckBox);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 204);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(394, 75);
            this.optionsGroupBox.TabIndex = 3;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // optionStrictFloatsCheckBox
            // 
            this.optionStrictFloatsCheckBox.AutoSize = true;
            this.optionStrictFloatsCheckBox.Location = new System.Drawing.Point(269, 48);
            this.optionStrictFloatsCheckBox.Name = "optionStrictFloatsCheckBox";
            this.optionStrictFloatsCheckBox.Size = new System.Drawing.Size(81, 17);
            this.optionStrictFloatsCheckBox.TabIndex = 6;
            this.optionStrictFloatsCheckBox.Text = "Strict Floats";
            this.toolTip.SetToolTip(this.optionStrictFloatsCheckBox, "Increases file size and reduces precision of vertices. However,\r\nmany format read" +
        "ers only handle very specific formats for text\r\nnumbers, so disabling this may c" +
        "ause problems.");
            this.optionStrictFloatsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionHumanReadableCheckBox
            // 
            this.optionHumanReadableCheckBox.AutoSize = true;
            this.optionHumanReadableCheckBox.Location = new System.Drawing.Point(143, 48);
            this.optionHumanReadableCheckBox.Name = "optionHumanReadableCheckBox";
            this.optionHumanReadableCheckBox.Size = new System.Drawing.Size(109, 17);
            this.optionHumanReadableCheckBox.TabIndex = 5;
            this.optionHumanReadableCheckBox.Text = "Human Readable";
            this.toolTip.SetToolTip(this.optionHumanReadableCheckBox, "Increases file size, but outputs text based formats in human-readable\r\nform (with" +
        " indentation and line breaks).");
            this.optionHumanReadableCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionVertexIndexReuseCheckBox
            // 
            this.optionVertexIndexReuseCheckBox.AutoSize = true;
            this.optionVertexIndexReuseCheckBox.Location = new System.Drawing.Point(12, 48);
            this.optionVertexIndexReuseCheckBox.Name = "optionVertexIndexReuseCheckBox";
            this.optionVertexIndexReuseCheckBox.Size = new System.Drawing.Size(119, 17);
            this.optionVertexIndexReuseCheckBox.TabIndex = 4;
            this.optionVertexIndexReuseCheckBox.Text = "Vertex Index Reuse";
            this.toolTip.SetToolTip(this.optionVertexIndexReuseCheckBox, "Reduces file size by attempting to reuse vertices with\r\nidentical attributes. May" +
        " increase export time.");
            this.optionVertexIndexReuseCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionModelGroupingComboBox
            // 
            this.optionModelGroupingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.optionModelGroupingComboBox.FormattingEnabled = true;
            this.optionModelGroupingComboBox.Items.AddRange(new object[] {
            "Group by Model",
            "Split by TMD ID",
            "Group All"});
            this.optionModelGroupingComboBox.Location = new System.Drawing.Point(12, 19);
            this.optionModelGroupingComboBox.Name = "optionModelGroupingComboBox";
            this.optionModelGroupingComboBox.Size = new System.Drawing.Size(115, 21);
            this.optionModelGroupingComboBox.TabIndex = 3;
            this.toolTip.SetToolTip(this.optionModelGroupingComboBox, "Choose how models are split up or grouped together.");
            // 
            // optionAttachLimbsCheckBox
            // 
            this.optionAttachLimbsCheckBox.AutoSize = true;
            this.optionAttachLimbsCheckBox.Checked = true;
            this.optionAttachLimbsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionAttachLimbsCheckBox.Location = new System.Drawing.Point(143, 21);
            this.optionAttachLimbsCheckBox.Name = "optionAttachLimbsCheckBox";
            this.optionAttachLimbsCheckBox.Size = new System.Drawing.Size(87, 17);
            this.optionAttachLimbsCheckBox.TabIndex = 1;
            this.optionAttachLimbsCheckBox.Text = "Attach Limbs";
            this.optionAttachLimbsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionExperimentalVertexColorCheckBox
            // 
            this.optionExperimentalVertexColorCheckBox.AutoSize = true;
            this.optionExperimentalVertexColorCheckBox.Checked = true;
            this.optionExperimentalVertexColorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionExperimentalVertexColorCheckBox.Location = new System.Drawing.Point(269, 21);
            this.optionExperimentalVertexColorCheckBox.Name = "optionExperimentalVertexColorCheckBox";
            this.optionExperimentalVertexColorCheckBox.Size = new System.Drawing.Size(106, 17);
            this.optionExperimentalVertexColorCheckBox.TabIndex = 2;
            this.optionExperimentalVertexColorCheckBox.Text = "OBJ Vertex Color";
            this.toolTip.SetToolTip(this.optionExperimentalVertexColorCheckBox, "Experimental feature supported by certain OBJ readers.");
            this.optionExperimentalVertexColorCheckBox.UseVisualStyleBackColor = true;
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 10000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // animationsGroupBox
            // 
            this.animationsGroupBox.Controls.Add(this.label2);
            this.animationsGroupBox.Controls.Add(this.label1);
            this.animationsGroupBox.Controls.Add(this.checkedAnimationsListBox);
            this.animationsGroupBox.Controls.Add(this.animationsOnRadioButton);
            this.animationsGroupBox.Controls.Add(this.animationsOffRadioButton);
            this.animationsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.animationsGroupBox.Location = new System.Drawing.Point(0, 279);
            this.animationsGroupBox.Name = "animationsGroupBox";
            this.animationsGroupBox.Size = new System.Drawing.Size(394, 186);
            this.animationsGroupBox.TabIndex = 4;
            this.animationsGroupBox.TabStop = false;
            this.animationsGroupBox.Text = "Animations";
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
            this.checkedAnimationsListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.checkedAnimationsListBox.Size = new System.Drawing.Size(370, 69);
            this.checkedAnimationsListBox.TabIndex = 2;
            this.checkedAnimationsListBox.TabStop = false;
            // 
            // animationsOnRadioButton
            // 
            this.animationsOnRadioButton.AutoSize = true;
            this.animationsOnRadioButton.Location = new System.Drawing.Point(101, 19);
            this.animationsOnRadioButton.Name = "animationsOnRadioButton";
            this.animationsOnRadioButton.Size = new System.Drawing.Size(101, 17);
            this.animationsOnRadioButton.TabIndex = 1;
            this.animationsOnRadioButton.Text = "Export Checked";
            this.animationsOnRadioButton.UseVisualStyleBackColor = true;
            this.animationsOnRadioButton.CheckedChanged += new System.EventHandler(this.animationsRadioButtons_CheckedChanged);
            // 
            // animationsOffRadioButton
            // 
            this.animationsOffRadioButton.AutoSize = true;
            this.animationsOffRadioButton.Checked = true;
            this.animationsOffRadioButton.Location = new System.Drawing.Point(12, 19);
            this.animationsOffRadioButton.Name = "animationsOffRadioButton";
            this.animationsOffRadioButton.Size = new System.Drawing.Size(83, 17);
            this.animationsOffRadioButton.TabIndex = 0;
            this.animationsOffRadioButton.TabStop = true;
            this.animationsOffRadioButton.Text = "Don\'t Export";
            this.animationsOffRadioButton.UseVisualStyleBackColor = true;
            this.animationsOffRadioButton.CheckedChanged += new System.EventHandler(this.animationsRadioButtons_CheckedChanged);
            // 
            // scanCancelMarginFlowLayoutPanel
            // 
            this.scanCancelMarginFlowLayoutPanel.Controls.Add(this.cancelButton);
            this.scanCancelMarginFlowLayoutPanel.Controls.Add(this.exportButton);
            this.scanCancelMarginFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.scanCancelMarginFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.scanCancelMarginFlowLayoutPanel.Location = new System.Drawing.Point(0, 465);
            this.scanCancelMarginFlowLayoutPanel.Name = "scanCancelMarginFlowLayoutPanel";
            this.scanCancelMarginFlowLayoutPanel.Padding = new System.Windows.Forms.Padding(0, 6, 11, 7);
            this.scanCancelMarginFlowLayoutPanel.Size = new System.Drawing.Size(394, 36);
            this.scanCancelMarginFlowLayoutPanel.TabIndex = 7;
            this.scanCancelMarginFlowLayoutPanel.WrapContents = false;
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
            // exportButton
            // 
            this.exportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.exportButton.Enabled = false;
            this.exportButton.Location = new System.Drawing.Point(227, 6);
            this.exportButton.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 0;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            // 
            // ExportModelsForm
            // 
            this.AcceptButton = this.exportButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(394, 511);
            this.Controls.Add(this.scanCancelMarginFlowLayoutPanel);
            this.Controls.Add(this.animationsGroupBox);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.texturesGroupBox);
            this.Controls.Add(this.formatGroupBox);
            this.Controls.Add(this.filePathGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(410, 39);
            this.Name = "ExportModelsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PSXPrev Export Models";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExportModelsForm_FormClosed);
            this.Load += new System.EventHandler(this.ExportModelsForm_Load);
            this.filePathGroupBox.ResumeLayout(false);
            this.filePathGroupBox.PerformLayout();
            this.formatGroupBox.ResumeLayout(false);
            this.formatGroupBox.PerformLayout();
            this.texturesGroupBox.ResumeLayout(false);
            this.texturesGroupBox.PerformLayout();
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.animationsGroupBox.ResumeLayout(false);
            this.animationsGroupBox.PerformLayout();
            this.scanCancelMarginFlowLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox filePathGroupBox;
        private System.Windows.Forms.Button selectFolderButton;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.GroupBox formatGroupBox;
        private System.Windows.Forms.RadioButton formatPLYRadioButton;
        private System.Windows.Forms.RadioButton formatOBJRadioButton;
        private System.Windows.Forms.GroupBox texturesGroupBox;
        private System.Windows.Forms.RadioButton texturesSingleRadioButton;
        private System.Windows.Forms.RadioButton texturesIndividualRadioButton;
        private System.Windows.Forms.RadioButton texturesOffRadioButton;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.CheckBox optionRedrawTexturesCheckBox;
        private System.Windows.Forms.CheckBox optionExperimentalVertexColorCheckBox;
        private System.Windows.Forms.CheckBox optionAttachLimbsCheckBox;
        private System.Windows.Forms.Label exportingModelsLabel;
        private System.Windows.Forms.CheckBox optionShareTexturesCheckBox;
        private System.Windows.Forms.CheckBox optionTiledTexturesCheckBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.RadioButton formatGLTF2RadioButton;
        private System.Windows.Forms.GroupBox animationsGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox checkedAnimationsListBox;
        private System.Windows.Forms.RadioButton animationsOnRadioButton;
        private System.Windows.Forms.RadioButton animationsOffRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton formatDAERadioButton;
        private System.Windows.Forms.ComboBox optionModelGroupingComboBox;
        private System.Windows.Forms.CheckBox optionHumanReadableCheckBox;
        private System.Windows.Forms.CheckBox optionVertexIndexReuseCheckBox;
        private System.Windows.Forms.FlowLayoutPanel scanCancelMarginFlowLayoutPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.CheckBox optionStrictFloatsCheckBox;
    }
}
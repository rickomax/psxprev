using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using PSXPrev.Forms.Utils;

namespace PSXPrev.Forms
{
    public partial class ScannerForm : Form
    {
        private bool _showAdvanved;

        public ScannerForm()
        {
            InitializeComponent();

            DoubleBuffered = true;

            // Add events that are not browsable in the designer.
            binSectorStartUpDown.TextChanged += binSectorStartSizeUpDown_ValueChanged;
            binSectorSizeUpDown.TextChanged  += binSectorStartSizeUpDown_ValueChanged;
        }

        private void ScannerForm_Load(object sender, EventArgs e)
        {
            if (Program.HasEntityResults)
            {
                // This setting needs to be preserved, since it changes how entities are loaded, and exported.
                // This really should be changed in the future though, so that it only changes renderer behavior.
                optionOldUVAlignmentCheckBox.Checked = !Program.FixUVAlignment;
                optionOldUVAlignmentCheckBox.Enabled = false;
            }

            ReadSettings(Settings.Instance, Settings.Instance.ScanOptions);
        }

        private void SetShowAdvanced(bool show)
        {
            SuspendLayout();
            showAdvancedButton.Text = (show ? "Hide Advanced" : "Show Advanced");// + (show ? " \u25B2" : " \u25BC");
            advancedOptionsGroupBox.Visible = show;
            advancedOffsetGroupBox.Visible = show;
            ResumeLayout();

            _showAdvanved = show;
        }

        private void showAdvancedButton_Click(object sender, EventArgs e)
        {
            SetShowAdvanced(!_showAdvanved);
        }

        private void selectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new CommonOpenFileDialog())
            {
                folderBrowserDialog.IsFolderPicker = true;
                folderBrowserDialog.Title = "Select a Folder to Scan";
                // Parameter name used to avoid overload resolution with WPF Window, which we don't have a reference to.
                if (folderBrowserDialog.ShowDialog(ownerWindowHandle: Handle) == CommonFileDialogResult.Ok)
                {
                    filePathTextBox.Text = folderBrowserDialog.FileName;
                }
            }
        }

        private void selectFileButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Everything (*.*)|*.*";
                openFileDialog.Title = "Select a File to Scan";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    filePathTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void selectBINButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Raw PS1 Bin Files (*.bin)|*.bin";
                openFileDialog.Title = "Select a BIN to Scan";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    filePathTextBox.Text = openFileDialog.FileName;
                    binContentsCheckBox.Checked = true;
                }
            }
        }

        private void selectISOButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Iso Files (*.iso)|*.iso";
                openFileDialog.Title = "Select an ISO to Scan";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    filePathTextBox.Text = openFileDialog.FileName;
                    isoContentsCheckBox.Checked = true;
                }
            }
        }

        private void filePathTextBox_TextChanged(object sender, EventArgs e)
        {
            scanButton.Enabled = File.Exists(filePathTextBox.Text) || Directory.Exists(filePathTextBox.Text);
        }

        private void binContentsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            binSectorCheckBox.Enabled = binContentsCheckBox.Checked;
            // Update if up/downs are enabled
            binSectorCheckBox_CheckedChanged(null, null);
        }

        private void binSectorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            binSectorStartUpDown.Enabled = binContentsCheckBox.Checked && binSectorCheckBox.Checked;
            binSectorSizeUpDown.Enabled  = binContentsCheckBox.Checked && binSectorCheckBox.Checked;
            // Update if invalid message is visible
            binSectorStartSizeUpDown_ValueChanged(null, null);
        }

        private void binSectorStartSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            //var userStart = (int)binSectorStartUpDown.Value;
            //var userSize  = (int)binSectorSizeUpDown.Value;
            // Parse text to show invalid message while typing
            var startText = binSectorStartUpDown.Text;
            var sizeText  = binSectorSizeUpDown.Text;
            if (int.TryParse(startText, out var userStart) && int.TryParse(sizeText, out var userSize))
            {
                var invalid = (userStart + userSize > Common.Parsers.BinCDStream.SectorRawSize);
                binSectorInvalidLabel.Visible = binContentsCheckBox.Checked && binSectorCheckBox.Checked && invalid;
            }
        }

        private void offsetStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            offsetStartUpDown.Enabled = offsetStartCheckBox.Checked;
        }

        private void offsetStopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            offsetStopUpDown.Enabled = !offsetStartOnlyCheckBox.Checked && offsetStopCheckBox.Checked;
        }

        private void offsetStartOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            offsetStopCheckBox.Enabled = !offsetStartOnlyCheckBox.Checked;
            // Update if up/down is enabled
            offsetStopCheckBox_CheckedChanged(null, null);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                // Add sane enter key behavior to numeric up/downs.
                var numericUpDown = this.GetFocusedControlOfType<NumericUpDown>();
                if (numericUpDown != null)
                {
                    // Swap focus to validate input (calling numericUpDown.Validate doesn't do anything).
                    Focus();
                    numericUpDown.Focus();
                    return true; // Enter key handled
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ScannerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // todo: Should we always save settings?
            //WriteSettings(Settings.Instance, null); // For now, at least save UI settings (Show Advanced).
            var options = CreateOptions();
            WriteSettings(Settings.Instance, options);
            Settings.Instance.Save();

            if (DialogResult != DialogResult.OK)
            {
                return;
            }

            //var options = CreateOptions();
            //WriteSettings(Settings.Instance, options);
            //Settings.Instance.Save();

            if (!Program.ScanAsync(options))
            {
                MessageBox.Show(this, $"Directory/File not found: {options.Path}", "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel; // Change dialog result so that Show returns false.
            }
        }

        private ScanOptions CreateOptions()
        {
            var options = new ScanOptions
            {
                // These settings are only present for loading and saving purposes.
                Path = filePathTextBox.Text,
                Filter = filterTextBox.Text,

                CheckAN = checkANCheckBox.Checked,
                CheckBFF = checkBFFCheckBox.Checked,
                CheckMOD = checkMODCheckBox.Checked,
                CheckHMD = checkHMDCheckBox.Checked,
                CheckPMD = checkPMDCheckBox.Checked,
                CheckPSX = checkPSXCheckBox.Checked,
                CheckTIM = checkTIMCheckBox.Checked,
                CheckTMD = checkTMDCheckBox.Checked,
                CheckTOD = checkTODCheckBox.Checked,
                CheckVDF = checkVDFCheckBox.Checked,

                IgnoreHMDVersion = optionIgnoreHMDVersionCheckBox.Checked,
                IgnoreTIMVersion = optionIgnoreTIMVersionCheckBox.Checked,
                IgnoreTMDVersion = optionIgnoreTMDVersionCheckBox.Checked,

                Alignment = (long)offsetAlignUpDown.Value,
                StartOffsetHasValue = offsetStartCheckBox.Checked,
                StopOffsetHasValue  = offsetStopCheckBox.Checked,
                StartOffsetValue = (long)offsetStartUpDown.Value,
                StopOffsetValue  = (long)offsetStopUpDown.Value,
                StartOffsetOnly = offsetStartOnlyCheckBox.Checked,
                NextOffset = offsetNextCheckBox.Checked,

                AsyncFileScan = optionAsyncScanCheckBox.Checked,
                //TopDownFileSearch = true, // Not that important, don't add to reduce UI clutter
                ReadISOContents = isoContentsCheckBox.Checked,
                ReadBINContents = binContentsCheckBox.Checked,
                //BINAlignToSector = false, // Just enter size into Align, don't add to reduce UI clutter
                BINSectorUserStartSizeHasValue = binSectorCheckBox.Checked,
                BINSectorUserStartValue = (int)binSectorStartUpDown.Value,
                BINSectorUserSizeValue  = (int)binSectorSizeUpDown.Value,

                LogToFile = optionLogToFileCheckBox.Checked,
                LogToConsole = !optionNoVerboseCheckBox.Checked,
                //UseConsoleColor = true, // Not that important, don't add to reduce UI clutter
                DebugLogging = optionDebugCheckBox.Checked,
                ErrorLogging = optionShowErrorsCheckBox.Checked,

                DrawAllToVRAM = optionDrawAllToVRAMCheckBox.Checked,
                FixUVAlignment = !optionOldUVAlignmentCheckBox.Checked,
            };

            return options;
        }

        private void ReadSettings(Settings settings, ScanOptions options)
        {
            if (options == null)
            {
                options = new ScanOptions();
            }

            filePathTextBox.Text = options.Path ?? string.Empty;
            filterTextBox.Text = options.Filter ?? ScanOptions.DefaultFilter;

            checkANCheckBox.Checked = options.CheckAN;
            checkBFFCheckBox.Checked = options.CheckBFF;
            checkHMDCheckBox.Checked = options.CheckHMD;
            checkMODCheckBox.Checked = options.CheckMOD;
            checkPMDCheckBox.Checked = options.CheckPMD;
            checkPSXCheckBox.Checked = options.CheckPSX;
            checkTIMCheckBox.Checked = options.CheckTIM;
            checkTMDCheckBox.Checked = options.CheckTMD;
            checkTODCheckBox.Checked = options.CheckTOD;
            checkVDFCheckBox.Checked = options.CheckVDF;

            optionIgnoreHMDVersionCheckBox.Checked = options.IgnoreHMDVersion;
            optionIgnoreTIMVersionCheckBox.Checked = options.IgnoreTIMVersion;
            optionIgnoreTMDVersionCheckBox.Checked = options.IgnoreTMDVersion;

            offsetAlignUpDown.SetValueSafe(options.Alignment);
            offsetStartCheckBox.Checked = options.StartOffsetHasValue;
            offsetStopCheckBox.Checked  = options.StopOffsetHasValue;
            offsetStartUpDown.SetValueSafe(options.StartOffsetValue);
            offsetStopUpDown.SetValueSafe(options.StopOffsetValue);
            offsetStartOnlyCheckBox.Checked = options.StartOffsetOnly;
            offsetNextCheckBox.Checked = options.NextOffset;

            optionAsyncScanCheckBox.Checked = options.AsyncFileScan;
            isoContentsCheckBox.Checked = options.ReadISOContents;
            binContentsCheckBox.Checked = options.ReadBINContents;
            binSectorCheckBox.Checked = options.BINSectorUserStartSizeHasValue;
            binSectorStartUpDown.SetValueSafe(options.BINSectorUserStartValue);
            binSectorSizeUpDown.SetValueSafe(options.BINSectorUserSizeValue);

            optionLogToFileCheckBox.Checked = options.LogToFile;
            optionNoVerboseCheckBox.Checked = !options.LogToConsole;
            optionDebugCheckBox.Checked = options.DebugLogging;
            optionShowErrorsCheckBox.Checked = options.ErrorLogging;

            optionDrawAllToVRAMCheckBox.Checked = options.DrawAllToVRAM;
            optionOldUVAlignmentCheckBox.Checked = !options.FixUVAlignment;

            SetShowAdvanced(settings.ShowAdvancedScanOptions);
        }

        private void WriteSettings(Settings settings, ScanOptions options)
        {
            if (options != null)
            {
                settings.ScanOptions = options.Clone();
            }
            settings.ShowAdvancedScanOptions = _showAdvanved;
        }


        public static bool Show(IWin32Window owner)
        {
            using (var form = new ScannerForm())
            {
                return form.ShowDialog(owner) == DialogResult.OK;
            }
        }
    }
}

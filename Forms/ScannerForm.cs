using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using PSXPrev.Forms.Utils;

namespace PSXPrev.Forms
{
    public partial class ScannerForm : Form
    {
        private bool _showAdvanved;
        private string _wildcardFilter = ScanOptions.DefaultFilter;
        private string _regexFilter = ScanOptions.DefaultRegexPattern;
        private string _regexFilterError;
        private Color _originalFilterForeColor;
        private string _originalFilterToolTip;

        public ScanOptions Options { get; private set; }

        public ScannerForm()
        {
            InitializeComponent();

            DoubleBuffered = true;

            _originalFilterForeColor = filterTextBox.ForeColor;
            _originalFilterToolTip = toolTip.GetToolTip(filterTextBox);

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

        private void ValidateCanScan()
        {
            var pathExists = File.Exists(filePathTextBox.Text) || Directory.Exists(filePathTextBox.Text);
            var validRegex = !filterUseRegexCheckBox.Checked || _regexFilterError == null;
            scanButton.Enabled = pathExists && validRegex;
        }

        private void ValidateFilter()
        {
            if (filterUseRegexCheckBox.Checked)
            {
                try
                {
                    var pattern = filterTextBox.Text;
                    if (string.IsNullOrWhiteSpace(pattern))
                    {
                        pattern = ScanOptions.DefaultRegexPattern;
                    }
                    new Regex(pattern, RegexOptions.IgnoreCase);
                    _regexFilterError = null;
                }
                catch (Exception exp)
                {
                    _regexFilterError = $"Error {exp.Message}"; // Message starts as "parsing ...", so prefix with "Error "
                    filterTextBox.ForeColor = Color.Red;
                    toolTip.SetToolTip(filterTextBox, _regexFilterError);
                    return;
                }
            }
            filterTextBox.ForeColor = _originalFilterForeColor;
            toolTip.SetToolTip(filterTextBox, _originalFilterToolTip);
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
            ValidateCanScan();
        }

        private void filterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (filterUseRegexCheckBox.Checked)
            {
                _regexFilter = filterTextBox.Text;
                ValidateFilter();
                ValidateCanScan();
            }
            else
            {
                _wildcardFilter = filterTextBox.Text;
            }
        }

        private void filterTextBox_MouseEnter(object sender, EventArgs e)
        {
            // Prevent WinForms moment where tooltip sometimes doesn't want to show over textboxes...
            toolTip.Active = false;
            toolTip.Active = true;
        }

        private void filterUseRegexCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (filterUseRegexCheckBox.Checked)
            {
                filterTextBox.Text = _regexFilter;
            }
            else
            {
                filterTextBox.Text = _wildcardFilter;
            }
            ValidateFilter();
            ValidateCanScan();
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

            Options = options;
        }

        private ScanOptions CreateOptions()
        {
            var options = new ScanOptions
            {
                // These settings are only present for loading and saving purposes.
                Path = filePathTextBox.Text,
                WildcardFilter = _wildcardFilter,
                RegexPattern = _regexFilter,
                UseRegex = filterUseRegexCheckBox.Checked,

                CheckAN = checkANCheckBox.Checked,
                CheckBFF = checkBFFCheckBox.Checked,
                CheckMOD = checkMODCheckBox.Checked,
                CheckHMD = checkHMDCheckBox.Checked,
                CheckPMD = checkPMDCheckBox.Checked,
                CheckPSX = checkPSXCheckBox.Checked,
                CheckSPT = checkSPTCheckBox.Checked,
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
                ReadBINContents = false, //todo
                ReadBINSectorData = binContentsCheckBox.Checked,
                BINSectorUserStartSizeHasValue = binSectorCheckBox.Checked,
                BINSectorUserStartValue = (int)binSectorStartUpDown.Value,
                BINSectorUserSizeValue  = (int)binSectorSizeUpDown.Value,

                LogToFile = optionLogToFileCheckBox.Checked,
                LogToConsole = optionNoVerboseCheckBox.Checked,
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
            _wildcardFilter = options.WildcardFilter ?? ScanOptions.EmptyFilter;
            _regexFilter    = options.RegexPattern   ?? ScanOptions.DefaultRegexPattern;
            filterTextBox.Text = !options.UseRegex ? _wildcardFilter : _regexFilter;
            filterUseRegexCheckBox.Checked = options.UseRegex;

            checkANCheckBox.Checked = options.CheckAN;
            checkBFFCheckBox.Checked = options.CheckBFF;
            checkHMDCheckBox.Checked = options.CheckHMD;
            checkMODCheckBox.Checked = options.CheckMOD;
            checkPMDCheckBox.Checked = options.CheckPMD;
            checkPSXCheckBox.Checked = options.CheckPSX;
            checkSPTCheckBox.Checked = options.CheckSPT;
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
            // todo: ReadBINContents
            binContentsCheckBox.Checked = options.ReadBINSectorData;
            binSectorCheckBox.Checked = options.BINSectorUserStartSizeHasValue;
            binSectorStartUpDown.SetValueSafe(options.BINSectorUserStartValue);
            binSectorSizeUpDown.SetValueSafe(options.BINSectorUserSizeValue);

            optionLogToFileCheckBox.Checked = options.LogToFile;
            optionNoVerboseCheckBox.Checked = options.LogToConsole;
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


        public static ScanOptions Show(IWin32Window owner)
        {
            using (var form = new ScannerForm())
            {
                if (form.ShowDialog(owner) == DialogResult.OK)
                {
                    return form.Options;
                }
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using PSXPrev.Forms.Dialogs;
using PSXPrev.Forms.Utils;
using Timer = System.Timers.Timer;

namespace PSXPrev.Forms
{
    public partial class ScannerForm : Form
    {
        private const int HistoryComboBoxDropDownPadding = 5;
        private const int HistoryComboBoxMaxDropDownWidth = 800;

        private bool _loading;
        private bool _showAdvanved;
        private string _wildcardFilter = ScanOptions.DefaultFilter;
        private string _regexFilter = ScanOptions.DefaultRegexPattern;
        private string _regexFilterError;
        private Color _originalFilterForeColor;
        private string _originalFilterToolTip;

        private readonly List<ScanHistoryItem> _historyItems = new List<ScanHistoryItem>();
        private int _selectedHistoryIndex;

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

            // Register all settings controls to update the current selected history
            foreach (var control in this.EnumerateAllControls())
            {
                if (control.Tag is string tagStr && tagStr == "NOTSETTING")
                {
                    continue; // Exclude controls marking themselves as unrelated to settings
                }

                if (control is CheckBox checkBox)
                {
                    checkBox.CheckedChanged += OnSettingsStateChanged;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.SelectedIndexChanged += OnSettingsStateChanged;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.ValueChanged += OnSettingsStateChanged;
                }
                else if (control is TextBox textBox)
                {
                    textBox.TextChanged += OnSettingsStateChanged;
                }
            }
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

        private void UpdateHistoryChanged()
        {
            if (!_loading && _selectedHistoryIndex != 0)
            {
                // Check if the selected history has been modified, if so, switch back to "Current"
                var current = CreateOptions();
                current.IsReadOnly = true;
                if (!_historyItems[_selectedHistoryIndex].Options.Equals(current))
                {
                    _loading = true;
                    historyComboBox.SelectedIndex = _selectedHistoryIndex = 0;
                    _loading = false;
                }
            }
        }

        private void PopulateScanHistory(Settings settings, ScanOptions options)
        {
            var loadingOld = _loading;
            _loading = true;

            _historyItems.Clear();
            historyComboBox.Items.Clear();

            // Add "Current" scan at the top
            var current = options?.Clone() ?? CreateOptions();
            current.IsReadOnly = true; // Mark as ReadOnly so that equality checks can be cached
            var index = 0;
            AddHistoryItem(current, 0);

            // Add bookmarked scans
            index = 1;
            foreach (var history in settings.ScanHistory)
            {
                if (history.IsBookmarked)
                {
                    AddHistoryItem(history, index++);
                }
            }

            // Add recent unbookmarked scans
            index = 1;
            foreach (var history in settings.ScanHistory)
            {
                if (!history.IsBookmarked)
                {
                    AddHistoryItem(history, index++);
                }
            }

            historyComboBox.SelectedIndex = _selectedHistoryIndex = 0;

            historyComboBox.UpdateDynamicDropDownWidth(HistoryComboBoxDropDownPadding, HistoryComboBoxMaxDropDownWidth);

            _loading = loadingOld;
        }

        private void AddHistoryItem(ScanOptions history, int index)
        {
            var historyItem = new ScanHistoryItem
            {
                Index = index,
                Options = history,
            };

            _historyItems.Add(historyItem);
            historyComboBox.Items.Add(historyItem.ToString());
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

        private void OnSettingsStateChanged(object sender, EventArgs e)
        {
            UpdateHistoryChanged();
        }

        private void historyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loading && historyComboBox.SelectedIndex != _selectedHistoryIndex)
            {
                if (_selectedHistoryIndex == 0)
                {
                    // If we're switching from the "Current" history item, then update it
                    var currentHistoryItem = _historyItems[0];
                    currentHistoryItem.Options = CreateOptions();
                    currentHistoryItem.Options.IsReadOnly = true; // Mark as ReadOnly so that equality checks can be cached
                }

                _selectedHistoryIndex = historyComboBox.SelectedIndex;
                var history = _historyItems[_selectedHistoryIndex].Options;
                ReadSettings(null, history);
            }
            else
            {
                _selectedHistoryIndex = historyComboBox.SelectedIndex;
            }
            historyRemoveButton.Enabled = _selectedHistoryIndex != 0;
        }

        private void historyBookmarkButton_Click(object sender, EventArgs e)
        {
            string defaultText = null;
            if (_selectedHistoryIndex != 0)
            {
                defaultText = _historyItems[_selectedHistoryIndex].Options.DisplayName;
            }
            var actionStr  = _selectedHistoryIndex != 0 ? "Rename" : "Enter a name for";
            var displayName = InputDialog.Show(this, $"{actionStr} the bookmark, or leave blank.", "Bookmark Scan History", defaultText);
            if (displayName != null)
            {
                displayName = !string.IsNullOrEmpty(displayName) ? displayName : null;

                int newSelectedIndex;
                ScanOptions history;
                if (_selectedHistoryIndex != 0)
                {
                    newSelectedIndex = -1; // We need to re-locate this item
                    history = _historyItems[_selectedHistoryIndex].Options;
                    history.DisplayName = displayName;
                    history.IsBookmarked = true;
                }
                else
                {
                    newSelectedIndex = 1;
                    history = CreateOptions();
                    history.DisplayName = displayName;
                    history.IsBookmarked = true;
                    Settings.Instance.AddScanHistory(history);
                }

                PopulateScanHistory(Settings.Instance, null);

                var historyNotFound = false;
                if (newSelectedIndex == -1)
                {
                    newSelectedIndex = 0; // Default to 0 on failure, which shouldn't happen
                    historyNotFound = true;
                    for (var i = 0; i < _historyItems.Count; i++)
                    {
                        // Find by exact instance
                        if (object.ReferenceEquals(_historyItems[i].Options, history))
                        {
                            newSelectedIndex = i;
                            historyNotFound = false;
                            break;
                        }
                    }
                }
                // Change to the selected bookmark index, but we don't need to update settings
                // (unless we failed to find the item).
                if (historyNotFound)
                {
                    historyComboBox.SelectedIndex = 0;
                }
                else
                {
                    _loading = !historyNotFound;
                    historyComboBox.SelectedIndex = _selectedHistoryIndex = newSelectedIndex;
                    _loading = false;
                }
            }
        }

        private void historyRemoveButton_Click(object sender, EventArgs e)
        {
            if (_selectedHistoryIndex != 0)
            {
                var result = MessageBox.Show(this, "Are you sure you want to remove this scan history?", "Remove Scan History", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    var history = _historyItems[_selectedHistoryIndex].Options;
                    Settings.Instance.ScanHistory.Remove(history);

                    PopulateScanHistory(Settings.Instance, null);

                    // Update to "Current", we need to update settings.
                    historyComboBox.SelectedIndex = 0;
                }
            }
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
            if (_loading)
            {
                return;
            }
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
            WriteSettings(Settings.Instance, options, (DialogResult == DialogResult.OK));
            if (Settings.ImplicitSave)
            {
                Settings.Instance.Save();
            }

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

            _loading = true;

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

            if (settings != null)
            {
                SetShowAdvanced(settings.ShowAdvancedScanOptions);

                PopulateScanHistory(settings, options);
            }

            ValidateFilter();
            ValidateCanScan();

            _loading = false;
        }

        private void WriteSettings(Settings settings, ScanOptions options, bool finished)
        {
            if (options != null)
            {
                settings.ScanOptions = options.Clone();
            }
            if (settings != null)
            {
                settings.ShowAdvancedScanOptions = _showAdvanved;
                // Only add to history if we start scanning.
                // Don't add to history if *it's bookmarked* ~~we're already using a selected recent history~~.
                var selectedHistory = _historyItems[_selectedHistoryIndex].Options;
                if (finished && options != null && (_selectedHistoryIndex == 0 || !selectedHistory.IsBookmarked))
                {
                    settings.AddScanHistory(options);
                }
            }
        }


        public static new ScanOptions Show(IWin32Window owner)
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


        private class ScanHistoryItem
        {
            public int Index { get; set; }
            public ScanOptions Options { get; set; }
            //public ScanOptions ModifiedOptions { get; set; }
            public string EqualityString { get; set; }
            public bool IsModified { get; set; } // Not supported yet

            public bool IsCurrent => Index == 0;

            public override string ToString()
            {
                if (IsCurrent)
                {
                    return "[Current Scan]";
                }
                else
                {
                    var modifiedStr = IsModified ? "*" : string.Empty;
                    var indexStr = Options.IsBookmarked ? "B" : $"{Index}";
                    var optionsStr = Options.ToString(70);
                    if (string.IsNullOrEmpty(Options.DisplayName))
                    {
                        optionsStr = optionsStr.ToLower();
                    }
                    return $"{modifiedStr}[{indexStr}]  {optionsStr}";
                }
            }
        }
    }
}

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

        private const string FormatTagPrefix = "format:";
        private const string UnstrictTagPrefix = "unstrict:";

        private bool _loading;
        private bool _showAdvanved;
        private string _wildcardFilter = ScanOptions.DefaultFilter;
        private string _regexFilter = ScanOptions.DefaultRegexPattern;
        private string _regexFilterError;
        private Color _originalFilterForeColor;
        private string _originalFilterToolTip;

        private readonly Dictionary<string, CheckBox> _formatCheckBoxes = new Dictionary<string, CheckBox>();
        private readonly Dictionary<string, CheckBox> _unstrictCheckBoxes = new Dictionary<string, CheckBox>();

        private readonly List<ScanHistoryItem> _historyItems = new List<ScanHistoryItem>();
        private int _selectedHistoryIndex;

        public ScanOptions Options { get; private set; }

        public ScannerForm()
        {
            // Note: In order to change form width, MinimumSize must be changed as well in the designer.
            InitializeComponent();

            DoubleBuffered = true;

            _originalFilterForeColor = filterTextBox.ForeColor;
            _originalFilterToolTip = toolTip.GetToolTip(filterTextBox);

            // Set default values for combo boxes (history combo box is setup elsewhere)
            binScanComboBox.SelectedIndex = 0;
            isoScanComboBox.SelectedIndex = 0;

            // Add events that are not browsable in the designer.
            binSectorStartUpDown.TextChanged += binSectorStartSizeUpDown_ValueChanged;
            binSectorSizeUpDown.TextChanged  += binSectorStartSizeUpDown_ValueChanged;

            // Register check boxes that specify what formats to scan
            foreach (var checkBox in this.EnumerateAllControlsOfType<CheckBox>())
            {
                if (checkBox.Tag is string tagStr)
                {
                    if (tagStr.StartsWith(FormatTagPrefix))
                    {
                        var format = tagStr.Substring(FormatTagPrefix.Length);
                        _formatCheckBoxes.Add(format, checkBox);
                        // We want CheckState.Indeterminate to change to Checked, so handle checking ourselves.
                        checkBox.AutoCheck = false;
                        // Note that Click DOES trigger with keyboard input
                        checkBox.Click += OnFormatClick;
                    }
                    else if (tagStr.StartsWith(UnstrictTagPrefix))
                    {
                        var format = tagStr.Substring(UnstrictTagPrefix.Length);
                        _unstrictCheckBoxes.Add(format, checkBox);
                    }
                }
            }

            // Register all settings controls to update the current selected history
            foreach (var control in this.EnumerateAllControls())
            {
                if (control.Tag is string tagStr && tagStr == "NOTSETTING")
                {
                    continue; // Exclude controls marking themselves as unrelated to settings
                }

                if (control is CheckBox checkBox)
                {
                    checkBox.CheckStateChanged += OnSettingsStateChanged;
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

        private void OnFormatClick(object sender, EventArgs e)
        {
            // Prevent triggering setting state changes until we're done updating all format checkboxes
            var loadingOld = _loading;
            _loading = true;

            // We can't just assigned Checked, since that has a property changed guard
            // (and we treat Checked as false when Indeterminate)
            var senderCheckBox = (CheckBox)sender;
            if (senderCheckBox.CheckState == CheckState.Checked)
            {
                // May be changed to Indeterminate later in the function
                senderCheckBox.CheckState = CheckState.Unchecked;
            }
            else
            {
                senderCheckBox.CheckState = CheckState.Checked;
            }

            // Check if we should be using implicit formats (when no formats are checked)
            var isImplicit = true;
            foreach (var formatCheckBox in _formatCheckBoxes.Values)
            {
                if (formatCheckBox.CheckState == CheckState.Checked)
                {
                    isImplicit = false;
                    break;
                }
            }

            // Set or unset implicit format check states
            foreach (var kvp in _formatCheckBoxes)
            {
                var format = kvp.Key;
                var formatCheckBox = kvp.Value;
                if (isImplicit)
                {
                    // Check checkboxes based on their implicit behavior
                    if (ScanFormats.IsImplicit(format))
                    {
                        formatCheckBox.CheckState = CheckState.Indeterminate;
                    }
                    else
                    {
                        formatCheckBox.CheckState = CheckState.Unchecked;
                    }
                }
                else if (formatCheckBox.CheckState == CheckState.Indeterminate)
                {
                    // Uncheck previously-implicit checkboxes
                    formatCheckBox.CheckState = CheckState.Unchecked;
                }

                // Update what unstrict check boxes are enabled
                if (_unstrictCheckBoxes.TryGetValue(format, out var unstrictCheckBox))
                {
                    unstrictCheckBox.Enabled = formatCheckBox.CheckState != CheckState.Unchecked;
                }
            }

            _loading = loadingOld;
            // Trigger setting state change now
            OnSettingsStateChanged(sender, EventArgs.Empty);
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
                    if (binScanComboBox.SelectedIndex == 0)
                    {
                        binScanComboBox.SelectedIndex = 2; // Default to scanning Data, since it will support more games
                    }
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
                    if (isoScanComboBox.SelectedIndex == 0)
                    {
                        isoScanComboBox.SelectedIndex = 1;
                    }
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

        private void optionLogToFileCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoggingChanged();
        }

        private void optionLogToConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoggingChanged();
        }

        private void UpdateLoggingChanged()
        {
            var loggingEnabled = (optionLogToFileCheckBox.Checked || optionLogToConsoleCheckBox.Checked);
            optionErrorLoggingCheckBox.Enabled = loggingEnabled;
            optionDebugLoggingCheckBox.Enabled = loggingEnabled;
        }

        private void binScanComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            binSectorCheckBox.Enabled = binScanComboBox.SelectedIndex != 0;
            // Update if up/downs are enabled
            binSectorCheckBox_CheckedChanged(null, null);
        }

        private void binSectorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            binSectorStartUpDown.Enabled = binScanComboBox.SelectedIndex != 0 && binSectorCheckBox.Checked;
            binSectorSizeUpDown.Enabled  = binScanComboBox.SelectedIndex != 0 && binSectorCheckBox.Checked;
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
                binSectorInvalidLabel.Visible = binScanComboBox.SelectedIndex != 0 && binSectorCheckBox.Checked && invalid;
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

                Alignment = (long)offsetAlignUpDown.Value,
                StartOffsetHasValue = offsetStartCheckBox.Checked,
                StopOffsetHasValue  = offsetStopCheckBox.Checked,
                StartOffsetValue = (long)offsetStartUpDown.Value,
                StopOffsetValue  = (long)offsetStopUpDown.Value,
                StartOffsetOnly = offsetStartOnlyCheckBox.Checked,
                NextOffset = offsetNextCheckBox.Checked,

                AsyncFileScan = optionAsyncScanCheckBox.Checked,
                //TopDownFileSearch = true, // Not that important, don't add to reduce UI clutter
                ReadISOContents = isoScanComboBox.SelectedIndex == 1,
                ReadBINContents = binScanComboBox.SelectedIndex == 1,
                ReadBINSectorData = binScanComboBox.SelectedIndex == 2,
                BINSectorUserStartSizeHasValue = binSectorCheckBox.Checked,
                BINSectorUserStartValue = (int)binSectorStartUpDown.Value,
                BINSectorUserSizeValue  = (int)binSectorSizeUpDown.Value,

                LogToFile = optionLogToFileCheckBox.Checked,
                LogToConsole = optionLogToConsoleCheckBox.Checked,
                DebugLogging = optionDebugLoggingCheckBox.Checked,
                ErrorLogging = optionErrorLoggingCheckBox.Checked,

                DrawAllToVRAM = optionDrawAllToVRAMCheckBox.Checked,
            };

            foreach (var kvp in _formatCheckBoxes)
            {
                var format = kvp.Key;
                var formatCheckBox = kvp.Value;
                if (formatCheckBox.CheckState == CheckState.Checked)
                {
                    // Add format explicitly (implicit is automatically handled otherwise)
                    options.AddFormat(format);
                }
            }

            foreach (var kvp in _unstrictCheckBoxes)
            {
                var format = kvp.Key;
                var unstrictCheckBox = kvp.Value;
                if (unstrictCheckBox.Checked)
                {
                    options.AddUnstrict(format);
                }
            }

            options.Validate();

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
            _wildcardFilter = options.ValidatedWildcardFilter;
            _regexFilter    = options.ValidatedRegexPattern;
            filterTextBox.Text = !options.UseRegex ? _wildcardFilter : _regexFilter;
            filterUseRegexCheckBox.Checked = options.UseRegex;

            foreach (var kvp in _formatCheckBoxes)
            {
                var format = kvp.Key;
                var formatCheckBox = kvp.Value;
                if (options.ContainsFormat(format, out var isImplicit))
                {
                    formatCheckBox.CheckState = isImplicit ? CheckState.Indeterminate : CheckState.Checked;
                }
                else
                {
                    formatCheckBox.CheckState = CheckState.Unchecked;
                }

                // Update what unstrict check boxes are enabled
                if (_unstrictCheckBoxes.TryGetValue(format, out var unstrictCheckBox))
                {
                    unstrictCheckBox.Enabled = formatCheckBox.CheckState != CheckState.Unchecked;
                }
            }

            foreach (var kvp in _unstrictCheckBoxes)
            {
                var format = kvp.Key;
                var unstrictCheckBox = kvp.Value;
                unstrictCheckBox.Checked = options.ContainsUnstrict(format);
            }

            offsetAlignUpDown.SetValueSafe(options.Alignment);
            offsetStartCheckBox.Checked = options.StartOffsetHasValue;
            offsetStopCheckBox.Checked  = options.StopOffsetHasValue;
            offsetStartUpDown.SetValueSafe(options.StartOffsetValue);
            offsetStopUpDown.SetValueSafe(options.StopOffsetValue);
            offsetStartOnlyCheckBox.Checked = options.StartOffsetOnly;
            offsetNextCheckBox.Checked = options.NextOffset;

            optionAsyncScanCheckBox.Checked = options.AsyncFileScan;
            if (options.ReadISOContents)
            {
                isoScanComboBox.SelectedIndex = 1;
            }
            else
            {
                isoScanComboBox.SelectedIndex = 0;
            }
            if (options.ReadBINContents)
            {
                binScanComboBox.SelectedIndex = 1;
            }
            else if (options.ReadBINSectorData)
            {
                binScanComboBox.SelectedIndex = 2;
            }
            else
            {
                binScanComboBox.SelectedIndex = 0;
            }
            binSectorCheckBox.Checked = options.BINSectorUserStartSizeHasValue;
            binSectorStartUpDown.SetValueSafe(options.BINSectorUserStartValue);
            binSectorSizeUpDown.SetValueSafe(options.BINSectorUserSizeValue);

            optionLogToFileCheckBox.Checked = options.LogToFile;
            optionLogToConsoleCheckBox.Checked = options.LogToConsole;
            optionDebugLoggingCheckBox.Checked = options.DebugLogging;
            optionErrorLoggingCheckBox.Checked = options.ErrorLogging;

            optionDrawAllToVRAMCheckBox.Checked = options.DrawAllToVRAM;

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

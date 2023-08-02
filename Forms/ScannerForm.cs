using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PSXPrev.Forms
{
    public partial class ScannerForm : Form
    {
        public ScannerForm()
        {
            InitializeComponent();
        }

        private void ScannerForm_Load(object sender, EventArgs e)
        {
            toolTip.SetToolTip(optionOldUVAlignmentCheckBox, "PSXPrev originally used UV alignment that\nranged from 0-256, however this was incorrect,\nand 0-255 is now used by default.");

            if (Program.HasEntityResults)
            {
                // This setting needs to be preserved, since it changes how entities are loaded, and exported.
                // This really should be changed in the future though, so that it only changes renderer behavior.
                optionOldUVAlignmentCheckBox.Checked = !Program.FixUVAlignment;
                optionOldUVAlignmentCheckBox.Enabled = false;
            }

            ReadSettings(Settings.Instance.ScanOptions);
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

        private void selectISOButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Iso Files (*.iso)|*.iso";
                openFileDialog.Title = "Select an ISO to Scan";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    filePathTextBox.Text = openFileDialog.FileName;
                }
            }
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

        private void filePathTextBox_TextChanged(object sender, EventArgs e)
        {
            scanButton.Enabled = File.Exists(filePathTextBox.Text) || Directory.Exists(filePathTextBox.Text);
        }

        private void ScannerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                return;
            }

            var options = new ScanOptions
            {
                // These settings are only present for loading and saving purposes.
                Path = filePathTextBox.Text,
                Filter = filterTextBox.Text,

                CheckAN = scanANCheckBox.Checked,
                CheckBFF = scanBFFCheckBox.Checked,
                CheckMOD = scanMODCheckBox.Checked,
                CheckHMD = scanHMDCheckBox.Checked,
                CheckPMD = scanPMDCheckBox.Checked,
                CheckPSX = scanPSXCheckBox.Checked,
                CheckTIM = scanTIMCheckBox.Checked,
                CheckTMD = scanTMDCheckBox.Checked,
                CheckTOD = scanTODCheckBox.Checked,
                CheckVDF = scanVDFCheckBox.Checked,

                IgnoreHMDVersion = optionIgnoreHMDVersionCheckBox.Checked,
                IgnoreTIMVersion = optionIgnoreTIMVersionCheckBox.Checked,
                IgnoreTMDVersion = optionIgnoreTMDVersionCheckBox.Checked,

                //todo
                StartOffset = optionNoOffsetCheckBox.Checked ? (long?)0 : null,
                StopOffset = optionNoOffsetCheckBox.Checked ? (long?)1 : null,
                //NextOffset = false, //todo

                //DepthFirstFileSearch = true, //todo
                //AsyncFileScan = true, //todo

                LogToFile = optionLogToFileCheckBox.Checked,
                LogToConsole = !optionNoVerboseCheckBox.Checked,
                Debug = optionDebugCheckBox.Checked,
                ShowErrors = optionShowErrorsCheckBox.Checked,
                //ConsoleColor = true, //todo

                DrawAllToVRAM = optionDrawAllToVRAMCheckBox.Checked,
                FixUVAlignment = !optionOldUVAlignmentCheckBox.Checked,
            };

            WriteSettings(options);

            if (!Program.ScanAsync(options))
            {
                MessageBox.Show(this, $"Directory/File not found: {options.Path}", "Scan Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel; // Change dialog result so that Show returns false.
            }
        }

        private void ReadSettings(ScanOptions options)
        {
            if (options == null)
            {
                options = new ScanOptions();
            }

            filePathTextBox.Text = options.Path ?? string.Empty;
            filterTextBox.Text = options.Filter ?? ScanOptions.DefaultFilter;

            scanANCheckBox.Checked = options.CheckAN;
            scanBFFCheckBox.Checked = options.CheckBFF;
            scanHMDCheckBox.Checked = options.CheckHMD;
            scanMODCheckBox.Checked = options.CheckMOD;
            scanPMDCheckBox.Checked = options.CheckPMD;
            scanPSXCheckBox.Checked = options.CheckPSX;
            scanTIMCheckBox.Checked = options.CheckTIM;
            scanTMDCheckBox.Checked = options.CheckTMD;
            scanTODCheckBox.Checked = options.CheckTOD;
            scanVDFCheckBox.Checked = options.CheckVDF;

            optionIgnoreHMDVersionCheckBox.Checked = options.IgnoreHMDVersion;
            optionIgnoreTIMVersionCheckBox.Checked = options.IgnoreTIMVersion;
            optionIgnoreTMDVersionCheckBox.Checked = options.IgnoreTMDVersion;

            // Arbitrarily choose -1
            optionNoOffsetCheckBox.Checked = ((options.StartOffset ?? -1) == 0 && (options.StopOffset ?? -1) == 1);

            optionLogToFileCheckBox.Checked = options.LogToFile;
            optionNoVerboseCheckBox.Checked = !options.LogToConsole;
            optionDebugCheckBox.Checked = options.Debug;
            optionShowErrorsCheckBox.Checked = options.ShowErrors;

            optionDrawAllToVRAMCheckBox.Checked = options.DrawAllToVRAM;
            optionOldUVAlignmentCheckBox.Checked = !options.FixUVAlignment;
        }

        private void WriteSettings(ScanOptions options)
        {
            Settings.Instance.ScanOptions = options.Clone();
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

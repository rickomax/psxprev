using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace PSXPrev.Forms
{
    public partial class LauncherForm : Form
    {
        public LauncherForm()
        {
            InitializeComponent();
        }

        private void selectFileButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Everything (*.*)|*.*";
                openFileDialog.Title = "Select a File to Scan";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    fileNameTextBox.Text = openFileDialog.FileName;
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
                    fileNameTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void selectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
            {
                folderBrowserDialog.IsFolderPicker = true;
                folderBrowserDialog.Title = "Select a Folder to Scan";
                // Parameter name used to avoid overload resolution with WPF Window, which we don't have a reference to.
                if (folderBrowserDialog.ShowDialog(ownerWindowHandle: Handle) == CommonFileDialogResult.Ok)
                {
                    fileNameTextBox.Text = folderBrowserDialog.FileName;
                }
            }
        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            Program.DoScan(fileNameTextBox.Text, filterTextBox.Text, new Program.ScanOptions
            {
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
                AutoAttachLimbs = optionAutoAttachLimbsCheckBox.Checked,
                //AutoPlayAnimations = false, //todo
                //AutoSelect = false, //todo
            });

            Close();
        }

        private void fileNameTextBox_TextChanged(object sender, EventArgs e)
        {
            scanButton.Enabled = File.Exists(fileNameTextBox.Text) || Directory.Exists(fileNameTextBox.Text);
        }
    }
}

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
                if (openFileDialog.ShowDialog() == DialogResult.OK)
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
                if (openFileDialog.ShowDialog() == DialogResult.OK)
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
                if (folderBrowserDialog.ShowDialog() == CommonFileDialogResult.Ok)
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

                IgnoreTMDVersion = optionIgnoreTMDVersionCheckBox.Checked,

                LogToFile = optionLogToFileCheckBox.Checked,
                NoVerbose = optionNoVerboseCheckBox.Checked,
                Debug = optionDebugCheckBox.Checked,
                ShowErrors = optionShowErrorsCheckBox.Checked,

                DrawAllToVRAM = optionDrawAllToVRAMCheckBox.Checked,
                AutoAttachLimbs = optionAutoAttachLimbsCheckBox.Checked,
                NoOffset = optionNoOffsetCheckBox.Checked,
            });

            Close();
        }

        private void fileNameTextBox_TextChanged(object sender, EventArgs e)
        {
            scanButton.Enabled = File.Exists(fileNameTextBox.Text) || Directory.Exists(fileNameTextBox.Text);
        }
    }
}

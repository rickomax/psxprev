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

        private void SelectFileButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Everything (*.*)|*.*";
                openFileDialog.Title = "Select a File to Scan";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilenameText.Text = openFileDialog.FileName;
                }
            }
        }

        private void SelectISOButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Iso Files (*.iso)|*.iso";
                openFileDialog.Title = "Select an ISO to Scan";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilenameText.Text = openFileDialog.FileName;
                }
            }
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
            {
                folderBrowserDialog.IsFolderPicker = true;
                folderBrowserDialog.Title = "Select a Folder to Scan";
                if (folderBrowserDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    FilenameText.Text = folderBrowserDialog.FileName;
                }
            }
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            Program.DoScan(FilenameText.Text, FilterText.Text, new Program.ScanOptions
            {
                CheckAN = scanForAnCheckBox.Checked,
                CheckBFF = scanForBffCheckBox.Checked,
                CheckCROC = crocCheckBox.Checked,
                CheckHMD = hmdCheckBox.Checked,
                CheckPMD = PMDCheckBox.Checked,
                CheckPSX = psxCheckBox.Checked,
                CheckTIM = TIMCheckBox.Checked,
                CheckTMD = TMDCheckBox.Checked,
                CheckTOD = TODCheckBox.Checked,
                CheckVDF = VDFCheckBox.Checked,

                IgnoreTMDVersion = ignoreVersionCheckBox.Checked,

                LogToFile = LogCheckBox.Checked,
                NoVerbose = NoVerboseCheckBox.Checked,
                Debug = DebugCheckBox.Checked,
                
                SelectFirstModel = selectFirstModelCheckBox.Checked,
                DrawAllToVRAM = drawAllToVRAMCheckBox.Checked,
                AutoAttachLimbs = autoAttachLimbsCheckBox.Checked,
            });

            Close();
        }

        private void FilenameText_TextChanged(object sender, EventArgs e)
        {
            ScanButton.Enabled = File.Exists(FilenameText.Text) || Directory.Exists(FilenameText.Text);
        }
    }
}

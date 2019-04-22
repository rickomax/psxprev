using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select a Folder to Scan";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    FilenameText.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            Program.DoScan(FilenameText.Text, FilterText.Text, TMDCheckBox.Checked, TMDAltCheckBox.Checked, TIMCheckBox.Checked, TIMAltCheckBox.Checked, PMDCheckBox.Checked, TODCheckBox.Checked, HMDModelsCheckBox.Checked, LogCheckBox.Checked, NoVerboseCheckBox.Checked, DebugCheckBox.Checked);
        }
    }
}

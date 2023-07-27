using System;
using System.Windows.Forms;
using PSXPrev.Common;

namespace PSXPrev.Forms.Dialogs
{
    public partial class SelectTMDForm : Form
    {
        public RootEntity SelectedTMD { get { return TMDListBox.SelectedItem as RootEntity; } }

        public SelectTMDForm()
        {
            InitializeComponent();
        }

        private void SelectTMDForm_Load(object sender, EventArgs e)
        {
            TMDListBox.Items.Clear();
            foreach (var entity in Program.AllEntities)
            {
                TMDListBox.Items.Add(entity);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

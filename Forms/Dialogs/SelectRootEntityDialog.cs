using System;
using System.Windows.Forms;
using PSXPrev.Common;

namespace PSXPrev.Forms.Dialogs
{
    public partial class SelectRootEntityDialog : Form
    {
        public RootEntity SelectedRootEntity => TMDListBox.SelectedItem as RootEntity;

        public SelectRootEntityDialog()
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
    }
}

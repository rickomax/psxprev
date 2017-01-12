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
    public partial class DialogForm : Form
    {
        public String ResultText
        {
            get { return mainTextBox.Text; }
        }

        public String LabelText
        {
            set { mainLabel.Text = value; }
        }

        public DialogForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

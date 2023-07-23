using System;
using System.Windows.Forms;

namespace PSXPrev.Forms
{
    public partial class DialogForm : Form
    {
        public string ResultText => mainTextBox.Text;

        public string LabelText
        {
            get => mainLabel.Text;
            set => mainLabel.Text = value;
        }

        public DialogForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static string Show(string text, string caption) => Show(null, text, caption);

        public static string Show(IWin32Window owner, string text, string caption)
        {
            using (var prompt = new DialogForm { LabelText = text, Text = caption })
            {
                prompt.ShowDialog(owner);
                return prompt.ResultText;
            }
        }
    }
}

using System;
using System.Windows.Forms;

namespace PSXPrev.Forms.Dialogs
{
    public partial class DialogForm : Form
    {
        public string InputText
        {
            get => mainTextBox.Text;
            set
            {
                mainTextBox.Text = value;
                mainTextBox.SelectAll();
            }
        }

        public string LabelText
        {
            get => mainLabel.Text;
            set => mainLabel.Text = value;
        }

        public DialogForm()
        {
            InitializeComponent();
        }


        // Returns null when canceled.
        public static string Show(string text, string caption, string defaultText = null) => Show(null, text, caption, defaultText);

        public static string Show(IWin32Window owner, string text, string caption, string defaultText = null)
        {
            using (var prompt = new DialogForm())
            {
                prompt.LabelText = text;
                prompt.Text = caption;
                prompt.InputText = defaultText ?? string.Empty;
                if (prompt.ShowDialog(owner) == DialogResult.OK)
                {
                    return prompt.InputText;
                }
            }
            return null; // Canceled
        }
    }
}

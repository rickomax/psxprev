using System.Collections;
using System.Text;
using PSXPrev.Forms;

namespace PSXPrev.Classes
{
    public static class Utils
    {
        public static string ToBitString(this BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string ShowDialog(string caption, string text)
        {
            var prompt = new DialogForm {Text = caption, LabelText = text};
            prompt.ShowDialog();
            return prompt.ResultText;
        }
    }
}

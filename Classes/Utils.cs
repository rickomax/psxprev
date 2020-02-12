using PSXPrev.Forms;

namespace PSXPrev.Classes
{
    public class Utils
    {
        public static string ShowDialog(string caption, string text)
        {
            var prompt = new DialogForm {Text = caption, LabelText = text};
            prompt.ShowDialog();
            return prompt.ResultText;
        }
    }
}

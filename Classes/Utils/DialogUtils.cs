using System.IO;
using PSXPrev.Forms;

namespace PSXPrev.Classes.Utils
{
    public class DialogUtils
    {
        public static string ShowDialog(string caption, string text)
        {
            var prompt = new DialogForm {Text = caption, LabelText = text};
            prompt.ShowDialog();
            return prompt.ResultText;
        }

        public static string SafeFileName(string entityName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                entityName = entityName.Replace(c, '\0');
            }
            return entityName;
        }
    }
}

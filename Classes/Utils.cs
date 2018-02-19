using System;
using System.Text;
using OpenTK;
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

        public static Vector3 CalculateNormal(Vector4 a, Vector4 b, Vector4 c)
        {
             return Vector3.Cross((b-a).Xyz.Normalized(), (c-a).Xyz.Normalized());
        }
    }
}

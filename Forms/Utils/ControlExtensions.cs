using System.Windows.Forms;
using PSXPrev.Common;

namespace PSXPrev.Forms.Utils
{
    public static class ControlExtensions
    {
        public static decimal Constrain(this NumericUpDown numericUpDown, decimal value)
        {
            return GeomMath.Clamp(value, numericUpDown.Minimum, numericUpDown.Maximum);
        }

        public static int Constrain(this ProgressBar progressBar, int value)
        {
            return GeomMath.Clamp(value, progressBar.Minimum, progressBar.Maximum);
        }

        public static int Constrain(this ToolStripProgressBar progressBar, int value)
        {
            return GeomMath.Clamp(value, progressBar.Minimum, progressBar.Maximum);
        }

        public static int Constrain(this TrackBar trackBar, int value)
        {
            return GeomMath.Clamp(value, trackBar.Minimum, trackBar.Maximum);
        }


        public static void SetValueSafe(this NumericUpDown numericUpDown, decimal value)
        {
            numericUpDown.Value = Constrain(numericUpDown, value);
        }

        public static void SetValueSafe(this ProgressBar progressBar, int value)
        {
            progressBar.Value = Constrain(progressBar, value);
        }

        public static void SetValueSafe(this ToolStripProgressBar progressBar, int value)
        {
            progressBar.Value = Constrain(progressBar, value);
        }

        public static void SetValueSafe(this TrackBar trackBar, int value)
        {
            trackBar.Value = Constrain(trackBar, value);
        }


        public static T GetFocusedControlOfType<T>(this ContainerControl container) where T : Control
        {
            var focusedControl = container.ActiveControl;
            var focusedContainer = focusedControl as IContainerControl;
            while (focusedContainer != null && !(focusedControl is T))
            {
                focusedControl = focusedContainer.ActiveControl;
                focusedContainer = focusedControl as IContainerControl;
            }

            return focusedControl as T;
        }
    }
}

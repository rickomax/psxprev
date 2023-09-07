using System;
using System.Collections.Generic;
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


        // ProgressBars introduced a moving animation in Vista, which means you can't instantly force a progress
        // bar to a specific position UNLESS the value being assigned is less than the current value.
        // This solves that by first assigning value + 1, then assigning value.
        // <https://stackoverflow.com/questions/977278/how-can-i-make-the-progress-bar-update-fast-enough>
        public static void SetValueInstant(this ProgressBar progressBar, int value)
        {
            value = Constrain(progressBar, value);
            if (value < progressBar.Maximum)
            {
                progressBar.Value = value + 1;
                progressBar.Value = value;
            }
            else if (progressBar.Maximum < int.MaxValue)
            {
                progressBar.Maximum++;
                progressBar.Value = value + 1;
                progressBar.Value = value;
                progressBar.Maximum--;
            }
        }

        public static void SetValueInstant(this ToolStripProgressBar progressBar, int value)
        {
            value = Constrain(progressBar, value);
            if (value < progressBar.Maximum)
            {
                progressBar.Value = value + 1;
                progressBar.Value = value;
            }
            else if (progressBar.Maximum < int.MaxValue)
            {
                progressBar.Maximum++;
                progressBar.Value = value + 1;
                progressBar.Value = value;
                progressBar.Maximum--;
            }
        }

        public static void UpdateValueInstant(this ProgressBar progressBar)
        {
            SetValueInstant(progressBar, progressBar.Value);
        }

        public static void UpdateValueInstant(this ToolStripProgressBar progressBar)
        {
            SetValueInstant(progressBar, progressBar.Value);
        }


        public static void UpdateDynamicDropDownWidth(this ComboBox comboBox, int padding = 0, int? maxWidth = null)
        {
            var width = comboBox.Width; // use combo box width as minimum width
            if (comboBox.Items.Count > 0)
            {
                var font = comboBox.Font;
                using (var graphics = comboBox.CreateGraphics())
                {
                    for (var i = 1; i < comboBox.Items.Count; i++)
                    {
                        var text = comboBox.Items[i]?.ToString();
                        if (text != null)
                        {
                            var textWidth = (int)graphics.MeasureString(text, font).Width + padding;
                            width = Math.Max(width, textWidth);
                            if (maxWidth.HasValue && width >= maxWidth.Value)
                            {
                                width = maxWidth.Value;
                                break; // We're as big as we want to get
                            }
                        }
                    }
                }
            }
            comboBox.DropDownWidth = width;
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


        public static IEnumerable<Control> EnumerateAllControls(this Control control)
        {
            return EnumerateAllControlsOfType<Control>(control);
        }

        public static IEnumerable<T> EnumerateAllControlsOfType<T>(this Control control) where T : Control
        {
            var queue = new Queue<Control>();
            queue.Enqueue(control);

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();

                foreach (var child in parent.Controls)
                {
                    if (child is Control childControl)
                    {
                        if (child is T t)
                        {
                            yield return t;
                        }
                        if (childControl.Controls.Count > 0)
                        {
                            queue.Enqueue(childControl);
                        }
                    }
                }
            }
        }
    }
}

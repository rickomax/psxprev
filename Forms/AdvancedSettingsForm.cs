using System;
using System.Windows.Forms;

namespace PSXPrev.Forms
{
    // Return true to refresh the property grid if a property was changed.
    public delegate bool AdvancedSettingsValidate(object settings, PropertyValueChangedEventArgs e);

    public partial class AdvancedSettingsForm : Form
    {
        public bool IsModified { get; set; }

        public object Settings
        {
            get => propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        public bool AllowCancel
        {
            get => cancelButton.Enabled;
            set
            {
                okCancelMarginFlowLayoutPanel.SuspendLayout();
                cancelButton.Enabled = value;
                cancelButton.Visible = value;
                CancelButton = (value ? cancelButton : null);
                okCancelMarginFlowLayoutPanel.ResumeLayout();
            }
        }

        //public Func<object, PropertyValueChangedEventArgs, bool> Validate { get; set; }
        public AdvancedSettingsValidate Validate { get; set; }

        public AdvancedSettingsForm()
        {
            InitializeComponent();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            IsModified = true;
            if (Validate?.Invoke(Settings, e) ?? false)
            {
                propertyGrid.Refresh();
            }
        }

        // Make sure to pass in a clone of the settings object when allowCancel is true.
        public static bool Show(IWin32Window owner, string title, object clonedSettings, out bool modified, bool allowCancel = true,
                                AdvancedSettingsValidate validate = null,
                                Action<PropertyGrid> configure = null)
        {
            using (var form = new AdvancedSettingsForm())
            {
                form.Text = title;
                form.Settings = clonedSettings;
                form.AllowCancel = allowCancel;
                form.Validate = validate;
                configure?.Invoke(form.propertyGrid);
                var result = form.ShowDialog(owner) == DialogResult.OK;
                modified = form.IsModified;
                return (result || !allowCancel);
            }
        }
    }
}

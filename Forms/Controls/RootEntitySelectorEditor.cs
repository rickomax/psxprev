using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using PSXPrev.Forms.Dialogs;

namespace PSXPrev.Forms.Controls
{
    public class RootEntitySelectorEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (Program.IsScanning)
            {
                MessageBox.Show("Please wait until the scan finishes.");
                return null;
            }
            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            using (var form = new SelectRootEntityDialog())
            {
                var result = svc != null ? svc.ShowDialog(form) : form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    return form.SelectedRootEntity;
                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
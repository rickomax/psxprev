using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using PSXPrev.Forms;

namespace PSXPrev.Classes
{
    public class RootEntitySelectorEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (Program.Scanning)
            {
                MessageBox.Show("Please wait until the scan finishes.");
                return null;
            }
            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (svc != null)
            {
                using (var form = new SelectTMDForm())
                {
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        return form.SelectedTMD;
                    }
                }
            }
            else
            {
                using (var form = new SelectTMDForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        return form.SelectedTMD;
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
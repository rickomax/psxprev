using System;
using System.Windows.Forms;

namespace PSXPrev.Forms.Controls
{
    public class ExtendedTreeView : TreeView
    {
        // Fix a bug where double clicking a checkbox desyncs its actual and visual checked state.
        // Of all the WinForms moments so far, this is the worst...
        // <https://social.msdn.microsoft.com/forums/windows/en-US/9d717ce0-ec6b-4758-a357-6bb55591f956/possible-bug-in-net-treeview-treenode-checked-state-inconsistent?forum=winforms>
        protected override void WndProc(ref Message m)
        {
            const int WM_LBUTTONDBLCLK = 0x203;

            // Filter WM_LBUTTONDBLCLK when we're showing checkboxes.
            if (m.Msg == WM_LBUTTONDBLCLK && CheckBoxes)
            {
                // See if we're over the checkbox. If so then we'll handle the toggling of it ourselves.
                var x = m.LParam.ToInt32() & 0xffff;
                var y = (m.LParam.ToInt32() >> 16) & 0xffff;
                var hitTestInfo = HitTest(x, y);

                var node = hitTestInfo.Node;
                if (node != null && hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                {
                    OnBeforeCheck(new TreeViewCancelEventArgs(node, false, TreeViewAction.ByMouse));
                    node.Checked = !node.Checked;
                    OnAfterCheck(new TreeViewEventArgs(node, TreeViewAction.ByMouse));
                    m.Result = IntPtr.Zero;
                    return; // Disable original behavior
                }
            }

            base.WndProc(ref m);
        }
    }
}

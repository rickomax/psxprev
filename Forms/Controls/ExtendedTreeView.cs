using PSXPrev.Forms.Utils;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PSXPrev.Forms.Controls
{
    public class ExtendedTreeView : TreeView
    {
#if DEBUG
        //private static int _wndProcCounter = 0;
#endif

        protected override void WndProc(ref Message m)
        {
#if DEBUG
            //Program.ConsoleLogger.WriteLine($"{Name} ({_wndProcCounter++}): msg=0x{m.Msg:x04} wparam=0x{(uint)m.WParam.ToInt32():x08} lparam=0x{(uint)m.LParam.ToInt32():x08}");
#endif
            if (m.Msg == NativeMethods.OCM_NOTIFY)
            {
                //struct NMHDR {
                //    IntPtr hwndFrom;
                //    uint   idFrom;
                //    uint   code;
                //}
                //var hwndFrom = Marshal.ReadIntPtr(m.LParam);
                //var idFrom = Marshal.ReadInt32(m.LParam + IntPtr.Size);
                var code = Marshal.ReadInt32(m.LParam + IntPtr.Size + sizeof(int));
                if (code == NativeMethods.NM_CUSTOMDRAW && DrawMode == TreeViewDrawMode.Normal)
                {
                    //struct NMCUSTOMDRAW {
                    //    NMHDR  hdr; // IntPtr[1], int[2]
                    //    uint   dwDrawStage;
                    //    IntPtr hdc;
                    //    int[4] rc;
                    //    IntPtr dwItemSpec;
                    //    uint   uItemState;
                    //    IntPtr lItemlParam;
                    //}
                    //struct NMTVCUSTOMDRAW {
                    //    NMCUSTOMDRAW nmcd; // IntPtr[4], int[8]
                    //    uint   clrText;
                    //    uint   clrTextBk;
                    //    int    iLevel;
                    //}
                    //var dwDrawStage = Marshal.ReadInt32(m.LParam + IntPtr.Size + sizeof(int) * 2);
                    
                    // Prevent an unholy amount of NM_CUSTOMDRAW message spam by telling the TreeView to shut up and not draw anything special.
                    // Originally the TreeView would end up handling custom draw events FOR EVERY TREE NODE IN THE HIERARCHY!
                    // AND WE ABSOLUTELY DO NOT NEED TO BE DRAWING 1000+ TREE NODES THAT ARE NOT ON SCREEN!

                    // Note, that TreeView handles custom draw for a reason during the CDDS_ITEMPREPAINT stage.
                    // If you select a tree node, then click down on another and drag the mouse,
                    // the clicked tree node will be highlighted until you start dragging, after which the original will be highlighted.
                    // Instead, we solve this by just selecting the node on mouse-down instead of mouse-click.
                    m.Result = IntPtr.Zero;
                    return; // Disable original behavior
                }
            }
            else if (m.Msg == NativeMethods.WM_TIMER)
            {
                // Prevent the TreeView from being a royal pain and scrolling the selection back into focus moments after the selection occurred.
                // This issue also exists with ListViews, so we can likely solve it the same way if we ever use any (ImageListView doesn't count).
                m.Result = IntPtr.Zero;
                return; // Disable original behavior
            }
            else if (m.Msg == NativeMethods.WM_LBUTTONDBLCLK && CheckBoxes)
            {
                // Fix a bug where double clicking a checkbox desyncs its actual and visual checked state.
                // Of all the WinForms moments, this is the worst (so far)...
                // <https://social.msdn.microsoft.com/forums/windows/en-US/9d717ce0-ec6b-4758-a357-6bb55591f956/possible-bug-in-net-treeview-treenode-checked-state-inconsistent?forum=winforms>

                // Filter WM_LBUTTONDBLCLK only when we're showing checkboxes.

                // See if we're over the checkbox. If so then we'll handle the toggling of it ourselves.
                NativeMethods.Int32ToSignedXY(m.LParam.ToInt32(), out var x, out var y);
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

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_LBUTTONDOWN)
            {
                // Fix the visual bug that occurs when selecting a node and then dragging a different node.
                // This is now a problem because we eliminated custom draw.
                // So to solve it, we just force selection on mouse-down instead of mouse-click.

                // Don't put this in WndProc so that the TreeView can handle its normal mouse-down behavior first.
                NativeMethods.Int32ToSignedXY(m.LParam.ToInt32(), out var x, out var y);
                var hitTestInfo = HitTest(x, y);

                var node = hitTestInfo.Node;
                if (node != null && hitTestInfo.Location == TreeViewHitTestLocations.Label)
                {
                    if (SelectedNode != node)
                    {
                        SelectedNode = node;
                    }
                    m.Result = IntPtr.Zero;
                    return; // Disable original behavior
                }
            }
            base.DefWndProc(ref m);
        }

        protected override void OnBeforeCheck(TreeViewCancelEventArgs e)
        {
            // Fix issue where pressing space bar over a node that has its checkbox hidden will cause the checkbox to reappear.

            // Unknown action means we're manually setting the node to checked.
            // Don't waste time with checking if its hidden when Unknown, especially if we're mass-checking all nodes.
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.IsCheckBoxHidden())
                {
                    e.Cancel = true; // Prevent hidden checkbox from reappearing
                    return;
                }
            }
            base.OnBeforeCheck(e);
        }
    }
}

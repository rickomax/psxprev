using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PSXPrev.Forms.Utils
{
    public static class TreeViewExtensions
    {
        /// <summary>
        /// Hides the checkbox for the specified node on a TreeView control.
        /// </summary>
        public static bool HideCheckBox(this TreeNode node)
        {
            // State image values are shifted by 12. 0 = no checkbox, 1 = unchecked, 2 = checked
            var tvi = new NativeMethods.TVITEM
            {
                hItem = node.Handle,
                mask = NativeMethods.TVIF_STATE,
                stateMask = NativeMethods.TVIS_STATEIMAGEMASK,
                state = 0 << 12,
            };
            var result = NativeMethods.SendMessage(node.TreeView.Handle, NativeMethods.TVM_SETITEM, IntPtr.Zero, ref tvi);
            return result != IntPtr.Zero;
        }

        /// <summary>
        /// Returns true if the checkbox is hidden for the specified node on a TreeView control.
        /// </summary>
        public static bool IsCheckBoxHidden(this TreeNode node)
        {
            // State image values are shifted by 12. 0 = no checkbox, 1 = unchecked, 2 = checked
            var stateMask = new IntPtr(NativeMethods.TVIS_STATEIMAGEMASK);
            var state = NativeMethods.SendMessage(node.TreeView.Handle, NativeMethods.TVM_GETITEMSTATE, node.Handle, stateMask).ToInt32();
            var stateImage = state >> 12;
            // Alt:
            /*var tvi = new NativeMethods.TVITEM
            {
                hItem = node.Handle,
                mask = NativeMethods.TVIF_STATE,
                stateMask = NativeMethods.TVIS_STATEIMAGEMASK,
            };
            SendMessage(node.TreeView.Handle, NativeMethods.TVM_GETITEM, IntPtr.Zero, ref tvi);
            var stateImage = tvi.state >> 12;*/
            return stateImage == 0;
        }
    }
}

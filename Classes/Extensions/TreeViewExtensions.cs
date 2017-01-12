using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PSXPrev.Classes.Extensions
{
    public static class TreeViewExtensions
    {
        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam,
            ref TVITEM lParam);

        public static void HideCheckBox(this TreeNode node)
        {
            var tvi = new TVITEM {hItem = node.Handle, mask = TVIF_STATE, stateMask = TVIS_STATEIMAGEMASK, state = 0};
            SendMessage(node.TreeView.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        private struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpszText;
            private readonly int cchTextMax;
            private readonly int iImage;
            private readonly int iSelectedImage;
            private readonly int cChildren;
            private readonly IntPtr lParam;
        }
    }
}
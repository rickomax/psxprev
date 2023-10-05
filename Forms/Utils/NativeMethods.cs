using System;
using System.Runtime.InteropServices;

namespace PSXPrev.Forms.Utils
{
    internal static class NativeMethods
    {
        // Constants

        public const int WM_TIMER         = 0x0113;
        public const int WM_LBUTTONDOWN   = 0x0201;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int OCM_NOTIFY       = 0x204e;
        public const int TV_FIRST         = 0x1100;
        public const int TVM_GETITEMSTATE = TV_FIRST + 39;
        public const int TVM_GETITEM      = TV_FIRST + 62; // (TVM_GETITEMW, TVM_GETITEMA is: TV_FIRST + 12)
        public const int TVM_SETITEM      = TV_FIRST + 63; // (TVM_SETITEMW, TVM_SETITEMA is: TV_FIRST + 13)

        public const int NM_CUSTOMDRAW    = -12;

        public const int TVIF_STATE          = 0x8;
        public const int TVIS_STATEIMAGEMASK = 0xF000;


        // Structures

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        public struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }


        // P/Invoke methods

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TVITEM lParam);


        // Helper methods

        public static void Int32ToSignedXY(int param, out int x, out int y)
        {
            x = (short)((param      ) & 0xffff);
            y = (short)((param >> 16) & 0xffff);
        }
    }
}

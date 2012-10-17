using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Parahalf
{
    public class User32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hwndChild, IntPtr hwndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, Int32 wParam, Int32 lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int newLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(IntPtr ZeroOnly, string lpWindowName);

        public static IntPtr FindWindowByCaption(string caption)
        {
            return FindWindow(IntPtr.Zero, caption);
        }

        [DllImport("user32", SetLastError = true)]
        static extern int GetKeyNameText(int lParam, StringBuilder lpString, int cchSize);

        public static string GetKeyName(byte key)
        {
            StringBuilder lpString = new StringBuilder(20);
            while (GetKeyNameText(key << 16, lpString, lpString.Capacity) == lpString.Capacity - 1)
            {
                lpString.Capacity *= 2;
            }
            return lpString.ToString();
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint virtualKeyCode, uint scanCode,  byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
            StringBuilder receivingBuffer,
            int bufferSize, uint flags);

        public static string GetCharsFromKeys(Keys keys, bool shift, bool altGr)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int) Keys.ShiftKey] = 0xff;
            if (altGr)
            {
                keyboardState[(int) Keys.ControlKey] = 0xff;
                keyboardState[(int) Keys.Menu] = 0xff;
            }
            ToUnicode((uint) keys, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }

        public const int HTCAPTION = 2;

        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_MINIMIZEBOX = 0x20000;
        public const int WS_THICKFRAME = 0x40000;
        public const int WS_SYSMENU = 0x80000;
        public const int WS_CAPTION = 0xC00000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_VISIBLE = 0x10000000;

        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_CLIENTEDGE = 0x00000200;
        public const int WS_EX_STATICEDGE = 0x00020000;

        public const int SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOREDRAW = 0x0008,
        SWP_NOACTIVATE = 0x0010,
        SWP_DRAWFRAME = 0x0020,
        SWP_FRAMECHANGED = 0x0020,
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOREPOSITION = 0x0200,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_DEFERERASE = 0x2000,
        SWP_ASYNCWINDOWPOS = 0x4000;

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
    }
}

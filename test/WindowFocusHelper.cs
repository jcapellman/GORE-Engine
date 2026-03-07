using System;
using System.Runtime.InteropServices;

namespace GORETest
{
    public static class WindowFocusHelper
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void BringToFront(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
                SetForegroundWindow(hWnd);
        }
    }
}

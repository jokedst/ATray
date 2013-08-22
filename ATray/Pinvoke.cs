using System.Diagnostics;

namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// WinAPI functions
    /// </summary>
    internal static class Pinvoke
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetModuleFileName(UIntPtr hModule, StringBuilder lpFilename, int nSize);

        [DllImport("coredll.dll", SetLastError = true)]
        static extern int GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);

        public static string GetForegroundWindowText()
        {
            const int NChars = 256;
            
            var buff = new StringBuilder(NChars);

            var handle = GetForegroundWindow();

            uint processID = 0;
            uint threadID = GetWindowThreadProcessId(handle, out processID);
            Process p = Process.GetProcessById((int) processID);
            var appName = p.ProcessName;

            if (GetWindowText(handle, buff, NChars) > 0)
            {
                return appName + ": " + buff.ToString();
                //// this.IDWindowLabel.Text = handle.ToString();
            }

            return appName;
        }
    }
}

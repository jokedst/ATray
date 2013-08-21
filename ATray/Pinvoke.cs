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
        public static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(int hWnd, StringBuilder text, int count);

        public static string GetForegroundWindowText()
        {
            const int NChars = 256;
            int handle = 0;
            var buff = new StringBuilder(NChars);

            handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, NChars) > 0)
            {
                return buff.ToString();
                //// this.IDWindowLabel.Text = handle.ToString();
            }

            return string.Empty;
        }
    }
}

namespace ATray
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// WinAPI functions
    /// </summary>
    internal static class WindowsInternals
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

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static string GetForegroundWindowText()
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, nChars) > 0)
                return buff.ToString();

            try
            {
                GetWindowThreadProcessId(handle, out uint processId);
                return Process.GetProcessById((int)processId).ProcessName;
            }
            catch (ArgumentException)
            {
                // The process probably died between the calls
                return string.Empty;
            }
        }

        public static string GetForegroundAppName()
        {
            var handle = GetForegroundWindow();
            try
            {
                GetWindowThreadProcessId(handle, out uint processId);
                return Process.GetProcessById((int)processId).ProcessName;
            }
            catch (ArgumentException)
            {
                // The process probably died between the calls
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                // The process probably died between the calls
                return string.Empty;
            }
        }

        public static void GetForegroundProcessInfo(out string name, out string title)
        {
            var proc = GetForegroundProcess();
            name = proc?.ProcessName;
            title = proc?.MainWindowTitle;
        }

        public static Process GetForegroundProcess()
        {
            var handle = GetForegroundWindow();
            try
            {
                GetWindowThreadProcessId(handle, out uint processId);
                return Process.GetProcessById((int)processId);
            }
            catch (ArgumentException)
            {
                // The process probably died between the calls
                return null;
            }
            catch (InvalidOperationException)
            {
                // The process probably died between the calls
                return null;
            }
        }

        /// <summary>
        /// Returns time since last user action, i.e. how long they've been idle
        /// </summary>
        /// <returns>User idle time in miliseconds</returns>
        public static uint GetIdleTime()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return (uint)Environment.TickCount - lastInPut.dwTime;
        }
    }
}

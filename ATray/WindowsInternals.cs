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
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetModuleFileName(UIntPtr hModule, StringBuilder lpFilename, int nSize);

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern int GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);


        private static Guid GUID_CONSOLE_DISPLAY_STATE = Guid.Parse("6fe69556-704a-47a0-8f24-c28d936fda47");
        public const int WM_POWERBROADCAST = 0x0218;
        public const int PBT_POWERSETTINGCHANGE = 0x8013;

        private static void RegisterForPowerNotifications(IntPtr hwnd)
        {
            var hPowerSrc = RegisterPowerSettingNotification(hwnd, ref GUID_CONSOLE_DISPLAY_STATE, 0);
        }

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

        // This structure is sent when the PBT_POWERSETTINGSCHANGE message is sent.
        // It describes the power setting that has changed and contains data about the change
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            //public byte Data;
        }
    }
}

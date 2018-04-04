using System.Management;

namespace ATray
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// WinAPI functions
    /// </summary>
    internal static class NativeMethods
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
        private static extern bool GetLastInputInfo(ref LastUserInput plii);

        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        [DllImport("user32")]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32")]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

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
            name = title = string.Empty;
            try
            {
                var handle = GetForegroundWindow();
                GetWindowThreadProcessId(handle, out uint processId);
                var proc = Process.GetProcessById((int)processId);
                name = proc?.ProcessName ?? string.Empty;


                const int nChars = 256;
                var stringBuffer = new StringBuilder(nChars);

                if (GetWindowText(handle, stringBuffer, nChars) > 0)
                    title = stringBuffer.ToString();


                //var proc = GetForegroundProcess();
                if(string.IsNullOrWhiteSpace(title))
                    title = proc?.MainWindowTitle ?? string.Empty;

                var pid = proc.Id;

                if (!(_pidCmdCache.Get(pid.ToString()) is bool filtered))
                {
                    var cmd = GetCommandLine(pid);
                    filtered = cmd.Contains("--user-data-dir");
                    _pidCmdCache.Add(pid.ToString(), filtered, DateTime.Now.AddHours(1));
                }
                if(filtered)
                    title = "[unknown]";
            }
            catch (ArgumentException) { }
            catch (InvalidOperationException)
            {
                // The process probably died between the calls
            }
        }
        
        private static readonly MemoryCache _pidCmdCache = new MemoryCache("Atray-pids");

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
        /// Example on how to detect Chrome incognito windows, in case we e.g. don't want to save the window title for those
        /// </summary>
        /// <returns></returns>
        public static int FindChromeIncognitoWindows()
        {
            int incog = 0;
            foreach (var process in Process.GetProcessesByName("chrome"))
            {
                using (ManagementObjectSearcher mos = new ManagementObjectSearcher(string.Format("SELECT * FROM Win32_Process WHERE ProcessId = {0}", process.Id)))
                {
                    foreach (ManagementObject mo in mos.Get())
                    {
                        // This writes a big dump of process info
                        //Console.WriteLine(mo.GetText(TextFormat.Mof));

                        if (mo["CommandLine"].ToString().Contains("--disable-databases"))
                        {
                            // Found incognito window
                            incog++;
                        }
                    }
                }
            }
            return incog;
        }

        public static string GetCommandLine(int pid)
        {
            using (ManagementObjectSearcher mos = new ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}"))
            {
                foreach (var mo in mos.Get().OfType<ManagementObject>())
                {
                    return mo?["CommandLine"]?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns time since last user action, i.e. how long they've been idle
        /// </summary>
        /// <returns>User idle time in miliseconds</returns>
        public static uint GetIdleTime()
        {
            var lastUserInput = new LastUserInput();
            lastUserInput.StructSize = (uint)Marshal.SizeOf(lastUserInput);
            GetLastInputInfo(ref lastUserInput);

            return (uint)Environment.TickCount - lastUserInput.LastActivityMillisecond;
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

        /// <summary> When the user was last active at the computer </summary>
        internal struct LastUserInput
        {
            public uint StructSize;
            public uint LastActivityMillisecond;
        }
    }
}

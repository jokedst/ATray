namespace ATray
{
    using System;
    using System.IO;
    using Microsoft.Win32;

    /// <summary>
    /// Helper class That checks, adds and removes autostart for this program
    /// </summary>
    public class AutoStart
    {
        private readonly string _applicationName;
        private readonly string _applicationPathToExe;

        public AutoStart(string applicationName, string applicationPathToExe)
        {
            _applicationName = applicationName;
            _applicationPathToExe = applicationPathToExe;
            if (_applicationPathToExe == null || !File.Exists(_applicationPathToExe))
                throw new ArgumentException("Given executable does not exist", nameof(applicationPathToExe));
        }

        /// <summary>
        /// Set application to start at user login to windows
        /// </summary>
        /// <param name="startAtWindowsLogin"></param>
        public void SetStartup(bool startAtWindowsLogin)
        {
            var runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (runKey == null)
                throw new Exception("Can not open windows registry key");

            if (startAtWindowsLogin)
            {
                // The app is usually installed in AppData\Local\ATray\app-1.X.X , but since this changes between versions
                // the autorun would stop woring on update. But since there is a ATray.exe in AppData\Local\ATray as well,
                // we can use that instead (it just starts the current version)
                var parentExe = Path.Combine(Path.GetDirectoryName(_applicationPathToExe), "..",
                    Path.GetFileName(_applicationPathToExe));
                parentExe = Path.GetFullPath(parentExe);
                var exeToStart = File.Exists(parentExe) ? parentExe : _applicationPathToExe;
                runKey.SetValue(_applicationName, exeToStart);
            }
            else
                runKey.DeleteValue(_applicationName, false);
        }

        public bool IsSetToAutoStart()
        {
            using (var runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return runKey?.GetValue(_applicationName) is string value && value == _applicationPathToExe;
            }
        }
    }
}

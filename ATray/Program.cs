namespace ATray
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    public static class Program
    {
        /// <summary> Directory for storing application data </summary>
        public static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName);

#if DEBUG
        internal static System.Drawing.Icon MainIcon = ATray.Properties.Resources.debug_icon;
#else
        internal static System.Drawing.Icon MainIcon = ATray.Properties.Resources.main_icon;
#endif

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

        }
    }
}

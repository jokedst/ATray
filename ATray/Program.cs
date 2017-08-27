namespace ATray
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using RepositoryManager;
    using Squirrel;

    public static class Program
    {
        /// <summary> Directory for storing application data </summary>
#if DEBUG
        internal static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName, Application.ProductName, "DEBUG");
        internal static System.Drawing.Icon MainIcon = Properties.Resources.debug_icon;
#else
        internal static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName, Application.ProductName);
        internal static System.Drawing.Icon MainIcon = Properties.Resources.main_icon;
#endif
        internal static string RepoListFilePath = Path.Combine(SettingsDirectory, "repositories.json");
        internal static string ConfigurationFilePath = Path.Combine(SettingsDirectory, "atray.ini");

        internal static RepositoryCollection Repositories;
        internal static Configuration Configuration;
        internal static MainWindow MainWindowInstance;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Due to the constant reading/writing to the activities files, running two instances of ATray (for the same config) is a really bad idea. So quit.
            var mutexName = "Jokedst.Atray.single-exe-mutex." + SettingsDirectory.Replace('\\','¤');
            Mutex mutex = new Mutex(false, mutexName);
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("An instance of the application is already running.");
                return;
            }

            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            // Load repos and config
            Repositories = new RepositoryCollection(RepoListFilePath);
            Configuration = new Configuration(ConfigurationFilePath);

            var updateTask = Task.Run(CheckForUpdates);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindowInstance = new MainWindow();
            Application.Run(MainWindowInstance);
        }

        private static async Task CheckForUpdates()
        {
            try
            {
                using (var mgr = new UpdateManager(@"E:\Projects\Misc\ATray\Releases"))
                {
                    var up = await mgr.UpdateApp();
                    Trace.TraceInformation("Update check " + up.Version);
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Update check threw an exception: " + e.Message);
            }
        }
    }
}

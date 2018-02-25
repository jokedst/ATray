namespace ATray
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
        internal static Task UpdateTask = null;

        internal static string GitBashLocation = null;
        internal static string TortoiseGitLocation = null;


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

            UpdateTask = Task.Run(() => UpdateApp(false));
            DetectInstalledPrograms();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindowInstance = new MainWindow();
            MainWindowInstance.ShowNotification("boot");
            Application.Run(MainWindowInstance);

            Trace.TraceInformation("LEAVING!");
        }

        private static void DetectInstalledPrograms()
        {
            var paths = Environment.GetEnvironmentVariable("Path")?.Split(';');
            if (paths == null) return;

            // Git bash
            GitBashLocation = paths.Where(p => p.Contains(@"\Git\"))
                                   .Select(p => Path.Combine(p, "git-bash.exe"))
                                   .FirstOrDefault(File.Exists)
                           ?? paths.Where(p => p.Contains(@"\Git\"))
                                   .Select(Path.GetDirectoryName)
                                   .Select(p => Path.Combine(p, "git-bash.exe"))
                                   .FirstOrDefault(File.Exists);

            if (GitBashLocation == null && File.Exists(@"C:\Program Files\Git\git-bash.exe"))
                GitBashLocation = @"C:\Program Files\Git\git-bash.exe";

            // TortoiseGit
            TortoiseGitLocation = paths.Where(p => p.Contains(@"\TortoiseGit\"))
                .Select(p => Path.Combine(p, "TortoiseGitProc.exe"))
                .FirstOrDefault(File.Exists);
            if (TortoiseGitLocation == null && File.Exists(@"C:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe"))
                TortoiseGitLocation = @"C:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe";
        }

        public static async Task UpdateApp(bool verbose)
        {
            Thread.CurrentThread.Name = "SquirrelUpdateThread";

            using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/jokedst/ATray"))
            {
                var updates = await mgr.CheckForUpdate();
                var latestVersion = updates?.ReleasesToApply?.OrderBy(x => x.Version).LastOrDefault();
                if (latestVersion == null)
                {
                    if (verbose)
                        MessageBox.Show("No Updates are available at this time.");
                    return;
                }

                if (MessageBox.Show(
                        $"An update to version {latestVersion.Version} is available. Do you want to update?",
                        "Update available", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    await mgr.DownloadReleases(new[] {latestVersion});
                    await mgr.ApplyReleases(updates);
                    await mgr.UpdateApp();
                    MainWindowInstance.ShowNotification("Atray has been updated and is restarting");
                    UpdateManager.RestartApp();
                }
            }
        }
    }
}

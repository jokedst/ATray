using System.Drawing;
using System.Reflection;
using ATray.Activity;
using ATray.Tools;
using Microsoft.Extensions.DependencyInjection;
using NuGet;

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
        //internal static Icon MainIcon = Properties.Resources.debug_icon;
#else
        internal static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName, Application.ProductName);
        //internal static Icon MainIcon = Properties.Resources.main_icon;
#endif
        internal static Icon GreyIcon;
        internal static Icon YellowIcon;
        internal static Icon MainIcon;

        internal static string RepoListFilePath = Path.Combine(SettingsDirectory, "repositories.json");
        internal static string ConfigurationFilePath = Path.Combine(SettingsDirectory, "atray.ini");

        private static IServiceProvider ServiceProvider;
        internal static Configuration Configuration;
        private static MainWindow MainWindowInstance;
        internal static Task UpdateTask;
        private static WebServer webServer;

        internal static string GitBashLocation;
        internal static string TortoiseGitLocation;
        private static TraceSource Tracer = new TraceSource("Atray.Program", SourceLevels.Information);
        internal static WorkPlayFilter ActivityClassifyer = new WorkPlayFilter();

        internal static IServiceProvider InitializeIoC()
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IRepositoryCollection, RepositoryCollection>(s =>
                    new RepositoryCollection(RepoListFilePath))
                .AddTransient<IAddRepositoryDialog, AddRepositoryForm>()
                .AddTransient<ISettingsDialog, SettingsForm>()
                .AddSingleton<MainWindow>()
                .AddSingleton<IShowNotifications, MainWindow>()
                .AddSingleton<IFactory<ISettingsDialog>, SimpleFactory<ISettingsDialog>>()
                .AddSingleton<IActivityMonitor, ActivityMonitor>()
                .AddSingleton(typeof(IPubSubHub<>), typeof(ObservableBase<>));
            var sp = serviceCollection.BuildServiceProvider();
            return sp;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Due to the constant reading/writing to the activities files, running two instances of ATray (for the same config) is a really bad idea. So quit.
            if (!new Mutex(false, "Jokedst.Atray.single-exe-mutex." + SettingsDirectory.Replace('\\','¤')).WaitOne(0, false))
            {
                MessageBox.Show("An instance of the application is already running.");
                return;
            }

            Tracer.Listeners.Add(new InternalTraceListener(){Name = "InternalTracer"});
            Tracer.TraceInformation("Starting ATray");

            ServiceProvider = InitializeIoC();
            LoadIcons();
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            // Load repos and config
            var repositories = ServiceProvider.GetService<IRepositoryCollection>();
            Configuration = new Configuration(ConfigurationFilePath);
            repositories.SetFileListening(FileListeningMode.AllChanges);

            if (Configuration.ActivateWebserver)
            {
                webServer = new WebServer("http://localhost:" + Configuration.Port);
                webServer.Run();
            }

            ActivityClassifyer.AddWorkPattern("devenv", ".*Cosmoz.*");
            ActivityClassifyer.AddWorkProgram("Slack");
            ActivityClassifyer.AddWorkProgram("Ssms");
            ActivityClassifyer.AddWorkPattern("chrome", ".*neovici.*");
            ActivityClassifyer.AddPlayPattern("chrome", @"\[unknown\]");
            ActivityClassifyer.AddPlayPattern("chrome", ".*ICA Banken.*");
            ActivityClassifyer.AddPlayPattern("chrome", @".*jokedst@gmail\.com.*");
            ActivityClassifyer.AddPlayProgram("RobloxPlayerBeta");

            UpdateTask = Task.Run(() => UpdateApp(false));
            DetectInstalledPrograms();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindowInstance = ServiceProvider.GetService<MainWindow>(); 
            MainWindowInstance.ShowNotification("boot");
            Application.Run(MainWindowInstance);

            Trace.TraceInformation("LEAVING!");
        }

        private static void LoadIcons()
        {
            int y = 0;
#if DEBUG
            y += 32;
#endif
            GreyIcon = Icon.FromHandle(Properties.Resources.icons.Clone(new Rectangle(0, y, 32, 32), Properties.Resources.icons.PixelFormat).GetHicon());
            YellowIcon = Icon.FromHandle(Properties.Resources.icons.Clone(new Rectangle(32, y, 32, 32), Properties.Resources.icons.PixelFormat).GetHicon());
            MainIcon = Icon.FromHandle(Properties.Resources.icons.Clone(new Rectangle(64, y, 32, 32), Properties.Resources.icons.PixelFormat).GetHicon());
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
            var thisVersion =new SemanticVersion( Assembly.GetExecutingAssembly().GetName().Version);

            try
            {
                using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/jokedst/ATray"))
                {
                    var updates = await mgr.CheckForUpdate();
                    var latestVersion = updates?.ReleasesToApply?.Where(x=>x.Version>thisVersion).OrderBy(x => x.Version).LastOrDefault();
                    if (latestVersion == null)
                    {
                        if (verbose)
                            MessageBox.Show("No Updates are available at this time.");
                        return;
                    }

                    if (MessageBox.Show($"An update to version {latestVersion.Version} is available. Do you want to update?",
                            "Update available", MessageBoxButtons.OKCancel) != DialogResult.OK)
                        return;

                    await mgr.DownloadReleases(new[] {latestVersion});
                    await mgr.ApplyReleases(updates);
                    await mgr.UpdateApp();
                    MainWindowInstance.ShowNotification("Atray has been updated and is restarting");
                    UpdateManager.RestartApp();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Update check failed with an exception: " + e.Message);
            }
        }
    }
}

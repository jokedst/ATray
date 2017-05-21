namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using Newtonsoft.Json;
    using RepositoryManager;

    public static class Program
    {
        /// <summary> Directory for storing application data </summary>
        public static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName);

        internal static string RepoListFilePath = Path.Combine(SettingsDirectory, "repositories.json");

#if DEBUG
        internal static System.Drawing.Icon MainIcon = ATray.Properties.Resources.debug_icon;
#else
        internal static System.Drawing.Icon MainIcon = ATray.Properties.Resources.main_icon;
#endif

        internal static List<ISourceRepository> Repositories;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            // Load repos if any
            if (File.Exists(RepoListFilePath))
            {
                Repositories = JsonConvert.DeserializeObject<List<ISourceRepository>>(File.ReadAllText(RepoListFilePath));
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}

namespace ATray
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using RepositoryManager;

    public static class Program
    {
        /// <summary> Directory for storing application data </summary>
        internal static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName);
        internal static string RepoListFilePath = Path.Combine(SettingsDirectory, "repositories.json");
        internal static string ConfigurationFilePath = Path.Combine(SettingsDirectory, "atray.ini");


#if DEBUG
        internal static System.Drawing.Icon MainIcon = ATray.Properties.Resources.debug_icon;
#else
        internal static System.Drawing.Icon MainIcon = ATray.Properties.Resources.main_icon;
#endif

        internal static RepositoryCollection Repositories;
        internal static Configuration Configuration;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            // Load repos and config
            Repositories = new RepositoryCollection(RepoListFilePath);
            Configuration = new Configuration(ConfigurationFilePath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }

    public class NonPublicPropertiesResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            var pi = member as PropertyInfo;
            if (pi != null)
            {
                prop.Readable = (pi.GetMethod != null);
                prop.Writable = (pi.SetMethod != null);
            }
            return prop;
        }
    }
}

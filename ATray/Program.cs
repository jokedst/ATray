namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
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
        internal static RepositoryCollection repositories;

        internal static JsonSerializerSettings JsonSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto};

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            // Load repos if any
            repositories = new RepositoryCollection(RepoListFilePath);
            //if (File.Exists(RepoListFilePath))
            //{
            //    Repositories = JsonConvert.DeserializeObject<List<ISourceRepository>>(File.ReadAllText(RepoListFilePath), JsonSettings);
            //}
            //Repositories = Repositories ?? new List<ISourceRepository>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }

        public static void SaveRepoList()
        {
            //File.WriteAllText(RepoListFilePath, JsonConvert.SerializeObject(Repositories, JsonSettings), Encoding.UTF8);
            repositories.Save();
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

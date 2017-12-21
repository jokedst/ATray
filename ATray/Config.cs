namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Microsoft.Win32;

    /// <summary>
    /// All configurable values should go here
    /// </summary>
    public class Configuration
    {
        [Description("If user is away this long, a brake has been taken (in seconds)"), Category("General"), DefaultValue(60)]
        public int MinimumBrakeLength { get; set; }
        [Description("How long the user may keep working without taking a break (in seconds)"), Category("General"), DefaultValue(20 * 60)]
        public int MaximumWorkTime { get; set; }
        [Description("How often we flush activity data to disk (in seconds)"), Category("General"), DefaultValue(5 * 60)]
        public int SaveInterval { get; set; }
        [Description("How often we should redraw the history graph (in minutes)"), Category("History"), DefaultValue(10)]
        public int HistoryRedrawTimeout { get; set; }
        [Description("A shared directory (e.g. dropbox) to share activity between computers"), Category("History"), Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string SharedActivityStorage { get; set; }
        [Description("Start application automatically at login"), Category("General"), DefaultValue(false)]
        public bool StartAtLogin { get; set; }

        public Configuration(string filename = null)
        {
            // Set defaults
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() is DefaultValueAttribute def)
                {
                    propertyInfo.SetValue(this, def.Value, null);
                }
            }
            _currentFileName = filename;
            if (filename!=null) ReadFromIniFile(filename);

            var applicationName = Application.ProductName;
#if DEBUG
            applicationName += "_DEBUG";
#endif
            this._autostart = new AutoStart(applicationName, Application.ExecutablePath);

            ValidateLoadedValues();
        }

        private void ValidateLoadedValues()
        {
            if(_autostart.IsSetToAutoStart() != StartAtLogin)
                _autostart.SetStartup(StartAtLogin);
        }

        public Configuration Clone()
        {
            return (Configuration) MemberwiseClone();
        }

        private string _currentFileName;
        private readonly AutoStart _autostart;

        /// <summary>
        /// Reads all (existing) settings from an ini-file
        /// </summary>
        /// <param name="filename"> File to read from </param>
        public void ReadFromIniFile(string filename = null)
        {
            filename = filename ?? _currentFileName;
            if (!File.Exists(filename)) return;

            // Load all sections from file
            var loaded = GetType().GetProperties()
                .Select(x => ((CategoryAttribute)x.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault())?.Category ?? "General")
                .Distinct()
                .ToDictionary(section => section, section => GetKeys(filename, section));

            //var loaded = GetKeys(filename, "General");
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var category = ((CategoryAttribute)propertyInfo.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault())?.Category ?? "General";
                var name = propertyInfo.Name;
                if (loaded.ContainsKey(category) && loaded[category].ContainsKey(name) && !string.IsNullOrEmpty(loaded[category][name]))
                {
                    var rawString = loaded[category][name];
                    var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    if (converter.IsValid(rawString))
                    {
                        propertyInfo.SetValue(this, converter.ConvertFromString(rawString), null);
                    }
                }
            }

            _currentFileName = filename;
        }

        /// <summary>
        /// Saves all settings to an ini-file, under "General" section
        /// </summary>
        /// <param name="filename">File to write to (default uses same file as loaded from)</param>
        public void SaveToIniFile(string filename = null)
        {
            filename = filename ?? _currentFileName;
            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Win32.WritePrivateProfileSection (that NppPlugin uses) doesn't work well with non-ASCII characters. So we roll our own.
            using (var fp = new StreamWriter(filename, false, Encoding.UTF8))
            {
                fp.WriteLine($"; {Application.ProductName} configuration file");

                foreach (var section in GetType()
                    .GetProperties()
                    .GroupBy(x => ((CategoryAttribute)x.GetCustomAttributes(typeof(CategoryAttribute), false)
                                      .FirstOrDefault())?.Category ?? "General"))
                {
                    fp.WriteLine(Environment.NewLine + "[{0}]", section.Key);
                    foreach (var propertyInfo in section.OrderBy(x => x.Name))
                    {
                        if (propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute description)
                            fp.WriteLine("; " + description.Description);
                        var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                        fp.WriteLine("{0}={1}", propertyInfo.Name, converter.ConvertToInvariantString(propertyInfo.GetValue(this, null)));
                    }
                }
            }
        }

        /// <summary>
        /// Read a section from an ini-file
        /// </summary>
        /// <param name="iniFile">Path to ini-file</param>
        /// <param name="category">Section to read</param>
        private Dictionary<string, string> GetKeys(string iniFile, string category)
        {
            var buffer = new byte[8 * 1024];

            GetPrivateProfileSection(category, buffer, buffer.Length, iniFile);
            var tmp = Encoding.UTF8.GetString(buffer).Trim('\0').Split('\0');
            return tmp.Select(x => x.Split(new[] { '=' }, 2))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);
        }


        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);
        
        /// <summary>
        /// Validates new setting values
        /// </summary>
        /// <param name="newConfig"></param>
        /// <returns> false if the new settings re rejected and should not be saved</returns>
        public bool ApplyChanges(Configuration newConfig)
        {
            // First validate the given values are correct

            // If all ok, act on changes
            if (StartAtLogin != newConfig.StartAtLogin)
                _autostart.SetStartup(newConfig.StartAtLogin);
            return true;
        }
    }
}

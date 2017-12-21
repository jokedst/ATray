using System.Diagnostics;

namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    internal class ActivityManager
    {
        private const string SavefilePattern = "Acts{0}.bin";
        private static readonly Dictionary<int, MonthActivities> ActivityCache = new Dictionary<int, MonthActivities>();
        private static readonly int ActivityFileFormatVersion = int.Parse(ConfigurationManager.AppSettings["ActivityFileFormatVersion"] ?? "1");

        static ActivityManager()
        {
            // LEGACY: Initially the activities were saved in the same dir as the .exe, but that's bad Windows citizenship so now it saves in AppData\Local
            // However, old files in the bin dir should be moved if there are any
            var thisDir = Directory.GetCurrentDirectory();
            var fileFilter = string.Format(SavefilePattern, "*");
            var legacyFiles = Directory.EnumerateFiles(".", fileFilter, SearchOption.TopDirectoryOnly).Count();
            if (legacyFiles > 0)
            {
                Program.MainWindowInstance.UIThread(() =>
                    MessageBox.Show(
                        $"There are {legacyFiles} old activity files ('Acts*.bin') in the Atray program directory (located at\"{thisDir}\").\n\nPlease move these to \"{Program.SettingsDirectory}\""));
            }
        }

        public static void SaveActivity(DateTime now, uint intervalLength, bool wasActive, string appName, string appTitle)
        {
            var currentSecond = (uint) now.TimeOfDay.TotalSeconds;

            // Check if we passed midnight. If so split it up into two activities and save those
            if (currentSecond + 1 < intervalLength)
            {
                // Yesterdays part
                SaveActivity(now.Date.AddSeconds(-1), intervalLength - currentSecond - 1, wasActive, appName, appTitle);

                // Todays part
                SaveActivity(now, currentSecond + 1, wasActive, appName, appTitle);
                return;
            }

            var activities = GetMonthActivity((short) now.Year, (byte) now.Month);
            var day = (byte) now.Day;
            if (!activities.Days.ContainsKey(day))
                activities.Days.Add(day, new List<ActivitySpan>());

            var appIndex = activities.GetApplicationNameIndex(appName);
            var titleIndex = activities.GetWindowTitleIndex(appTitle);

            if (activities.Days[day].Any())
            {
                var last = activities.Days[day].Last();

                // Check if the last activity was same type AND it didn't end too long ago (e.g. the computer was shut off or something)
                if (last.WasActive == wasActive && last.EndSecond + (intervalLength * 2) >= currentSecond)
                {
                    // In V2 files the active window need to be the same too
                    if (ActivityFileFormatVersion == 1 || (last.ApplicationNameIndex == appIndex &&
                                                           last.WindowTitleIndex == titleIndex))
                    {
                        // The last activity had the same state as this, just update it
                        activities.Days[day].Last().EndSecond = currentSecond;
                        StoreActivity(activities);
                        return;
                    }
                }
            }

            // Can't update previous activity, create a new one
            activities.Days[day].Add(new ActivitySpan
            {
                StartSecond = currentSecond - intervalLength + 1,
                EndSecond = currentSecond,
                WasActive = wasActive,
                ApplicationNameIndex = appIndex,
                WindowTitleIndex = titleIndex
            });

            StoreActivity(activities);
        }

        public static MonthActivities GetMonthActivity(short year, byte month)
        {
            var key = (year * 100) + month;

            // TODO: Move this to top so the cache is used first
            if (ActivityCache.ContainsKey(key))
                return ActivityCache[key];

            var filepath = Path.Combine(Program.SettingsDirectory, "Acts" + key + ".bin");
            if (File.Exists(filepath))
            {
                var monthActivities = new MonthActivities(filepath);
                ActivityCache.Add(key, monthActivities);
                return monthActivities;
            }

            var newMonth = new MonthActivities(year, month);
            ActivityCache.Add(key, newMonth);
            return newMonth;
        }

        private static readonly Regex FileNameParser = new Regex(string.Format(SavefilePattern, @"(\d*)"), RegexOptions.Compiled);

        public static SortedDictionary<int, string> ListAvailableMonths()
        {
            var result = new SortedDictionary<int, string>();
            var fileFilter = string.Format(SavefilePattern, "*");
            foreach (var file in Directory.EnumerateFiles(Program.SettingsDirectory, fileFilter, SearchOption.TopDirectoryOnly))
            {
                var match = FileNameParser.Match(file);
                result.Add(int.Parse(match.Groups[1].Value), file);
            }

            return result;
        }

        private static void StoreActivity(MonthActivities activities)
        {
            var key = (activities.Year * 100) + activities.Month;
            var filename = Path.Combine(Program.SettingsDirectory, "Acts" + key + ".bin");

            switch (ActivityFileFormatVersion)
            {
                case 1: activities.WriteToFile(filename); break;
                case 2: activities.WriteToFileV2(filename); break;
                default:
                    throw new ConfigurationErrorsException($"Unknown file format version '{ActivityFileFormatVersion}'");
            }
            ActivityCache[key] = activities;

            var sharedFolder = Program.Configuration.SharedActivityStorage;
            if (string.IsNullOrWhiteSpace(sharedFolder) || !Directory.Exists(sharedFolder)) return;
            try
            {
                var sharedFilename = $"{Environment.MachineName}_Acts{key}.bin";
#if DEBUG
                // Debugging must not overwrite prod files
                sharedFilename +=  ".debug";
#endif
                File.Copy(filename, Path.Combine(sharedFolder, sharedFilename), true);
            }
            catch (Exception e)
            {
                // Non-critical exception
                Trace.TraceWarning($"Could not copy activities file to shared directory: {e}");
            }
        }
    }
}
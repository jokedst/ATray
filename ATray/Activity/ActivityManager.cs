using System.Runtime.Caching;

namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ActivityManager
    {
        public const string AllComputers = "*";
        public static readonly string ThisComputer = Environment.MachineName;


        private const string SavefilePattern = "Acts{0}.bin";
        private static readonly Dictionary<int, MonthActivities> ActivityCache = new Dictionary<int, MonthActivities>();
        private static readonly Dictionary<string, Dictionary<string, MonthActivities>> SharedActivityCache = new Dictionary<string, Dictionary<string, MonthActivities>>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly int ActivityFileFormatVersion = int.Parse(ConfigurationManager.AppSettings["ActivityFileFormatVersion"] ?? "1");
        private static readonly MemoryCache BlurredCache = new MemoryCache("blurredActivites");

#if DEBUG
        private static string SharedPath => Program.Configuration.SharedActivityStorage == null ? null :
            Program.Configuration.SharedActivityStorage.EndsWith("DEBUG")
            ? Program.Configuration.SharedActivityStorage 
            : Path.Combine(Program.Configuration.SharedActivityStorage, "DEBUG");
#else
        private static string SharedPath => Program.Configuration.SharedActivityStorage;
#endif
        
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

            var activities = GetLocalComputerMonthActivity((short) now.Year, (byte) now.Month);
            var dayNumber = (byte) now.Day;
            if (!activities.Days.ContainsKey(dayNumber))
                activities.Days.Add(dayNumber, new DayActivityList(activities, dayNumber));
            var dayActivities = activities.Days[dayNumber];

            var appIndex = activities.GetApplicationNameIndex(appName);
            var titleIndex = activities.GetWindowTitleIndex(appTitle);

            if (dayActivities.Any())
            {
                var last = dayActivities.Last();

                // Check if the last activity was same type AND it didn't end too long ago (e.g. the computer was shut off or something)
                if (last.WasActive == wasActive && last.EndSecond + (intervalLength * 2) >= currentSecond)
                {
                    // In V2 files the active window need to be the same too
                    if (ActivityFileFormatVersion == 1 || (last.ApplicationNameIndex == appIndex &&
                                                           last.WindowTitleIndex == titleIndex))
                    {
                        // The last activity had the same state as this, just update it
                        dayActivities.Last().EndSecond = currentSecond;
                        StoreActivity(activities);
                        return;
                    }
                }
            }

            // Can't update previous activity, create a new one
            dayActivities.Add(new ActivitySpan(activities, dayNumber)
            {
                StartSecond = currentSecond - intervalLength + 1,
                EndSecond = currentSecond,
                WasActive = wasActive,
                ApplicationNameIndex = appIndex,
                WindowTitleIndex = titleIndex
            });

            StoreActivity(activities);
        }
        
        public static Dictionary<string, MonthActivities> GetSharedMonthActivities(short year, byte month, string computers, int blurAmount)
        {
            if (blurAmount == 0)
                return GetSharedMonthActivities(year, month, computers);

            var cachekey = $"{computers ?? "*"}_Acts{year * 100 + month}.bin Blur={blurAmount}".ToLowerInvariant();
            var cacheItem = (Dictionary<string, MonthActivities>)BlurredCache.Get(cachekey);
            if (cacheItem != null)
                return cacheItem;

            var activities = GetSharedMonthActivities(year, month, computers);
            var blurrer = new Blurrer();
            activities = blurrer.Blur(activities, blurAmount * blurAmount);

            // Check if the year/month is ~now, meaning we can only have a short cache time
            DateTimeOffset cacheExpire;
            if (new DateTime(year, month, 1).AddMonths(1).AddHours(-22) > DateTime.Now)
                cacheExpire = DateTimeOffset.Now.AddMinutes(5);
            else
                cacheExpire = DateTimeOffset.Now.AddHours(8);
            BlurredCache.Add(cachekey, activities, cacheExpire);

            return activities;
        }

        /// <summary>
        /// Get activities for given month for the specified <paramref name="computers"/>
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="computers"> Name of computers to get. '*' means all computers </param>
        /// <returns></returns>
        public static Dictionary<string, MonthActivities> GetSharedMonthActivities(short year, byte month, string computers)
        {
            if (computers.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                return new Dictionary<string, MonthActivities> {[ThisComputer] = GetLocalComputerMonthActivity(year, month)};
            var key = $"{computers ?? "*"}_Acts{year * 100 + month}.bin";
            
            if (SharedActivityCache.ContainsKey(key))
                return SharedActivityCache[key];

            var sharedPath = SharedPath;
            foreach (var file in Directory.EnumerateFiles(sharedPath, key, SearchOption.TopDirectoryOnly))
            {
                var monthActivities = new MonthActivities(file);
                var computerInFile = Path.GetFileName(file).Split('_')[0];
                if (!SharedActivityCache.ContainsKey(key))
                    SharedActivityCache.Add(key, new Dictionary<string, MonthActivities>(StringComparer.OrdinalIgnoreCase));
                SharedActivityCache[key].Add(computerInFile, monthActivities);
            }

            if (SharedActivityCache.ContainsKey(key))
                return SharedActivityCache[key];
            return new Dictionary<string, MonthActivities>();
        }

        private static MonthActivities GetLocalComputerMonthActivity(short year, byte month)
        {
            var key = (year * 100) + month;
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

        public static ISet<string> GetComputers()
        {
            if (string.IsNullOrEmpty(SharedPath)) return new HashSet<string>();
            return new HashSet<string>(Directory.EnumerateFiles(SharedPath, $"*_Acts*.bin", SearchOption.TopDirectoryOnly)
                .Select(file => Path.GetFileName(file).Split(new[] {'_'}, 2)[0]));
        }

        private static readonly Regex FileNameParser = new Regex(string.Format(SavefilePattern, @"(\d*)"), RegexOptions.Compiled);

        public static SortedDictionary<int, string> ListAvailableMonthsFiles(bool includeShared = false)
        {
            var result = new SortedDictionary<int, string>();
            var fileFilter = string.Format(SavefilePattern, "*");
            foreach (var file in Directory.EnumerateFiles(Program.SettingsDirectory, fileFilter, SearchOption.TopDirectoryOnly))
            {
                var match = FileNameParser.Match(file);
                result.Add(int.Parse(match.Groups[1].Value), file);
            }

            if (includeShared)
            {
                fileFilter = "*_" + fileFilter;
                foreach (var file in Directory.EnumerateFiles(SharedPath, fileFilter, SearchOption.TopDirectoryOnly))
                {
                    var match = FileNameParser.Match(file);
                    var key = int.Parse(match.Groups[1].Value);
                    if (result.ContainsKey(key))
                        result[key] += "|" + file;
                    else
                        result.Add(key, file);
                }
            }

            return result;
        }

        public static List<int> ListAvailableMonths(string computer)
        {
            if(string.IsNullOrEmpty(computer)) throw new ArgumentException("No computer was given (use '.' for current computer)");
            var local = computer.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);

            var fileFilter = string.Format(SavefilePattern, "*"); //"Acts{0}.bin"
            var path = Program.SettingsDirectory;
            if (!local)
            {
                fileFilter = computer + "_" + fileFilter;
                path = SharedPath;
            }

            if (string.IsNullOrEmpty(path)) return new List<int>();

            var result = new HashSet<int>();
            foreach (var file in Directory.EnumerateFiles(path, fileFilter, SearchOption.TopDirectoryOnly))
            {
                var match = FileNameParser.Match(file);
                if (!match.Success) continue;
                var key = int.Parse(match.Groups[1].Value);
                result.Add(key);
            }

            return result.OrderBy(x => x).ToList();
        }

        public static List<int> ListAvailableMonths(bool includeShared = false)
        {
            var result = new HashSet<int>();
            var fileFilter = string.Format(SavefilePattern, "*");
            foreach (var file in Directory.EnumerateFiles(Program.SettingsDirectory, fileFilter, SearchOption.TopDirectoryOnly))
            {
                var match = FileNameParser.Match(file);
                result.Add(int.Parse(match.Groups[1].Value));
            }

            if (includeShared)
            {
                fileFilter = "*_" + fileFilter;
                foreach (var file in Directory.EnumerateFiles(SharedPath, fileFilter, SearchOption.TopDirectoryOnly))
                {
                    var match = FileNameParser.Match(file);
                    if(!match.Success) continue;
                    var key = int.Parse(match.Groups[1].Value);
                    result.Add(key);
                }
            }

            return result.OrderBy(x => x).ToList();
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

            var sharedFolder = SharedPath;
            if (string.IsNullOrWhiteSpace(sharedFolder) || !Directory.Exists(sharedFolder)) return;
            try
            {
                var sharedFilename = $"{Environment.MachineName}_Acts{key}.bin";
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
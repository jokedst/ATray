namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ActivityManager
    {
        private const string SavePath = ".";

        private const string SavefilePattern = "Acts{0}.bin";

        private static readonly Dictionary<int, MonthActivities> ActivityCache = new Dictionary<int, MonthActivities>();

        public static void SaveActivity(DateTime now, uint intervalLength, bool wasActive, string appName, string appTitle)
        {
            var currentSecond = (uint) now.TimeOfDay.TotalSeconds;

            // Check if we passed midnight. If so split it up into two activities and save those
            if (currentSecond + 1 < intervalLength)
            {
                // Yesterdays part
                SaveActivity(now.Date.AddSeconds(-1), intervalLength - currentSecond - 1, wasActive, appName, appTitle);

                // Todays part
                SaveActivity(now, currentSecond + 1, wasActive, appName,appTitle);
                return;
            }

            var activities = GetMonthActivity((short) now.Year, (byte) now.Month);
            var day = (byte) now.Day;
            if (!activities.Days.ContainsKey(day))
                activities.Days.Add(day, new List<ActivitySpan>());


            if (activities.Days[day].Any())
            {
                var last = activities.Days[day].Last();

                // Check if the last activity was same type AND it didn't end too long ago (e.g. the computer was shut off or something)
                if (last.WasActive == wasActive && last.EndSecond + (intervalLength * 2) >= currentSecond)
                {
                    // The last activity had the same state as this, just update it
                    activities.Days[day].Last().EndSecond = currentSecond;
                    StoreActivity(activities);
                    return;
                }
            }

            // Can't update previous activity, create a new one
            activities.Days[day].Add(new ActivitySpan
            {
                StartSecond = currentSecond - intervalLength + 1,
                EndSecond = currentSecond,
                WasActive = wasActive
            });

            StoreActivity(activities);
        }

        public static MonthActivities GetMonthActivity(short year, byte month)
        {
            var key = (year * 100) + month;

            var filepath = Path.Combine(SavePath, "Acts" + key + ".bin");
            if (File.Exists(filepath))
            {
                return new MonthActivities(filepath);
            }

            // TODO: Move this to top so the cache is used first
            if (ActivityCache.ContainsKey(key))
                return ActivityCache[key];

            var newMonth = new MonthActivities(year, month);
            ActivityCache.Add(key, newMonth);
            return newMonth;
        }

        private static readonly Regex FileNameParser = new Regex(string.Format(SavefilePattern, @"(\d*)"), RegexOptions.Compiled);

        public static SortedDictionary<int, string> ListAvailableMonths()
        {
            var result = new SortedDictionary<int, string>();
            var fileFilter = string.Format(SavefilePattern, "*");
            foreach (var file in Directory.EnumerateFiles(SavePath, fileFilter, SearchOption.TopDirectoryOnly))
            {
                var match = FileNameParser.Match(file);
                result.Add(int.Parse(match.Groups[1].Value), file);
            }

            return result;
        }

        private static void StoreActivity(MonthActivities activities)
        {
            var key = (activities.Year * 100) + activities.Month;

            activities.WriteToFile(Path.Combine(SavePath, "Acts" + key + ".bin"));
        }
    }
}
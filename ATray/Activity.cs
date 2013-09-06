namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class ActivityManager
    {
        private const string SavePath = ".";

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

        private static void StoreActivity(MonthActivities activities)
        {
            var key = activities.Year * 100 + activities.Month;

            activities.WriteToFile(Path.Combine(SavePath, "Acts" + key + ".bin"));
        }

        private static readonly Dictionary<int, MonthActivities> ActivityCache = new Dictionary<int, MonthActivities>();

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
    }

    [Serializable]
    internal class MonthActivities
    {
        public short Year;
        public byte Month;
        public readonly Dictionary<byte, List<ActivitySpan>> Days = new Dictionary<byte, List<ActivitySpan>>();
        public List<string> ApplicationNames;
        public List<string> WindowTitles;

        public MonthActivities(short year, byte month)
        {
            Year = year;
            Month = month;
        }


        private void LoadFileV1(BinaryReader br, string filepath)
        {
            Year = br.ReadInt16();
                    Month = br.ReadByte();
                    var days = br.ReadInt32();
                    for (int i = 0; i < days; i++)
                    {
                        var dayNumber = br.ReadByte();
                        var actCount = br.ReadInt32();
                        var acts = new List<ActivitySpan>();
                        for (int a = 0; a < actCount; a++)
                        {
                            var act = new ActivitySpan
                                {
                                    StartSecond = br.ReadUInt32(),
                                    EndSecond = br.ReadUInt32(),
                                    WasActive = br.ReadBoolean()
                                };
                            acts.Add(act);
                        }

                        Days.Add(dayNumber, acts);
                    }

                    // Read the footer
                    var footer = br.ReadString();
                    if (footer != "KTHXBYE")
                        throw new Exception("Could not read file " + filepath + ", incorrect footer");
        }

        private void LoadFileV2(BinaryReader br, string filepath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new MonthActivities loaded from a file
        /// </summary>
        /// <param name="filepath">The file to load</param>
        public MonthActivities(string filepath)
        {
            using (Stream filestream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var br = new BinaryReader(filestream, Encoding.ASCII))
                {
                    // Read the header, always 12 bytes
                    var buffer = new byte[12];
                    br.Read(buffer, 0, 12);
                    var header = Encoding.ASCII.GetString(buffer);

                    switch (header)
                    {
                        case "ATRAY_ACTV1 ": LoadFileV1(br, filepath);
                            break;
                        case "ATRAY_ACTV2 ": LoadFileV2(br, filepath);
                            break;
                        default:
                            throw new Exception("Could not read file " + filepath + ", incorrect header");
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current object to a file
        /// </summary>
        /// <param name="filepath"></param>
        public void WriteToFile(string filepath)
        {
            Stream filestream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None);
            var bw = new BinaryWriter(filestream, Encoding.ASCII);
            // Header, including version number ("V1")
            bw.Write(Encoding.ASCII.GetBytes("ATRAY_ACTV1 "));
            bw.Write(Year);
            bw.Write(Month);
            bw.Write(Days.Count);
            foreach (var day in Days)
            {
                bw.Write(day.Key);
                bw.Write(day.Value.Count);
                foreach (var act in day.Value)
                {
                    bw.Write(act.StartSecond);
                    bw.Write(act.EndSecond);
                    bw.Write(act.WasActive);
                }
            }
            // Footer. Should perhaps have some crc or something?
            bw.Write("KTHXBYE");
            bw.Close();
            filestream.Close();
        }

        /// <summary>
        /// Saves the current object to a file
        /// </summary>
        /// <param name="filepath"></param>
        public void WriteToFileV2(string filepath)
        {
            Stream filestream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None);
            var bw = new BinaryWriter(filestream, Encoding.ASCII);
            // Header, including version number ("V1")
            bw.Write(Encoding.ASCII.GetBytes("ATRAY_ACTV2 "));
            bw.Write(Year);
            bw.Write(Month);
            bw.Write(Days.Count);
            foreach (var day in Days)
            {
                bw.Write(day.Key);
                bw.Write(day.Value.Count);
                foreach (var act in day.Value)
                {
                    bw.Write(act.StartSecond);
                    bw.Write(act.EndSecond);
                    bw.Write(act.WasActive);
                    if (act.WasActive)
                    {
                        bw.Write(act.ApplicationNameIndex);
                        bw.Write(act.WindowTitleIndex);
                    }
                }
            }

            bw.Write(ApplicationNames.Count);
            foreach (var applicationName in ApplicationNames)
            {
                bw.Write(applicationName);
            }

            bw.Write(WindowTitles.Count);
            foreach (var applicationName in WindowTitles)
            {
                bw.Write(applicationName);
            }

            // Footer. Should perhaps have some crc or something?
            bw.Write("KTHXBYE");
            bw.Close();
            filestream.Close();
        }
    }

    [Serializable]
    internal class ActivitySpan
    {
        public uint StartSecond;
        public uint EndSecond;
        public bool WasActive;
        public int ApplicationNameIndex;
        public int WindowTitleIndex;
    }
}

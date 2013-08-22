using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ATray
{
    class ActivityManager
    {
        private const string SavePath = ".";

        public static void SaveActivity(DateTime now, uint intervalLength, bool wasActive)
        {
            var currentSecond = (uint) now.TimeOfDay.TotalSeconds;

            // Check if we passed midnight. If so split it up into two activities and save those
            if (currentSecond + 1 < intervalLength)
            {
                // Yeasterdays part
                SaveActivity(now.Date.AddSeconds(-1), intervalLength - currentSecond - 1, wasActive);
                // Todays part
                SaveActivity(now, currentSecond + 1, wasActive);
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
                if (last.WasActive == wasActive && last.EndSecond + intervalLength*2 >= currentSecond)
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
            //// Using the worthless BinaryFormatter, but at least it's better than XML...
            //var formatter = new BinaryFormatter();
            //Stream stream = new FileStream(Path.Combine(SavePath, "Act" + key + ".bin"), FileMode.Create, FileAccess.Write, FileShare.None);
            //formatter.Serialize(stream, activities);
            //stream.Close();

            activities.WriteToFile(Path.Combine(SavePath, "Acts" + key + ".bin"));
        }

        private static readonly Dictionary<int, MonthActivities> FakeStorage = new Dictionary<int, MonthActivities>();

        public static MonthActivities GetMonthActivity(short year, byte month)
        {
            var key = year*100 + month;

            var filepath = Path.Combine(SavePath, "Acts" + key + ".bin");
            if (File.Exists(filepath))
            {
                //Stream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None);
                //var formatter = new BinaryFormatter();
                //var obj = (MonthActivities)formatter.Deserialize(stream);
                //stream.Close();

                //return obj;
                return new MonthActivities(filepath);
            }

            if (FakeStorage.ContainsKey(key))
                return FakeStorage[key];

            var newMonth = new MonthActivities(year, month);
            FakeStorage.Add(key, newMonth);
            return newMonth;
        }
    }

    [Serializable]
    class MonthActivities
    {
        public short Year;
        public byte Month;
        public readonly Dictionary<byte, List<ActivitySpan>> Days = new Dictionary<byte, List<ActivitySpan>>();

        public MonthActivities(short year, byte month)
        {
            Year = year;
            Month = month;
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
                    var buffer = new byte[12];
                    br.Read(buffer, 0, 12);
                    var header = Encoding.ASCII.GetString(buffer);
                    if (header != "ATRAY_ACTV1 ")
                        throw new Exception("Could not read file " + filepath + ", incorrect header");

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
            }
        }

        public void WriteToFile(string filepath)
        {
            Stream filestream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None);
            var bw = new BinaryWriter(filestream, Encoding.ASCII);
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
    }
}

namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A collection of all activity during a month. 
    /// This is the level we save at, each month gets its own file
    /// </summary>
    /// <remarks>
    /// The file format is pretty simple:
    /// First 12 bytes is a header (first 10 is ATRAY_ACTV, then version number in ascii, space padded)
    /// Next 2 bytes is year
    /// Next byte is month
    /// Then a bunch of data (see implementation)
    /// The file must end with the string "KTHXBYE" as encoded by BinaryWriter
    /// </remarks>
    [Serializable]
    public class MonthActivities
    {
        public short Year;
        public byte Month;
        public readonly Dictionary<byte, DayActivityList> Days = new Dictionary<byte, DayActivityList>();
        public List<string> ApplicationNames = new List<string>();
        public List<string> WindowTitles = new List<string>();
        public string ComputerName { get; }

        public MonthActivities(short year, byte month)
            : this(year, month, Environment.MachineName) { }

        public MonthActivities(short year, byte month, string computerName)
        {
            Year = year;
            Month = month;
            ComputerName = computerName;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="toCopy"></param>
        public MonthActivities(MonthActivities toCopy)
        { 
            Year = toCopy.Year;
            Month = toCopy.Month;
            ComputerName = toCopy.ComputerName;
            ApplicationNames = new List<string>(toCopy.ApplicationNames);
            WindowTitles = new  List<string>(toCopy.WindowTitles);
            foreach (var day in toCopy.Days)
            {
                var activityList = new DayActivityList(this, day.Value.DayNumber);
                foreach (var act in day.Value)
                {
                    var copy = new ActivitySpan(activityList, act.StartSecond, act.EndSecond, act.WasActive,
                        act.ApplicationName(), act.WindowTitle());
                    copy.Classify(act.Classification, act.ClassificationFrom);
                    activityList.Add(copy);
                }
                this.Days[day.Key] = activityList;
            }
        }

        public int GetApplicationNameIndex(string appName) => ApplicationNames.IndexOfOrAdd(appName);
        public int GetWindowTitleIndex(string appName) => WindowTitles.IndexOfOrAdd(appName);

        public uint EarliestSecond => Days.Count == 0 ? 0 : Days.Min(x => x.Value.FirstSecond);
        public uint LatestSecond => Days.Count == 0 ? 0 : Days.Max(x => x.Value.LastSecond);

        private void LoadFileV1(BinaryReader br, string filepath)
        {
            Year = br.ReadInt16();
            Month = br.ReadByte();
            var days = br.ReadInt32();
            for (int i = 0; i < days; i++)
            {
                var dayNumber = br.ReadByte();
                var actCount = br.ReadInt32();
                var acts = new DayActivityList(this, dayNumber);
                for (int a = 0; a < actCount; a++)
                {
                    var act = new ActivitySpan(this, acts)
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
            Year = br.ReadInt16();
            Month = br.ReadByte();
            var days = br.ReadInt32();
            for (int i = 0; i < days; i++)
            {
                var dayNumber = br.ReadByte();
                var actCount = br.ReadInt32();
                var acts = new DayActivityList(this, dayNumber);
                for (int a = 0; a < actCount; a++)
                {
                    var act = new ActivitySpan(this, acts)
                    {
                        StartSecond = br.ReadUInt32(),
                        EndSecond = br.ReadUInt32(),
                        WasActive = br.ReadBoolean()
                    };
                    if (act.WasActive)
                    {
                        act.ApplicationNameIndex = br.ReadInt32();
                        act.WindowTitleIndex = br.ReadInt32();
                    }
                    acts.Add(act);
                }

                Days.Add(dayNumber, acts);
            }

            ApplicationNames = new List<string>(br.ReadStrings());
            WindowTitles = new List<string>(br.ReadStrings());

            // Read the footer
            var footer = br.ReadString();
            if (footer != "KTHXBYE")
                throw new Exception("Could not read file " + filepath + ", incorrect footer");
        }

        /// <summary>
        /// Creates a new MonthActivities loaded from a file
        /// </summary>
        /// <param name="filepath">The file to load</param>
        public MonthActivities(string filepath)
        {
            using (var br = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None), Encoding.UTF8))
            {
                // Read the header, always 12 bytes
                var buffer = new byte[12];
                br.Read(buffer, 0, 12);
                var header = Encoding.ASCII.GetString(buffer);

                switch (header)
                {
                    case "ATRAY_ACTV1 ":
                        LoadFileV1(br, filepath);
                        break;
                    case "ATRAY_ACTV2 ":
                        LoadFileV2(br, filepath);
                        break;
                    default:
                        throw new Exception("Could not read file " + filepath + ", incorrect header");
                }
            }

            var fileNameParts = Path.GetFileName(filepath).Split('_');
            if (fileNameParts.Length > 1 && fileNameParts[1].StartsWith("Acts"))
                ComputerName = fileNameParts[0];
            else ComputerName = Environment.MachineName;

            if (Program.ActivityClassifyer != null)
            {
                Program.ActivityClassifyer.Classify(this, GuessWorkPlay.SameBlock);
            }
        }

        /// <summary>
        /// Saves the current object to a file
        /// </summary>
        /// <param name="filepath"></param>
        public void WriteToFile(string filepath)
        {
            using (Stream filestream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(filestream, Encoding.UTF8, true))
            {
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
            }
        }

        /// <summary>
        /// Saves the current object to a file
        /// </summary>
        /// <param name="filepath"></param>
        public void WriteToFileV2(string filepath)
        {
            // To minimize the risk of data loss due to crash while writing, write to RAM first
            using (var memoryStream = new MemoryStream())
            {
                using (var bw = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    // Header, including version number ("V2")
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

                    bw.Write(ApplicationNames);
                    bw.Write(WindowTitles);

                    // Footer. Should perhaps have some crc or something?
                    bw.Write("KTHXBYE");
                }

                File.WriteAllBytes(filepath, memoryStream.ToArray());
            }
        }
    }
}
namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// All activities for a month
    /// </summary>
    /// <remarks>
    /// Activities are stored in a file. The file format is pretty simple:
    /// First 12 bytes is a header (first 10 is ATRAY_ACTV, then version number in ascii, space padded)
    /// Next 2 bytes is year
    /// Next byte is month
    /// Then a bunch of data (see implementation)
    /// The file must end with the string "KTHXBYE" as encoded by BinaryWriter
    /// </remarks>
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
            using (Stream filestream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var br = new BinaryReader(filestream, Encoding.UTF8))
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
            var bw = new BinaryWriter(filestream, Encoding.UTF8);
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
            var bw = new BinaryWriter(filestream, Encoding.UTF8);
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
            bw.Close();
            filestream.Close();
        }
    }
}
namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A list of all activities in a day
    /// </summary>
    [Serializable]
    public class DayActivityList : List<ActivitySpan>
    {
        public DayActivityList(MonthActivities owner,int dayNumber)
        {
            Owner = owner;
            DayNumber = dayNumber;
        }

        public MonthActivities Owner { get; }
        public int DayNumber { get; }

        public uint FirstSecond => this.FirstOrDefault()?.StartSecond ?? 0;
        public uint LastSecond => this.LastOrDefault()?.EndSecond ?? 0;

        public uint FirstActiveSecond => this.FirstOrDefault(x=>x.WasActive)?.StartSecond ?? 0;
        public uint LastActiveSecond => this.LastOrDefault(x=>x.WasActive)?.EndSecond ?? 0;
    }
}
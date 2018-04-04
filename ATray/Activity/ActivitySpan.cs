namespace ATray.Activity
{
    using System;

    /// <summary>
    /// Represents a timespan where the user was either active or not
    /// </summary>
    /// <remarks> An activityspan never crosses midnight </remarks>
    [Serializable]
    public class ActivitySpan
    {
        public MonthActivities Owner { get; }
        public DayActivityList Day { get; }

        public uint StartSecond;
        public uint EndSecond;
        public bool WasActive;
        public int ApplicationNameIndex;
        public int WindowTitleIndex;

        [NonSerialized] public WorkPlayType Classification;

        /// <summary>
        /// Default constructor (for deserializers)
        /// </summary>
        public ActivitySpan() { }

        public ActivitySpan(MonthActivities owner, DayActivityList day)
        {
            Owner = owner;
            Day = day;
        }

        public ActivitySpan(MonthActivities owner, byte day)
        {
            Owner = owner;
            Day = owner.Days[day];
        }

        public ActivitySpan( DayActivityList day, uint startSecond, uint endSecond, bool wasActive, string applicationName, string windowTitle)
        {
            Owner = day.Owner;
            Day = day;
            this.StartSecond = startSecond;
            this.EndSecond = endSecond;
            this.WasActive = wasActive;
            this.ApplicationNameIndex = Owner.GetApplicationNameIndex(applicationName);
            this.WindowTitleIndex = Owner.GetWindowTitleIndex(windowTitle);
        }

        public string ApplicationName()
        {
            return Owner.ApplicationNames[this.ApplicationNameIndex];
        }

        public string WindowTitle()
        {
            return Owner.WindowTitles[this.WindowTitleIndex];
        }
    }
}

namespace ATray.Activity
{
    using System;

    /// <summary>
    /// Represents a timespan where the user was either active or not
    /// </summary>
    /// <remarks> An activityspan never crosses midnight </remarks>
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

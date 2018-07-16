namespace ATray.Activity
{
    public enum WorkPlayType
    {
        Unknown = 0,
        Work = 1,
        Play = 2,
        Both = 3,
    }

    /// <summary>
    /// How we guess work/play when we don't know
    /// </summary>
    public enum GuessWorkPlay
    {
        Never = 0,
        SameBlock = 1,
        Agressive = 2
    }
}
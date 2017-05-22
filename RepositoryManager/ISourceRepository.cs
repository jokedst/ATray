namespace RepositoryManager
{
    using System;

    /// <summary>
    /// Represents a version controlled directory
    /// </summary>
    public interface ISourceRepository
    {
        /// <summary> Repository Name </summary>
        string Name { get; set; }

        /// <summary> Directory path </summary>
        string Location { get; set; }

        /// <summary> Last known status </summary>
        RepoStatus LastStatus { get; }

        /// <summary> Time the last status is from </summary>
        DateTime LastStatusAt { get; }

        /// <summary> Updates the status </summary>
        /// <returns></returns>
        RepoStatus UpdateStatus();

        /// <summary>
        /// Updates the dir with remote changes
        /// </summary>
        /// <param name="onlyIfNoMerge"> Only update if no merges has to be done </param>
        /// <returns></returns>
        bool Update(bool onlyIfNoMerge);

        /// <summary> How often this repo should be updated </summary>
        Schedule UpdateSchedule { get; set; }

        /// <summary>
        /// What automatic actions to perform when remote changes are detected
        /// </summary>
        AutoAction AutomaticAction { get; set; }

        /// <summary>
        /// Checks if this is a valid repository
        /// </summary>
        bool Valid();
    }

    /// <summary>
    /// How often the repo should be checked for updates
    /// </summary>
    public enum Schedule
    {
        Never=0,
        EveryMinute=1,
        FifthMinute=5,
        EveryHour=60,
        EveryDay=60*24
    }

    /// <summary>
    /// Automatic actions to perform on detected change
    /// </summary>
    [Flags]
    public enum AutoAction
    {
        /// <summary> Do nothing, not even fetch </summary>
        None = 0,
        /// <summary> Fetch remote changes </summary>
        Fetch = 1,
        /// <summary> If local repo is unmodified, pull remote changes </summary>
        PullWhenUnmodified = 2,
    }

    /// <summary>
    /// Type of repository
    /// </summary>
    [Flags]
    public enum RepositoryType
    {
        /// <summary> git repository </summary>
        Git = 1,
        /// <summary> Subversion (SVN) repository </summary>
        Svn = 2
    }
}
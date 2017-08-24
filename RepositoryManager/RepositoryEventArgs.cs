using System;

namespace RepositoryManager
{
    /// <summary>
    /// An event regarding a repository update
    /// </summary>
    public class RepositoryEventArgs : EventArgs
    {
        /// <summary> Create repo update event arguments </summary>
        /// <param name="repoLocation">Repo location</param>
        /// <param name="oldStatus">Status before event</param>
        /// <param name="newStatus">Status after event</param>
        /// <param name="name">Name of repo</param>
        /// <param name="eventType"> Type of event </param>
        public RepositoryEventArgs(string repoLocation, RepoStatus oldStatus, RepoStatus newStatus, string name, RepositoryEventType eventType)
        {
            Location = repoLocation;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Name = name;
            EventType = eventType;
        }

        /// <summary> Location of repository </summary>
        public string Location { get; }
        /// <summary> Status before event </summary>
        public RepoStatus OldStatus { get; }
        /// <summary> Status after event </summary>
        public RepoStatus NewStatus { get; }
        /// <summary> Name of repository </summary>
        public string Name { get; }
        /// <summary> Type of event </summary>
        public RepositoryEventType EventType { get; }

    }
}
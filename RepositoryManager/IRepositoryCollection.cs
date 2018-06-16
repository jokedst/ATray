using System;
using System.Collections.Generic;

namespace RepositoryManager
{
    /// <summary>
    /// Manages a set of repositories
    /// </summary><inheritdoc />
    public interface IRepositoryCollection : IEnumerable<ISourceRepository>
    {
        /// <summary>
        /// Reloads the repo list from file
        /// </summary>
        void ReloadFromFile();

        /// <summary>
        /// Trigger update of repos that should be according to their schedule
        /// </summary>
        void TriggerScheduledUpdates();

        /// <summary>
        /// Trigger repo updates. All updates are done in separate threads
        /// </summary>
        /// <param name="where"></param>
        /// <param name="ignoreRunningUpdates"> If true starts an update even if one is running </param>     
        void TriggerUpdate(Func<ISourceRepository, bool> where, bool ignoreRunningUpdates=false);

        /// <summary>
        /// Save the current list of repos to a file
        /// </summary>
        /// <param name="filepath"> File to save to. If null will use same as when loaded/saved last time </param>
        void Save(string filepath = null);

        /// <summary>
        /// Checks if a name is used by this collection
        /// </summary>
        /// <param name="repositoryName"> Repository name to check </param>
        /// <returns> true if exists, false if not </returns>
        bool ContainsName(string repositoryName);

        /// <summary> Add a repository to the collection </summary>
        void Add(ISourceRepository repo);

        /// <summary> Remove a repository from the collection </summary>
        /// <param name="repositoryLocation"> repo location </param>
        bool Remove(string repositoryLocation);

        /// <summary>
        /// Starts the scheduler
        /// </summary>
        void StartScheduler();

        /// <summary>
        /// Stops the scheduler. No more updates will be started, but running updates will not be cancelled
        /// </summary>
        void StopScheduler();

        /// <summary>
        /// Trigger an update of a repo
        /// </summary>
        /// <param name="location"></param>
        void UpdateRepo(string location);

        /// <summary>
        /// Trigger a pull of a repo
        /// </summary>
        /// <param name="location"></param>
        void PullRepo(string location);

        /// <summary> Raised when a repo has been updated </summary>
        event RepositoryEventHandler RepositoryUpdated;

        /// <summary> Raised when status has changed on a repo </summary>
        event RepositoryEventHandler RepositoryStatusChanged;

        /// <summary> Raised when a repository is added or removed from the list </summary>
        event RepositoryEventHandler RepositoryListChanged;

        /// <summary>
        /// Sets file listening mode
        /// </summary>
        /// <param name="mode"> how to listen on file changes </param>
        void SetFileListening(FileListeningMode mode);

        /// <summary>
        /// Gets a repository by it's name
        /// </summary>
        ISourceRepository GetByName(string repoName);

        /// <summary>
        /// Call to inform collection that a repo has been modified in some way
        /// </summary>
        /// <param name="repository"> Repo that was modified. Must be part of collection already </param>
        void RepositoryModified(ISourceRepository repository);
    }
}
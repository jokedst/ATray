namespace RepositoryManager
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Manages a set of repositories
    /// </summary>
    public class RepositoryCollection : IEnumerable<ISourceRepository>
    {
        private const int SampleFrequency = 5000;
        /// <summary> Settings for serializing and deserializing JSON </summary>
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        /// <summary> Internal list of repos </summary>
        private List<ISourceRepository> _repositories;
        private string RepoListFilePath { get; set; }
        private Timer _timer;
        private ConcurrentDictionary<string, Task> runningUpdates = new ConcurrentDictionary<string, Task>();
        
        /// <summary>
        /// Load a reposet from a file
        /// </summary>
        /// <param name="filepath"></param>
        public RepositoryCollection(string filepath)
        {
            RepoListFilePath = filepath;
            if (File.Exists(RepoListFilePath))
            {
                _repositories = JsonConvert.DeserializeObject<List<ISourceRepository>>(File.ReadAllText(RepoListFilePath), JsonSettings);
            }
            _repositories = _repositories ?? new List<ISourceRepository>();

            _timer = new Timer(TimerTick, null, SampleFrequency, SampleFrequency);
        }

        /// <summary>
        /// Reloads the repo list from file
        /// </summary>
        public void ReloadFromFile()
        {
            lock (_repositories)
            {
                if (File.Exists(RepoListFilePath))
                    _repositories = JsonConvert.DeserializeObject<List<ISourceRepository>>(File.ReadAllText(RepoListFilePath), JsonSettings);
                else
                    _repositories = new List<ISourceRepository>();
            }
        }

        private void TimerTick(object tick)
        {
            lock (_repositories)
            {
                foreach (var repository in _repositories.Where(repo=>repo.UpdateSchedule != Schedule.Never))
                {
                    if (runningUpdates.TryGetValue(repository.Location, out Task task))
                    {
                        if (!task.IsCompleted) continue;
                    }

                    // Check if it's time to update
                    if (repository.LastStatusAt.AddMinutes((int)repository.UpdateSchedule) >= DateTime.Now)
                        continue;

                    Trace.TraceInformation("About to update repo " + repository.Name);

                    var repoUnclousure = repository;
                    var newTask = Task.Factory.StartNew(() => CheckRepo(repoUnclousure));
                    runningUpdates.AddOrUpdate(repository.Location, newTask, (loc, oldTask) => newTask);
                }
            }
        }

        /// <summary>
        /// Save the current list of repos to a file
        /// </summary>
        /// <param name="filepath"> File to save to. If null will use same as when loaded/saved last time </param>
        public void Save(string filepath = null)
        {
            if (filepath != null) RepoListFilePath = filepath;
            if (RepoListFilePath == null) throw new ArgumentNullException(nameof(filepath), "Can not save file - no path specified");
            lock (_repositories)
                File.WriteAllText(RepoListFilePath, JsonConvert.SerializeObject(_repositories, JsonSettings), Encoding.UTF8);
        }

        /// <summary> Add a repository to the collection </summary>
        public void Add(ISourceRepository repo)
        {
            lock (_repositories)
                _repositories.Add(repo);
        }

        /// <summary>
        /// Finds repositorys under the given root path
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="types"> Which types of repositories to search for </param>
        /// <returns></returns>
        public static List<ISourceRepository> FindRepositories(string rootPath, RepositoryType types = RepositoryType.Git|RepositoryType.Svn)
        {
            return Directory.EnumerateDirectories(rootPath, ".???", SearchOption.AllDirectories)
                .Where(x => types.HasFlag(RepositoryType.Git) && x.EndsWith(".git") ||
                            types.HasFlag(RepositoryType.Svn) && x.EndsWith(".svn"))
                .Select(x => x.EndsWith(".git") ? (ISourceRepository) new GitRepository(x) : new SvnRepository(x))
                .Where(r => r.Valid())
                .ToList();
        }

        /// <summary>
        /// Starts the scheduler
        /// </summary>
        public void StartScheduler()
        {
            _timer.Change(SampleFrequency, SampleFrequency);
        }

        /// <summary>
        /// Stops the scheduler. No more updates will be started, but running updates will not be cancelled
        /// </summary>
        public void StopScheduler()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Checks if a repo needs to update, and if so starts the update and triggers an event when finished
        /// </summary>
        private void CheckRepo(ISourceRepository repo)
        {
            if (repo.LastStatusAt.AddMinutes((int) repo.UpdateSchedule) >= DateTime.Now)
                return;
            var previousStatus = repo.LastStatus;
            repo.UpdateStatus();
            OnRepositoryUpdated(new RepositoryEventArgs(repo.Location, previousStatus, repo.LastStatus));
        }

        /// <summary> Delegate for repo update events </summary>
        public delegate void RepositoryEventHandler(object sender, RepositoryEventArgs e);

        /// <summary> Raised when a repo has been updated </summary>
        public event RepositoryEventHandler RepositoryUpdated;

        /// <summary> Overridable event logic </summary>
        protected virtual void OnRepositoryUpdated(RepositoryEventArgs e)
        {
            RepositoryUpdated?.Invoke(this, e);
        }

        /// <inheritdoc />
        public IEnumerator<ISourceRepository> GetEnumerator()
        {
            // TODO: Thread safety?
            return _repositories.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // TODO: Thread safety?
            return ((IEnumerable) _repositories).GetEnumerator();
        }
    }

    /// <summary>
    /// An event regarding a repository update
    /// </summary>
    public class RepositoryEventArgs : EventArgs
    {
        /// <summary> Create repo update event arguments </summary>
        /// <param name="repoLocation">Repo location</param>
        /// <param name="oldStatus">Status before event</param>
        /// <param name="newStatus">Status after event</param>
        public RepositoryEventArgs(string repoLocation, RepoStatus oldStatus, RepoStatus newStatus)
        {
            Location = repoLocation;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }

        /// <summary> Location of repository </summary>
        public string Location { get; }
        /// <summary> Status before event </summary>
        public RepoStatus OldStatus { get; }
        /// <summary> Status after event </summary>
        public RepoStatus NewStatus { get; }
    }
}

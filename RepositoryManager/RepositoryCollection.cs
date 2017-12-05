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
    using Git;
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
        private readonly Timer _timer;
        private readonly ConcurrentDictionary<string, Task> runningUpdates = new ConcurrentDictionary<string, Task>();
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// Load a reposet from a file
        /// </summary>
        /// <param name="filepath"></param>
        public RepositoryCollection(string filepath)
        {
            RepoListFilePath = filepath;
            //if (File.Exists(RepoListFilePath))
            //{
            //    _repositories = JsonConvert.DeserializeObject<List<ISourceRepository>>(File.ReadAllText(RepoListFilePath), JsonSettings);
            //}
            //_repositories = _repositories ?? new List<ISourceRepository>();
            ReloadFromFile();
            _timer = new Timer(TimerTick, null, SampleFrequency, SampleFrequency);
        }

        /// <summary>
        /// Reloads the repo list from file
        /// </summary>
        public void ReloadFromFile()
        {
            lock (_lockObject)
            {
                if (File.Exists(RepoListFilePath))
                {
                    var json = File.ReadAllText(RepoListFilePath);
                    // Legacy fix for a moved class
                    json = json.Replace("RepositoryManager.GitRepository, RepositoryManager", "RepositoryManager.Git.GitRepository, RepositoryManager");
                    _repositories = JsonConvert.DeserializeObject<List<ISourceRepository>>(json, JsonSettings);
                }
                else _repositories = new List<ISourceRepository>();
            }
        }

        private void TimerTick(object tick)
        {
            TriggerScheduledUpdates();
        }

        /// <summary>
        /// Trigger update of repos that should be according to their schedule
        /// </summary>
        public void TriggerScheduledUpdates()
        {
            // Only repos with  schedule, and where lastupdate was long enough ago
            TriggerUpdate(repo => repo.UpdateSchedule != Schedule.Never
                && repo.LastStatusAt.AddMinutes((int)repo.UpdateSchedule) <= DateTime.Now);
        }

        /// <summary>
        /// Trigger repo updates. All updates are done in separate threads
        /// </summary>
        /// <param name="where"></param>
        /// <param name="ignoreRunningUpdates"> If true starts an update even if one is running </param>     
        public void TriggerUpdate(Func<ISourceRepository, bool> where, bool ignoreRunningUpdates=false)
        {
            lock (_lockObject)
            {
                foreach (var repository in _repositories.Where(where))
                {
                    if (runningUpdates.TryGetValue(repository.Location, out Task task))
                    {
                        if (!task.IsCompleted) continue;
                    }

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
            lock (_lockObject)
                File.WriteAllText(RepoListFilePath, JsonConvert.SerializeObject(_repositories, JsonSettings), Encoding.UTF8);
        }

        /// <summary> Add a repository to the collection </summary>
        public void Add(ISourceRepository repo)
        {
            lock (_lockObject)
                _repositories.Add(repo);

            // Raise event
            OnRepositoryListChanged(new RepositoryEventArgs(repo.Location, RepoStatus.Unknown, repo.LastStatus, repo.Name, RepositoryEventType.Added));
        }

        /// <summary> Remove a repository from the collection </summary>
        /// <param name="repositoryLocation"> repo location </param>
        public bool Remove(string repositoryLocation)
        {
            var result = false;
            ISourceRepository repo = null;
            lock (_lockObject)
            {
                repo = _repositories.FirstOrDefault(x => x.Location == repositoryLocation);
                if(repo != null)
                {
                    result = _repositories.Remove(repo);
                }
            }

            if(result)
                // Raise event
                OnRepositoryListChanged(new RepositoryEventArgs(repo.Location, repo.LastStatus, repo.LastStatus, repo.Name, RepositoryEventType.Removed));
            return result;
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
            var eventArgs = new RepositoryEventArgs(repo.Location, previousStatus, repo.LastStatus, repo.Name, RepositoryEventType.Updated);
            OnRepositoryUpdated(eventArgs);

            if(previousStatus != repo.LastStatus)
                OnRepositoryStatusChanged(eventArgs);
        }

        /// <summary>
        /// Trigger an update of a repo
        /// </summary>
        /// <param name="location"></param>
        public void UpdateRepo(string location)
        {
            lock (_lockObject)
            {
                foreach (var repository in _repositories.Where(repo => repo.Location == location))
                {
                    // This does NOT let running updates finish - if something broke this might fix it
                    var repoUnclousure = repository;
                    var newTask = Task.Factory.StartNew(() => CheckRepo(repoUnclousure));
                    runningUpdates.AddOrUpdate(repository.Location, newTask, (loc, oldTask) => newTask);
                }
            }
        }

        /// <summary>
        /// Trigger a pull of a repo
        /// </summary>
        /// <param name="location"></param>
        public void PullRepo(string location)
        {
            lock (_lockObject)
            {
                foreach (var repository in _repositories.Where(repo => repo.Location == location))
                {
                    var repoUnclousure = repository;
                    // If another task is running on this repo, schedule the pull to be done after
                    if (runningUpdates.TryGetValue(repository.Location, out Task task))
                    {
                        if (!task.IsCompleted)
                        {
                            task.ContinueWith(t=> PullRepoTask(repoUnclousure));
                            return;
                        }
                    }

                    var newTask = Task.Factory.StartNew(() => PullRepoTask(repoUnclousure));
                    runningUpdates.AddOrUpdate(repository.Location, newTask, (loc, oldTask) => newTask);
                }
            }
        }

        private void PullRepoTask(ISourceRepository repo)
        {
            var previousStatus = repo.LastStatus;
            repo.Update(true);
            var eventArgs = new RepositoryEventArgs(repo.Location, previousStatus, repo.LastStatus, repo.Name, RepositoryEventType.Updated);
            OnRepositoryUpdated(eventArgs);

            if (previousStatus != repo.LastStatus)
                OnRepositoryStatusChanged(eventArgs);
        }

        /// <summary> Delegate for repo update events </summary>
        public delegate void RepositoryEventHandler(object sender, RepositoryEventArgs e);

        /// <summary> Raised when a repo has been updated </summary>
        public event RepositoryEventHandler RepositoryUpdated;
        /// <summary> Raised when status has changed on a repo </summary>
        public event RepositoryEventHandler RepositoryStatusChanged;
        /// <summary> Raised when a repository is added or removed from the list </summary>
        public event RepositoryEventHandler RepositoryListChanged;

        /// <summary> Overridable event logic </summary>
        protected virtual void OnRepositoryUpdated(RepositoryEventArgs e)
        {
            RepositoryUpdated?.Invoke(this, e);
        }

        /// <summary> Overridable event logic </summary>
        protected virtual void OnRepositoryStatusChanged(RepositoryEventArgs e)
        {
            RepositoryStatusChanged?.Invoke(this, e);
        }

        /// <summary> Overridable event logic </summary>
        protected virtual void OnRepositoryListChanged(RepositoryEventArgs e)
        {
            RepositoryListChanged?.Invoke(this, e);
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
}

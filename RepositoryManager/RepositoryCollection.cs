﻿namespace RepositoryManager
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
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Manages a set of repositories
    /// </summary><inheritdoc />
    public class RepositoryCollection : IRepositoryCollection
    {
        private const int SampleFrequency = 5000;
        /// <summary> Settings for serializing and deserializing JSON </summary>
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto , Converters = new List<JsonConverter>{new StringEnumConverter()} };

        /// <summary> Internal list of repos </summary>
        private List<ISourceRepository> _repositories;
        private Dictionary<string, ISourceRepository> _repositoriesByName;

        private string _repoListFilePath;
        private readonly Timer _timer;
        private readonly ConcurrentDictionary<string, Task> _runningUpdates = new ConcurrentDictionary<string, Task>();
        private readonly object _lockObject = new object();

        private FileListeningMode _useFileListeners;
        private readonly Dictionary<string, DelayedFileSystemWatcher> _fileListeners = new Dictionary<string, DelayedFileSystemWatcher>();

        /// <summary>
        /// Creates a new empty <see cref="RepositoryCollection"/>
        /// </summary>
        public RepositoryCollection()
        {
            _repositories = new List<ISourceRepository>();
            _repositoriesByName = new Dictionary<string, ISourceRepository>();
            _timer = new Timer(TimerTick, null, SampleFrequency, SampleFrequency);
        }

        /// <summary>
        /// Load a reposet from a file
        /// </summary>
        /// <param name="filepath"></param>
        public RepositoryCollection(string filepath)
        {
            _repoListFilePath = filepath;
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
                if (File.Exists(_repoListFilePath))
                {
                    var json = File.ReadAllText(_repoListFilePath);
                    // Legacy fix for a moved class
                    json = json.Replace("RepositoryManager.GitRepository, RepositoryManager", "RepositoryManager.Git.GitRepository, RepositoryManager");
                    _repositories = JsonConvert.DeserializeObject<List<ISourceRepository>>(json, JsonSettings);
                    _repositoriesByName = new Dictionary<string, ISourceRepository>(StringComparer.OrdinalIgnoreCase);

                    foreach (var repository in _repositories)
                    {
                        // Name must be unique
                        if (_repositoriesByName.ContainsKey(repository.Name))
                        {
                            var uniqueIndex = 2;
                            string name;
                            do name = $"{repository.Name} ({uniqueIndex++})";
                            while (_repositoriesByName.ContainsKey(name));
                            repository.Name = name;
                        }
                        _repositoriesByName.Add(repository.Name, repository);
                        repository.RefreshLocalStatus();
                        repository.RepositoryStatusChanged += OnRepoChangedEventPropagator;
                    }
                }
                else _repositories = new List<ISourceRepository>();
            }
        }

        /// <summary>
        /// Save the current list of repos to a file
        /// </summary>
        /// <param name="filepath"> File to save to. If null will use same as when loaded/saved last time </param>
        public void Save(string filepath = null)
        {
            if (filepath != null) _repoListFilePath = filepath;
            if (_repoListFilePath == null) throw new ArgumentNullException(nameof(filepath), "Can not save file - no path specified");
            lock (_lockObject)
                File.WriteAllText(_repoListFilePath, JsonConvert.SerializeObject(_repositories, JsonSettings), Encoding.UTF8);
        }

        /// <summary>
        /// Trigger update of repos according to their schedule
        /// </summary>
        private void TimerTick(object tick)
        {
            // Only repos with a schedule, and where lastupdate was long enough ago
            TriggerUpdate(repo => repo.UpdateSchedule != Schedule.Never
                && repo.LastStatusAt.AddMinutes((int)repo.UpdateSchedule) <= DateTime.Now);
        }

        /// <summary>
        /// Trigger an update of a repo
        /// </summary>
        /// <param name="location"></param>
        /// <param name="force"> If true does not wait for running updates </param>
        public Task TriggerUpdate(string location, bool force)
        {
            lock (_lockObject)
            {
                var repository = _repositories.FirstOrDefault(x => x.Location == location);
                if (!force && _runningUpdates.TryGetValue(repository.Location, out var task) && !task.IsCompleted)
                    return task;

                Trace.TraceInformation("About to update repo " + repository.Name);
                var repoUnclousure = repository;
                var newTask = Task.Factory.StartNew(() => CheckRepo(repoUnclousure));
                _runningUpdates.AddOrUpdate(repository.Location, newTask, (loc, oldTask) => newTask);
                return newTask.ContinueWith(t=>Task.Delay(10000));
            }
        }

        /// <summary>
        /// Trigger repo updates. All updates are done in separate threads
        /// </summary>
        /// <param name="where"> Predicate that must be true to trigger update</param>
        /// <param name="force"> If true starts an update even if one is running </param>     
        public void TriggerUpdate(Func<ISourceRepository, bool> where, bool force = false)
        {
            lock (_lockObject)
            {
                foreach (var repository in _repositories.Where(where))
                {
                    if (!force && _runningUpdates.TryGetValue(repository.Location, out var task) && !task.IsCompleted)
                        continue;

                    Trace.TraceInformation("About to update repo " + repository.Name);
                    var repoUnclousure = repository;
                    var newTask = Task.Factory.StartNew(() => CheckRepo(repoUnclousure));
                    _runningUpdates.AddOrUpdate(repository.Location, newTask, (loc, oldTask) => newTask);
                }
            }
        }

        /// <summary>
        /// Checks if a name is used by this collection
        /// </summary>
        /// <param name="repositoryName"> Repository name to check </param>
        /// <returns> true if exists, false if not </returns>
        public bool ContainsName(string repositoryName)
        {
            return _repositoriesByName.ContainsKey(repositoryName);
        }

        /// <summary>
        /// Gets a repository by it's name
        /// </summary>
        public ISourceRepository GetByName(string repoName)
        {
            return _repositoriesByName.TryGetValue(repoName, out var repo) ? repo : null;
        }

        /// <inheritdoc />
        public void RepositoryModified(ISourceRepository repository)
        {
            lock (_lockObject)
            {
                // Find the repo
                var repo = _repositoriesByName.Single(x => x.Value == repository);
                if (repo.Key != repo.Value.Name)
                {
                    _repositoriesByName.Remove(repo.Key);
                    _repositoriesByName.Add(repository.Name, repository);
                }
            }
        }

        /// <summary> Add a repository to the collection </summary>
        public void Add(ISourceRepository repo)
        {
            lock (_lockObject)
            {
                if(_repositoriesByName.ContainsKey(repo.Name))
                    throw new ArgumentException($"A repository with the name '{repo.Name}' already exist", nameof(repo));
                _repositories.Add(repo);
                _repositoriesByName.Add(repo.Name, repo);
                if (_useFileListeners != FileListeningMode.None)
                {
                    // Since we still use repo location as key, but can have severla (i know, stupid) we must check so we don't create two file listeners for same location
                    if(!_fileListeners.ContainsKey(repo.Location))
                        _fileListeners.Add(repo.Location, new DelayedFileSystemWatcher(_useFileListeners==FileListeningMode.IndexOnly?repo.IndexLocation:repo.Location, repo.RefreshLocalStatus));
                }

                repo.RefreshLocalStatus();
                repo.RepositoryStatusChanged += OnRepoChangedEventPropagator;
            }

            // Raise event
            OnRepositoryListChanged(new RepositoryEventArgs(repo.Location, RepoStatus.Unknown, repo.LastStatus, repo.Name, RepositoryEventType.Added));
        }

        /// <summary>
        /// Forwards the event to any listeners of this object
        /// </summary>
        private void OnRepoChangedEventPropagator(object sender, RepositoryEventArgs args)
        {
            OnRepositoryStatusChanged(args);
        }

        /// <summary> Remove a repository from the collection </summary>
        /// <param name="repositoryLocation"> repo location </param>
        public bool Remove(string repositoryLocation)
        {
            var removedSomething = false;
            ISourceRepository repo;
            lock (_lockObject)
            {
                repo = _repositories.FirstOrDefault(x => x.Location == repositoryLocation);
                if(repo != null)
                {
                    removedSomething = _repositories.Remove(repo);
                    repo.RepositoryStatusChanged -= OnRepoChangedEventPropagator;
                }

                if (_fileListeners.ContainsKey(repositoryLocation))
                {
                    _fileListeners[repositoryLocation].Dispose();
                    _fileListeners.Remove(repositoryLocation);
                }
            }

            // Raise event
            if(removedSomething)
                OnRepositoryListChanged(new RepositoryEventArgs(repo.Location, repo.LastStatus, repo.LastStatus, repo.Name, RepositoryEventType.Removed));
            return removedSomething;
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
            repo.RefreshRemoteStatus();
            var eventArgs = new RepositoryEventArgs(repo.Location, previousStatus, repo.LastStatus, repo.Name, RepositoryEventType.Updated);
            OnRepositoryUpdated(eventArgs);

            //if(previousStatus != repo.LastStatus) OnRepositoryStatusChanged(eventArgs);
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
                    if (_runningUpdates.TryGetValue(repository.Location, out Task task))
                    {
                        if (!task.IsCompleted)
                        {
                            task.ContinueWith(t=> PullRepoTask(repoUnclousure));
                            return;
                        }
                    }

                    var newTask = Task.Factory.StartNew(() => PullRepoTask(repoUnclousure));
                    _runningUpdates.AddOrUpdate(repository.Location, newTask, (loc, oldTask) => newTask);
                }
            }
        }

        private void PullRepoTask(ISourceRepository repo)
        {
            var previousStatus = repo.LastStatus;
            repo.Update(true);
            var eventArgs = new RepositoryEventArgs(repo.Location, previousStatus, repo.LastStatus, repo.Name, RepositoryEventType.Updated);
            OnRepositoryUpdated(eventArgs);

            //if (previousStatus != repo.LastStatus)
            //    OnRepositoryStatusChanged(eventArgs);
        }

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

        /// <summary>
        /// Sets file listening mode
        /// </summary>
        /// <param name="mode"> how to listen on file changes </param>
        public void SetFileListening(FileListeningMode mode)
        {
            lock (_lockObject)
            {
                foreach (var listener in _fileListeners)
                {
                    listener.Value.Dispose();
                }
                _fileListeners.Clear();
                _useFileListeners = mode;
                if (mode == FileListeningMode.None) return;

                foreach (var repository in _repositories)
                {
                    var path = mode== FileListeningMode.IndexOnly ? repository.IndexLocation : repository.Location;
                    var repo2 = repository;

                    if (!_fileListeners.ContainsKey(repo2.Location))
                        _fileListeners.Add(repo2.Location,new DelayedFileSystemWatcher(path, () => repo2.RefreshLocalStatus()));
                }
            }
        }

        /// <summary>
        /// Returns the highest ("worst") status of all repositories.
        /// </summary>
        /// <returns>Status of the repo with highest status, or Unknown if no repos are registered</returns>
        public RepoStatus WorstStatus()
        {
            if (_repositories?.Count > 0)
                return _repositories.Max(x => x.LastStatus);
            return RepoStatus.Unknown;
        }
    }

    /// <summary> Delegate for repo update events </summary>
    public delegate void RepositoryEventHandler(object sender, RepositoryEventArgs e);
}

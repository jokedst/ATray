namespace RepositoryManager.Git
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using LibGit2Sharp;
    using LibGit2Sharp.Handlers;

    /// <summary>
    /// Tracks a local git repository
    /// </summary>
    public class GitRepository : ISourceRepository
    {
        private RepoStatus _lastStatus;

        /// <inheritdoc />
        public string Name { get; set; }
        /// <inheritdoc />
        public string Location { get; set; }
        /// <inheritdoc />
        public RepoStatus LastStatus
        {
            get => _lastStatus;
            private set
            {
                _lastStatus = value;
                LastStatusAt = DateTime.Now;
            }
        }
        /// <inheritdoc />
        public DateTime LastStatusAt { get; protected set; }
        /// <inheritdoc />
        public Schedule UpdateSchedule { get; set; }
        /// <inheritdoc />
        public AutoAction AutomaticAction { get; set; }
        /// <summary> git-specific staus </summary>
        public RepoStatusFlags GitStatus { get; protected set; }

        /// <summary> For serializers </summary>
        protected GitRepository() { }

        /// <summary>
        /// Create new git tracker for given location
        /// </summary>
        public GitRepository(string location) 
            : this(location, null, default(DateTime), Schedule.Never, AutoAction.Fetch, RepoStatus.Unknown)
        {
        }

        /// <summary>
        /// Create new git tracker for given location
        /// </summary>
        public GitRepository(string location, string name, DateTime lastStatusAt, Schedule updateSchedule, AutoAction automaticAction, RepoStatus lastStatus)
        {
            // We assume the path is a directory
            if (location.EndsWith(Path.DirectorySeparatorChar.ToString()))
                location = location.Substring(0, location.Length - 1);
            var pathParts = location.Split(Path.DirectorySeparatorChar);
            Name = pathParts[pathParts.Length - 1];
            if (Name == ".git")
            {
                Name = pathParts[pathParts.Length - 2];
                location = location.Substring(0, location.Length - 5);
            }

            Name = name ?? Name;
            Location = location;
            _lastStatus = lastStatus;
            LastStatusAt = lastStatusAt;
            UpdateSchedule = updateSchedule;
            AutomaticAction = automaticAction;
        }

        /// <inheritdoc />
        public RepoStatus UpdateStatus()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var repo = new Repository(Location))
                {
                    if (!repo.Head.IsTracking) return LastStatus = RepoStatus.Disconnected;
                    var credentialHelper = repo.Config.Get<string>("credential.helper");
                    var origin = repo.Network.Remotes[repo.Head.RemoteName];
                    if (origin == null) return RepoStatus.Error;
                    var fetchOptions = new FetchOptions();
                    if (credentialHelper?.Value == "wincred")
                        fetchOptions.CredentialsProvider = CredentialsProvider.WinCred;
                    else
                        fetchOptions.CredentialsProvider = CredentialsProvider.MicrosoftAlm;
                    
                    try
                    {
                        Commands.Fetch(repo, origin.Name, Enumerable.Empty<string>(), fetchOptions, null);
                    }
                    catch (LibGit2SharpException e)
                    {
                        Trace.TraceInformation($"Fetch remote failed (probably offline): {e.Message}");
                    }
                    RefreshLocalStatus();
                }
            }
            catch (RepositoryNotFoundException)
            {
                LastStatus = RepoStatus.Error;
            }
            
            Trace.TraceInformation($"GLOBAL: Git update took {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
            return LastStatus;
        }

        /// <summary>
        /// Refreshes the status without talking to remote servers
        /// </summary>
        public void RefreshLocalStatus()
        {
            try
            {
                using (var repo = new Repository(Location))
                {
                    var status = repo.RetrieveStatus(new StatusOptions());

                    var dirty = status.IsDirty;
                    var behind = (repo.Head.TrackingDetails.BehindBy ?? 0) != 0;
                    var ahead = (repo.Head.TrackingDetails.AheadBy ?? 0) != 0;

                    var repoStatus = RepoStatusFlags.Clean;
                    if (dirty) repoStatus |= RepoStatusFlags.LocalChanges;
                    if (behind) repoStatus |= RepoStatusFlags.RemoteUnmergedCommits;
                    if (ahead) repoStatus |= RepoStatusFlags.LocalUnpushedCommits;
                    GitStatus = repoStatus;

                    if (dirty)
                    {
                        if (behind)
                        {
                            // ahead doesn't matter
                            LastStatus = RepoStatus.Conflict;
                        }
                        else
                        {
                            // ahead doesn't matter
                            LastStatus = RepoStatus.Dirty;
                        }
                    }
                    else
                    {
                        if (behind)
                        {
                            if (ahead)
                                LastStatus = RepoStatus.Conflict;
                            else
                                LastStatus = RepoStatus.Behind;
                        }
                        else
                        {
                            if (ahead)
                                LastStatus = RepoStatus.Ahead;
                            else
                                LastStatus = RepoStatus.Clean;
                        }
                    }

                    // TODO: If conflict, check if auto-mergable (and if so set status "Mergable"
                    // This can be done in git with merge-tree - 
                    // - first fetch
                    // - get base commit with merge-base (latest common ancestor)
                    // - run merge-tree to see changes. Any "<<" indicates a conflict
                    // -- theoretically the output from merge-tree can be used to update the index (but fuck me if I know how)
                    // fetch + "git merge-tree `git merge-base master 9-branch ` master 9-branch | grep ‘changed in both’"
                    // which might not work on local changes...
                }
            }
            catch (RepositoryNotFoundException)
            {
                LastStatus = RepoStatus.Error;
                GitStatus = RepoStatusFlags.Error;
            }
        }

        /// <inheritdoc />
        public bool Update(bool onlyIfNoMerge)
        {
            var stopwatch = Stopwatch.StartNew();
            var wasUppdated = false;
            try
            {
                using (var repo = new Repository(Location))
                {
                    var options = new MergeOptions();
                    options.FastForwardStrategy = onlyIfNoMerge ? FastForwardStrategy.FastForwardOnly : FastForwardStrategy.Default;
                    var results = repo.MergeFetchedRefs(null, options);
                    Trace.TraceInformation($"GLOBAL: Git merge (status:{results.Status}) took {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
                    wasUppdated = results.Status == MergeStatus.FastForward;
                }
                RefreshLocalStatus();
            }
            catch (Exception e)
            {
                Trace.TraceInformation($"ERROR: Git merge threw exception and took {stopwatch.ElapsedMilliseconds / 1000.0} seconds: {e.Message}");
            }

            return wasUppdated;
        }

        /// <inheritdoc />
        public bool Valid() 
        {
            try
            {
                using (var repo = new Repository(Location))
                {
                    if (repo.Info.WorkingDirectory != null)
                        return true;
                }
            }
            catch (Exception)
            {
                // Ignore exceptions, regardless this is invalid
            }
            return false;
        }

        /// <inheritdoc />
        public IEnumerable<string> PossibleActions(RepoStatus status)
        {
            var ret = new List<string> {"Update"};
            switch (status)
            {
                case RepoStatus.Unknown: return Enumerable.Empty<string>();
                case RepoStatus.Disconnected: return new[] {"Configure remote"};
                case RepoStatus.Error: return new[] {"Error info"};
                    
            }

            if (status==(RepoStatus.Dirty))
            {
                ret.Add("Commit");
            }
                return ret;
        }

        /// <inheritdoc />
        public void PerformAction(string actionName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string IndexLocation => Path.Combine(this.Location, ".git");

        /// <summary>
        /// Checks status of remote without doing a fetch
        /// </summary>
        public void LsRemote()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var options = new RepositoryOptions();
                options.WorkingDirectoryPath = Location;
                using (var repo = new Repository(Location))
                {
                    var origin = repo.Network.Remotes["origin"];
                    if (origin == null) return;

                    CredentialsHandler creds = null;
                    var credentialHelper = repo.Config.Get<string>("credential.helper", ConfigurationLevel.Local);
                    if (credentialHelper?.Value == "wincred")
                        creds = CredentialsProvider.WinCred;
                    else//if (credentialHelper?.Value == "manager")
                    {
                        creds = CredentialsProvider.MicrosoftAlm;
                    }

                    var timer = Stopwatch.StartNew();
                    var refs = Repository.ListRemoteReferences(origin.Url, creds);
                    foreach (var reference in refs)
                    {
                        Console.WriteLine(reference.CanonicalName);
                    }
                    Console.WriteLine("Second loop: " + timer.Elapsed.ToString());

                    Console.WriteLine("---one more time---");

                    timer.Restart();
                    var references = repo.Network.ListReferences(origin, creds);
                    foreach (var reference in references)
                    {
                        Console.WriteLine(reference.CanonicalName);
                    }
                    Console.WriteLine("First loop: " + timer.Elapsed.ToString());

                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
            }
            Trace.TraceInformation($"GLOBAL: Git ls-remote took {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
        }
    }
}
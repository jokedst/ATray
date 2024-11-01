namespace RepositoryManager
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using SharpSvn;

    /// <summary>
    /// Tracks a SVN repository
    /// </summary>
    /// <remarks>
    /// This will likely never be finished since I've migrated all my SVN stuff to git
    /// </remarks>
    public class SvnRepository : ISourceRepository
    {
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
                var previousValue = _lastStatus;
                _lastStatus = value;
                LastStatusAt = DateTime.Now;

                if (previousValue != _lastStatus)
                {
                    Trace.WriteLine($"Git repo '{Name}' status changed from {previousValue} to {_lastStatus}");
                    OnRepositoryStatusChanged(new RepositoryEventArgs(Location, previousValue, _lastStatus, Name, RepositoryEventType.Updated));
                }
            }
        }

        /// <inheritdoc />
        public DateTime LastStatusAt { get; private set; }
        /// <inheritdoc />
        public Schedule UpdateSchedule { get; set; }
        /// <inheritdoc />
        public AutoAction AutomaticAction { get; set; }

        /// <inheritdoc />
        public bool Valid()
        {
            using (var client = new SvnClient())
            {
                return client.TryGetRepository(Location, out Uri uri, out Guid id);
            }
        }

        private static readonly TraceSource Log = new TraceSource(nameof(SvnRepository));
        private RepoStatus _lastStatus;

        /// <summary>
        /// Create new SVN tracker for given location
        /// </summary>
        public SvnRepository(string location, string name = null, DateTime lastStatusAt = default(DateTime), Schedule updateSchedule = Schedule.Never, AutoAction automaticAction = AutoAction.Fetch, RepoStatus lastStatus = RepoStatus.Unknown)
        {
            // We assume the path is a directory
            if (location.EndsWith(Path.DirectorySeparatorChar.ToString()))
                location = location.Substring(0, location.Length - 1);
            var pathParts = location.Split(Path.DirectorySeparatorChar);
            Name = pathParts[pathParts.Length - 1];
            if (Name == ".svn")
            {
                Name = pathParts[pathParts.Length - 2];
                location = location.Substring(0, location.Length - 5);
            }

            Location = location;
            _lastStatus = lastStatus;

            Name = name ?? Name;
            LastStatusAt = lastStatusAt;
            UpdateSchedule = updateSchedule;
            AutomaticAction = automaticAction;
        }

        /// <inheritdoc/>
        public RepoStatus RefreshRemoteStatus()
        {
            var stopwatch = Stopwatch.StartNew();
            using (SvnClient client = new SvnClient())
            {
                //client.Log(@"G:\Projects\RSH\InboxEDI", new SvnLogArgs{Limit = 1}, (sender, eventArgs) => Console.WriteLine(eventArgs.LogMessage));
                Collection<SvnLogEventArgs> logItems;
                client.GetLog(Location, new SvnLogArgs { Limit = 1 }, out logItems);
                Console.WriteLine(logItems[0].Revision + ": " + logItems[0].LogMessage);

                Collection<SvnStatusEventArgs> statuses;
                client.GetStatus(Location, new SvnStatusArgs { RetrieveRemoteStatus = true }, out statuses);
                //foreach (var status in statuses)
                //{
                //    Console.WriteLine(status.Revision + " " + status.Path);
                //}

                // Do a dry-run merge to see if conflicts would happen. WARN: SLOW!!
                //var svnMergeArgs = new SvnMergeArgs {DryRun = true};
                //var res = client.Merge(this.Location, this.Location, new SvnRevisionRange(SvnRevision.Base, SvnRevision.Head), svnMergeArgs);
                //if (svnMergeArgs.Warnings != null)
                //    foreach (var svnException in svnMergeArgs.Warnings)
                //    {
                //        Console.WriteLine(svnException.Message);
                //    }
            }
            
            Log.TraceInformation($"sourece: SVN update took {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
            Trace.TraceInformation($"gloabl: SVN update took {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
            LastStatusAt = DateTime.Now;
            return LastStatus;
        }

        /// <summary> NOT IMPLEMENTED </summary>
        public bool Update(bool onlyIfNoMerge)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string IndexLocation => Path.Combine(this.Location, ".svn");

        /// <inheritdoc />
        public void RefreshLocalStatus()
        {
            //throw new NotImplementedException();
            Trace.TraceInformation($"SVN RefreshLocalStatus for {Location}");
        }

        /// <inheritdoc />
        public event RepositoryEventHandler RepositoryStatusChanged;

        /// <summary> Overridable event logic </summary>
        protected virtual void OnRepositoryStatusChanged(RepositoryEventArgs e)
        {
            RepositoryStatusChanged?.Invoke(this, e);
        }
    }
}
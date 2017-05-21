using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using SharpSvn;

namespace RepositoryManager
{
    /// <summary>
    /// Tracks a SVN repository
    /// </summary>
    /// <remarks>
    /// This will likely never be finished since I've migrated all my SVN stuff to git
    /// </remarks>
    public class SvnRepository : ISourceRepository
    {
        /// <inheritdoc />
        public string Name { get; }
        /// <inheritdoc />
        public string Location { get; }
        /// <inheritdoc />
        public RepoStatus LastStatus { get; }
        /// <inheritdoc />
        public DateTime LastStatusAt { get; private set; }
        /// <inheritdoc />
        public Schedule UpdateSchedule { get; set; }
        /// <inheritdoc />
        public AutoAction AutomaticAction { get; set; }

        private static readonly TraceSource Log = new TraceSource(nameof(SvnRepository));

        public SvnRepository(string path)
        {
            // We assume the path is a directory
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path = path.Substring(0, path.Length - 1);
            var pathParts = path.Split(Path.DirectorySeparatorChar);
            this.Name = pathParts[pathParts.Length - 1];
            if (this.Name == ".svn")
            {
                this.Name = pathParts[pathParts.Length - 2];
                path = path.Substring(0, path.Length - 5);
            }

            this.Location = path;
            this.LastStatus = RepoStatus.Unknown;
        }

        public RepoStatus UpdateStatus()
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
            throw new System.NotImplementedException();
        }
    }
}
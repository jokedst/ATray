namespace RepositoryManager
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    ///     Watches a file path and fires an event when it changes, but waits until current "burst" of events has stopped for 2
    ///     seconds
    /// </summary>
    /// <inheritdoc />
    public class DelayedFileSystemWatcher : IDisposable
    {
        private readonly PostponedEvent _postponedEvent;

        private readonly FileSystemWatcher _watcher;

        /// <summary>
        ///     Creates a new DelayedFileSystemWatcher
        /// </summary>
        /// <param name="path"> The file system path to watch </param>
        /// <param name="callbackAction"> Action to perform on file change </param>
        public DelayedFileSystemWatcher(string path, Action callbackAction)
        {
            _postponedEvent = new PostponedEvent(2000, callbackAction);
            _watcher = new FileSystemWatcher(path) {EnableRaisingEvents = true};
            _watcher.Changed += (s, e) =>
            {
                Trace.TraceInformation($"Watched path {path} modified ({e.ChangeType.ToString()} {e.FullPath})");
                _postponedEvent.StartOrUpdate();
            };
        }

        /// <summary> Releases all resources used by the current instance of <see cref="DelayedFileSystemWatcher" />. </summary>
        /// <inheritdoc />
        public void Dispose()
        {
            _watcher?.Dispose();
            _postponedEvent?.Dispose();
        }
    }
}
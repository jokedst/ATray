namespace RepositoryManager
{
    /// <summary>
    /// How we listen to file events
    /// </summary>
    public enum FileListeningMode
    {
        /// <summary> File events ae ignored </summary>
        None,
        /// <summary> Only local commits to the index </summary>
        IndexOnly,
        /// <summary> All file events </summary>
        AllChanges
    }
}
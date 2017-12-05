namespace RepositoryManager
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Generic tools for a source repository technology
    /// </summary>
    public interface ISourceControlProvider
    {
        /// <summary>
        /// Find source repositories in the subfolders of <paramref name="rootPath"/>
        /// </summary>
        /// <param name="rootPath"> Root directory to search </param>
        /// <returns> List of paths that contains a repository </returns>
        IEnumerable<string> FindRepositories(string rootPath);

        /// <summary>
        /// Returns current (local) status of a given repository directory
        /// </summary>
        /// <param name="repoPath"></param>
        /// <returns></returns>
        RepoStatus GetStatus(string repoPath);
    }
}
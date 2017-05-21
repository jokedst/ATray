using System.Collections.Generic;

namespace RepositoryManager
{
    using System.Linq;

    public interface ISourceControlProvider
    {
        IEnumerable<string> FindRepositories(string rootPath);

        RepoStatus GetStatus(string repoPath);
    }
}
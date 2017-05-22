using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RepositoryManager
{
    public class SvnProvider : ISourceControlProvider {
        public IEnumerable<string> FindRepositories(string rootPath)
        {
            var gitDirs = Directory.EnumerateDirectories(rootPath, ".svn", SearchOption.AllDirectories);
            return gitDirs.Select(x => x.Substring(0, x.Length - 4));
        }

        public RepoStatus GetStatus(string repoPath)
        {
            throw new System.NotImplementedException();
        }
    }
}
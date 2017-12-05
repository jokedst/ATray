namespace RepositoryManager.Git
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CredentialManagement;
    using LibGit2Sharp;
    
    /// <inheritdoc/>
    public class GitProvider : ISourceControlProvider
    {
        /// <inheritdoc/>
        public IEnumerable<string> FindRepositories(string rootPath)
        {
            var gitDirs = Directory.EnumerateDirectories(rootPath, ".git", SearchOption.AllDirectories);
            return gitDirs.Select(x => x.Substring(0, x.Length - 4));
        }

        /// <inheritdoc/>
        public RepoStatus GetStatus(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                var credentialHelper = repo.Config.Get<string>("credential.helper");
                var origin = repo.Network.Remotes["origin"];
                if (origin == null) return RepoStatus.Error;
                var fetchOptions = new FetchOptions();
                if (credentialHelper != null && credentialHelper.Value == "wincred") fetchOptions.CredentialsProvider = WinCredCredentialsProvider;
                ////repo.Network.Fetch(origin, fetchOptions);
                Commands.Fetch(repo, origin.Name, Enumerable.Empty<string>(), fetchOptions, null);
                var status = repo.RetrieveStatus();

                var dirty = status.IsDirty;
            }

            return RepoStatus.Error;
        }

        private static Credentials WinCredCredentialsProvider(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var creds = new Credential { Target = url, Type = CredentialType.Generic };
            creds.Load();

            return new UsernamePasswordCredentials { Username = creds.Username, Password = creds.Password };
        }
    }
}
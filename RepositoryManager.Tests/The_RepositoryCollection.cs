using RepositoryManager.Git;

namespace RepositoryManager.Tests
{
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using NUnit.Framework;
    using RepositoryManager;

    [TestFixture]
    public class RepositoryCollectionFacts
    {
        private string _path;
        protected string dir(string repo) => Path.Combine(_path, repo);

        [OneTimeSetUp]
        public void Prepair()
        {
            // Unzip test dirs
            _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_path);
            var zipPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestRepos.zip");
            ZipFile.ExtractToDirectory(zipPath, _path);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            Directory.Delete(_path, true);
        }

        [Test]
        public void can_find_repos_xps()
        {
            var repos = RepositoryCollection.FindRepositories(_path);

            Assert.True(repos.Any(x => x.Name == "Clean"));
            Assert.True(repos.Any(x => x.Name == "Dirty")); 
            Assert.True(repos.Any(x => x.Name == "OnLocalBranch"));
        }

        [Test]
        public void can_find_git_repos()
        {
            var repos = RepositoryCollection.FindRepositories(_path, RepositoryType.Git);

            Assert.True(repos.Any(x => x.Name == "Clean"));
            Assert.True(repos.Any(x => x.Name == "Dirty"));
            Assert.True(repos.Any(x => x.Name == "OnLocalBranch"));
        }

        [Test]
        public void can_get_dirty_status_from_dir()
        {
            var dirty = new GitRepository(dir("Dirty"));
            var status = dirty.RefreshRemoteStatus();
            Assert.AreEqual(RepoStatus.Dirty,status);
        }

        [Test]
        public void Status_behind_if_master_behind_when_on_local_branch()
        {
            var dirty = new GitRepository(dir("BranchDetachedMasterBehind"));
            var status = dirty.RefreshRemoteStatus();
            Assert.AreEqual(RepoStatus.Behind, status);
        }

        [Test]
        public void can_add_repos_and_check_status()
        {
            var repos = new RepositoryCollection();
            var branchedRepo = new GitRepository(dir("OnLocalBranch"));
            branchedRepo.RefreshLocalStatus();
            repos.Add(branchedRepo);
            Assert.AreEqual(RepoStatus.Clean, repos.WorstStatus());
        }
    }
}

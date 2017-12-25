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
            Assert.AreEqual(2, repos.Count);
        }

        [Test]
        public void can_find_git_repos()
        {
            var repos = RepositoryCollection.FindRepositories(_path, RepositoryType.Git);

            Assert.True(repos.Any(x => x.Name == "Clean"));
            Assert.AreEqual(2, repos.Count);
        }

        //[Test]
        //public void can_find_svn_repos()
        //{
        //    var repos = RepositoryCollection.FindRepositories(_path, RepositoryType.Svn);

        //    Assert.True(repos.Any(x => x.Name == "svnClean"));
        //    Assert.AreEqual(3, repos.Count);
        //}
    }
}

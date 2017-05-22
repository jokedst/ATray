namespace RepositoryManager.Tests
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using NUnit.Framework;
    using RepositoryManager;

    [TestFixture]
    public class The_RepositoryCollection
    {
        private string _path;

        [OneTimeSetUp]
        public void Prepair()
        {
            // Tricky to do portable without e.g. unzipping a bunch of repos in a gitignored dir... anyway
            if (Environment.MachineName == "JOCKESTOR")
                _path = @"E:\Projects\Misc\Gitest.testRepos";

            // TODO: Unzip test dirs
            //var _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            //ZipFile.ExtractToDirectory("path/to/zip", _path);
        }

        [Test]
        public void can_find_repos()
        {
            var repos = RepositoryCollection.FindRepositories(_path);

            Assert.True(repos.Any(x => x.Name == "gitMergable"));
            Assert.AreEqual(8, repos.Count);
        }

        [Test]
        public void can_find_git_repos()
        {
            var repos = RepositoryCollection.FindRepositories(_path, RepositoryType.Git);

            Assert.True(repos.Any(x => x.Name == "gitMergable"));
            Assert.AreEqual(5, repos.Count);
        }

        [Test]
        public void can_find_svn_repos()
        {
            var repos = RepositoryCollection.FindRepositories(_path, RepositoryType.Svn);

            Assert.True(repos.Any(x => x.Name == "svnClean"));
            Assert.AreEqual(3, repos.Count);
        }
    }
}

namespace RepositoryManager.Tests
{
    using NUnit.Framework;
    using RepositoryManager;

    [TestFixture]
    public class GitRepository_constructor
    {
        [TestCase(@"c:\hej\du", @"c:\hej\du")]
        [TestCase(@"c:\hej\du", @"c:\hej\du\")]
        [TestCase(@"c:\hej\du", @"c:\hej\du\.git")]
        [TestCase(@"c:\hej\du", @"c:\hej\du\.git\")]
        public void can_figure_out_the_repo_path(string actualPath, string givenPath)
        {
            Assert.AreEqual(actualPath, new GitRepository(givenPath).Location);
        }

        [TestCase(@"du", @"c:\hej\du")]
        [TestCase(@"du", @"c:\hej\du\")]
        [TestCase(@"du", @"c:\hej\du\.git")]
        [TestCase(@"du", @"c:\hej\du\.git\")]
        public void can_figure_out_the_repo_name(string repoName, string givenPath)
        {
            Assert.AreEqual(repoName, new GitRepository(givenPath).Name);
        }

        [Test]
        public void leaves_the_repo_uninitialized()
        {
            var repo = new GitRepository("random string");

            Assert.AreEqual(RepoStatus.Unknown, repo.LastStatus);
        }
    }
}

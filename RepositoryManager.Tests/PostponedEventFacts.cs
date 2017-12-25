namespace RepositoryManager.Tests
{
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class PostponedEventFacts
    {
        [Test]
        public void FiresEvent()
        {
            var fires = 0;
            var pe = new PostponedEvent(100, () => fires++);

            pe.StartOrUpdate();
            Thread.Sleep(200);

            Assert.AreEqual(1,fires);
        }

        [Test]
        public void DontFireEventPrematurly()
        {
            var fires = 0;
            var pe = new PostponedEvent(50, () => fires++);

            Thread.Sleep(200);
            Assert.AreEqual(0, fires);

            pe.StartOrUpdate();
            Thread.Sleep(200);

            Assert.AreEqual(1, fires);
        }

        [Test]
        public void PostponesEvent()
        {
            var fires = 0;
            var pe = new PostponedEvent(100, () => fires++);

            pe.StartOrUpdate();
            Thread.Sleep(50);
            pe.StartOrUpdate();
            Thread.Sleep(50);
            pe.StartOrUpdate();
            Thread.Sleep(50);
            pe.StartOrUpdate();
            Thread.Sleep(50);
            pe.StartOrUpdate();
            Thread.Sleep(150);

            Assert.AreEqual(1, fires);
        }

        [Test]
        public void MaxReachedFiresEvent()
        {
            var fires = 0;
            var pe = new PostponedEvent(100000, () => fires++, 3);

            pe.StartOrUpdate();
            pe.StartOrUpdate();
            Assert.AreEqual(0, fires);
            pe.StartOrUpdate();

            Assert.AreEqual(1, fires);
        }

        [Test]
        public void CanFireSeveralTimes()
        {
            var fires = 0;
            var pe = new PostponedEvent(50, () => fires++, 3);

            pe.StartOrUpdate();
            Thread.Sleep(100);
            pe.StartOrUpdate();
            Thread.Sleep(100);
            pe.StartOrUpdate();
            Thread.Sleep(100);

            Assert.AreEqual(3, fires);
        }
    }
}

namespace Atray.Tests
{
    using System;
    using ATray.Activity;
    using NUnit.Framework;

    [TestFixture]
    public class AtrayFacts
    {
        [Test]
        public void Something()
        {
            var m = new MonthActivities(2000, 1);
            
            Assert.AreEqual(Environment.MachineName, m.ComputerName);
        }
    }
}

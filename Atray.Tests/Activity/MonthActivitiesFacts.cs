namespace Atray.Tests.Activity
{
    using System;
    using ATray.Activity;
    using NUnit.Framework;

    [TestFixture]
    public class MonthActivitiesFacts
    {
        [Test]
        public void New_MonthActivities_gets_local_computer_name()
        {
            var m = new MonthActivities(2000, 1);
            
            Assert.AreEqual(Environment.MachineName, m.ComputerName);
        }
    }
}

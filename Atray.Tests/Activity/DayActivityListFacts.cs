using System;
using System.Linq;
using ATray.Activity;
using NUnit.Framework;

namespace Atray.Tests.Activity
{
    public class RangeContainerFacts
    {
        [Test]
        public void CanCombine()
        {
            var list1 = new RangeContainer<uint>((a, b) => Math.Abs(a - b) == 1) { { 0, 100 }, { 200, 300 } };
            var list2 = new RangeContainer<uint>((a, b) => Math.Abs(a - b) == 1) { { 90, 110 }, { 190, 310 } };

            list1.Add(list2);

            Assert.AreEqual(2,list1.Count);
            Assert.True(list1.Any(x => x.Start == 0 && x.End == 110));
            Assert.True(list1.Any(x => x.Start == 190 && x.End == 310));
        }
    }
}

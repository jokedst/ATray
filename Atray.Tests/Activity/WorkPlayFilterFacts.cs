namespace Atray.Tests.Activity
{
    using System.Linq;
    using ATray.Activity;
    using NUnit.Framework;

    [TestFixture]
    public class WorkPlayFilterFacts
    {
        private readonly WorkPlayFilter _simpleFilter;
        private readonly WorkPlayFilter _complexFilter;

        public WorkPlayFilterFacts()
        {
            _simpleFilter = new WorkPlayFilter();
            _simpleFilter.AddWorkProgram("work");
            _simpleFilter.AddPlayProgram("play");

            _complexFilter = new WorkPlayFilter();
            _complexFilter.AddWorkComputer("workPC");
            _complexFilter.AddPlayComputer("playPC");
        }

        private void GenerateDay(MonthActivities history,params (uint start, bool active, string exe, string title)[] p)
        {
            byte dayNr = history.Days.Keys.OrderBy(x => x).LastOrDefault();
            var day = new DayActivityList(history, ++dayNr);
            history.Days.Add(dayNr, day);

            for (var i = 0; i < p.Length; i++)
            {
                var act = p[i];
                var end = i + 1 < p.Length ? p[i + 1].start : act.start + 1;
                day.Add(new ActivitySpan(day, act.start,end, act.active, act.exe,act.title));
            }
        }

        [Test]
        public void Can_classify_work()
        {
            var o = new WorkPlayFilter();
           
            o.AddWorkProgram("work.exe");

            var history = new MonthActivities(2018, 2, "AL");
            var day = new DayActivityList(history, 1);
            history.Days.Add(1, day);
            day.Add(new ActivitySpan(history, day)
            {
                StartSecond = 100,
                EndSecond = 1000,
                WasActive = true,
                ApplicationNameIndex = history.GetApplicationNameIndex("work.exe"),
                WindowTitleIndex = history.GetWindowTitleIndex("cool project")
            });

            Assert.AreEqual(WorkPlayType.Unknown, day[0].Classification);
            o.Classify(history);
            Assert.AreEqual(WorkPlayType.Work, day[0].Classification);
        }

        [Test]
        public void Can_interpolate_between_work_and_play()
        {
            var history = new MonthActivities(2018, 2, "AL");
            GenerateDay(history,
                (50,true,"meh",""),
                (100, true, "work", "file1"),
                (200, true, "meh",""),
                (5000, false, "meh2",""), 
                (5100, true, "play", "" ),
                (10000, true, "meh",""));

            _simpleFilter.Classify(history);

            Assert.AreEqual(WorkPlayType.Work, history.Days[1][0].Classification);
            Assert.AreEqual(WorkPlayType.Work, history.Days[1][1].Classification);
            Assert.AreEqual(WorkPlayType.Work, history.Days[1][2].Classification);
            Assert.AreEqual(WorkPlayType.Play, history.Days[1][3].Classification);
            Assert.AreEqual(WorkPlayType.Play, history.Days[1][4].Classification);
            Assert.AreEqual(WorkPlayType.Play, history.Days[1][5].Classification);
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace ATray.Activity
{
    /// <summary>
    /// Makes the history easier to read by removing small acivities and merging small gaps
    /// </summary>
    /// <remarks>
    /// We can't modify the existing activities, since that would (for current month) be saved and change the history 
    /// </remarks>
    class Blurrer
    {
        // Configurable l8r?
        const int FillGapsSmallerThanSeconds = 10*60;
        const int RemoveLonersSmallerThanSeconds = 10*60;

        public Dictionary<string, MonthActivities> Blur(Dictionary<string, MonthActivities> history)
        {
            return history.ToDictionary(x => x.Key, x => Blur(x.Value));
        }

        public MonthActivities Blur(MonthActivities monthActivities)
        {
            var m = new MonthActivities(monthActivities);
            foreach (var day in m.Days)
            {
                Blur(day.Value);
            }

            return m;
        }

        /// <summary>
        /// Actually modifies the day
        /// </summary>
        private void Blur(DayActivityList day)
        {
            // Fill small gaps. 
            uint lastActiveSecond = 0;
            var lastActiveIndex = -1;
            for (int i = 0; i < day.Count; i++)
            {
                var act = day[i];
                if (act.WasActive)
                {
                    if (act.StartSecond - lastActiveSecond < FillGapsSmallerThanSeconds && act.StartSecond>FillGapsSmallerThanSeconds)
                    {
                        var middlesecond = (lastActiveSecond + act.StartSecond) / 2;
                        day[lastActiveIndex].EndSecond = middlesecond;
                        day[i].StartSecond = middlesecond + 1;

                        // Delete any activities in this gap
                        for (int toRemove = i - 1; toRemove > lastActiveIndex; toRemove--)
                        {
                            day.RemoveAt(toRemove);
                            i--;
                        }
                    }

                    lastActiveIndex = i;
                    lastActiveSecond = act.EndSecond;
                }
            }

            // Find loners
            var active = day.Where(x => x.WasActive).Select(a => new[] {a.StartSecond, a.EndSecond}).ToList();
            for (var i = 0; i < active.Count-1; i++)
            {
                while (i < active.Count - 1&&  active[i + 1][0] == active[i][1] + 1)
                {
                    active[i][1] = active[i + 1][1];
                    active.RemoveAt(i + 1);
                }
            }

            foreach (var loner in active.Where(a => a[1] - a[0] < RemoveLonersSmallerThanSeconds))
            {
                day.RemoveAll(a => a.StartSecond >= loner[0] && a.EndSecond <= loner[1]);
            }
        }
    }
}

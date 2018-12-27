using System;
using System.Collections.Generic;
using System.Linq;

namespace ATray.Activity
{
    /// <summary>
    /// Makes the history easier to read by removing small activities and merging small gaps
    /// </summary>
    /// <remarks>
    /// We can't modify the existing activities, since that would (for current month) be saved and change the history 
    /// </remarks>
    class Blurrer
    {
        private int _blurAmount = 25;

        public Blurrer(int blurAmount = 25)
        {
            this.BlurAmount = blurAmount;
        }

        /// <summary>
        /// Amount of blur, 0-100, where 100 will remove features smaller than 2.5 hours
        /// </summary>
        public int BlurAmount
        {
            get => _blurAmount;
            set => _blurAmount = Math.Min(Math.Max(value, 0), 100);
        }

        /// <summary>
        /// Blurs the history, using the set <see cref="BlurAmount"/>. Returns a copy, original is unmodified.
        /// </summary>
        /// <param name="history"> History to blur </param>
        /// <returns> Blurred copy of <paramref name="history"/></returns>
        public Dictionary<string, MonthActivities> Blur(Dictionary<string, MonthActivities> history)
        {
            return history.ToDictionary(x => x.Key, x => Blur(x.Value, _blurAmount*_blurAmount));
        }

        /// <summary>
        /// Blurs the <paramref name="history"/> (returns a copy, original is unmodified).
        /// </summary>
        /// <param name="history"> History to blur </param>
        /// <param name="secondsRequired"> All features smaller than this will be removed </param>
        /// <returns> Blurred copy of <paramref name="history"/></returns>
        public Dictionary<string, MonthActivities> Blur(Dictionary<string, MonthActivities> history, int secondsRequired)
        {
            return history.ToDictionary(x => x.Key, x => Blur(x.Value, secondsRequired));
        }

        private MonthActivities Blur(MonthActivities monthActivities, int secondsRequired)
        {
            var m = new MonthActivities(monthActivities);
            foreach (var day in m.Days)
            {
                Blur(day.Value, secondsRequired);
            }

            return m;
        }

        /// <summary>
        /// Actually modifies the day, removing all features smaller than <paramref name="secondsRequired"/>
        /// </summary>
        private void Blur(DayActivityList day, int secondsRequired)
        {
            // Fill small gaps. 
            uint lastActiveSecond = 0;
            var lastActiveIndex = -1;
            for (int i = 0; i < day.Count; i++)
            {
                var act = day[i];
                if (act.WasActive)
                {
                    if (act.StartSecond - lastActiveSecond < secondsRequired && act.StartSecond > secondsRequired)
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
            for (var i = 0; i < active.Count - 1; i++)
            {
                while (i < active.Count - 1 && active[i + 1][0] == active[i][1] + 1)
                {
                    active[i][1] = active[i + 1][1];
                    active.RemoveAt(i + 1);
                }
            }

            foreach (var loner in active.Where(a => a[1] - a[0] < secondsRequired))
            {
                day.RemoveAll(a => a.StartSecond >= loner[0] && a.EndSecond <= loner[1]);
            }
        }
    }
}

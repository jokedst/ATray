using System;
using System.Collections.Generic;

namespace ATray.Activity
{
    /// <summary>
    /// Classifies activities as work or play
    /// </summary>
    public class WorkPlayFilter
    {
        /// <summary> All activities on these computers are always work </summary>
        public List<string> WorkComputers { get; } = new List<string>();

        /// <summary> All activities in these programs are always work </summary>
        public List<string> WorkPrograms { get; } = new List<string>();

        /// <summary>
        /// Filters activities so only work or play remains
        /// </summary>
        /// <param name="history"></param>
        /// <param name="workorplay"></param>
        /// <returns></returns>
        public Dictionary<string, MonthActivities> Filter(Dictionary<string, MonthActivities> history, WorkPlayType workorplay)
        {
            if (workorplay == WorkPlayType.Both) return history;
            

            throw new NotImplementedException();
        }
    }
}

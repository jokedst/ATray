using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text.RegularExpressions;
using ATray.Dialogs;

namespace ATray.Activity
{
    using ATray;

    /// <summary>
    /// Classifies activities as work or play
    /// </summary>
    /// <remarks>
    /// It works by classifyig a set of programs as "work" or "play".
    /// All unclassified acivities in direct proximity is classified as the same.
    /// </remarks>
    public class WorkPlayFilter
    {
        protected class Pattern
        {
            public Regex Reg { get; }
            public int Prio { get; }
            public bool IsWork { get; }

            public Pattern(int prio, string pattern, bool isWork)
            {
                this.Prio = prio;
                this.IsWork = isWork;
                this.Reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            public string OriginalPattern => Reg.ToString();
        }

        protected Dictionary<string, Pattern> WorkPlayPatterns = new Dictionary<string, Pattern>();

        public void Classify(MonthActivities history)
        {
            var patterns = WorkPlayPatterns.Values.OrderBy(x => x.Prio).ToList();
            foreach (var day in history.Days)
            {
                WorkPlayType lastType = WorkPlayType.Unknown;
                var unsureActs = new List<ActivitySpan>();

                for (var a = 0; a < day.Value.Count; a++)
                {
                    var act = day.Value[a];
                    var akey = Key(history.ComputerName, act.ApplicationName(), act.WindowTitle());
                    
                    var found = patterns.FirstOrDefault(x => x.Reg.IsMatch(akey));
                    if (found == null)
                    {
                        unsureActs.Add(act);
                    }
                    else
                    {
                        var thisActType = found.IsWork ? WorkPlayType.Work : WorkPlayType.Play;

                        if (unsureActs.Any())
                        {
                            if(lastType == WorkPlayType.Unknown || lastType == thisActType)
                                foreach (var unsureAct in unsureActs)
                                    unsureAct.Classification = thisActType;
                            else
                            {
                                // last was X, this is Y, how to classify those in between? Meh, give them roughly half each.
                                var middleSecond = (unsureActs[0].StartSecond + unsureActs.Last().EndSecond) / 2;
                                foreach (var unsureAct in unsureActs)
                                {
                                    var actMiddleSecond = (unsureAct.StartSecond + unsureAct.EndSecond) / 2;
                                    unsureAct.Classification = (actMiddleSecond<middleSecond?lastType:thisActType);
                                }
                            }
                            unsureActs.Clear();
                        }

                        act.Classification = thisActType;
                        lastType = thisActType;
                    }
                }

                if (unsureActs.Count == 0) continue;

                if (lastType == WorkPlayType.Unknown)
                {
                    // No activity during this day could be classified
                    var date = new DateTime(history.Year, history.Month, day.Key);
                    lastType = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
                        ? WorkPlayType.Play
                        : WorkPlayType.Work;
                }

                foreach (var act in unsureActs)
                {
                    act.Classification = lastType;
                }
            }
        }

        public string Key(string computer, string program, string title) => $"^{computer}\t{program}\t{title}$";
        public string KeyPattern(string computer, string program, string title) => $"^{computer ?? ".*"}\t{program ?? ".*"}\t{title ?? ".*"}$";
        
        protected void AddPattern(Pattern playPattern)
        {
            if (!WorkPlayPatterns.ContainsKey(playPattern.OriginalPattern))
                WorkPlayPatterns.Add(playPattern.OriginalPattern, playPattern);
        }

        public void AddWorkComputer(string workpc) => AddPattern(new Pattern(1, KeyPattern(workpc, null, null), true));
        public void AddPlayComputer(string playpc) => AddPattern(new Pattern(2, KeyPattern(playpc, null, null), false));
        public void AddWorkPattern(string exe, string titlePattern) => AddPattern(new Pattern(3, KeyPattern(null, exe, titlePattern), true));
        public void AddPlayPattern(string exe, string titlePattern) => AddPattern(new Pattern(4, KeyPattern(null, exe, titlePattern), false));
        public void AddWorkProgram(string exe) => AddPattern(new Pattern(5, KeyPattern(null, exe, null), true));
        public void AddPlayProgram(string exe) => AddPattern(new Pattern(6, KeyPattern(null, exe, null), false));
    }
}

namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    [Description("Work/Play Classifyer"), Category("Activity"), DisplayName("aname")]
    public class ActivityClassifyer
    {
        [Description("Name of executable"), Category("Classifyer")]
        public string Executable { get; set; }
        [Description("Window title pattern"), Category("Classifyer")]
        public string WindowTitleRegEx { get; set; }
        public ActivityClassifyer() { }
        public override string ToString()
        {
            return Executable + (string.IsNullOrEmpty(WindowTitleRegEx) ? "" : "\\"+ WindowTitleRegEx);
        }
    }
    public class SomeTypeEditor : CollectionEditor
    {

        public SomeTypeEditor(Type type) : base(type) { }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object result = base.EditValue(context, provider, value);

            // assign the temporary collection from the UI to the property
            ((Configuration)context.Instance).WorkActivities = (List<ActivityClassifyer>)result;

            return result;
        }

        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            form.Shown += (sender, args) => ShowDescription(form);
            //form.Shown += delegate { ShowDescription(form); };
            return form;
        }
        static void ShowDescription(Control control)
        {
            if (control is PropertyGrid grid) grid.HelpVisible = true;
            foreach (Control child in control.Controls)
            {
                ShowDescription(child);
            }
        }

        protected override string GetDisplayText(object value)
        {
            if (value is ActivityClassifyer classifyer)
                return classifyer.ToString();
            return base.GetDisplayText(value);
        }
    }

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
            public Regex RegularExpression { get; }
            public int Prio { get; }
            public bool IsWork { get; }

            public Pattern(int prio, string pattern, bool isWork)
            {
                this.Prio = prio;
                this.IsWork = isWork;
                this.RegularExpression = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            public string OriginalPattern => RegularExpression.ToString();
        }

        protected Dictionary<string, Pattern> WorkPlayPatterns = new Dictionary<string, Pattern>();

        public void Classify(MonthActivities history, GuessWorkPlay guessWhenUnknown = GuessWorkPlay.Agressive)
        {
            var patterns = WorkPlayPatterns.Values.OrderBy(x => x.Prio).ToList();
            foreach (var day in history.Days)
            {
                WorkPlayType lastType = WorkPlayType.Unknown;
                var unsureActs = new List<ActivitySpan>();
                uint lastActiveSecond = 0;

                for (var a = 0; a < day.Value.Count; a++)
                {
                    var act = day.Value[a];
                    var akey = Key(history.ComputerName, act.ApplicationName(), act.WindowTitle());

                    var firstAfterGap = false;
                    if (act.WasActive)
                    {
                        if (lastActiveSecond==0||act.StartSecond - lastActiveSecond > 60 * 60)
                        {
                            // There has been a 1 hour gap - don't guess across it
                            firstAfterGap = true;
                            if (unsureActs.Any() && guessWhenUnknown == GuessWorkPlay.SameBlock)
                            {
                                if(lastType != WorkPlayType.Unknown)
                                    foreach (var unsureAct in unsureActs)
                                        unsureAct.Classification = lastType;
                                unsureActs.Clear();
                                lastType = WorkPlayType.Unknown;
                            }
                        }

                        lastActiveSecond = act.EndSecond;
                    }
                    
                    var found = patterns.FirstOrDefault(x => x.RegularExpression.IsMatch(akey));
                    if (found == null)
                    {
                        unsureActs.Add(act);
                    }
                    else
                    {
                        var thisActType = found.IsWork ? WorkPlayType.Work : WorkPlayType.Play;

                        if (guessWhenUnknown >= GuessWorkPlay.SameBlock && unsureActs.Any())
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

                if (guessWhenUnknown == GuessWorkPlay.Never || unsureActs.Count == 0) continue;

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

        public string Key(string computer, string program, string title) => $"{computer}\t{program}\t{title}";
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

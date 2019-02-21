namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Tools;

    public class ActivityClassifyer
    {
        private string _windowTitleRegEx;

        [Description("Name of executable"), Category("Classifyer")]
        public string Executable { get; set; }

        [Description("Window title pattern"), Category("Classifyer")]
        public string WindowTitleRegEx
        {
            get => _windowTitleRegEx;
            set => _windowTitleRegEx = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public override string ToString() => Executable + (string.IsNullOrEmpty(WindowTitleRegEx) ? "" : $" [{WindowTitleRegEx}]");

        public override bool Equals(object obj)
        {
            return obj is ActivityClassifyer that && this.Executable == that.Executable &&
                   this.WindowTitleRegEx == that.WindowTitleRegEx;
        }

        public override int GetHashCode()
        {
            return ((Executable?.GetHashCode() ?? 0) * 397) ^ (WindowTitleRegEx?.GetHashCode() ?? 0);
        }
    }

    [TypeConverter(typeof(ActivityClassifyerCollectionConverter))]
    public class ActivityClassifyerCollection : List<ActivityClassifyer>
    {
        public void Add(string executable, string windowTitle = null)
        {
            this.Add(new ActivityClassifyer {Executable = executable, WindowTitleRegEx = windowTitle});
        }
    }

    public class ActivityClassifyerCollectionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string s))
                return base.ConvertFrom(context, culture, value);

            var ret = new ActivityClassifyerCollection();
            foreach (var row in Regex.Unescape(s).Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                var v = row.Split(new []{'\t'}, 2);
                var item = new ActivityClassifyer {Executable = v[0]};
                if (v.Length > 1)
                    item.WindowTitleRegEx = v[1];
                ret.Add(item);
            }

            return ret;
        }
        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is ActivityClassifyerCollection collection)
            {
                return Regex.Escape( string.Join("\n", collection.Select(x => x.Executable+(string.IsNullOrEmpty(x.WindowTitleRegEx)?"":"\t"+x.WindowTitleRegEx))));
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// Graphical editor of a list of classifiers
    /// </summary>
    public class ActivityClassifyerEditor : CollectionEditor
    {
        public ActivityClassifyerEditor(Type type) : base(type) { }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var result = base.EditValue(context, provider, value);
            // This was needed before, but not anymore? Odd.
            // ((Configuration)context.Instance).WorkActivities = (ActivityClassifyerCollection)result;
            return result;
        }

        /// <summary>
        /// Activates the help panel in the editor
        /// </summary>
        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            form.Shown += (sender, args) =>
            {
                foreach (var propertyGrid in form.DepthFirst<Control>(x => x.Controls.Cast<Control>()).OfType<PropertyGrid>())
                    propertyGrid.HelpVisible = true;
            };
            return form;
        }

        /// <summary>
        /// Makes the list show a summary of the object (default is the type)
        /// </summary>
        protected override string GetDisplayText(object value) => value.ToString();
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
                var reason = "";
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
                                        unsureAct.Classify( lastType,"Blocktail: "+reason);
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
                                    unsureAct.Classify(thisActType, "Blockhead: " + found.OriginalPattern);
                            else
                            {
                                // last was X, this is Y, how to classify those in between? Meh, give them roughly half each.
                                var middleSecond = (unsureActs[0].StartSecond + unsureActs.Last().EndSecond) / 2;
                                foreach (var unsureAct in unsureActs)
                                {
                                    var actMiddleSecond = (unsureAct.StartSecond + unsureAct.EndSecond) / 2;
                                    if (actMiddleSecond < middleSecond)
                                        unsureAct.Classify(lastType, "interpolateL: " + reason);
                                    else unsureAct.Classify(thisActType, "InterpolateR: " + found.OriginalPattern);
                                }
                            }
                            unsureActs.Clear();
                        }
                        
                        act.Classify(thisActType, "Match: " + found.OriginalPattern);
                        lastType = thisActType;
                        reason = found.OriginalPattern;
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
                    reason = "Day of week";
                }

                foreach (var act in unsureActs)
                {
                    act.Classify(lastType,"Post: "+reason);
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

        public void AddPattern(ActivityClassifyer classifyer, bool isWork)
        {
            var prio = 3 + (isWork ? 0 : 1) + (string.IsNullOrEmpty(classifyer.WindowTitleRegEx) ? 2 : 0);
            this.AddPattern(new Pattern(prio, KeyPattern(null, classifyer.Executable, classifyer.WindowTitleRegEx), isWork));
        }

        public void AddPatterns(IEnumerable<ActivityClassifyer> classifyers, bool isWork)
        {
            foreach (var classifyer in classifyers)
            {
                AddPattern(classifyer, isWork);
            }
        }

        public void AddWorkComputer(string workpc) => AddPattern(new Pattern(1, KeyPattern(workpc, null, null), true));
        public void AddPlayComputer(string playpc) => AddPattern(new Pattern(2, KeyPattern(playpc, null, null), false));
        public void AddWorkPattern(string exe, string titlePattern) => AddPattern(new Pattern(3, KeyPattern(null, exe, titlePattern), true));
        public void AddPlayPattern(string exe, string titlePattern) => AddPattern(new Pattern(4, KeyPattern(null, exe, titlePattern), false));
        public void AddWorkProgram(string exe) => AddPattern(new Pattern(5, KeyPattern(null, exe, null), true));
        public void AddPlayProgram(string exe) => AddPattern(new Pattern(6, KeyPattern(null, exe, null), false));

        public void Clear() => this.WorkPlayPatterns.Clear();
    }
}

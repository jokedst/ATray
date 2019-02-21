namespace ATray.Activity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A list of all activities in a day
    /// </summary>
    [Serializable]
    public class DayActivityList : List<ActivitySpan>
    {
        public DayActivityList(MonthActivities owner,int dayNumber)
        {
            Owner = owner;
            DayNumber = dayNumber;
        }

        public MonthActivities Owner { get; }
        public int DayNumber { get; }

        public uint FirstSecond => this.FirstOrDefault()?.StartSecond ?? 0;
        public uint LastSecond => this.LastOrDefault()?.EndSecond ?? 0;

        public uint FirstActiveSecond => this.FirstOrDefault(x=>x.WasActive)?.StartSecond ?? 0;
        public uint LastActiveSecond => this.LastOrDefault(x=>x.WasActive)?.EndSecond ?? 0;

        public List<(uint start, uint end)> TimespansWhere(Predicate<ActivitySpan> predicate)
        {
            var lastValue = false;
            uint lastStart = 0;
            uint lastEnd = 0;
            var result = new List<(uint start, uint end)>();
            foreach (var span in this)
            {
                if (span.StartSecond - 1 > lastEnd)
                {
                    // Gap between activities
                    if (lastValue)
                    {
                        result.Add((lastStart, lastEnd));
                        lastValue = false;
                    }
                }
                var value = predicate(span);
                if (value && !lastValue)
                {
                    lastStart = span.StartSecond;
                }
                if (!value && lastValue)
                {
                    result.Add((lastStart,span.EndSecond));
                }
                lastEnd = span.EndSecond;
                lastValue = value;
            }
            return result;
        }

        public RangeContainer<uint> RangesWhere(Predicate<ActivitySpan> predicate)
        {
            var result = RangeContainer.UintRangeContainer();
            result.Add(this.Where(x => predicate(x)).Select(r => new Range<uint>(r.StartSecond, r.EndSecond)));
            return result;
        }
    }

    /// <summary>
    /// An inclusive uint range
    /// </summary>
    public class Range<T>: IComparable<Range<T>> where T : IComparable<T>
    {
        public Range(T start, T end)
        {
            this.Start = start;
            this.End = end;
        }

        public T Start { get; }
        public T End { get; }
        public int CompareTo(Range<T> other)
        {
            var val = Start.CompareTo(other.Start);
            return val != 0 ? val : End.CompareTo(other.End);
        }

        public override bool Equals(object obj)
        {
            return obj is Range<T> other && this.Equals(other);
        }

        protected bool Equals(Range<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Start, other.Start) && EqualityComparer<T>.Default.Equals(End, other.End);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Start) * 397) ^ EqualityComparer<T>.Default.GetHashCode(End);
            }
        }

        public bool Overlaps(Range<T> other)
        {
            //return (other.Start < End && other.End > Start);
            return (other.Start.CompareTo(End)<=0 && other.End.CompareTo(Start)>=0);
        }
    }
}
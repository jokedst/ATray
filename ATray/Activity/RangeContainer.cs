using System;
using System.Collections;
using System.Collections.Generic;

namespace ATray.Activity
{
    public static class RangeContainer
    {
        public static RangeContainer<uint> UintRangeContainer() => new RangeContainer<uint>((a, b) => Math.Abs(a - b) == 1);
        public static Func<uint, uint, bool> UintAdjacent = (a, b) => Math.Abs(a - b) == 1;
        
    }

    public class RangeContainer<T>:ICollection<Range<T>>
        where T : IComparable<T>
    {
        private readonly Func<T, T, bool> _adjacent;

        public RangeContainer(Func<T,T, bool> adjacentFunc)
        {
            _adjacent = adjacentFunc;
        }
        private readonly SortedSet<Range<T>> _ranges = new SortedSet<Range<T>>();
        public void Add(T start, T end) => this.Add(new Range<T>(start, end));
        public void Add(Range<T> range)
        {
            Range<T> mergeable = null;
            foreach (var existing in _ranges)
            {
                if(existing.Start.CompareTo( range.End)>0 && !_adjacent(existing.Start, range.End))break;

                if (existing.Overlaps(range)
                    || _adjacent(existing.Start, range.End)
                    || _adjacent(range.Start, existing.End))
                {
                    mergeable = existing;
                }
            }
            if (mergeable == null)
            {
                _ranges.Add(range);
                return;
            }
            _ranges.Remove(mergeable);
            var merged = new Range<T>(mergeable.Start.CompareTo(range.Start)<0?mergeable.Start:range.Start,
                mergeable.End.CompareTo(range.End) > 0 ? mergeable.End : range.End);
            this.Add(merged);
        }

        public void Add(IEnumerable<Range<T>> ranges)
        {
            foreach (var range in ranges)
            {
                this.Add(range);
            }
        }

        public void Clear()
        {
            _ranges.Clear();
        }

        public bool Contains(Range<T> item)
        {
            return _ranges.Contains(item);
        }

        public void CopyTo(Range<T>[] array, int arrayIndex)
        {
            _ranges.CopyTo(array, arrayIndex);
        }

        public bool Remove(Range<T> item)
        {
            return _ranges.Remove(item);
        }

        public int Count => _ranges.Count;

        public bool IsReadOnly => ((ICollection<Range<T>>)_ranges).IsReadOnly;

        public IEnumerator<Range<T>> GetEnumerator()
        {
            return _ranges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _ranges).GetEnumerator();
        }
    }
}
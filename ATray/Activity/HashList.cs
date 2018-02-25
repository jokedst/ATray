using System;
using System.Collections;
using System.Collections.Generic;

namespace ATray.Activity
{
    public class HashList<T> : IList<T>, ISet<T>
    {
        private readonly IList<T> _listImplementation;
        private readonly HashSet<T> _hashSet;

        public HashList()
        {
            _listImplementation = new List<T>();
            _hashSet = new HashSet<T>();
        }

        public HashList(IEqualityComparer<T> comparer)
        {
            _hashSet = new HashSet<T>(comparer);
            _listImplementation = new List<T>();
        }

        public HashList(IEnumerable<T> collection)
        {
            _hashSet = new HashSet<T>(collection);
            _listImplementation = new List<T>(_hashSet);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _listImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_listImplementation).GetEnumerator();
        }

        /// <inheritdoc cref="ISet{T}.UnionWith"/>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            foreach (var obj in other)
                Add(obj);
        }

        /// <inheritdoc cref="ISet{T}.IntersectWith"/>
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (_hashSet.Count == 0 || ReferenceEquals(other, this))
                return;
            var objs = other as HashSet<T> ?? (other as HashList<T>)?._hashSet ?? new HashSet<T>(other);

            for (var i = 0; i < _listImplementation.Count; i++)
            {
                if (objs.Contains(_listImplementation[i])) continue;
                _hashSet.Remove(_listImplementation[i]);
                _listImplementation.RemoveAt(i--);
            }
        }

        /// <inheritdoc cref="ISet{T}.ExceptWith"/>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (_hashSet.Count == 0)
                return;
            if (ReferenceEquals(other, this))
                Clear();
            else
            {
                foreach (var obj in other)
                    Remove(obj);
            }
        }
            
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _hashSet.SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSubsetOf(other);
        }
        
        /// <summary> Determines whether the current <see cref="HashList{T}" /> object and a specified collection share common elements.</summary>
        /// <param name="other"> The collection to compare to the current <see cref="HashList{T}" /> object.</param>
        /// <returns> <see langword="true" /> if the <see cref="HashList{T}" /> object and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException"> <paramref name="other" /> is <see langword="null" />.</exception><inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            return _hashSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _hashSet.SetEquals(other);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
                if (_hashSet.Add(item))
                    _listImplementation.Add(item);
        }

        public bool Add(T item)
        {
            if (!_hashSet.Add(item)) return false;
            _listImplementation.Add(item);
            return true;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            _listImplementation.Clear();
            _hashSet.Clear();
        }

        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _listImplementation.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _hashSet.Remove(item) && _listImplementation.Remove(item);
        }

        public int Count => _listImplementation.Count;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return _listImplementation.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if(_hashSet.Add(item))
                _listImplementation.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            var item = _listImplementation[index];
            _hashSet.Remove(item);
            _listImplementation.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _listImplementation[index];
            set
            {
                if(_hashSet.Contains(value))
                {
                    if (_listImplementation[index].Equals(value)) return; // Replaced by itself. Sure, why not.
                    throw new InvalidOperationException("Item already exists in collection");
                    // Maybe allow this but _remove_ the index? Like this:
                    ////_listImplementation.RemoveAt(index);
                    ////return;
                }

                _hashSet.Remove(_listImplementation[index]);
                _hashSet.Add(value);
                _listImplementation[index] = value;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LogCollections
{
    public sealed class LogSortedSet<T> : LogCollection<T>, ISet<T>
        where T : IComparable<T>
    {
        private SortedSet<T> _set;

        public LogSortedSet(
            string name,
            int id,
            Func<T, int> keyProvider,
            Func<T, byte[]> serializer,
            Func<byte[], T> deserializer,
            long maxFileSize = sizeof(byte) * 1024 * 1024 * 25,
            int compactEvery = 100_000,
            Comparer<T> comparer = null) 
            : base(
                  name,
                  id,
                  keyProvider,
                  serializer,
                  deserializer,
                  maxFileSize,
                  compactEvery)
        {            
            _set = new SortedSet<T>(comparer ?? Comparer<T>.Default);

            foreach (var entry in _log)
            {
                if ((entry.Key & ~((int)Operation.Add)) != 0)
                {
                    _set.Add(_deserializer(entry.Value));
                }
                else
                {
                    _set.Remove(_deserializer(entry.Value));
                }
            }
        }



        #region Implementation of ISet<T>
        public int Count => _set.Count;

        public bool IsReadOnly => false;

        public bool Add(T item)
        {
            var entry = new LogEntry
            {
                Id = _id,
                Key = _keyProvider(item) | (int)Operation.Add,
                Value = _serializer(item)
            };
            _log.Append(entry);

            MaybeCompact();

            return _set.Add(item);
        }
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => _set.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);
        public void ExceptWith(IEnumerable<T> other) => _set.ExceptWith(other);
        public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
        public void IntersectWith(IEnumerable<T> other) => _set.IntersectWith(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);
        public bool Remove(T item)
        {
            var entry = new LogEntry
            {
                Id = _id,
                Key = _keyProvider(item) & ~((int)Operation.Add),
                Value = _serializer(item)
            };
            _log.Append(entry);

            MaybeCompact();

            return _set.Remove(item);
        }
        public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => _set.SymmetricExceptWith(other);
        public void UnionWith(IEnumerable<T> other) => _set.UnionWith(other);
        void ICollection<T>.Add(T item) => this.Add(item);
        IEnumerator IEnumerable.GetEnumerator() => ((ISet<T>)this._set).GetEnumerator();
        #endregion
    }
}

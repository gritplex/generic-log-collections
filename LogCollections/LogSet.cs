using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LogCollections
{
    public sealed class LogSet<T> : LogCollection<T>, ISet<T>
    {
        public LogSet(
            string folder,
            string name,
            int id,
            Func<T, Guid> keyProvider,
            Func<T, byte[]> serializer,
            Func<byte[], T> deserializer,
            bool readOnly = false,
            long maxFileSize = sizeof(byte) * 1024 * 1024 * 25,
            int compactEvery = 100_000,
            EqualityComparer<T> comparer = null) 
            : base(
                  folder,
                  name, 
                  id, 
                  keyProvider,
                  serializer,
                  deserializer,
                  readOnly,
                  maxFileSize,
                  compactEvery)
        {
            _internal = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);
            InitFromLog();
        }       

        #region Implementation of ISet<T>
        public int Count => _internal.Count;

        public bool IsReadOnly => _readOnly;

        public bool Add(T item)
        {
            var entry = new LogEntry(_id, _keyProvider(item), c_Add, _serializer(item));
            _log.Append(entry);

            MaybeCompact();

            return ((HashSet<T>)_internal).Add(item);
        }
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => _internal.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _internal.CopyTo(array, arrayIndex);
        public void ExceptWith(IEnumerable<T> other) => ((HashSet<T>)_internal).ExceptWith(other);
        public IEnumerator<T> GetEnumerator() => _internal.GetEnumerator();
        public void IntersectWith(IEnumerable<T> other) => ((HashSet<T>)_internal).IntersectWith(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => ((HashSet<T>)_internal).IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => ((HashSet<T>)_internal).IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => ((HashSet<T>)_internal).IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => ((HashSet<T>)_internal).IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => ((HashSet<T>)_internal).Overlaps(other);
        public bool Remove(T item)
        {
            var entry = new LogEntry(_id, _keyProvider(item), c_Remove, _serializer(item));
            _log.Append(entry);

            MaybeCompact();

            return _internal.Remove(item);
        }
        public bool SetEquals(IEnumerable<T> other) => ((HashSet<T>)_internal).SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => ((HashSet<T>)_internal).SymmetricExceptWith(other);
        public void UnionWith(IEnumerable<T> other) => ((HashSet<T>)_internal).UnionWith(other);
        void ICollection<T>.Add(T item) => this.Add(item);
        IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();
        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LogCollections
{
    public sealed class LogSet<T> : ISet<T>
    {
        private int _id;
        private string _name;
        private long _maxFileSize;
        private int _compactEvery;
        private int _opCounter;
        private Func<T, byte[]> _serializer;
        private Func<byte[], T> _deserializer;
        private Func<T, int> _keyProvider;
        private BinaryLog _log;
        private HashSet<T> _set;

        public LogSet(
            string name,
            int id,
            Func<T, int> keyProvider,
            Func<T, byte[]> serializer,
            Func<byte[], T> deserializer,
            long maxFileSize = sizeof(byte) * 1024 * 1024 * 25,
            int compactEvery = 100_000,
            EqualityComparer<T> comparer = null)
        {
            _name = name;
            _id = id;
            _serializer = serializer;
            _deserializer = deserializer;
            _keyProvider = keyProvider;
            _maxFileSize = maxFileSize;
            _compactEvery = compactEvery;
            _opCounter = 0;
            _log = new BinaryLog(_name, _maxFileSize);
            _set = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);

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

        private void MaybeCompact()
        {
            ++_opCounter;
            if (_opCounter > _compactEvery)
            {
                Compact();
                _opCounter = 0;
            }
        }

        private void Compact()
        {
            _log.Compact();
        }

        [Flags]
        private enum Operation
        {
            Add = 0b0100_0000_0000_0000_0000_0000_0000_0000,
            Mask = 0b0011_1111_1111_1111_1111_1111_1111_1111,
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
        IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();
        #endregion
    }
}

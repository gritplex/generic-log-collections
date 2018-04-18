using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace LogCollections
{
    public abstract class LogCollection<T>
    {
        protected readonly int _id;
        protected readonly string _name;
        protected readonly string _folder;
        protected readonly long _maxFileSize;
        protected readonly int _compactEvery;
        protected int _opCounter;
        protected Func<T, int> _idProvider;
        protected Func<T, byte[]> _serializer;
        protected Func<byte[], T> _deserializer;
        protected Func<T, Guid> _keyProvider;
        protected BinaryLog _log;

        protected ICollection<T> _internal;
        protected readonly bool _readOnly;

        public LogCollection(
            string folder,
            string name,
            int id,
            Func<T, Guid> keyProvider,
            Func<T, byte[]> serializer,
            Func<byte[], T> deserializer,
            bool readOnly = false,
            long maxFileSize = sizeof(byte) * 1024 * 1024 * 25,
            int compactEvery = 100_000)
        {
            _name = name;
            _folder = folder;
            _id = id;
            _serializer = serializer;
            _deserializer = deserializer;
            _keyProvider = keyProvider;
            _maxFileSize = maxFileSize;
            _compactEvery = compactEvery;
            _readOnly = readOnly;
            _opCounter = 0;

            _idProvider = new Func<T, int>(d => _id);

            _log = new BinaryLog(_folder, _name, _maxFileSize, _readOnly);
        }

        public LogCollection(
            string folder,
            string name,
            int defaultId,
            Func<T, int> idProvider,
            Func<T, Guid> keyProvider,
            Func<T, byte[]> serializer,
            Func<byte[], T> deserializer,
            bool readOnly = false,
            long maxFileSize = sizeof(byte) * 1024 * 1024 * 25,
            int compactEvery = 100_000) : this(folder,name, defaultId, keyProvider, serializer, deserializer, readOnly, maxFileSize, compactEvery)
        {
            if(idProvider != null)
                _idProvider = idProvider;
        }

        protected virtual void InitFromLog()
        {
            if(_internal is null) throw new NullReferenceException($"{nameof(_internal)} of ICollection<{typeof(T).Name}>");

            foreach (var entry in _log)
            {
                if ((entry.Meta & c_Add) != 0)
                {
                    _internal.Add(_deserializer(entry.Value));
                }
                else if((entry.Meta & c_Remove) != 0)
                {
                    _internal.Remove(_deserializer(entry.Value));
                }
            }
        }

        public virtual void Compact(bool useThreadPool = true)
        {
            _log.Compact(useThreadPool);
        }

        protected virtual void MaybeCompact()
        {
            ++_opCounter;
            if (_opCounter > _compactEvery)
            {
                Compact();
                _opCounter = 0;
            }
        }

        public const byte c_Add    = 0b1000_0000;
        public const byte c_Remove = 0b0100_0000;
    }
}

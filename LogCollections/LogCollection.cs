using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace LogCollections
{
    public abstract class LogCollection<T>
    {
        protected int _id;
        protected string _name;
        protected long _maxFileSize;
        protected int _compactEvery;
        protected int _opCounter;
        protected Func<T, byte[]> _serializer;
        protected Func<byte[], T> _deserializer;
        protected Func<T, Guid> _keyProvider;
        protected BinaryLog _log;

        public LogCollection(
            string name,
            int id,
            Func<T, Guid> keyProvider,
            Func<T, byte[]> serializer,
            Func<byte[], T> deserializer,
            long maxFileSize = sizeof(byte) * 1024 * 1024 * 25,
            int compactEvery = 100_000)
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
        }

        protected virtual void Compact()
        {
            _log.Compact();
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

        protected const byte c_Add = 0b1000_0000;
        protected const byte c_Remove = 0b0111_1111;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogCollections
{
    public class FileStorage
    {
        private string _fileName;

        private FileStream _stream;
        private BinaryWriter _writer;

        public FileStorage(string fileName, bool readOnly = false)
        {
            _fileName = fileName;

            if (!readOnly)
            {
                _stream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writer = new BinaryWriter(_stream, Encoding.UTF8);
                _stream.Seek(0, SeekOrigin.End);
            }
        }

        internal long Append(in LogEntry entry)
        {
            _writer.Write(entry.Id);

            _writer.Write(entry.Meta);

            foreach (byte b in entry.Key.ToByteArray())
            {
                _writer.Write(b);
            }

            _writer.Write(entry.Value.Length);
            _writer.Write(entry.Value);

            _writer.Flush();

            return _stream.Length;
        }

        public IEnumerator<LogEntry> GetEnumerator()
        {
            using (var fStream = File.Open(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new BinaryReader(fStream))
            {
                while (fStream.Position < fStream.Length)
                {
                    var id = reader.ReadInt32();
                    var meta = reader.ReadByte();
                    var key =  new Guid(reader.ReadBytes(16));
                    var length = reader.ReadInt32();
                    var value = reader.ReadBytes(length);

                    yield return new LogEntry(id, key, meta ,value);
                }
            }
        }

        public void Close()
        {
            _writer?.Flush();
            _writer?.Close();
        }

    }

    public readonly struct LogEntry
    {
        public readonly int Id;
        public readonly byte Meta;
        public readonly Guid Key;
        public readonly byte[] Value;

        public LogEntry(int id, Guid key, byte meta, byte[] value)
        {
            Id = id;
            Meta = meta;
            Key = key;
            Value = value;
        }
    }
}

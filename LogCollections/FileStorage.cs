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

        internal long Append(LogEntry entry)
        {
            _writer.Write(entry.Id);
            _writer.Write(entry.Key);
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
                    var key = reader.ReadInt32();
                    var length = reader.ReadInt32();
                    var value = reader.ReadBytes(length);

                    yield return new LogEntry
                    {
                        Id = id,
                        Key = key,
                        Value = value
                    };
                }
            }
        }

        internal void Close()
        {
            _writer?.Flush();
            _writer?.Close();
        }

    }

    public struct LogEntry
    {
        public int Id;
        public int Key;
        public byte[] Value;
    }
}

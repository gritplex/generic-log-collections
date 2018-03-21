using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LogCollections.FileHelpers;

namespace LogCollections
{
    internal class LogCompacter : ILogCompacter
    {
        private string _log;
        private readonly int _upTo;
        private readonly long _maxFileSize;
        private readonly string _folder;

        internal LogCompacter(string folder, string logName, int maxLogNumber, long maxFileSize)
        {
            _log = logName;
            _upTo = maxLogNumber;
            _maxFileSize = maxFileSize;
            _folder = folder;
        }

        public int Compact()
        {
            if (_upTo <= 0) return 0;

            var cache = new Dictionary<int, Dictionary<Guid, LogEntry>>(10000);
            for (int i = 0; i < _upTo; i++)
            {
                string file = GetFileName(_folder, _log, i);
                if (File.Exists(file))
                {
                    var reader = new FileStorage(file, true);
                    foreach (var entry in reader)
                    {
                        if (cache.ContainsKey(entry.Id))
                        {
                            cache[entry.Id][entry.Key] = entry;
                        }
                        else
                        {
                            cache[entry.Id] = new Dictionary<Guid, LogEntry>()
                            {
                                { entry.Key, entry }
                            };
                        }
                    }
                    reader.Close();
                }
            }

            using (var writer = new BinaryLog(_folder, $"compactification-{_log}", _maxFileSize))
            {
                foreach (var id in cache.Keys)
                {
                    foreach (var key in cache[id].Keys)
                    {
                        writer.Append(new LogEntry(id, key, cache[id][key].Meta, cache[id][key].Value));
                    }
                }
            }

            var logFileNames = GetAllFileNamesForLog(_folder, _log);
            foreach (var fName in logFileNames)
            {
                var nb = GetNumber(fName);
                if (nb > -1 && nb < _upTo)
                {
                    File.Delete(fName);
                }
            }

            int maxCompacted = 0;
            var compactedFileNames = GetAllFileNamesForLog(_folder, $"compactification-{_log}");
            foreach (var cfName in compactedFileNames)
            {
                var nb = GetNumber(cfName);
                if (nb > -1 && nb < _upTo && File.Exists(cfName))
                {
                    File.Move(cfName, GetFileName(_folder, _log, nb));
                    maxCompacted = Math.Max(nb, maxCompacted);
                }
            }

            if (File.Exists(GetFileName(_folder, $"compactification-{_log}")))
            {
                maxCompacted++;
                File.Move(GetFileName(_folder, $"compactification-{_log}"), GetFileName(_folder, $"{_log}", maxCompacted));
            }

            return maxCompacted;
        }
    }
}

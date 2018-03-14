using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LogCollections.FileHelpers;

namespace LogCollections
{
    internal class LogCompacter
    {
        private string _log;
        private int _upTo;
        private long _maxFileSize;

        internal LogCompacter(string logName, int maxLogNumber, long maxFileSize)
        {
            _log = logName;
            _upTo = maxLogNumber;
            _maxFileSize = maxFileSize;
        }

        internal int Compact()
        {
            if (_upTo <= 0) return 0;

            var cache = new Dictionary<int, Dictionary<Guid, LogEntry>>(10000);
            for (int i = 0; i < _upTo; i++)
            {
                string file = GetFileName(_log, i);
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

            using (var writer = new BinaryLog($"compactification-{_log}", _maxFileSize))
            {
                foreach (var id in cache.Keys)
                {
                    foreach (var key in cache[id].Keys)
                    {
                        writer.Append(new LogEntry(id, key, cache[id][key].Meta, cache[id][key].Value));
                    }
                }
            }

            var logFileNames = GetAllFileNamesForLog(_log);
            foreach (var fName in logFileNames)
            {
                var nb = GetNumber(fName);
                if (nb > -1 && nb < _upTo)
                {
                    File.Delete(fName);
                }
            }

            int maxCompacted = 0;
            var compactedFileNames = GetAllFileNamesForLog($"compactification-{_log}");
            foreach (var cfName in compactedFileNames)
            {
                var nb = GetNumber(cfName);
                if (nb > -1 && nb < _upTo && File.Exists(cfName))
                {
                    File.Move(cfName, GetFileName(_log, nb));
                    maxCompacted = Math.Max(nb, maxCompacted);
                }
            }

            if (File.Exists(GetFileName($"compactification-{_log}")))
            {
                maxCompacted++;
                File.Move(GetFileName($"compactification-{_log}"), GetFileName($"{_log}", maxCompacted));
            }

            return maxCompacted;
        }
    }
}

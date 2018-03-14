using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static LogCollections.FileHelpers;

namespace LogCollections
{
    public class BinaryLog : IDisposable
    {
        private string _name;
        private long _maxFileSize;
        private int _currentLogNumber;
        private bool _readOnly;

        private FileStorage _fileStorage;

        public BinaryLog(string name, long maxFileSize, bool readOnly = false)
        {
            _name = name;
            _maxFileSize = maxFileSize;
            _readOnly = readOnly;

            _currentLogNumber = GetCurrentLogNumber(_name, _maxFileSize);
            _fileStorage = new FileStorage(GetFileName(_name), _readOnly);
        }
        
        public void Append(in LogEntry entry)
        {
            if (_readOnly) return;

            long fsize = _fileStorage.Append(entry);

            if(fsize > _maxFileSize)
            {
                _fileStorage.Close();
                _currentLogNumber++;
                File.Move(GetFileName(_name), GetFileName(_name, _currentLogNumber));
                _fileStorage = new FileStorage(GetFileName(_name));
            }
        }

        public IEnumerator<LogEntry> GetEnumerator()
        {
            var fileNames = GetAllFileNamesForLog(_name);
            foreach (var fileName in fileNames)
            {
                var reader = new FileStorage(fileName, true);
                foreach (var entry in reader)
                {
                    yield return entry;
                }
                reader.Close();
            }
        }

        public void Compact()
        {
            if (_currentLogNumber == 0 || _readOnly) return;
            var compactor = new LogCompacter(_name, _currentLogNumber + 1, _maxFileSize);
            compactor.Compact();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileStorage.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BinaryLog() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

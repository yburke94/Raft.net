using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raft.Server.Log
{
    public class EncodedEntryRegister
    {
        private readonly Dictionary<Guid, EncodedEntry> _logs = new Dictionary<Guid, EncodedEntry>();

        public void AddLogEntry(Guid eventId, long logIdx, byte[] encodedLog, Task logTask)
        {
            if(_logs.ContainsKey(eventId))
                throw new InvalidOperationException("An encoded log already exisst for this event.");

            if (logTask == null || logTask.IsCompleted)
                throw new ArgumentException("The passed in Task for the log entry is invalid.");

            _logs.Add(eventId, new EncodedEntry(logIdx, encodedLog));

            logTask.ContinueWith(_ => _logs.Remove(eventId), TaskContinuationOptions.ExecuteSynchronously);
        }

        public bool HasLogEntry(Guid eventId)
        {
            return _logs.ContainsKey(eventId);
        }

        public KeyValuePair<long, byte[]> GetEncodedLog(Guid eventId)
        {
            if (!_logs.ContainsKey(eventId))
                throw new KeyNotFoundException(
                    "Failed to find encoded log entry for key. Please ensure this has been set. " +
                    "This may also have been automatically evicted. Please ensure the MaxAccessTimes is correctly configured.");

            var encodedLogEntry = _logs[eventId];

            return new KeyValuePair<long, byte[]>(encodedLogEntry.LogIndex, encodedLogEntry.Data);
        }

        public void EvictEntry(Guid eventId)
        {
            _logs.Remove(eventId);
        }

        private class EncodedEntry
        {
            public EncodedEntry(long logIdx, byte[] data)
            {
                LogIndex = logIdx;
                Data = data;
            }

            public long LogIndex { get; private set; }
            public byte[] Data { get;  private set; }
        }
    }
}

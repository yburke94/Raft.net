using System;
using System.Collections.Generic;

namespace Raft.Server.Log
{
    public class LogEntryRegister
    {
        private readonly int _maxAccessTimes;
        private readonly Dictionary<Guid, Entry> _logs = new Dictionary<Guid, Entry>();

        // TODO: Find better way to evict
        public LogEntryRegister(int maxAccessTimes)
        {
            _maxAccessTimes = maxAccessTimes;
        }

        public void AddEncodedLog(Guid eventId, long logIdx, byte[] encodedLog)
        {
            if(_logs.ContainsKey(eventId))
                throw new InvalidOperationException("An encoded log already exisst for this event.");

            _logs.Add(eventId, new Entry(logIdx, encodedLog));
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
            encodedLogEntry.TimesAccessed++;

            if (encodedLogEntry.TimesAccessed >= _maxAccessTimes)
                _logs.Remove(eventId);

            return new KeyValuePair<long, byte[]>(encodedLogEntry.LogIndex, encodedLogEntry.Data);
        }

        public void EvictEntry(Guid eventId)
        {
            _logs.Remove(eventId);
        }

        private class Entry
        {
            public Entry(long logIdx, byte[] data)
            {
                LogIndex = logIdx;
                Data = data;
                TimesAccessed = 0;
            }

            public long LogIndex { get; set; }
            public byte[] Data { get;  private set; }
            public int TimesAccessed { get; set; }
        }
    }
}

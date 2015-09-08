using System;
using Raft.Contracts.Persistance;
using Raft.Infrastructure;
using Raft.Infrastructure.Compression;

namespace Raft.Core.Data
{
    /// <summary>
    /// In memory representation of the committed log.
    /// </summary>
    /// <remarks>
    /// The log itself is persisted via the configured <see cref="IWriteDataBlocks" /> object.
    /// This in-memory representation should be constructed on startup by reading all previous entries in the persisted log.
    /// </remarks>
    [Obsolete("Remove once new log impl with compression is finished.")]
    internal class InMemoryLog
    {
        public InMemoryLog()
        {
            
        }

        // To delete
        private const int LogIncrementSize = 64;
        private RaftLogEntry[] _log = new RaftLogEntry[LogIncrementSize];


        /// <summary>
        /// 
        /// The index of the array maps to the term number.
        /// The value stored at a given index is the offset for the term Ziplist entry in the compressedTemrs ziplist.
        /// </summary>
        private long[] _termZiplistOffsetMap;

        /// <summary>
        /// 
        /// </summary>
        private Ziplist _compressedTerms;

        /// <summary>
        /// 
        /// </summary>
        private Ziplist _currentTermEntries;

        public bool HasEntry(long commitIndex)
        {
            if (commitIndex == 0)
                return false;

            var logEntry = _log[commitIndex - 1];
            return logEntry != null;
        }

        public long? GetTermForEntry(long commitIndex)
        {
            if (commitIndex == 0)
                return 0;

            var logEntry = _log[commitIndex - 1];
            return logEntry != null ? (long?)logEntry.Term : null;
        }

        public byte[] GetLogEntry(long commitIndex)
        {
            if (commitIndex == 0)
                return null;

            var logEntry = _log[commitIndex - 1];
            return logEntry != null ? logEntry.Entry : null;
        }

        public void SetLogEntry(long commitIndex, long term, byte[] entry)
        {
            if (commitIndex < 1)
                throw new IndexOutOfRangeException("Commit index for log must start from 1.");

            if (entry == null)
                throw new ArgumentException("Entry must not be null.", "entry");

            if (commitIndex > _log.Length)
            {
                var newLog = new RaftLogEntry[_log.Length + LogIncrementSize];
                _log.CopyTo(newLog, 0);
                _log = newLog;
            }

            _log[commitIndex - 1] = new RaftLogEntry(term, entry);
        }

        public void TruncateLog(long truncateFromIndex)
        {
            for (var i = truncateFromIndex; i < _log.Length; i++)
                _log[i] = default(RaftLogEntry);
        }

        private class RaftLogEntry
        {
            public RaftLogEntry(long term, byte[] entry)
            {
                Term = term;
                Entry = entry;
            }

            public readonly long Term;

            public readonly byte[] Entry;
        }

        private class LogEntry
        {
            public LogEntry(long term)
            {
                Term = term;
            }

            public readonly long Term;

            public readonly byte[] Entry;
        }
    }
}

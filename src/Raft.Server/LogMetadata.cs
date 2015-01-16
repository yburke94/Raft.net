using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Raft.Server
{
    public class LogMetadata : ILogMetadata
    {
        public LogMetadata(long journalIdx, long nextOffset)
        {
            CurrentJournalIndex = journalIdx;
            NextJournalEntryOffset = nextOffset;
        }

        public long CurrentJournalIndex { get; private set; }

        public long NextJournalEntryOffset { get; private set; }

        public void IncrementJournalIndex()
        {
            CurrentJournalIndex++;
            NextJournalEntryOffset = 0;
        }

        public void AddLogEntryToIndex(long logEntryIdx, long dataLength)
        {
            NextJournalEntryOffset = NextJournalEntryOffset + dataLength;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LogMetadataEntry
    {
        public readonly long LogEntryIdx;
        public readonly long JournalFileIdx;
        public readonly long FileOffset;

        public LogMetadataEntry(long logEntryIdx , long journalFileIdx, long fileOffset)
        {
            LogEntryIdx = logEntryIdx;
            JournalFileIdx = journalFileIdx;
            FileOffset = fileOffset;
        }
    }
}
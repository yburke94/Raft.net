using System;
using System.Runtime.InteropServices;

namespace Raft.Server
{
    public class LogMetadata : ILogMetadata
    {
        private readonly IMmioService _mmioService;
        private readonly long _viewAccessorSize;

        public LogMetadata(IMmioService mmioService, long viewAccessorSize)
        {
            var metadataTypeSize = Marshal.SizeOf(typeof (LogMetadataEntry));

            if (viewAccessorSize <= metadataTypeSize)
                throw new ArgumentException("The viewAccessorSize must be greater than the size of the metadata entry type.", "viewAccessorSize");

            if (viewAccessorSize % metadataTypeSize != 0)
                throw new ArgumentException("The viewAccessorSize must be a multiple of the size of the metadata entry type.", "viewAccessorSize");

            _mmioService = mmioService;
            _viewAccessorSize = viewAccessorSize;
        }

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
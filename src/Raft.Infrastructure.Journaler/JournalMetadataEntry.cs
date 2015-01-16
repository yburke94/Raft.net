using System.Runtime.InteropServices;

namespace Raft.Infrastructure.Journaler
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct JournalMetadataEntry
    {
        public readonly long JournalEntryIdx;

        public readonly long JournalFileIdx;

        public readonly long StartPosition;

        public readonly long EndPosition;

        public readonly long PaddedBytesCount;
    }
}

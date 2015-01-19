namespace Raft.Infrastructure.Journaler
{
    internal class EntryMetadata
    {
        private readonly long _journalOffset;

        public int Length { get; private set; }

        public int Padding { get; private set; }

        /// <summary>
        /// Computed based on current offset and metadata values written prior to the journal entry.
        /// </summary>
        /// <remarks>This value will not be written to the journal file.</remarks>
        public long EntryStartPosition {
            get { return _journalOffset + (sizeof (int) * 2); }
        }

        public EntryMetadata(long journalOffset, int length, int padding)
        {
            _journalOffset = journalOffset;
            Length = length;
            Padding = padding;
        }
    }
}
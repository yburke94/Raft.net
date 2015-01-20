namespace Raft.Infrastructure.Journaler.Readers
{
    public class JournalReadResult
    {
        public int JournalIndex { get; private set; }
        public long Index { get; private set; }
        public byte[] Entry { get; private set; }

        public JournalReadResult(int journalIndex, long index, byte[] entry)
        {
            JournalIndex = journalIndex;
            Index = index;
            Entry = entry;
        }
    }
}
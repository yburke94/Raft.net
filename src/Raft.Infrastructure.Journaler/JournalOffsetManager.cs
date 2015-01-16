namespace Raft.Infrastructure.Journaler
{
    internal class JournalOffsetManager : IJournalOffsetManager
    {
        public JournalOffsetManager()
        {
            
        }

        public int CurrentJournalIndex
        {
            get { return 0; }
        }

        public long NextJournalEntryOffset
        {
            get { return 0; }
        }

        public void IncrementJournalIndex()
        {

        }

        public void AddJournalEntry(long entrySize, long? padding)
        {
        }

        public void Flush()
        {
        }
    }
}

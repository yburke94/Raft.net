namespace Raft.Infrastructure.Journaler
{
    internal class JournalOffsetManager : IJournalOffsetManager
    {
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

        public void UpdateJournalOffset(int entrySize)
        {
            throw new System.NotImplementedException();
        }
    }
}

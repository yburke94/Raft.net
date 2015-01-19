using System;

namespace Raft.Infrastructure.Journaler
{
    internal interface IJournalFileWriter : IDisposable
    {
        void SetJournal(int journalIdx, long startingPosition = 0L);

        void WriteJournalEntry(byte[] bytes);

        void Flush();
    }
}
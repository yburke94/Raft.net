using System;

namespace Raft.Infrastructure.Journaler.Writers
{
    internal interface IJournalFileWriter : IDisposable
    {
        void SetJournal(int journalIdx, long startingPosition);

        void WriteJournalEntry(byte[] bytes);

        void Flush();
    }
}
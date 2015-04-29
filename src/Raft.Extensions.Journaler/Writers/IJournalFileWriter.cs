using System;

namespace Raft.Extensions.Journaler.Writers
{
    internal interface IJournalFileWriter : IDisposable
    {
        void SetJournal(int journalIdx, long startingPosition);

        void WriteJournalEntry(byte[] bytes);

        void Flush();
    }
}
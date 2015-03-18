using System;
using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents
{
    internal class CommitCommandRequested : IEventTranslator<CommitCommandRequested>
    {
        public long? PreviousLogIndex { get; set; }

        public long? PreviousLogTerm { get; set; }

        public long? LeaderCommit { get; set; }

        public byte[][] Entries { get; set; }

        public CommitCommandRequested Translate(CommitCommandRequested existingEvent, long sequence)
        {
            if (PreviousLogIndex == null)
                throw new InvalidOperationException("PreviousLogIndex must be set in order to translate event.");

            if (PreviousLogTerm == null)
                throw new InvalidOperationException("PreviousLogTerm must be set in order to translate event.");

            if (LeaderCommit == null)
                throw new InvalidOperationException("LeaderCommit must be set in order to translate event.");

            if (Entries == null)
                throw new InvalidOperationException("Entry must be set in order to translate event.");

            existingEvent.PreviousLogIndex = PreviousLogIndex;
            existingEvent.PreviousLogTerm = PreviousLogTerm;
            existingEvent.LeaderCommit = LeaderCommit;
            existingEvent.Entries = Entries;
            return existingEvent;
        }
    }
}
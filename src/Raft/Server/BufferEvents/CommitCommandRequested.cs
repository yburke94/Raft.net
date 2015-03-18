using System;
using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents
{
    internal class CommitCommandRequested : IEventTranslator<CommitCommandRequested>
    {
        public byte[] Entry { get; set; }

        public CommitCommandRequested Translate(CommitCommandRequested existingEvent, long sequence)
        {
            if (Entry == null)
                throw new InvalidOperationException("Entry must be set in order to translate event.");

            existingEvent.Entry = Entry;
            return existingEvent;
        }
    }
}
using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents.Translators
{
    internal class CommitCommandRequestedTranslator : IEventTranslator<CommitCommandRequested>
    {
        private readonly byte[] _entry;

        public CommitCommandRequestedTranslator(byte[] entry)
        {
            _entry = entry;
        }

        public CommitCommandRequested Translate(CommitCommandRequested existingEvent, long sequence)
        {
            existingEvent.Entry = _entry;
            return existingEvent;
        }
    }
}
using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents.Translators
{
    public class CommitCommandRequestedTranslator : ITranslator<CommitCommandRequested>
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
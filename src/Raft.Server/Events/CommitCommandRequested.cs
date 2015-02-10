using Raft.Infrastructure.Disruptor;

namespace Raft.Server.Events
{
    internal class CommitCommandRequested
    {
        public byte[] Entry { get; private set; }

        internal class Translator : ITranslator<CommitCommandRequested>
        {
            private readonly byte[] _entry;

            public Translator(byte[] entry)
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
}
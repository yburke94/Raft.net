using Raft.Infrastructure.Disruptor;

namespace Raft.Server.Events
{
    internal class ApplyCommandRequested
    {
        public long LogIdx { get; set; }

        private class Translator : ITranslator<ApplyCommandRequested>
        {
            private readonly long _logIdx;

            public Translator(long logIdx)
            {
                _logIdx = logIdx;
            }

            public ApplyCommandRequested Translate(ApplyCommandRequested existingEvent, long sequence)
            {
                existingEvent.LogIdx = _logIdx;
                return existingEvent;
            }
        }
    }
}
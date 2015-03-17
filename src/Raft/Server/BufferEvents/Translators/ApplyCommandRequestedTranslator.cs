using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents.Translators
{
    internal class ApplyCommandRequestedTranslator : IEventTranslator<ApplyCommandRequested>
    {
        private readonly long _logIdx;

        public ApplyCommandRequestedTranslator(long logIdx)
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
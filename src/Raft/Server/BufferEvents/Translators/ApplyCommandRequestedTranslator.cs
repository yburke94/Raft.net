using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents.Translators
{
    public class ApplyCommandRequestedTranslator : ITranslator<ApplyCommandRequested>
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
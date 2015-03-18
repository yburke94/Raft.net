using System;
using Raft.Infrastructure.Disruptor;

namespace Raft.Server.BufferEvents
{
    internal class ApplyCommandRequested : IEventTranslator<ApplyCommandRequested>
    {
        public long? LogIdx { get; set; }

        public ApplyCommandRequested Translate(ApplyCommandRequested existingEvent, long sequence)
        {
            if (LogIdx == null)
                throw new InvalidOperationException("LogIdx must be set in order to translate event.");

            existingEvent.LogIdx = LogIdx;
            return existingEvent;
        }
    }
}
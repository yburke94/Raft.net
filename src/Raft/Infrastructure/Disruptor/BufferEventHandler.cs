using System;
using Disruptor;

namespace Raft.Infrastructure.Disruptor
{
    internal abstract class BufferEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : BufferEvent
    {
        protected long Sequence = 0;
        protected bool EndOfBatch = false;

        public abstract void Handle(TEvent @event);

        public void OnNext(TEvent data, long sequence, bool endOfBatch)
        {
            if (data.IsFaulted())
                return;

            if (data.IsCompletedSuccessfully())
                return;

            try
            {
                Sequence = sequence;
                EndOfBatch = endOfBatch;
                Handle(data);
            }
            catch (Exception exc)
            {
                data.CompletionSource.SetException(exc);
                throw;
            }
        }
    }
}

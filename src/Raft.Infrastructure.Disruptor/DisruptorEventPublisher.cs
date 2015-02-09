using System;
using Disruptor;

namespace Raft.Infrastructure.Disruptor
{
    public class DisruptorEventPublisher<T> : IEventPublisher<T> where T : class
    {
        private readonly EventPublisher<T> _eventPublisher;

        public DisruptorEventPublisher(RingBuffer<T> ringBuffer)
        {
            _eventPublisher = new EventPublisher<T>(ringBuffer);
        }

        public void PublishEvent(Func<T, long, T> translator)
        {
            _eventPublisher.PublishEvent(translator);
        }

        public void PublishEvent(Func<T, long, T> translator, TimeSpan timeout)
        {
            _eventPublisher.PublishEvent(translator, timeout);
        }
    }
}
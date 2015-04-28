using System;
using System.Threading.Tasks;
using Disruptor;

namespace Raft.Infrastructure.Disruptor
{
    internal class PublishToBuffer<T> : IPublishToBuffer<T> where T : BufferEvent
    {
        private readonly EventPublisher<T> _eventPublisher;

        public PublishToBuffer(RingBuffer<T> ringBuffer)
        {
            _eventPublisher = new EventPublisher<T>(ringBuffer);
        }

        public Task PublishEvent(IEventTranslator<T> translator)
        {
            Task task = null;
            _eventPublisher.PublishEvent((@event, l) =>
            {
                var newEvent = translator.Translate(@event, l);
                task = newEvent.CompletionSource.Task;
                return newEvent;
            });

            return task;
        }

        public Task PublishEvent(IEventTranslator<T> translator, TimeSpan timeout)
        {
            Task task = null;
            _eventPublisher.PublishEvent((@event, l) =>
            {
                var newEvent = translator.Translate(@event, l);
                task = newEvent.CompletionSource.Task;
                return newEvent;
            }, timeout);

            return task;
        }
    }
}

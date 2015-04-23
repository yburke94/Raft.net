using System;
using System.Threading.Tasks;
using Disruptor;

namespace Raft.Infrastructure.Disruptor
{
    // TODO: Test these classes!!!
    internal class PublishToBuffer<T> : IPublishToBuffer<T> where T : class
    {
        private readonly EventPublisher<T> _eventPublisher;

        public PublishToBuffer(RingBuffer<T> ringBuffer)
        {
            _eventPublisher = new EventPublisher<T>(ringBuffer);
        }

        public void PublishEvent(IEventTranslator<T> translator)
        {
            _eventPublisher.PublishEvent(translator.Translate);
        }

        public void PublishEvent(IEventTranslator<T> translator, TimeSpan timeout)
        {
            _eventPublisher.PublishEvent(translator.Translate, timeout);
        }
    }

    internal class PublishToBuffer<TEvent, TResult> : IPublishToBuffer<TEvent, TResult>
        where TEvent : class, IFutureEvent<TResult>
        where TResult : class
    {
        private readonly EventPublisher<TEvent> _eventPublisher;

        public PublishToBuffer(RingBuffer<TEvent> ringBuffer)
        {
            _eventPublisher = new EventPublisher<TEvent>(ringBuffer);
        }

        public Task<TResult> PublishEvent(IEventTranslator<TEvent> translator)
        {
            Task<TResult> task = null;
            _eventPublisher.PublishEvent((@event, l) =>
            {
                var newEvent = translator.Translate(@event, l);
                task = newEvent.CompletionSource.Task;
                return newEvent;
            });

            return task;
        }

        public Task<TResult> PublishEvent(IEventTranslator<TEvent> translator, TimeSpan timeout)
        {
            Task<TResult> task = null;
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

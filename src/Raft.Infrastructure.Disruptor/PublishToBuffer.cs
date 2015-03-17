using System;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;

namespace Raft.Infrastructure.Disruptor
{
    public class PublishToBuffer<T> : IPublishToBuffer<T> where T : class
    {
        private readonly EventPublisher<T> _eventPublisher;

        public PublishToBuffer(RingBuffer<T> ringBuffer)
        {
            _eventPublisher = new EventPublisher<T>(ringBuffer);
        }

        public void PublishEvent(ITranslator<T> translator)
        {
            _eventPublisher.PublishEvent(translator.Translate);
        }

        public void PublishEvent(ITranslator<T> translator, TimeSpan timeout)
        {
            _eventPublisher.PublishEvent(translator.Translate, timeout);
        }
    }

    public class PublishToBuffer<TEvent, TResult> : IPublishToBuffer<TEvent, TResult>
        where TEvent : class, IFutureEvent<TResult>
        where TResult : class
    {
        private readonly EventPublisher<TEvent> _eventPublisher;

        public PublishToBuffer(RingBuffer<TEvent> ringBuffer)
        {
            _eventPublisher = new EventPublisher<TEvent>(ringBuffer);
        }

        public Task<TResult> PublishEvent(ITranslator<TEvent> translator)
        {
            Task<TResult> task = null;
            _eventPublisher.PublishEvent((@event, l) =>
            {
                var newEvent = translator.Translate(@event, l);
                task = newEvent.CompletionSource.Task;
                return newEvent;
            });

            SpinWait.SpinUntil(() => task != null);
            return task;
        }

        public Task<TResult> PublishEvent(ITranslator<TEvent> translator, TimeSpan timeout)
        {
            Task<TResult> task = null;
            _eventPublisher.PublishEvent((@event, l) =>
            {
                var newEvent = translator.Translate(@event, l);
                task = newEvent.CompletionSource.Task;
                return newEvent;
            }, timeout);

            SpinWait.SpinUntil(() => task != null);
            return task;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raft.Infrastructure.Disruptor;

namespace Raft.Tests.Unit.TestHelpers
{
    internal class TestBufferPublisher<TEvent, TResult> : IPublishToBuffer<TEvent, TResult>
        where TEvent : class, IFutureEvent<TResult> where TResult : class
    {
        public TestBufferPublisher()
        {
            Events = new List<TEvent>();
        }
        internal IList<TEvent> Events { get; private set; }

        public Task<TResult> PublishEvent(IEventTranslator<TEvent> translator)
        {
            var taskCompletionSource = new TaskCompletionSource<TResult>();
            Events.Add(translator.Translate(Activator.CreateInstance<TEvent>(), 0));
            taskCompletionSource.SetResult(default(TResult));
            return taskCompletionSource.Task;
        }

        public Task<TResult> PublishEvent(IEventTranslator<TEvent> translator, TimeSpan timeout)
        {
            return PublishEvent(translator);
        }
    }
}

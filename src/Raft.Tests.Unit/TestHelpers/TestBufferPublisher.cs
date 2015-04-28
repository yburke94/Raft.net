using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raft.Infrastructure.Disruptor;

namespace Raft.Tests.Unit.TestHelpers
{
    internal class TestBufferPublisher<TEvent> : IPublishToBuffer<TEvent>
        where TEvent : BufferEvent
    {
        private Action _publishAction = null;

        public TestBufferPublisher()
        {
            Events = new List<TEvent>();
        }

        internal IList<TEvent> Events { get; private set; }

        public Task PublishEvent(IEventTranslator<TEvent> translator)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            Events.Add(translator.Translate(Activator.CreateInstance<TEvent>(), 0));
            taskCompletionSource.SetResult(new object());

            if (_publishAction != null)
            {
                _publishAction();
            }

            return taskCompletionSource.Task;
        }

        public Task PublishEvent(IEventTranslator<TEvent> translator, TimeSpan timeout)
        {
            return PublishEvent(translator);
        }

        public void OnPublish(Action action, bool deleteAfterUse = true)
        {
            _publishAction = deleteAfterUse
                ? () => {
                    action();
                    _publishAction = null;
                }
                : action;
        }
    }
}

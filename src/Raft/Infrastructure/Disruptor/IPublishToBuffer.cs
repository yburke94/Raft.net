using System;
using System.Threading.Tasks;

namespace Raft.Infrastructure.Disruptor
{
    internal interface IPublishToBuffer<TEvent> where TEvent : BufferEvent
    {
        Task PublishEvent(IEventTranslator<TEvent> translator);
        Task PublishEvent(IEventTranslator<TEvent> translator, TimeSpan timeout);
    }
}

using System;
using System.Threading.Tasks;

namespace Raft.Infrastructure.Disruptor
{
    internal interface IPublishToBuffer<TEvent> where TEvent : class
    {
        void PublishEvent(IEventTranslator<TEvent> translator);
        void PublishEvent(IEventTranslator<TEvent> translator, TimeSpan timeout);
    }

    internal interface IPublishToBuffer<TEvent, TResult>
        where TEvent : class, IFutureEvent<TResult>
        where TResult : class
    {
        Task<TResult> PublishEvent(IEventTranslator<TEvent> translator);
        Task<TResult> PublishEvent(IEventTranslator<TEvent> translator, TimeSpan timeout);
    }
}

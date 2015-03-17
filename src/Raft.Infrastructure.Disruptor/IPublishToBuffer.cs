using System;
using System.Threading.Tasks;

namespace Raft.Infrastructure.Disruptor
{
    public interface IPublishToBuffer<TEvent> where TEvent : class
    {
        void PublishEvent(ITranslator<TEvent> translator);
        void PublishEvent(ITranslator<TEvent> translator, TimeSpan timeout);
    }

    public interface IPublishToBuffer<TEvent, TResult>
        where TEvent : class, IFutureEvent<TResult>
        where TResult : class
    {
        Task<TResult> PublishEvent(ITranslator<TEvent> translator);
        Task<TResult> PublishEvent(ITranslator<TEvent> translator, TimeSpan timeout);
    }

    public interface IFutureEvent<T>
    {
        TaskCompletionSource<T> CompletionSource { get; }
    }
}

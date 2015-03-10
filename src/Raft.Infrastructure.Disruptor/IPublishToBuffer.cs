using System;

namespace Raft.Infrastructure.Disruptor
{
    public interface IPublishToBuffer<T> where T : class
    {
        void PublishEvent(Func<T, long, T> translator);
        void PublishEvent(Func<T, long, T> translator, TimeSpan timeout);
    }
}

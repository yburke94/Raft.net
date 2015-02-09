using Disruptor;
using Raft.Server.Services;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcLogWriter : IEventHandler<CommitRequestedEvent>
    {
        public void OnNext(CommitRequestedEvent data, long sequence, bool endOfBatch)
        {
            throw new System.NotImplementedException();
        }
    }
}

using System;
using Disruptor;
using Raft.Server.Services;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcCommandApplier : IEventHandler<ApplyRequestedEvent>
    {
        public void OnNext(ApplyRequestedEvent data, long sequence, bool endOfBatch)
        {
            throw new NotImplementedException();
        }
    }
}

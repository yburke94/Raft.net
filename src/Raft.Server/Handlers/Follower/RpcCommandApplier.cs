using System;
using Disruptor;
using Raft.Server.Events;
using Raft.Server.Services;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcCommandApplier : IEventHandler<ApplyCommandRequested>
    {
        public void OnNext(ApplyCommandRequested data, long sequence, bool endOfBatch)
        {
            throw new NotImplementedException();
        }
    }
}

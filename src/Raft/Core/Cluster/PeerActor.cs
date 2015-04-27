using System;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Infrastructure.Wcf;
using Raft.Service.Contracts;

namespace Raft.Core.Cluster
{
    internal class PeerActor : Actor<ReplicateRequest>, IDisposable
    {
        private readonly INode _node;
        private readonly IServiceProxyFactory<IRaftService> _proxyFactory;

        public long NextIndex { get; private set; }

        private long _matchIndex; // ?

        public PeerActor(INode node, IServiceProxyFactory<IRaftService> proxyFactory)
        {
            _node = node;
            _proxyFactory = proxyFactory;

            NextIndex = _node.Properties.CommitIndex+1;
        }

        public override void Handle(ReplicateRequest message)
        {
            try
            {
                var client = _proxyFactory.GetProxy();
                var response = client.AppendEntries(new AppendEntriesRequest
                {
                    LeaderId = _node.Properties.NodeId,
                    Term = _node.Properties.CurrentTerm,
                    PreviousLogIndex = _node.Properties.CommitIndex,
                    PreviousLogTerm = _node.Properties.CurrentTerm,
                    LeaderCommit = _node.Properties.CommitIndex,
                    Entries = new[] { message.Entry }
                });

                if (response.Success)
                    NextIndex++;
            }
            catch { }
        }

        public void Dispose()
        {
            CompleteActor();
        }
    }
}
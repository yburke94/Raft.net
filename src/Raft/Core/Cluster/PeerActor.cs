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
            while (true)
            {
                try
                {
                    var previousEntryIndex = NextIndex - 1;
                    var previousEntryTerm = _node.Log[previousEntryIndex].Value;

                    var client = _proxyFactory.GetProxy();
                    var response = client.AppendEntries(new AppendEntriesRequest
                    {
                        LeaderId = _node.Properties.NodeId,
                        Term = _node.Properties.CurrentTerm,
                        PreviousLogIndex = previousEntryIndex,
                        PreviousLogTerm = previousEntryTerm,
                        LeaderCommit = _node.Properties.CommitIndex,
                        Entries = new[] { message.Entry }
                    });

                    if (response.Success)
                    {
                        NextIndex = message.EntryIdx+1;
                        break;
                    }

                    if (NextIndex > 1)
                        NextIndex--;
                }
                catch { }
            }
        }

        public void Dispose()
        {
            CompleteActor();
        }
    }
}
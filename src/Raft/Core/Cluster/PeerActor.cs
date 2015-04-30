using System;
using System.Collections.Generic;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Infrastructure.Wcf;
using Raft.Service.Contracts;
using Serilog;

namespace Raft.Core.Cluster
{
    internal class PeerActor : Actor<ReplicateRequest>, IDisposable
    {
        private readonly INode _node;
        private readonly IServiceProxyFactory<IRaftService> _proxyFactory;
        private readonly ILogger _logger;

        public Guid NodeId { get; private set; }
        public long NextIndex { get; private set; }

        private long _matchIndex; // ?

        public PeerActor(Guid nodeId, INode node,
            IServiceProxyFactory<IRaftService> proxyFactory, ILogger logger)
        {
            _node = node;
            _proxyFactory = proxyFactory;
            _logger = logger;

            NodeId = nodeId;
            NextIndex = _node.Properties.CommitIndex+1;
        }

        public override void Handle(ReplicateRequest message)
        {
            var entryStack = new Stack<byte[]>();
            entryStack.Push(message.Entry);

            while (true)
            {
                try
                {
                    var previousEntryIndex = NextIndex - 1;
                    var previousEntryTerm = _node.Log.GetTermForEntry(previousEntryIndex).Value;

                    var client = _proxyFactory.GetProxy();
                    var response = client.AppendEntries(new AppendEntriesRequest
                    {
                        LeaderId = _node.Properties.NodeId,
                        Term = _node.Properties.CurrentTerm,
                        PreviousLogIndex = previousEntryIndex,
                        PreviousLogTerm = previousEntryTerm,
                        LeaderCommit = _node.Properties.CommitIndex,
                        Entries = entryStack.ToArray()
                    });

                    if (response.Success)
                    {
                        NextIndex = message.EntryIdx + 1;
                        break;
                    }

                    if (response.Term > _node.Properties.CurrentTerm)
                        _logger.Warning(
                            "Failed to replicate to node:{nodeId} as it's term({term}) is greater " +
                            "than current leader's term({leaderTerm}). An election will likely be triggered.",
                            NodeId, response.Term, _node.Properties.CurrentTerm);

                    if (NextIndex == 1) continue;

                    var previousEntry = _node.Log.GetLogEntry(--NextIndex);
                    entryStack.Push(previousEntry);
                }
                catch(Exception exc)
                {
                    _logger.Error(exc, "An exception was thrown trying to replicate to node: {nodeId}", NodeId);
                }
            }
        }

        public void Dispose()
        {
            CompleteActor();
        }
    }
}
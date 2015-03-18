using Disruptor;
using Microsoft.Practices.ServiceLocation;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Extensions;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcCommandApplier : IEventHandler<ApplyCommandRequested>
    {
        private readonly INode _node;
        private readonly CommandRegister _commandRegister;
        private readonly IServiceLocator _serviceLocator;
        private readonly IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> _nodePublisher;

        public RpcCommandApplier(INode node, CommandRegister commandRegister, IServiceLocator serviceLocator,
            IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> nodePublisher)
        {
            _node = node;
            _commandRegister = commandRegister;
            _serviceLocator = serviceLocator;
            _nodePublisher = nodePublisher;
        }

        public void OnNext(ApplyCommandRequested data, long sequence, bool endOfBatch)
        {
            var appliedDifference = data.LogIdx-_node.Data.LastApplied;
            var logsToApply = EnumerableUtilities.Range(_node.Data.LastApplied + 1, (int)appliedDifference);
            foreach (var logIdx in logsToApply)
            {
                var command = _commandRegister.Get(_node.Data.CurrentTerm, logIdx);

                // The term may have been increased before the command was applied. In which case, rely on log matching to fix.
                if (command == null) continue;

                command.Execute(_serviceLocator);
                _nodePublisher.PublishEvent(new NodeCommandScheduled
                {
                    Command = new ApplyEntry
                    {
                        EntryIdx = logIdx
                    }
                }).Wait();
            }
        }
    }
}

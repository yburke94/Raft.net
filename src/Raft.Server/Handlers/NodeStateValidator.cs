using System.Runtime.Serialization.Formatters;
using Raft.Core;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 1 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator*
    ///     LogEncoder
    ///     LogReplicator
    ///     LogWriter
    /// </summary>
    internal class NodeStateValidator : RaftEventHandler
    {
        private readonly IRaftNode _raftNode;

        public NodeStateValidator(IRaftNode raftNode)
        {
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var internalCommand = @event.Command as IRaftInternalCommand;

            if (internalCommand != null)
                internalCommand.NodeAction(_raftNode);
            else
                _raftNode.LogEntry();
        }
    }
}
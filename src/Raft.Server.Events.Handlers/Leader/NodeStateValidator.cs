using Raft.Core.StateMachine;
using Raft.Server.Commands.Internal;

namespace Raft.Server.Events.Handlers.Leader
{
    /// <summary>
    /// 1 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator*
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator
    ///     CommandFinalizer
    /// </summary>
    public class NodeStateValidator : LeaderEventHandler
    {
        private readonly IRaftNode _raftNode;

        public NodeStateValidator(IRaftNode raftNode)
        {
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduled @event)
        {
            var internalCommand = @event.Command as IRaftInternalCommand;

            if (internalCommand != null)
                internalCommand.NodeAction(_raftNode);
            else
                _raftNode.ScheduleCommandExecution();
        }
    }
}
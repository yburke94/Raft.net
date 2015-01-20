using Raft.Core;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 5 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogReplicator
    ///     LogWriter
    ///     CommandFinalizer*
    /// </summary>
    internal class CommandFinalizer : RaftEventHandler
    {
        private readonly IRaftNode _raftNode;

        public CommandFinalizer(IRaftNode raftNode)
        {
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            if (@event.TaskCompletionSource != null)
                @event.TaskCompletionSource.SetResult(new CommandExecutionResult(true));

            _raftNode.AddLogEntry();
        }
    }
}

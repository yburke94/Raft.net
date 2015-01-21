using Raft.Core;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 5 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator
    ///     CommandApplier*
    /// </summary>
    internal class CommandApplier : RaftEventHandler
    {
        private readonly IRaftNode _raftNode;
        private readonly RaftServerContext _context;

        public CommandApplier(IRaftNode raftNode, RaftServerContext context)
        {
            _raftNode = raftNode;
            _context = context;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            @event.Command.Execute(_context);
            _raftNode.ApplyCommand();

            if (@event.TaskCompletionSource != null)
                @event.TaskCompletionSource.SetResult(new CommandExecutionResult(true));
        }
    }
}

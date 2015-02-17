using Raft.Core;
using Raft.Server.Events;

namespace Raft.Server.Handlers.Leader
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
    internal class CommandApplier : LeaderEventHandler
    {
        private readonly IRaftNode _raftNode;
        private readonly RaftServerContext _context;

        public CommandApplier(IRaftNode raftNode, RaftServerContext context)
        {
            _raftNode = raftNode;
            _context = context;
        }

        public override void Handle(CommandScheduled @event)
        {
            @event.Command.Execute(_context);
            _raftNode.ApplyCommand(@event.LogEntry.Index);

            if (@event.TaskCompletionSource != null)
                @event.TaskCompletionSource.SetResult(new CommandExecuted(true));
        }
    }
}

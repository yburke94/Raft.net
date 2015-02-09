using System.Runtime.InteropServices;
using Raft.Core;
using Raft.Server.Log;

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
        private readonly EncodedEntryRegister _entryRegister;
        private readonly RaftServerContext _context;

        public CommandApplier(IRaftNode raftNode, EncodedEntryRegister entryRegister, RaftServerContext context)
        {
            _raftNode = raftNode;
            _entryRegister = entryRegister;
            _context = context;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var entryIdx = _entryRegister.GetEncodedLog(@event.Id).Key;

            @event.Command.Execute(_context);
            _raftNode.ApplyCommand(entryIdx);

            if (@event.TaskCompletionSource != null)
                @event.TaskCompletionSource.SetResult(new CommandExecutionResult(true));
        }
    }
}

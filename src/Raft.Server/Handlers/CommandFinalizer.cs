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
        private readonly LogRegister _logRegister;
        private readonly IRaftNode _raftNode;

        public CommandFinalizer(LogRegister logRegister, IRaftNode raftNode)
        {
            _logRegister = logRegister;
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            if (@event.TaskCompletionSource != null)
                @event.TaskCompletionSource.SetResult(new LogResult(true));

            if (_logRegister.HasLogEntry(@event.Id))
                _logRegister.EvictEntry(@event.Id);

            _raftNode.EntryLogged();
        }
    }
}

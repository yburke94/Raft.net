using Microsoft.Practices.ServiceLocation;
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
        private readonly IServiceLocator _serviceLocator;

        public CommandApplier(IRaftNode raftNode, IServiceLocator serviceLocator)
        {
            _raftNode = raftNode;
            _serviceLocator = serviceLocator;
        }

        public override void Handle(CommandScheduled @event)
        {
            @event.Command.Execute(_serviceLocator);
            _raftNode.ApplyCommand(@event.LogEntry.Index);

            if (@event.TaskCompletionSource != null)
                @event.TaskCompletionSource.SetResult(new CommandExecuted(true));
        }
    }
}

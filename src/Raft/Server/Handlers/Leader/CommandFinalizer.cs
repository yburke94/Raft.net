using Microsoft.Practices.ServiceLocation;
using Raft.Contracts;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 4 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator
    ///     CommandFinalizer*
    /// </summary>
    internal class CommandFinalizer : LeaderEventHandler
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> _nodePublisher;

        public CommandFinalizer(IServiceLocator serviceLocator,
            IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> nodePublisher)
        {
            _serviceLocator = serviceLocator;
            _nodePublisher = nodePublisher;
        }

        public override void Handle(CommandScheduled @event)
        {
            // An entry is considered committed once it has been written to persistant storage and replicated.
            _nodePublisher.PublishEvent(new NodeCommandScheduled
            {
                Command = new CommitEntry
                {
                    EntryIdx = @event.LogEntry.Index,
                    EntryTerm = @event.LogEntry.Term
                }
            }).Wait();

            @event.Command.Execute(_serviceLocator);
            _nodePublisher.PublishEvent(new NodeCommandScheduled
            {
                Command = new ApplyEntry
                {
                    EntryIdx = @event.LogEntry.Index
                }
            }).Wait();

            if (@event.CompletionSource != null)
                @event.CompletionSource.SetResult(new CommandExecutionResult(true));
        }
    }
}

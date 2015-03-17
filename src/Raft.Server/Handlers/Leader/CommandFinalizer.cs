using Microsoft.Practices.ServiceLocation;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.BufferEvents.Translators;
using Raft.Server.Data;
using Raft.Server.Handlers.Core;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 5 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator
    ///     CommandFinalizer*
    /// </summary>
    public class CommandFinalizer : LeaderEventHandler
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
            _nodePublisher.PublishEvent(new NodeCommandTranslator(new CommitEntry
            {
                EntryIdx = @event.LogEntry.Index,
                EntryTerm = @event.LogEntry.Term
            })).Wait();


            @event.Command.Execute(_serviceLocator);
            _nodePublisher.PublishEvent(new NodeCommandTranslator(new ApplyEntry
            {
                EntryIdx = @event.LogEntry.Index
            })).Wait();

            if (@event.CompletionSource != null)
                @event.CompletionSource.SetResult(new CommandExecutionResult(true));
        }
    }
}

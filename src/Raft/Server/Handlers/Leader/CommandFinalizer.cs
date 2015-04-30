using Microsoft.Practices.ServiceLocation;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;

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
    internal class CommandFinalizer : BufferEventHandler<CommandScheduled>
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly IPublishToBuffer<InternalCommandScheduled> _nodePublisher;

        public CommandFinalizer(IServiceLocator serviceLocator,
            IPublishToBuffer<InternalCommandScheduled> nodePublisher)
        {
            _serviceLocator = serviceLocator;
            _nodePublisher = nodePublisher;
        }

        public override void Handle(CommandScheduled @event)
        {
            // An entry is considered committed once it has been written to persistant storage and replicated.
            _nodePublisher.PublishEvent(new InternalCommandScheduled
            {
                Command = new CommitEntry
                {
                    EntryIdx = @event.LogEntry.Index,
                    EntryTerm = @event.LogEntry.Term,
                    Entry = @event.EncodedEntry
                }
            }).Wait();

            @event.Command.Execute(_serviceLocator);
            _nodePublisher.PublishEvent(new InternalCommandScheduled
            {
                Command = new ApplyEntry
                {
                    EntryIdx = @event.LogEntry.Index
                }
            }).Wait();

            @event.CompleteEvent();
        }
    }
}

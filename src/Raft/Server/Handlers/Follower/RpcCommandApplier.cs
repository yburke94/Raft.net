using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Extensions;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcCommandApplier : BufferEventHandler<AppendEntriesRequested>
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly INode _node;
        private readonly CommandRegister _register;
        private readonly IPublishToBuffer<InternalCommandScheduled> _nodePublisher;

        public RpcCommandApplier(IServiceLocator serviceLocator, INode node, CommandRegister register,
            IPublishToBuffer<InternalCommandScheduled> nodePublisher)
        {
            _serviceLocator = serviceLocator;
            _node = node;
            _register = register;
            _nodePublisher = nodePublisher;
        }

        public override void Handle(AppendEntriesRequested @event)
        {
            if (!@event.LeaderCommit.HasValue)
                throw new InvalidOperationException("The event data is invalid. LeaderCommit must have a value set.");

            if (@event.EntriesDeserialized != null)
            {
                if (@event.EntriesDeserialized.Any(x => x.Index <= @event.LeaderCommit))
                    ApplyLogMatchingCommands(@event.EntriesDeserialized, @event.LeaderCommit.Value);

                if (@event.EntriesDeserialized.Any(x => x.Index > @event.LeaderCommit))
                    AddUncommittedCommandsToRegister(@event.EntriesDeserialized, @event.LeaderCommit.Value);
            }

            if (_node.Properties.LastApplied == @event.LeaderCommit) return;

            ApplyCommandsFromPreviousRequests(@event.LeaderCommit.Value);

            @event.CompleteEvent();
        }

        private void ApplyLogMatchingCommands(LogEntry[] entries, long leaderCommit)
        {
            var entriesFiltered = entries
                .Where(x => x.Index <= leaderCommit)
                .OrderBy(x => x.Index)
                .ToList();

            for (var idx = 0; idx < entriesFiltered.Count; idx++)
            {
                var entry = entriesFiltered[idx];
                if (idx != 0)
                {
                    var prevEntry = entriesFiltered[idx - 1];
                    if (prevEntry.Index + 1 != entry.Index)
                        throw new InvalidOperationException(string.Format(
                            "Error applying log entries received from request. " +
                            "The entries received were not in sequential order. " +
                            "The last log idx successfuly applied was {0}. " +
                            "Expected {1} to follow but instead received {2}.",
                            prevEntry.Index, prevEntry.Index + 1, entry.Index));
                }

                entry.Command.Execute(_serviceLocator);
                ApplyEntryToNode(entry.Index);
            }
        }

        private void AddUncommittedCommandsToRegister(LogEntry[] entries, long leaderCommit)
        {
            entries
                .Where(x => x.Index > leaderCommit)
                .OrderBy(x => x.Index)
                .ToList().ForEach(entry => {
                    _register.Add(_node.Properties.CurrentTerm, entry.Index, entry.Command);
                });
        }

        private void ApplyCommandsFromPreviousRequests(long leaderCommit)
        {
            var appliedDifference = leaderCommit - _node.Properties.LastApplied;
            var logsToApply = EnumerableUtilities.Range(_node.Properties.LastApplied + 1, (int)appliedDifference);
            foreach (var logIdx in logsToApply)
            {
                var command = _register.Get(_node.Properties.CurrentTerm, logIdx);
                if (command == null)
                    throw new InvalidOperationException(string.Format(
                            "Error applying log entries from command register. " +
                            "The register is missing an entry for log index {0}. ", logIdx));

                command.Execute(_serviceLocator);
                ApplyEntryToNode(logIdx);
            }
        }

        private void ApplyEntryToNode(long entryIdx)
        {
            _nodePublisher.PublishEvent(new InternalCommandScheduled
            {
                Command = new ApplyEntry { EntryIdx = entryIdx }
            }).Wait();
        }
    }
}

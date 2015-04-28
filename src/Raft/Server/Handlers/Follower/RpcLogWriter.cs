using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using Raft.Contracts.Persistance;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcLogWriter : BufferEventHandler<AppendEntriesRequested>
    {
        private readonly IWriteDataBlocks _writeDataBlocks;
        private readonly INode _node;
        private readonly IPublishToBuffer<InternalCommandScheduled> _nodePublisher;

        public RpcLogWriter(IWriteDataBlocks writeDataBlocks, INode node,
            IPublishToBuffer<InternalCommandScheduled> nodePublisher)
        {
            _writeDataBlocks = writeDataBlocks;
            _node = node;
            _nodePublisher = nodePublisher;
        }

        public override void Handle(AppendEntriesRequested @event)
        {
            if (@event.Entries == null || !@event.Entries.Any()) return;

            var entries = @event.Entries
                .Select(entry =>
                {
                    using (var ms = new MemoryStream(entry))
                    {
                        var logEntry = Serializer.DeserializeWithLengthPrefix<LogEntry>(ms, PrefixStyle.Base128);
                        return new
                        {
                            Entry = entry,
                            DeserializedEntry = logEntry
                        };
                    }
                })
                .OrderBy(x => x.DeserializedEntry.Index)
                .ToList();

            var lowestIdx = entries.First().DeserializedEntry.Index;
            if (lowestIdx != _node.Properties.CommitIndex + 1)
                throw new InvalidOperationException(string.Format(
                    "The request sent was invalid. The current commit index for the node is '{0}'. " +
                    "The lowest log entry index was expected to be '{1}' but was '{2}'.",
                    _node.Properties.CommitIndex, _node.Properties.CommitIndex + 1, lowestIdx));

            @event.EntriesDeserialized = entries.Select(x => x.DeserializedEntry).ToArray();

            var dataBlocks = entries
                .Select(x => new DataBlock
                {
                    Data = x.Entry,
                    Metadata = new Dictionary<string, string>
                    {
                        {"BodyType", typeof (LogEntry).AssemblyQualifiedName}
                    }
                })
                .ToArray();

            _writeDataBlocks.WriteBlocks(dataBlocks);
            entries.ForEach(entry => _nodePublisher.PublishEvent(
                new InternalCommandScheduled
                {
                    Command = new CommitEntry
                    {
                        EntryIdx = entry.DeserializedEntry.Index,
                        EntryTerm = entry.DeserializedEntry.Term
                    }
                }).Wait());
        }
    }
}

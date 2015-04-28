using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Raft.Contracts.Persistance;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcLogTruncator : BufferEventHandler<AppendEntriesRequested>
    {
        private readonly INode _node;
        private readonly IWriteDataBlocks _writeDataBlocks;
        private readonly IPublishToBuffer<InternalCommandScheduled> _nodePublisher;

        public RpcLogTruncator(INode node, IWriteDataBlocks writeDataBlocks,
            IPublishToBuffer<InternalCommandScheduled> nodePublisher)
        {
            _node = node;
            _writeDataBlocks = writeDataBlocks;
            _nodePublisher = nodePublisher;
        }

        public override void Handle(AppendEntriesRequested @event)
        {
            if (!@event.PreviousLogIndex.HasValue || !@event.PreviousLogTerm.HasValue)
                return;

            if (@event.PreviousLogIndex.Equals(_node.Properties.CommitIndex))
                return;

            if (@event.PreviousLogIndex > _node.Properties.CommitIndex
                || @event.PreviousLogTerm > _node.Properties.CurrentTerm)
                throw new InvalidOperationException(
                    "This command is invalid and should not be published to the buffer.");

            var truncateCommandEntry = new TruncateLogCommandEntry
            {
                TruncateFromIndex = @event.PreviousLogIndex.Value
            };

            using (var ms = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(ms, truncateCommandEntry, PrefixStyle.Base128);
                _writeDataBlocks.WriteBlock(new DataBlock
                {
                    Data = ms.ToArray(),
                    Metadata = new Dictionary<string, string>
                    {
                        {"BodyType", typeof (TruncateLogCommandEntry).AssemblyQualifiedName}
                    }
                });
            }

            _nodePublisher.PublishEvent(
                new InternalCommandScheduled
                {
                    Command = new TruncateLog
                    {
                        TruncateFromIndex = @event.PreviousLogIndex.Value
                    }
                });
        }
    }
}

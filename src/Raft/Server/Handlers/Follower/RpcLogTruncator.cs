using System;
using System.Collections.Generic;
using System.IO;
using Disruptor;
using ProtoBuf;
using Raft.Contracts.Persistance;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcLogTruncator : IEventHandler<AppendEntriesRequested>
    {
        private readonly INode _node;
        private readonly IWriteDataBlocks _writeDataBlocks;
        private readonly IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> _nodePublisher;

        public RpcLogTruncator(INode node, IWriteDataBlocks writeDataBlocks,
            IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> nodePublisher)
        {
            _node = node;
            _writeDataBlocks = writeDataBlocks;
            _nodePublisher = nodePublisher;
        }

        public void OnNext(AppendEntriesRequested data, long sequence, bool endOfBatch)
        {
            if (!data.PreviousLogIndex.HasValue || !data.PreviousLogTerm.HasValue)
                return;

            if (data.PreviousLogIndex.Equals(_node.Data.CommitIndex))
                return;

            if (data.PreviousLogIndex > _node.Data.CommitIndex
                || data.PreviousLogTerm > _node.Data.CurrentTerm)
                throw new InvalidOperationException(
                    "This command is invalid and should not be published to the buffer.");

            var truncateCommandEntry = new TruncateLogCommandEntry
            {
                TruncateFromIndex = data.PreviousLogIndex.Value
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
                new NodeCommandScheduled
                {
                    Command = new TruncateLog
                    {
                        TruncateFromIndex = data.PreviousLogIndex.Value
                    }
                });
        }
    }
}

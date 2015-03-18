using System.IO;
using Disruptor;
using ProtoBuf;
using Raft.Core.Commands;
using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Journaler;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcLogWriter : IEventHandler<CommitCommandRequested>
    {
        private readonly IWriteDataBlocks _writeDataBlocks;
        private readonly CommandRegister _commandRegister;
        private readonly IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> _nodePublisher;

        public RpcLogWriter(IWriteDataBlocks writeDataBlocks, CommandRegister commandRegister,
            IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> nodePublisher)
        {
            _writeDataBlocks = writeDataBlocks;
            _commandRegister = commandRegister;
            _nodePublisher = nodePublisher;
        }

        public void OnNext(CommitCommandRequested data, long sequence, bool endOfBatch)
        {
            try
            {
                // TODO: This needs to take into account fixing the log in the event of log matching.
                // TODO: As the commands are decoded, it should apply them if their index is <= CommitIndex.
                // TODO: Both the log Commit and Apply logic can and should be extracted and shared with the leader buffer.
                // TODO: Test the crap out of this...
                // LogEntry decodedEntry;
                // using (var stream = new MemoryStream(data.Entry, false))
                // {
                //     decodedEntry = Serializer.DeserializeWithLengthPrefix<LogEntry>(stream, PrefixStyle.Base128);
                // }
                // 
                // // TODO: Generate checksum and compare?.
                // _writeDataBlocks.WriteBlock(data.Entry);
                // _nodePublisher.PublishEvent(new NodeCommandScheduled
                // {
                //     Command = new CommitEntry
                //     {
                //         EntryIdx = decodedEntry.Index,
                //         EntryTerm = decodedEntry.Term
                //     }
                // }).Wait();
                // 
                // _commandRegister.Add(decodedEntry.Term, decodedEntry.Index, decodedEntry.Command);
            }
            catch
            {
                //  TODO: Add logging and log error
                // _log.Error("Failed to apply log: {logIndex} for term: {term}")
            }
        }
    }
}

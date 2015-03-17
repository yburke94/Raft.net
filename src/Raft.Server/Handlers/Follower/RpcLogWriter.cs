using System.IO;
using Disruptor;
using ProtoBuf;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Journaler;
using Raft.Server.BufferEvents;
using Raft.Server.BufferEvents.Translators;
using Raft.Server.Data;
using Raft.Server.Handlers.Core;

namespace Raft.Server.Handlers.Follower
{
    public class RpcLogWriter : IEventHandler<CommitCommandRequested>
    {
        private readonly IJournal _journal;
        private readonly CommandRegister _commandRegister;
        private readonly IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> _nodePublisher;

        public RpcLogWriter(IJournal journal, CommandRegister commandRegister,
            IPublishToBuffer<NodeCommandScheduled, NodeCommandResult> nodePublisher)
        {
            _journal = journal;
            _commandRegister = commandRegister;
            _nodePublisher = nodePublisher;
        }

        public void OnNext(CommitCommandRequested data, long sequence, bool endOfBatch)
        {
            try
            {
                LogEntry decodedEntry;
                using (var stream = new MemoryStream(data.Entry, false))
                {
                    decodedEntry = Serializer.DeserializeWithLengthPrefix<LogEntry>(stream, PrefixStyle.Base128);
                }

                // TODO: Generate checksum and compare?.
                _journal.WriteBlock(data.Entry);
                _nodePublisher.PublishEvent(new NodeCommandTranslator(new CommitEntry
                {
                    EntryIdx = decodedEntry.Index,
                    EntryTerm = decodedEntry.Term
                })).Wait();

                _commandRegister.Add(decodedEntry.Term, decodedEntry.Index, decodedEntry.Command);
            }
            catch
            {
                //  TODO: Add logging and log error
                // _log.Error("Failed to apply log: {logIndex} for term: {term}")
            }
        }
    }
}

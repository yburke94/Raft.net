using System;
using System.Collections.Generic;
using System.Linq;
using Raft.Core;
using Raft.Infrastructure.Journaler;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Log;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 4 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogWriter*
    ///     LogReplicator
    ///     CommandApplier
    /// </summary>
    internal class LogWriter : RaftEventHandler, ISkipInternalCommands
    {
        private readonly EncodedLogRegister _encodedLogRegister;
        private readonly IJournal _journal;
        private readonly IRaftNode _raftNode;

        private readonly IDictionary<long, Guid> _entrySequenceIdMap = new Dictionary<long, Guid>();

        public LogWriter(EncodedLogRegister encodedLogRegister, IJournal journal, IRaftNode raftNode)
        {
            _encodedLogRegister = encodedLogRegister;
            _journal = journal;
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            _entrySequenceIdMap.Add(Sequence, @event.Id);

            if (!EndOfBatch)
                return;

            var blocksToWrite = _entrySequenceIdMap.OrderBy(x => x.Key)
                .Select(x => _encodedLogRegister.GetEncodedLog(x.Value))
                .ToArray();

            _journal.WriteBlocks(blocksToWrite);

            for (var i = 0; i < _entrySequenceIdMap.Count; i++)
            {
                _raftNode.AddLogEntry();
            }

            _entrySequenceIdMap.Clear();
        }
    }
}

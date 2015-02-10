using System;
using System.Collections.Generic;
using System.Linq;
using Raft.Core;
using Raft.Infrastructure.Journaler;
using Raft.Server.Events;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Log;

namespace Raft.Server.Handlers.Leader
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
    internal class LogWriter : LeaderEventHandler, ISkipInternalCommands
    {
        private readonly EncodedEntryRegister _encodedEntryRegister;
        private readonly IJournal _journal;
        private readonly IRaftNode _raftNode;

        private readonly IDictionary<long, Guid> _entryIndexIdMap = new Dictionary<long, Guid>();

        public LogWriter(EncodedEntryRegister encodedEntryRegister, IJournal journal, IRaftNode raftNode)
        {
            _encodedEntryRegister = encodedEntryRegister;
            _journal = journal;
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduled @event)
        {
            var logIdx = _encodedEntryRegister.GetEncodedLog(@event.Id).Key;
            _entryIndexIdMap.Add(logIdx, @event.Id);

            if (!EndOfBatch)
                return;

            var blocksToWrite = _entryIndexIdMap.OrderBy(x => x.Key)
                .Select(x => _encodedEntryRegister.GetEncodedLog(x.Value).Value)
                .ToArray();

            _journal.WriteBlocks(blocksToWrite);

            _entryIndexIdMap.Select(x => x.Key).ToList()
                .ForEach(_raftNode.CommitLogEntry);

            _entryIndexIdMap.Clear();
        }
    }
}

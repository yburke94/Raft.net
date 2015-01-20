using System;
using System.Collections.Generic;
using System.Linq;
using Raft.Infrastructure.Journaler;
using Raft.Server.Log;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 4 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogReplicator
    ///     LogWriter*
    ///     CommandFinalizer
    /// </summary>
    internal class LogWriter : RaftEventHandler, ISkipInternalCommands
    {
        private readonly LogRegister _logRegister;
        private readonly IJournal _journal;

        private readonly IDictionary<long, Guid> _entrySequenceIdMap = new Dictionary<long, Guid>();

        public LogWriter(LogRegister logRegister, IJournal journal)
        {
            _logRegister = logRegister;
            _journal = journal;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            _entrySequenceIdMap.Add(Sequence, @event.Id);

            if (!EndOfBatch)
                return;

            var blocksToWrite = _entrySequenceIdMap.OrderBy(x => x.Key)
                .Select(x => _logRegister.GetEncodedLog(x.Value))
                .ToArray();

            _journal.WriteBlocks(blocksToWrite);

            _entrySequenceIdMap.Values.ToList()
                .ForEach(x => _logRegister.EvictEntry(x));

            _entrySequenceIdMap.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using Raft.Contracts.Persistance;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 2 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     LogEncoder
    ///     LogWriter*
    ///     LogReplicator
    ///     CommandFinalizer
    /// </summary>
    internal class LogWriter : BufferEventHandler<CommandScheduled>
    {
        private readonly IWriteDataBlocks _writeDataBlocks;

        public LogWriter(IWriteDataBlocks writeDataBlocks)
        {
            _writeDataBlocks = writeDataBlocks;
        }

        public override void Handle(CommandScheduled @event)
        {
            if (@event.LogEntry == null || @event.EncodedEntry == null)
                throw new InvalidOperationException("Must set EncodedEntry on event before executing this step.");

            _writeDataBlocks.WriteBlock(new DataBlock
            {
                Data = @event.EncodedEntry,
                Metadata = new Dictionary<string, string>
                {
                    {"BodyType", typeof (LogEntry).AssemblyQualifiedName}
                }
            });
        }
    }
}
